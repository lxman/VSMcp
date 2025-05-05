using System;
using System.Collections.Generic;
using System.IO;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace VSMcp.Extension
{
    /// <summary>
    /// Interface for Visual Studio services.
    /// </summary>
    public interface IVSServices
    {
        string GetActiveDocumentText();
        bool SetActiveDocumentText(string text);
        string[] GetOpenDocuments();
        string GetSolutionInfo();
        bool OpenDocument(string path);
        string[] GetProjectItems();
        string GetFileContent(string path);
        bool WriteFileContent(string path, string content);
    }
    
    /// <summary>
    /// Implementation of Visual Studio services.
    /// </summary>
    public class VSServices : IVSServices
    {
        private readonly AsyncPackage _package;
        private readonly VSOutputWindowPane _outputPane;
        
        public VSServices(AsyncPackage package, VSOutputWindowPane outputPane)
        {
            this._package = package;
            this._outputPane = outputPane;
        }
        
        /// <summary>
        /// Gets the text of the active document.
        /// </summary>
        public string GetActiveDocumentText()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            
            try
            {
                var dte = GetDte();
                
                if (dte.ActiveDocument == null)
                {
                    return "No active document.";
                }
                
                var textDocument = dte.ActiveDocument.Object("TextDocument") as TextDocument;
                if (textDocument == null)
                {
                    return "Active document is not a text document.";
                }
                
                EditPoint startPoint = textDocument.StartPoint.CreateEditPoint();
                string text = startPoint.GetText(textDocument.EndPoint);
                
                return text;
            }
            catch (Exception ex)
            {
                LogError($"Error getting active document text: {ex.Message}");
                return $"Error getting active document text: {ex.Message}";
            }
        }
        
        /// <summary>
        /// Sets the text of the active document.
        /// </summary>
        public bool SetActiveDocumentText(string text)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            
            try
            {
                var dte = GetDte();
                
                if (dte.ActiveDocument == null)
                {
                    LogError("No active document.");
                    return false;
                }
                
                var textDocument = dte.ActiveDocument.Object("TextDocument") as TextDocument;
                if (textDocument == null)
                {
                    LogError("Active document is not a text document.");
                    return false;
                }
                
                EditPoint startPoint = textDocument.StartPoint.CreateEditPoint();
                EditPoint endPoint = textDocument.EndPoint.CreateEditPoint();
                startPoint.Delete(endPoint);
                startPoint.Insert(text);
                
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Error setting active document text: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Gets a list of all open documents.
        /// </summary>
        public string[] GetOpenDocuments()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            
            try
            {
                var dte = GetDte();
                
                var openDocs = new List<string>();
                foreach (Document doc in dte.Documents)
                {
                    openDocs.Add($"{doc.Name} ({doc.FullName})");
                }
                
                return openDocs.ToArray();
            }
            catch (Exception ex)
            {
                LogError($"Error getting open documents: {ex.Message}");
                return new[] { $"Error getting open documents: {ex.Message}" };
            }
        }
        
        /// <summary>
        /// Gets information about the current solution.
        /// </summary>
        public string GetSolutionInfo()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            
            try
            {
                var dte = GetDte();
                
                if (dte.Solution == null || string.IsNullOrEmpty(dte.Solution.FullName))
                {
                    return "No solution is open.";
                }
                
                string solutionName = Path.GetFileNameWithoutExtension(dte.Solution.FullName);
                string solutionPath = dte.Solution.FullName;
                
                var projectInfo = new List<string>();
                foreach (Project project in dte.Solution.Projects)
                {
                    projectInfo.Add($"Project: {project.Name}, Items: {GetProjectItemCount(project)}");
                }
                
                string result = $"Solution: {solutionName}\n";
                result += $"Path: {solutionPath}\n";
                result += $"Projects: {projectInfo.Count}\n";
                result += string.Join("\n", projectInfo);
                
                return result;
            }
            catch (Exception ex)
            {
                LogError($"Error getting solution info: {ex.Message}");
                return $"Error getting solution info: {ex.Message}";
            }
        }
        
        /// <summary>
        /// Gets the count of items in a project.
        /// </summary>
        private int GetProjectItemCount(Project project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            
            try
            {
                if (project.ProjectItems == null)
                {
                    return 0;
                }
                
                return project.ProjectItems.Count;
            }
            catch
            {
                return 0;
            }
        }
        
        /// <summary>
        /// Opens a document by path.
        /// </summary>
        public bool OpenDocument(string path)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            
            try
            {
                var dte = GetDte();
                
                if (!File.Exists(path))
                {
                    LogError($"File not found: {path}");
                    return false;
                }
                
                dte.ItemOperations.OpenFile(path);
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Error opening document: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Gets a list of all project items in the solution.
        /// </summary>
        public string[] GetProjectItems()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            
            try
            {
                var dte = GetDte();
                
                if (dte.Solution == null || string.IsNullOrEmpty(dte.Solution.FullName))
                {
                    return new[] { "No solution is open." };
                }
                
                var projectItems = new List<string>();
                foreach (Project project in dte.Solution.Projects)
                {
                    CollectProjectItems(project.ProjectItems, project.Name, projectItems);
                }
                
                return projectItems.ToArray();
            }
            catch (Exception ex)
            {
                LogError($"Error getting project items: {ex.Message}");
                return new[] { $"Error getting project items: {ex.Message}" };
            }
        }
        
        /// <summary>
        /// Recursively collects all project items.
        /// </summary>
        private void CollectProjectItems(ProjectItems items, string prefix, List<string> result)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            
            if (items == null)
            {
                return;
            }
            
            foreach (ProjectItem item in items)
            {
                try
                {
                    string name = item.Name;
                    string fullPath = item.FileNames[0];  // Use the first file name (items can have multiple)
                    
                    result.Add($"{prefix}\\{name} ({fullPath})");
                    
                    // Recursively process child items
                    CollectProjectItems(item.ProjectItems, $"{prefix}\\{name}", result);
                }
                catch
                {
                    // Skip items that cause errors
                }
            }
        }
        
        /// <summary>
        /// Gets the content of a file by path.
        /// </summary>
        public string GetFileContent(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    return $"File not found: {path}";
                }
                
                return File.ReadAllText(path);
            }
            catch (Exception ex)
            {
                LogError($"Error reading file: {ex.Message}");
                return $"Error reading file: {ex.Message}";
            }
        }
        
        /// <summary>
        /// Writes content to a file by path.
        /// </summary>
        public bool WriteFileContent(string path, string content)
        {
            try
            {
                File.WriteAllText(path, content);
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Error writing file: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Gets the DTE service.
        /// </summary>
        private DTE2 GetDte()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            
            var dte = _package.GetServiceAsync(typeof(SDTE)).Result as DTE2;
            if (dte == null)
            {
                throw new InvalidOperationException("Could not get DTE service.");
            }
            
            return dte;
        }
        
        /// <summary>
        /// Logs an error message.
        /// </summary>
        private void LogError(string message)
        {
            _outputPane?.WriteLineAsync(message).Wait();
        }
    }
}