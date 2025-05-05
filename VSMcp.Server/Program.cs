using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace VSMcp.Server
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Starting VS MCP Server with ModelContextProtocol v0.1.0-preview.11");
            
            // Create and configure the host
            IHost builder = Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole(options => 
                    {
                        options.LogToStandardErrorThreshold = LogLevel.Trace;
                    });
                    logging.SetMinimumLevel(LogLevel.Trace);
                })
                .ConfigureServices(services =>
                {
                    // Configure MCP server
                    services.AddMcpServer()
                        .WithStdioServerTransport()
                        .WithToolsFromAssembly();
                })
                .Build();

            Console.WriteLine("VS MCP Server configured and ready to start");
            
            // Start the host and wait for it to stop
            await builder.RunAsync();
        }
    }
    
    // Tool provider class with static methods that will be discovered and registered automatically
    [McpServerToolType]
    public static class DemoTools
    {
        // Echo tool - returns the input message
        [McpServerTool]
        [Description("Echoes back the provided message")]
        public static string Echo([Description("The message to echo")] string message)
        {
            Console.WriteLine($"Echo tool called with message: {message}");
            return $"Echo: {message}";
        }
        
        // Get current time tool
        [McpServerTool]
        [Description("Returns the current date and time")]
        public static string GetCurrentTime()
        {
            var currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Console.WriteLine($"Returning current time: {currentTime}");
            return $"Current time: {currentTime}";
        }
        
        // Count words in text
        [McpServerTool]
        [Description("Counts the number of words in a text")]
        public static string CountWords([Description("The text to analyze")] string text)
        {
            int wordCount = text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
            Console.WriteLine($"Counted {wordCount} words in text");
            
            return $"Word count: {wordCount}, Text: {text}";
        }
        
        // Simple calculator
        [McpServerTool]
        [Description("Performs a basic calculation")]
        public static string Calculate(
            [Description("The operation to perform (add, subtract, multiply, divide)")] string operation,
            [Description("The first number")] double a,
            [Description("The second number")] double b)
        {
            Console.WriteLine($"Performing calculation: {a} {operation} {b}");
            
            double result;
            switch (operation.ToLower())
            {
                case "add":
                    result = a + b;
                    break;
                case "subtract":
                    result = a - b;
                    break;
                case "multiply":
                    result = a * b;
                    break;
                case "divide":
                    if (b == 0)
                    {
                        return "Error: Cannot divide by zero";
                    }
                    result = a / b;
                    break;
                default:
                    return $"Error: Unknown operation: {operation}";
            }
            
            return $"Result of {a} {operation} {b} = {result}";
        }
    }
}
