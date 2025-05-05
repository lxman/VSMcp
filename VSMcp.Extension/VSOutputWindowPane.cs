using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace VSMcp.Extension
{
    /// <summary>
    /// Helper class for working with the Visual Studio output window.
    /// </summary>
    public class VSOutputWindowPane
    {
        private readonly IVsOutputWindowPane _pane;
        private readonly AsyncPackage _package;
        
        private VSOutputWindowPane(AsyncPackage package, IVsOutputWindowPane pane)
        {
            this._package = package;
            this._pane = pane;
        }
        
        /// <summary>
        /// Creates a new output window pane.
        /// </summary>
        public static async Task<VSOutputWindowPane> CreateAsync(AsyncPackage package, string paneName)
        {
            await package.JoinableTaskFactory.SwitchToMainThreadAsync();
            
            // Get the output window service
            var outputWindow = await package.GetServiceAsync(typeof(SVsOutputWindow)) as IVsOutputWindow;
            if (outputWindow == null)
            {
                throw new InvalidOperationException("Could not get output window service.");
            }
            
            // Create a new pane
            var paneGuid = new Guid($"{paneName}-{Guid.NewGuid()}");
            outputWindow.CreatePane(ref paneGuid, paneName, 1, 1);
            
            // Get the pane
            outputWindow.GetPane(ref paneGuid, out var pane);
            
            return new VSOutputWindowPane(package, pane);
        }
        
        /// <summary>
        /// Writes a line to the output window pane.
        /// </summary>
        public async Task WriteLineAsync(string message)
        {
            await _package.JoinableTaskFactory.SwitchToMainThreadAsync();
            
            if (_pane != null)
            {
                _pane.OutputStringThreadSafe($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - {message}\n");
                _pane.Activate();
            }
        }
    }
}