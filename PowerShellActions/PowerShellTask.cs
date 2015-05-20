using System;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using Microsoft.Deployment.WindowsInstaller;

namespace PowerShellActions
{
    internal class PowerShellTask : IDisposable, IExitCode
    {
        private readonly Session _session;

        /// <summary>
        ///     The context that the Windows PowerShell script will run under.
        /// </summary>
        private Pipeline _pipeline;

        internal PowerShellTask(string script, Session session)
        {
            _session = session;
            Runspace runspace = RunspaceFactory.CreateRunspace(new WixHost(session, this));

            _pipeline = runspace.CreatePipeline();
            _pipeline.Commands.AddScript(script);
            _pipeline.Runspace.Open();
            _pipeline.Runspace.SessionStateProxy.SetVariable("session", session);
        }

        internal PowerShellTask(string file, string arguments, Session session)
        {
            _session = session;
            RunspaceConfiguration runspaceConfiguration = RunspaceConfiguration.Create();
            Runspace runspace = RunspaceFactory.CreateRunspace(new WixHost(session, this), runspaceConfiguration);

            runspace.Open();

            var scriptInvoker = new RunspaceInvoke(runspace);
            scriptInvoker.Invoke("Set-ExecutionPolicy RemoteSigned -Scope Process");

            _pipeline = runspace.CreatePipeline();

            // http://stackoverflow.com/a/530418/25702
            _pipeline.Commands.AddScript(string.Format("& '{0}' {1}", file, arguments));
            _pipeline.Runspace.SessionStateProxy.SetVariable("session", session);
        }

        public int ExitCode { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Execute Script, return false if any errors
        /// </summary>
        /// <returns></returns>
        public bool Execute()
        {
            var results = _pipeline.Invoke();

            var record = new Record(0);
            record[0] = string.Format("Exit code {0}", ExitCode);
            _session.Message(InstallMessage.Info, record);

            if (results.Any())
                _session.Log("Output");

            foreach (var r in results)
            {
                _session.Log(r.BaseObject.ToString());
            }

            var errors = Errors();

            if (errors != null)
            {
                _session.Log( "Non-terminating errors" );

                record[0] = errors;
                _session.Message(InstallMessage.Error, record);
            }

            return ((_pipeline.Error == null) || (_pipeline.Error.Count == 0)) && errors == null && ExitCode == 0;
        }

        public string Errors()
        {
            // check for errors (non-terminating)
            if (_pipeline.Error.Count > 0)
            {
                var builder = new StringBuilder();

                // iterate over Error PipeLine until end
                while (!_pipeline.Error.EndOfPipeline)
                {
                    // read one PSObject off the pipeline
                    var value = _pipeline.Error.Read() as PSObject;
                    if (value != null)
                    {
                        // get the ErrorRecord
                        var r = value.BaseObject as ErrorRecord;
                        if (r != null)
                        {
                            // build whatever kind of message you want
                            builder.AppendLine(r.InvocationInfo.MyCommand.Name + " : " + r.Exception.Message);
                            builder.AppendLine(r.InvocationInfo.PositionMessage);
                            builder.AppendLine(string.Format("+ CategoryInfo: {0}", r.CategoryInfo));
                            builder.AppendLine(
                            string.Format("+ FullyQualifiedErrorId: {0}", r.FullyQualifiedErrorId));
                        }
                    }
                }
                return builder.ToString();
            }

            return null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_pipeline.Runspace != null)
                {
                    _pipeline.Runspace.Dispose();
                    _pipeline = null;
                }
            }
        }
    }
}