using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

using Microsoft.Deployment.WindowsInstaller;

namespace PowerShellActions
{
    internal class PowerShellTask : IDisposable
    {
        /// <summary>
        ///     The context that the Windows PowerShell script will run under.
        /// </summary>
        private Pipeline _pipeline;

        internal PowerShellTask(string script, Session session)
        {
            Runspace runspace = RunspaceFactory.CreateRunspace(new WixHost(session));

            _pipeline = runspace.CreatePipeline();
            _pipeline.Commands.AddScript(script);
            _pipeline.Runspace.Open();
            _pipeline.Runspace.SessionStateProxy.SetVariable("session", session);
        }

        internal PowerShellTask(string file, string arguments, Session session)
        {
            RunspaceConfiguration runspaceConfiguration = RunspaceConfiguration.Create();
            Runspace runspace = RunspaceFactory.CreateRunspace(new WixHost(session), runspaceConfiguration);

            runspace.Open();

            var scriptInvoker = new RunspaceInvoke(runspace);
            scriptInvoker.Invoke("Set-ExecutionPolicy RemoteSigned -Scope Process");

            _pipeline = runspace.CreatePipeline();

            // http://stackoverflow.com/a/530418/25702
            _pipeline.Commands.AddScript(string.Format("& '{0}' {1}", file, arguments));
            _pipeline.Runspace.SessionStateProxy.SetVariable("session", session);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool Execute()
        {
            _pipeline.Invoke();

            return _pipeline.HadErrors;
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