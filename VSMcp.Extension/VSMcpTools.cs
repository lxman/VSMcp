using System.ComponentModel;
using Microsoft.VisualStudio.Shell;
using ModelContextProtocol.Server;

namespace VSMcp.Extension
{
    /// <summary>
    /// Provides MCP tools for Visual Studio integration.
    /// </summary>
    [McpServerToolType]
    public class VSMcpTools
    {
        private readonly IVSServices _vsServices;
        
        public VSMcpTools(IVSServices vsServices)
        {
            this._vsServices = vsServices;
        }
        
        [McpServerTool]
        [Description("Gets the text of the currently active document in Visual Studio")]
        public string GetActiveDocumentText()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return _vsServices.GetActiveDocumentText();
        }
        
        [McpServerTool]
        [Description("Sets the text of the currently active document in Visual Studio")]
        public string SetActiveDocumentText([Description("The new text content")] string text)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            bool success = _vsServices.SetActiveDocumentText(text);
            return success ? "Document updated successfully" : "Failed to update document";
        }
        
        [McpServerTool]
        [Description("Gets a list of all open documents in Visual Studio")]
        public string[] GetOpenDocuments()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return _vsServices.GetOpenDocuments();
        }
        
        [McpServerTool]
        [Description("Gets information about the current solution")]
        public string GetSolutionInfo()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return _vsServices.GetSolutionInfo();
        }
        
        [McpServerTool]
        [Description("Opens a document by path")]
        public string OpenDocument([Description("The path to the document")] string path)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            bool success = _vsServices.OpenDocument(path);
            return success ? "Document opened successfully" : "Failed to open document";
        }
        
        [McpServerTool]
        [Description("Gets a list of all project items in the solution")]
        public string[] GetProjectItems()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return _vsServices.GetProjectItems();
        }
        
        [McpServerTool]
        [Description("Gets the content of a file by path")]
        public string GetFileContent([Description("The path to the file")] string path)
        {
            return _vsServices.GetFileContent(path);
        }
        
        [McpServerTool]
        [Description("Writes content to a file by path")]
        public string WriteFileContent(
            [Description("The path to the file")] string path,
            [Description("The content to write")] string content)
        {
            bool success = _vsServices.WriteFileContent(path, content);
            return success ? "File written successfully" : "Failed to write file";
        }
    }
}