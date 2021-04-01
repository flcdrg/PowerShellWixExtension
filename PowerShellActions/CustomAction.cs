using System;
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

        private const string PowerShellFilesElevatedUninstallDeferredProperty = "PowerShellFilesElevatedUninstallDeferred";
        private const string PowerShellFilesUninstallDeferredProperty = "PowerShellFilesUninstallDeferred";
        private const string PowerShellScriptsElevatedUninstallDeferredProperty = "PowerShellScriptsElevatedUninstallDeferred";
        private const string PowerShellScriptsUninstallDeferredProperty = "PowerShellScriptsUninstallDeferred";

        [CustomAction]
        public static ActionResult PowerShellFilesImmediate(Session session)
        {
            session.Log("PowerShellFilesImmediate start");

            return FilesImmediate(session, 0, PowerShellFilesDeferredProperty);
        }

        [CustomAction]
        public static ActionResult PowerShellFilesDeferred(Session session)
        {
            session.Log("PowerShellFilesDeferred start");

            return FilesDeferred(session, PowerShellFilesDeferredProperty);
        }

        [CustomAction]
        public static ActionResult PowerShellFilesElevatedImmediate(Session session)
        {
            session.Log("PowerShellFilesElevatedImmediate start");

            return FilesImmediate(session, 1, PowerShellFilesElevatedDeferredProperty);
        }

        [CustomAction]
        public static ActionResult PowerShellFilesElevatedDeferred(Session session)
        {
            session.Log("PowerShellFilesElevatedDeferred start");

            return FilesDeferred(session, PowerShellFilesElevatedDeferredProperty);
        }

        [CustomAction]
        public static ActionResult PowerShellFilesUninstall(Session session)
        {
            session.Log("PowerShellFilesUninstall start");

            return FilesImmediate(session, 0, PowerShellFilesUninstallDeferredProperty);
        }


        [CustomAction]
        public static ActionResult PowerShellFilesUninstallDeferred(Session session)
        {
            session.Log("PowerShellFilesUninstallDeferred start");

            return FilesDeferred(session, PowerShellFilesUninstallDeferredProperty);
        }


        [CustomAction]
        public static ActionResult PowerShellFilesElevatedUninstall(Session session)
        {
            session.Log("PowerShellFilesElevatedUninstall start");

            return FilesImmediate(session, 1, PowerShellFilesElevatedUninstallDeferredProperty);
        }

        [CustomAction]
        public static ActionResult PowerShellFilesElevatedUninstallDeferred(Session session)
        {
            session.Log("PowerShellFilesElevatedUninstallDeferred start");

            return FilesDeferred(session, PowerShellFilesElevatedUninstallDeferredProperty);
        }

        [CustomAction]
        public static ActionResult PowerShellScriptsImmediate(Session session)
        {
            session.Log("PowerShellScriptsImmediate start");

            return ScriptsImmediate(session, 0, PowerShellScriptsDeferredProperty);
        }

        [CustomAction]
        public static ActionResult PowerShellScriptsDeferred(Session session)
        {
            session.Log("PowerShellScriptsDeferred start");

            return ScriptsDeferred(session, PowerShellScriptsDeferredProperty);
        }

        [CustomAction]
        public static ActionResult PowerShellScriptsElevatedImmediate(Session session)
        {
            session.Log("PowerShellScriptsElevatedImmediate start");

            return ScriptsImmediate(session, 1, PowerShellScriptsElevatedDeferredProperty);
        }

        [CustomAction]
        public static ActionResult PowerShellScriptsElevatedDeferred(Session session)
        {
            session.Log("PowerShellScriptsElevatedDeferred start");

            return ScriptsDeferred(session, PowerShellScriptsElevatedDeferredProperty);
        }

        [CustomAction]
        public static ActionResult PowerShellScriptsUninstall(Session session)
        {
            session.Log("PowerShellScriptsUninstall start");

            return ScriptsImmediate(session, 0, PowerShellScriptsUninstallDeferredProperty);
        }

        [CustomAction]
        public static ActionResult PowerShellScriptsUninstallDeferred(Session session)
        {
            session.Log("PowerShellScriptsUninstallDeferred start");

            return ScriptsDeferred(session, PowerShellScriptsUninstallDeferredProperty);
        }

        [CustomAction]
        public static ActionResult PowerShellScriptsElevatedUninstall(Session session)
        {
            session.Log("PowerShellScriptsElevatedUninstall start");

            return ScriptsImmediate(session, 1, PowerShellScriptsElevatedUninstallDeferredProperty);
        }

        [CustomAction]
        public static ActionResult PowerShellScriptsElevatedUninstallDeferred(Session session)
        {
            session.Log("PowerShellScriptsElevatedUninstallDeferred start");

            return ScriptsDeferred(session, PowerShellScriptsElevatedUninstallDeferredProperty);
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
                using (View view = db.OpenView(string.Format("SELECT `Id`, `Script`, `IgnoreErrors` FROM `PowerShellScripts` WHERE `Elevated` = {0} ORDER BY `Order`", elevated)))
                {
                    view.Execute();

                    Record row;
                    while ((row = view.Fetch()) != null)
                    {
                        string condition = row["Condition"]?.ToString();
                        if (!string.IsNullOrEmpty(condition) && !session.EvaluateCondition(condition))
                        {
                            session.Log($"Condition evaluated to false. Skip PS script {row["Id"]?.ToString()}");
                            continue;
                        }

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
                    session.Log($"Executing PowerShell script {datum.Id}:\n{script}");

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
                using (View view = db.OpenView(string.Format("SELECT `Id`, `File`, `Arguments`, `IgnoreErrors` FROM `{0}` WHERE `Elevated` = {1} ORDER BY `Order`", tableName, elevated)))
                {
                    view.Execute();

                    doc = new XDocument(new XDeclaration("1.0", "utf-16", "yes"), new XElement("r"));

                    foreach (Record row in view)
                    {
                        string condition = row["Condition"]?.ToString();
                        if (!string.IsNullOrEmpty(condition) && !session.EvaluateCondition(condition))
                        {
                            session.Log($"Condition evaluated to false. Skip PS file {row["Id"]?.ToString()}");
                            continue;
                        }                        

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
                    session.Log($"Executing PowerShell file: '{file}' {arguments}");

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