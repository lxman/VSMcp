using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;
using ModelContextProtocol.Protocol.Types;

namespace VSMcp.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("MCP Client - Direct Process Connection Test");
            Console.WriteLine("=========================================");

            try
            {
                // No need to start server process, StdioClientTransport will do that
                Console.WriteLine("Setting up client transport...");

                // Create client options
                var clientOptions = new McpClientOptions
                {
                    ClientInfo = new Implementation
                    {
                        Name = "Direct Process Test Client",
                        Version = "1.0.0"
                    }
                };

                try
                {
                    // Switch to StdioClientTransport with direct command
                    Console.WriteLine("Creating StdioClientTransport...");
                    
                    var transportOptions = new StdioClientTransportOptions
                    {
                        Name = "VS MCP Server",
                        Command = "dotnet",
                        Arguments = new[] { "run", "--project", "C:/Users/jorda/RiderProjects/AiTest/VSMcp.Server/VSMcp.Server.csproj", "--no-build" }
                    };
                    
                    var transport = new StdioClientTransport(transportOptions);

                    Console.WriteLine("Attempting to connect to server...");
                    
                    // Use a cancellation token to limit connection attempt time
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                    
                    IMcpClient client = await McpClientFactory.CreateAsync(transport, clientOptions, 
                        cancellationToken: cts.Token);
                    
                    Console.WriteLine("Connection successful!");
                    
                    Console.WriteLine("Retrieving tools...");
                    IList<McpClientTool> tools = await client.ListToolsAsync(null, cts.Token);
                    Console.WriteLine($"Found {tools.Count} tools:");
                    
                    foreach (McpClientTool tool in tools)
                    {
                        Console.WriteLine($"- {tool.Name}: {tool.Description ?? "No description"}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Connection error: {ex.Message}");
                    Console.WriteLine($"Type: {ex.GetType().FullName}");
                    
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                        Console.WriteLine($"Inner exception type: {ex.InnerException.GetType().FullName}");
                    }
                }
                finally
                {
                    // No process to clean up - StdioClientTransport handles that
                    Console.WriteLine("Client connection attempt completed");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
