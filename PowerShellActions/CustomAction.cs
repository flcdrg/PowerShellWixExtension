﻿using System;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using Microsoft.Deployment.WindowsInstaller;

using View = Microsoft.Deployment.WindowsInstaller.View;
using System.Collections.Generic;
using System.IO;

namespace PowerShellActions
{
    public class CustomActions
    {
        public const uint TickIncrement = 10000;

        // Specify or calculate the total number of ticks the custom action adds to the length of the ProgressBar
        public const uint TotalTicks = TickIncrement * NumberItems;
        private const uint NumberItems = 100;
        private const string PowerShellFilesElevatedDeferredProperty = "PowerShellFilesElevatedDeferred";
        private const string PowerShellFilesDeferredProperty = "PowerShellFilesDeferred";
        private const string PowerShellScriptsElevatedDeferredProperty = "PowerShellScriptsElevatedDeferred";
        private const string PowerShellScriptsDeferredProperty = "PowerShellScriptsDeferred";

        [CustomAction]
        public static ActionResult PowerShellFilesImmediate(Session session)
        {
            return FilesImmediate(session, 0, PowerShellFilesDeferredProperty);
        }

        [CustomAction]
        public static ActionResult PowerShellFilesElevatedImmediate(Session session)
        {
            return FilesImmediate(session, 1, PowerShellFilesElevatedDeferredProperty);
        }

        [CustomAction]
        public static ActionResult PowerShellFilesDeferred(Session session)
        {
            session.Log("PowerShellFilesDeferred start");

            return FilesDeferred(session, PowerShellFilesDeferredProperty);
        }

        [CustomAction]
        public static ActionResult PowerShellFilesElevatedDeferred(Session session)
        {
            session.Log("PowerShellFilesElevatedDeferred start");

            return FilesDeferred(session, PowerShellFilesElevatedDeferredProperty);
        }

        [CustomAction]
        public static ActionResult PowerShellScriptsImmediate(Session session)
        {
            return ScriptsImmediate(session, 0, PowerShellScriptsDeferredProperty);
        }

        [CustomAction]
        public static ActionResult PowerShellScriptsElevatedImmediate(Session session)
        {
            return ScriptsImmediate(session, 1, PowerShellScriptsElevatedDeferredProperty);
        }

        [CustomAction]
        public static ActionResult PowerShellScriptsDeferred(Session session)
        {
            return ScriptsDeferred(session, PowerShellScriptsDeferredProperty);
        }

        [CustomAction]
        public static ActionResult PowerShellScriptsElevatedDeferred(Session session)
        {
            return ScriptsDeferred(session, PowerShellScriptsElevatedDeferredProperty);
        }

        [Serializable]
        public class ScriptActionData
        {
            public string Id { get; set; }
            public string Script { get; set; }
            public bool IgnoreErrors { get; set; }
        }

        private static ActionResult ScriptsImmediate(Session session, int elevated, string deferredProperty)
        {
            Database db = session.Database;

            if (!db.Tables.Contains("PowerShellScripts"))
                return ActionResult.Success;

            try
            {
                List<ScriptActionData> scripts = new List<ScriptActionData>();
                using (View view = db.OpenView(string.Format("SELECT `Id`, `Script`, `IgnoreErrors` FROM `PowerShellScripts` WHERE `Elevated` = {0}", elevated)))
                {
                    view.Execute();

                    Record row;
                    while ((row = view.Fetch()) != null)
                    {
                        string script = Encoding.Unicode.GetString(Convert.FromBase64String(row["Script"].ToString()));
                        script = session.Format(script);
                        script = Convert.ToBase64String(Encoding.Unicode.GetBytes(script));

                        ScriptActionData data = new ScriptActionData()
                        {
                            Id = row["Id"].ToString(),
                            Script = script,
                            IgnoreErrors = row["IgnoreErrors"].ToString() == "1"
                        };

                        scripts.Add(data);
                        session.Log("Adding {0} to CustomActionData", data.Id);
                    }
                }

                XmlSerializer srlz = new XmlSerializer(scripts.GetType());
                using (StringWriter sw = new StringWriter())
                {
                    srlz.Serialize(sw, scripts);
                    session[deferredProperty] = sw.ToString();
                }

                // Tell the installer to increase the value of the final total
                // length of the progress bar by the total number of ticks in
                // the custom action.
                MessageResult iResult;
                using (var hProgressRec = new Record(2))
                {
                    hProgressRec[1] = 3;
                    hProgressRec[2] = TotalTicks;
                    iResult = session.Message(InstallMessage.Progress, hProgressRec);
                }

                if (iResult == MessageResult.Cancel)
                {
                    return ActionResult.UserExit;
                }

                return ActionResult.Success;
            }
            catch (Exception ex)
            {
                session.Log(ex.ToString());
                return ActionResult.Failure;
            }
            finally
            {
                db.Close();
            }
        }

        private static ActionResult ScriptsDeferred(Session session, string deferredProperty)
        {
            MessageResult iResult;
            using (var hActionRec = new Record(3))
            {
                hActionRec[1] = deferredProperty;
                hActionRec[2] = "PowerShell Scripts";
                hActionRec[3] = "[1] of [2], [3]";
                iResult = session.Message(InstallMessage.ActionStart, hActionRec);
            }

            if (iResult == MessageResult.Cancel)
            {
                return ActionResult.UserExit;
            }

            // Tell the installer to use explicit progress messages.
            using (var hProgressRec = new Record(3))
            {
                hProgressRec[1] = 1;
                hProgressRec[2] = 1;
                hProgressRec[3] = 0;
                iResult = session.Message(InstallMessage.Progress, hProgressRec);
            }

            if (iResult == MessageResult.Cancel)
            {
                return ActionResult.UserExit;
            }

            try
            {
                List<ScriptActionData> scripts = new List<ScriptActionData>();
                string cad = session["CustomActionData"];
                XmlSerializer srlz = new XmlSerializer(scripts.GetType());
                if (string.IsNullOrWhiteSpace(cad))
                {
                    session.Log("Nothing to do");
                    return ActionResult.Success;
                }

                using (StringReader sr = new StringReader(cad))
                {
                    IEnumerable<ScriptActionData> tempScripts = srlz.Deserialize(sr) as IEnumerable<ScriptActionData>;
                    if (tempScripts != null)
                    {
                        scripts.AddRange(tempScripts);
                    }
                }

                foreach (ScriptActionData datum in scripts)
                {
                    string script = Encoding.Unicode.GetString(Convert.FromBase64String(datum.Script));
                    session.Log("Executing PowerShell script:\n{0}", script);

                    using (var task = new PowerShellTask(script, session))
                    {
                        bool result = false;
                        try
                        {
                            result = task.Execute();
                            session.Log("PowerShell terminating errors: {0}", result);
                        }
                        catch (Exception ex)
                        {
                            result = false;
                            session.Log(ex.ToString());
                        }
                        if (!result)
                        {
                            if (datum.IgnoreErrors)
                            {
                                session.Log("Script execution failed. Ignoring error");
                                continue;
                            }

                            session.Log("Returning Failure");
                            return ActionResult.Failure;
                        }
                    }
                }

                return ActionResult.Success;
            }
            catch (Exception ex)
            {
                session.Log("PowerShell terminating error, returning Failure");
                session.Log(ex.ToString());
                return ActionResult.Failure;
            }
        }

        private static ActionResult FilesImmediate(Session session, int elevated, string deferredProperty)
        {
            Database db = session.Database;

            const string tableName = "PowerShellFiles";
            if (!db.Tables.Contains(tableName))
                return ActionResult.Success;

            try
            {
                XDocument doc;
                using (View view = db.OpenView(string.Format("SELECT `Id`, `File`, `Arguments`, `IgnoreErrors` FROM `{0}` WHERE `Elevated` = {1}", tableName, elevated)))
                {
                    view.Execute();

                    doc = new XDocument(new XDeclaration("1.0", "utf-16", "yes"), new XElement("r"));

                    foreach (Record row in view)
                    {
                        var args = session.Format(row["Arguments"].ToString());
                        var IgnoreErrors = session.Format(row["IgnoreErrors"].ToString());

                        session.Log("args '{0}'", args);

                        doc.Root.Add(new XElement("d", new XAttribute("Id", row["Id"]),
                            new XAttribute("file", session.Format(row["File"].ToString())), new XAttribute("args", args), new XAttribute("IgnoreErrors", IgnoreErrors)));
                    }
                }

                var cad = new CustomActionData { { "xml", doc.ToString() } };

                session[deferredProperty] = cad.ToString();

                // Tell the installer to increase the value of the final total
                // length of the progress bar by the total number of ticks in
                // the custom action.
                MessageResult iResult;
                using (var hProgressRec = new Record(2))
                {
                    hProgressRec[1] = 3;
                    hProgressRec[2] = TotalTicks;
                    iResult = session.Message(InstallMessage.Progress, hProgressRec);
                }

                if (iResult == MessageResult.Cancel)
                {
                    return ActionResult.UserExit;
                }

                return ActionResult.Success;
            }
            catch (Exception ex)
            {
                session.Log(ex.Message);
                return ActionResult.Failure;
            }
            finally
            {
                db.Close();
            }
        }

        private static ActionResult FilesDeferred(Session session, string deferredProperty)
        {

            // Installer is executing the installation script. Set up a
            // record specifying appropriate templates and text for
            // messages that will inform the user about what the custom
            // action is doing. Tell the installer to use this template and
            // text in progress messages.
            MessageResult iResult;
            using (var hActionRec = new Record(3))
            {
                hActionRec[1] = deferredProperty;
                hActionRec[2] = "PowerShell Files";
                hActionRec[3] = "[1] of [2], [3]";
                iResult = session.Message(InstallMessage.ActionStart, hActionRec);
            }

            if (iResult == MessageResult.Cancel)
            {
                return ActionResult.UserExit;
            }

            // Tell the installer to use explicit progress messages.
            using (var hProgressRec = new Record(3))
            {
                hProgressRec[1] = 1;
                hProgressRec[2] = 1;
                hProgressRec[3] = 0;
                iResult = session.Message(InstallMessage.Progress, hProgressRec);
            }

            if (iResult == MessageResult.Cancel)
            {
                return ActionResult.UserExit;
            }

            try
            {
                if (!session.CustomActionData.ContainsKey("xml"))
                {
                    session.Log("Skipping as no CustomActionData key 'xml'");
                    return ActionResult.NotExecuted;
                }

                string content = session.CustomActionData["xml"];

                XDocument doc = XDocument.Parse(content);

                foreach (XElement row in doc.Root.Elements("d"))
                {
                    string file = row.Attribute("file").Value;

                    string arguments = row.Attribute("args").Value;
                    string IgnoreErrors = row.Attribute("IgnoreErrors").Value;

                    using (var task = new PowerShellTask(file, arguments, session))
                    {
                        try
                        {
                            bool result = task.Execute();
                            session.Log("PowerShell non-terminating errors: {0}", !result);
                            if (!result)
                            {
                                if (!IgnoreErrors.Equals("0"))
                                {
                                    session.Log("Ignoring failure due to 'IgnoreErrors' marking");
                                }
                                else
                                {
                                    session.Log("Returning Failure");
                                    return ActionResult.Failure;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (!IgnoreErrors.Equals("0"))
                            {
                                session.Log("Ignoring PowerShell error due to 'IgnoreErrors' marking");
                                session.Log(ex.ToString());
                            }
                            else
                            {
                                session.Log("PowerShell terminating error, returning Failure");
                                session.Log(ex.ToString());
                                return ActionResult.Failure;
                            }
                        }
                    }
                }

                return ActionResult.Success;
            }
            catch (Exception ex)
            {
                session.Log("PowerShell terminating error, returning Failure");
                session.Log(ex.ToString());
                return ActionResult.Failure;
            }
        }
    }
}