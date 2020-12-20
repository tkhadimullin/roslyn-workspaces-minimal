using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoslynBoilerplate {
    public class RoslynProjectProvider
    {
        public string PathToSolution { get; private set; }
        public string ProjectName { get; private set; }

        public RoslynProjectProvider(string pathToSolution, string projectName)
        {
            var _ = typeof(Microsoft.CodeAnalysis.CSharp.Formatting.CSharpFormattingOptions);
            PathToSolution = pathToSolution;
            ProjectName = projectName;
        }

        private MSBuildWorkspace _Workspace;

        public MSBuildWorkspace Workspace
        {
            get
            {
                if (_Workspace == null)
                {
                    var path = "C:\\Program Files\\dotnet\\sdk\\3.1.404";                    
                    MSBuildLocator.RegisterMSBuildPath(path);
                    _Workspace = MSBuildWorkspace.Create();

                }

                return _Workspace;
            }
        }

        private Solution _Solution;

        public Solution Solution
        {
            get
            {
                if (_Solution == null)
                {
                    _Solution = Workspace.OpenSolutionAsync(PathToSolution).Result;

                    if (Workspace.Diagnostics.Count > 0)
                    { 
                        StringBuilder sb = new StringBuilder();

                        foreach (var diagnostic in Workspace.Diagnostics)
                        {
                            sb.Append(diagnostic.Message).Append(Environment.NewLine);
                        }

                        throw new ApplicationException(sb.ToString());
                    }
                }

                return _Solution;
            }
        }

        private Project _Project;

        /// <summary>
        /// Singleton Project in a solution
        /// </summary>
        public Project Project
        {
            get
            {
                if (_Project == null)
                {
                    _Project = Solution.Projects.FirstOrDefault(p => p.Name.Equals(ProjectName, StringComparison.InvariantCultureIgnoreCase));

                    if (_Project == null)
                    {
                        throw new ApplicationException($"Cannot find project {ProjectName}");
                    }
                }

                return _Project;
            }
        }

        private Compilation _Compilation;

        /// <summary>
        /// Singleton compilation of the project
        /// </summary>
        public Compilation ProjectCompilation
        {
            get
            {
                if (_Compilation == null)
                {
                    _Compilation = Project.GetCompilationAsync().Result;
                }

                return _Compilation;
            }
        }

        private List<ClassDeclarationSyntax> _Classes;

        public List<ClassDeclarationSyntax> Classes
        {
            get
            {
                if (_Classes == null)
                {
                    _Classes = new List<ClassDeclarationSyntax>();

                    foreach (var document in Project.Documents)
                    {
                        var tree = document.GetSyntaxTreeAsync().Result;

                        var semanticModel = ProjectCompilation.GetSemanticModel(tree);

                        foreach (var type in tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
                        {
                            _Classes.Add(type);
                        }
                    }
                }

                return _Classes;
            }
        }
    }
}
