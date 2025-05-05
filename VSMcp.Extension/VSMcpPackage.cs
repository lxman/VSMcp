using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace VSMcp.Extension
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid("b83fbfd7-9d14-4f27-9f3a-37223dc91308")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class VSMcpPackage : AsyncPackage
    {
        private IHost _mcpHost;
        private VSOutputWindowPane _outputPane;

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // Ensure we're on the UI thread
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            
            // Create output window pane for logging
            _outputPane = await VSOutputWindowPane.CreateAsync(this, "Visual Studio MCP Server");
            await _outputPane.WriteLineAsync("Initializing Visual Studio MCP Server...");
            
            // Initialize command
            await ShowServerStatusCommand.InitializeAsync(this);
            
            // Initialize MCP server
            await InitializeMcpServerAsync();
            
            await _outputPane.WriteLineAsync("Visual Studio MCP Server initialized successfully.");
        }
        
        private async Task InitializeMcpServerAsync()
        {
            await _outputPane.WriteLineAsync("Configuring MCP server...");
            
            // Create and configure the MCP host
            _mcpHost = Host.CreateDefaultBuilder()
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddProvider(new VSLoggerProvider(_outputPane));
                })
                .ConfigureServices(services =>
                {
                    // Register VS services
                    services.AddSingleton<IVSServices>(new VSServices(this, _outputPane));
                    
                    // Configure MCP server
                    services.AddMcpServer()
                        .WithStdioServerTransport()
                        .WithToolsFromAssembly();
                })
                .Build();
            
            // Start the MCP host
            await _mcpHost.StartAsync();
            
            await _outputPane.WriteLineAsync("MCP server started and ready to accept connections from Claude Desktop.");
        }
        
        /// <summary>
        /// Shows the MCP server status and connection info.
        /// </summary>
        public async Task ShowServerStatusAsync()
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync();
            
            VsShellUtilities.ShowMessageBox(
                this,
                "The Visual Studio MCP Server is running and ready to accept connections from Claude Desktop.\n\n" +
                "To connect Claude Desktop to Visual Studio, add the following to your Claude Desktop configuration:\n\n" +
                "{\n" +
                "  \"mcpServers\": {\n" +
                "    \"visualstudio\": {\n" +
                "      \"command\": \"C:\\\\Program Files\\\\Microsoft Visual Studio\\\\2022\\\\Community\\\\Common7\\\\IDE\\\\devenv.exe\",\n" +
                "      \"args\": [\n" +
                "        \"/rootsuffix\",\n" +
                "        \"Exp\",\n" +
                "        \"/command\",\n" +
                "        \"VSMcp.Extension.ShowServerStatus\"\n" +
                "      ]\n" +
                "    }\n" +
                "  }\n" +
                "}",
                "Visual Studio MCP Server",
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _mcpHost?.StopAsync().GetAwaiter().GetResult();
                _mcpHost?.Dispose();
            }
            
            base.Dispose(disposing);
        }
    }
}