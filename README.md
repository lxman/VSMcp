# Visual Studio MCP Example

This solution demonstrates how to create and use a Model Context Protocol (MCP) server using version 0.1.0-preview.11 of the ModelContextProtocol NuGet package.

## Projects

The solution contains two projects:

1. **VSMcp.Server**: An MCP server implementation
2. **VSMcp.Client**: A client application that connects to the server

## VSMcp.Server

A standalone MCP server that exposes tools via the Model Context Protocol. It includes:

- Echo tool
- Current time tool
- Word count tool
- Calculator tool

## VSMcp.Client

A console application that demonstrates how to connect to the MCP server and use its tools. It:

- Starts the server as a separate process
- Connects to it using stdio transport
- Lists available tools
- Provides an interactive interface to execute tools

## Building and Running

### Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 or JetBrains Rider

### Running the Demo

1. Build the solution
2. Set VSMcp.Client as the startup project
3. Run the application

The client will start the server automatically and connect to it.

## Using with VS Code

To use this MCP server with VS Code and GitHub Copilot:

1. Build the VSMcp.Server project
2. Create a `.vscode/mcp.json` file in your VS Code workspace:

```json
{
  "inputs": [],
  "servers": {
    "vsmcp-demo": {
      "type": "stdio",
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "<path-to-solution>/VSMcp.Server/VSMcp.Server.csproj"
      ]
    }
  }
}
```

3. Open VS Code with GitHub Copilot and agent mode enabled
4. The MCP server tools should now be available to GitHub Copilot

## Notes

- This implementation uses ModelContextProtocol version 0.1.0-preview.11, which is an early preview version
- The API may change significantly in future versions
