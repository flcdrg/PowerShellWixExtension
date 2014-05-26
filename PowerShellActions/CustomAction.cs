using System;
using System.Text;
using System.Web;
using System.Xml.Linq;
using Microsoft.Deployment.WindowsInstaller;

using View = Microsoft.Deployment.WindowsInstaller.View;

namespace PowerShellActions
{
    public class CustomActions
    {
        public const uint TickIncrement = 10000;

        // Specify or calculate the total number of ticks the custom action adds to the length of the ProgressBar
        public const uint TotalTicks = TickIncrement * NumberItems;
        private const uint NumberItems = 100;

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

                var doc = new XDocument(new XDeclaration("1.0", "utf-16", "yes"), new XElement("r"));

                foreach (Record row in view)
                {
                    // XML comes in with entities already. We need to decode these before putting them back into XML again
                    var args = HttpUtility.HtmlDecode(session.Format(row["Arguments"].ToString()));

                    session.Log("args '{0}'", args);

                    doc.Root.Add(new XElement("d", new XAttribute("Id", row["Id"]), new XAttribute("file", session.Format(row["File"].ToString())), new XAttribute("args", args)));
                }

                var cad = new CustomActionData { { "xml", doc.ToString() } };

                session["PowerShellFilesDeferred"] = cad.ToString();

                // Tell the installer to increase the value of the final total
                // length of the progress bar by the total number of ticks in
                // the custom action.
                var hProgressRec = new Record(2);

                hProgressRec[1] = 3;
                hProgressRec[2] = TotalTicks;
                MessageResult iResult = session.Message(InstallMessage.Progress, hProgressRec);
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

        [CustomAction]
        public static ActionResult PowerShellFilesDeferred(Session session)
        {
            session.Log("PowerShellFilesDeferred start");
            var hActionRec = new Record(3);
            var hProgressRec = new Record(3);

            // Installer is executing the installation script. Set up a
            // record specifying appropriate templates and text for
            // messages that will inform the user about what the custom
            // action is doing. Tell the installer to use this template and
            // text in progress messages.
            hActionRec[1] = "PowerShellFilesDeferred";
            hActionRec[2] = "PowerShell Files";
            hActionRec[3] = "[1] of [2], [3]";
            MessageResult iResult = session.Message(InstallMessage.ActionStart, hActionRec);
            if (iResult == MessageResult.Cancel)
            {
                return ActionResult.UserExit;
            }

            // Tell the installer to use explicit progress messages.
            hProgressRec[1] = 1;
            hProgressRec[2] = 1;
            hProgressRec[3] = 0;
            iResult = session.Message(InstallMessage.Progress, hProgressRec);
            if (iResult == MessageResult.Cancel)
            {
                return ActionResult.UserExit;
            }

            try
            {
                string content = session.CustomActionData["xml"];

                XDocument doc = XDocument.Parse(content);

                foreach (XElement row in doc.Root.Elements("d"))
                {
                    string file = row.Attribute("file").Value;

                    string arguments = row.Attribute("args").Value;

                    using (var task = new PowerShellTask(file, arguments, session))
                    {
                        task.Execute();
                    }
                }

                return ActionResult.Success;
            }
            catch (Exception ex)
            {
                session.Log(ex.ToString());
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
                var hProgressRec = new Record(2);

                hProgressRec[1] = 3;
                hProgressRec[2] = TotalTicks;
                MessageResult iResult = session.Message(InstallMessage.Progress, hProgressRec);
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

        // Specify or calculate the number of ticks in an increment to the ProgressBar
        [CustomAction]
        public static ActionResult PowerShellScriptsDeferred(Session session)
        {
            var hActionRec = new Record(3);
            var hProgressRec = new Record(3);

            // Installer is executing the installation script. Set up a
            // record specifying appropriate templates and text for
            // messages that will inform the user about what the custom
            // action is doing. Tell the installer to use this template and
            // text in progress messages.
            hActionRec[1] = "PowerShellScriptsDeferred";
            hActionRec[2] = "PowerShell Scripts";
            hActionRec[3] = "[1] of [2], [3]";
            MessageResult iResult = session.Message(InstallMessage.ActionStart, hActionRec);
            if (iResult == MessageResult.Cancel)
            {
                return ActionResult.UserExit;
            }

            // Tell the installer to use explicit progress messages.
            hProgressRec[1] = 1;
            hProgressRec[2] = 1;
            hProgressRec[3] = 0;
            iResult = session.Message(InstallMessage.Progress, hProgressRec);
            if (iResult == MessageResult.Cancel)
            {
                return ActionResult.UserExit;
            }

            try
            {
                CustomActionData data = session.CustomActionData;

                foreach (var datum in data)
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
                session.Log(ex.ToString());
                return ActionResult.Failure;
            }
        }
    }
}
