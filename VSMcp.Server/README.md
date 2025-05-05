# Visual Studio MCP Server

A Model Context Protocol (MCP) server implementation using version 0.1.0-preview.11 of the ModelContextProtocol NuGet package.

## Overview

This project implements a standalone MCP server that can be used to expose functionality to MCP-compatible clients, including AI assistants like GitHub Copilot. It uses the early preview version (0.1.0-preview.11) of the ModelContextProtocol NuGet package.

## Features

The server exposes the following tools via MCP:

- **echo**: Echoes back the provided message
- **get-current-time**: Returns the current date and time
- **count-words**: Counts the number of words in a text
- **calculate**: Performs a basic calculation (add, subtract, multiply, divide)

## Prerequisites

- .NET 8.0 SDK

## Running the Server

```bash
dotnet run
```

The server will start and listen for MCP clients using standard input/output (stdio) transport.

## Connecting with an MCP Client

To connect to this server from an MCP client, you'll need to:

1. Configure the client to use stdio transport
2. Launch this server process
3. Connect the client's stdin/stdout to the server's stdout/stdin

## MCP Configuration for VS Code

If you're using VS Code with MCP support, you can add this server to your `.vscode/mcp.json` file:

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
        "<path-to-project>/VSMcp.Server/VSMcp.Server.csproj"
      ]
    }
  }
}
```

## Example Usage

Once connected, clients can:

1. List the available tools
2. Execute the tools with parameters
3. Receive the results

For example, to use the `calculate` tool:

```
Input: { "tool": "calculate", "parameters": { "operation": "add", "a": 5, "b": 3 } }
Output: { "success": true, "result": { "result": 8 } }
```

## Extending

To add more tools, simply add more methods to the `DemoTools` class or create additional classes with the `[ToolProvider]` attribute. Each method should be annotated with `[Tool]` and have appropriate `[ToolParameter]` attributes for its parameters.

## Notes

- This implementation uses ModelContextProtocol version 0.1.0-preview.11, which is an early preview version
- The API may change significantly in future versions
