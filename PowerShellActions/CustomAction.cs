using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Microsoft.Deployment.WindowsInstaller;

namespace PowerShellActions
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult PowerShellFilesImmediate(Session session)
        {
            Database db = session.Database;

            if (!db.Tables.Contains("PowerShellFiles"))
                return ActionResult.Success;

            try
            {
                View view = db.OpenView("SELECT `Id`, `File`, `Arguments` FROM `PowerShellFiles`");
                view.Execute();

                var data = new CustomActionData();

                XDocument doc = new XDocument(new XElement("r"));

                foreach (Record row in view)
                {
                    doc.Root.Add(new XElement("d", new XAttribute("Id", row["Id"]), new XAttribute("file", session.Format(row["File"].ToString())), new XAttribute("args", session.Format( row["Arguments"].ToString()))));
                }

                session["PowerShellFilesDeferred"] = doc.ToString();

                // Tell the installer to increase the value of the final total
                // length of the progress bar by the total number of ticks in
                // the custom action.
                Record hProgressRec = new Record(2);

                hProgressRec[1] = 3;
                hProgressRec[2] = iTotalTicks;
                MessageResult iResult = session.Message(InstallMessage.Progress, hProgressRec);
                if ((iResult == MessageResult.Cancel))
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

        [CustomAction]
        public static ActionResult PowerShellFilesDeferred(Session session)
        {
            session.Log("PowerShellFilesDeferred start");
            Record hActionRec = new Record(3);
            Record hProgressRec = new Record(3);

            // Installer is executing the installation script. Set up a
            // record specifying appropriate templates and text for
            // messages that will inform the user about what the custom
            // action is doing. Tell the installer to use this template and
            // text in progress messages.

            hActionRec[1] = "PowerShellFilesDeferred";
            hActionRec[2] = "PowerShell Files";
            hActionRec[3] = "[1] of [2], [3]";
            MessageResult iResult = session.Message(InstallMessage.ActionStart, hActionRec);
            if ((iResult == MessageResult.Cancel))
            {
                return ActionResult.UserExit;
            }

            // Tell the installer to use explicit progress messages.
            hProgressRec[1] = 1;
            hProgressRec[2] = 1;
            hProgressRec[3] = 0;
            iResult = session.Message(InstallMessage.Progress, hProgressRec);
            if ((iResult == MessageResult.Cancel))
            {
                return ActionResult.UserExit;
            }

            try
            {
                var doc = XDocument.Parse(session.CustomActionData.ToString());

                foreach (var row in doc.Root.Elements("d"))
                {
                    string file = row.Attribute("file").Value;

                    string arguments = row.Attribute("args").Value;

                    using (var sr = new System.IO.StreamReader(file))
                    {
                        string content = sr.ReadToEnd();

                        using (var task = new PowerShellTask(file, arguments, session))
                        {
                            task.Execute();
                        }
                    }
                }
                return ActionResult.Success;
            }
            catch (Exception ex)
            {
                session.Log(ex.Message);
                return ActionResult.Failure;
            }
        }
        [CustomAction]
        public static ActionResult PowerShellScriptsImmediate(Session session)
        {
            Database db = session.Database;

            if (!db.Tables.Contains("PowerShellScripts"))
                return ActionResult.Success;

            try
            {
                View view = db.OpenView("SELECT `Id`, `Script` FROM `PowerShellScripts`");
                view.Execute();

                var data = new CustomActionData();

                foreach (Record row in view)
                {
                    data[row["Id"].ToString()] = row["Script"].ToString();
                }

                session["PowerShellScriptsDeferred"] = data.ToString();

                // Tell the installer to increase the value of the final total
                // length of the progress bar by the total number of ticks in
                // the custom action.
                Record hProgressRec = new Record(2);

                hProgressRec[1] = 3;
                hProgressRec[2] = iTotalTicks;
                MessageResult iResult = session.Message(InstallMessage.Progress, hProgressRec);
                if ((iResult == MessageResult.Cancel))
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

        private static void ExtendProgressForCustomAction(Session session, int total)
        {
            var record = new Record(2);
            record[1] = 3;
            record[2] = total;
            session.Message(InstallMessage.Progress, record);
        }

        // Specify or calculate the number of ticks in an increment
        // to the ProgressBar
        public const uint iTickIncrement = 10000;

        // Specify or calculate the total number of ticks the custom action adds to the length of the ProgressBar
        const uint iNumberItems = 100;
        public const uint iTotalTicks = iTickIncrement * iNumberItems;


        [CustomAction]
        public static ActionResult PowerShellScriptsDeferred(Session session)
        {
            Record hActionRec = new Record(3);
            Record hProgressRec = new Record(3);

            // Installer is executing the installation script. Set up a
            // record specifying appropriate templates and text for
            // messages that will inform the user about what the custom
            // action is doing. Tell the installer to use this template and
            // text in progress messages.

            hActionRec[1] = "PowerShellScriptsDeferred";
            hActionRec[2] = "PowerShell Scripts";
            hActionRec[3] = "[1] of [2], [3]";
            MessageResult iResult = session.Message(InstallMessage.ActionStart, hActionRec);
            if ((iResult == MessageResult.Cancel))
            {
                return ActionResult.UserExit;
            }

            // Tell the installer to use explicit progress messages.
            hProgressRec[1] = 1;
            hProgressRec[2] = 1;
            hProgressRec[3] = 0;
            iResult = session.Message(InstallMessage.Progress, hProgressRec);
            if ((iResult == MessageResult.Cancel))
            {
                return ActionResult.UserExit;
            }

            try
            {
                CustomActionData data = session.CustomActionData;

                foreach (KeyValuePair<string, string> datum in data)
                {
                    string script = Encoding.Unicode.GetString(Convert.FromBase64String(datum.Value));

                    using (var task = new PowerShellTask(script, session))
                    {
                        task.Execute();
                    }
                }
                return ActionResult.Success;
            }
            catch (Exception ex)
            {
                session.Log(ex.Message);
                return ActionResult.Failure;
            }
        }

        private static void DisplayWarningMessage(Session session, string message)
        {
            var record = new Record(0);
            record[0] = message;
            session.Message(InstallMessage.Warning, record);
        }
    }
}