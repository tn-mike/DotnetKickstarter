using DotnetKickstarter.Application.DTOs;
using DotnetKickstarter.Application.Interfaces;
using DotnetKickstarter.Domain.Validators;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetKickstarter.Application.Services
{
    internal class ProcessService : IProcessService
    {
        private readonly IMapData mapData;

        public ProcessService(IMapData mapData) 
        {
            this.mapData = mapData;
        }

        public string Run()
        {
            try
            {
                var config = mapData.MapApplicationSettings();
                var input = config.AppplicationKickstar;

                bool confirm = AppInformation(input, "Proceed with project generation? (y/n): ");
                if (!confirm)
                {
                    return "";
                }

                confirm = AppInformation(input, "Are you sure????? (y/n): ");
                if (!confirm)
                {
                    return "";
                }

                var validator = new ConfigSettingValidator();
                ValidationResult result = validator.Validate(config.AppplicationKickstar);

                if (!result.IsValid)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.ErrorMessage));
                    throw new Exception($"Validation failed: {errors}");
                }

                CreateSolution(input);

                Console.WriteLine("Continuing with project generation...");
                return "";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed generate project because " + ex.Message);
                return "";
            }
        }

        private bool AppInformation(ApplicationKickstar input, string confirmMessage)
        {
            Console.WriteLine("\n========== Review Configuration ==========");
            Console.WriteLine($"Base Path:            {input.basePath}");
            Console.WriteLine($"Solution Name:        {input.solutionName}");
            Console.WriteLine($"Dotnet Version:       {input.dotnetVersion}");
            Console.WriteLine($"Include Unit Test:    {input.includeTestProject}");
            Console.WriteLine($"Disable HTTPS:        {input.disableHttps}");
            Console.WriteLine($"Generate CI/CD Files: {input.generateCiCdFiles}");
            Console.WriteLine($"Jenkins App Name:     {input.jenkinsAppName}");
            Console.WriteLine($"Jenkins App Path:     {input.jenkinsAppPath}");
            Console.WriteLine($"Jenkins Product:      {input.jenkinsProduct}");
            Console.WriteLine($"Jenkins Namespace:    {input.jenkinsNamespace}");
            Console.WriteLine("==========================================\n");

            Console.Write(confirmMessage);
            var confirm = Console.ReadLine()?.Trim().ToLower();
            if (confirm != "y")
            {
                Console.WriteLine("Cancelled by user.");
                return false;
            }
            else
            {
                return true;
            }
        }

        private void RunCLI(string fileName, string arguments)
        {
            Console.WriteLine($"> {fileName} {arguments}");
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardOutput = false,
                    UseShellExecute = true,
                    CreateNoWindow = false
                }
            };
            process.Start();
            process.WaitForExit();
        }

        private void CreateSolution(ApplicationKickstar input) 
        {
            string solutionPath = Path.Combine(input.basePath, input.solutionName);
            string srcPath = Path.Combine(solutionPath, "src", input.solutionName);
            string testPath = Path.Combine(solutionPath, "tests", "UnitTest", $"{input.solutionName}.UnitTest");

            Directory.CreateDirectory(solutionPath);
            Directory.SetCurrentDirectory(solutionPath);

            RunCLI("dotnet", $"new sln -n {input.solutionName}");
            Directory.CreateDirectory(Path.Combine(solutionPath, "src"));

            if (input.includeTestProject)
                Directory.CreateDirectory(Path.Combine(solutionPath, "tests", "UnitTest"));

            string httpsFlag = input.disableHttps ? "--no-https" : "";
            RunCLI("dotnet", $"new webapi -n {input.solutionName} --framework {input.dotnetVersion} {httpsFlag} -o \"{srcPath}\"");
            RunCLI("dotnet", $"sln add \"{Path.Combine(srcPath, $"{input.solutionName}.csproj")}\"");

            if (input.includeTestProject)
            {
                RunCLI("dotnet", $"new xunit -n {input.solutionName}.UnitTest --framework {input.dotnetVersion} -o \"{testPath}\"");
                RunCLI("dotnet", $"sln add \"{Path.Combine(testPath, $"{input.solutionName}.UnitTest.csproj")}\"");
                RunCLI("dotnet", $"add \"{Path.Combine(testPath, $"{input.solutionName}.UnitTest.csproj")}\" reference \"{Path.Combine(srcPath, $"{input.solutionName}.csproj")}\"");
            }

            string[] folders;
            if (input.isCleanArchitecture)
            {
                folders = CreateCleanArchitectureFolders(srcPath);
            }
            else
            {
                folders = CreateLayerArchitectureFolders(srcPath);
            }

            string csprojPath = Path.Combine(srcPath, $"{input.solutionName}.csproj");
            AddFoldersToCsproj(csprojPath, folders);


            Console.WriteLine("Project generation complete.");
        }

        private string[] CreateCleanArchitectureFolders(string srcPath)
        {
            string[] folders = new[]
            {
                "Domain/Entities",
                "Domain/Validators",
                "Application/DTOs",
                "Application/Interfaces",
                "Application/Services",
                "Application/DependencyInjection",
                "Infrastructure/Persistence",
                "Infrastructure/Services",
                "Infrastructure/DependencyInjection",
                "Presentation/Controllers",
                "Presentation/DependencyInjection"
            };

            foreach (var folder in folders)
            {
                Directory.CreateDirectory(Path.Combine(srcPath, folder));
            }

            return folders;
        }

        private string[] CreateLayerArchitectureFolders(string srcPath)
        {
            string[] folders = new[]
            {
                "Models",
                "Services/Interfaces",
                "DataAccess/Interfaces",
                "APIHelper"
            };

            foreach (var folder in folders)
            {
                Directory.CreateDirectory(Path.Combine(srcPath, folder));
            }

            return folders;
        }

        private void AddFoldersToCsproj(string csprojPath, string[] folders)
        {
            if (!File.Exists(csprojPath))
            {
                Console.WriteLine($"CSProj file not found: {csprojPath}");
                return;
            }

            var csprojContent = File.ReadAllText(csprojPath);
            var itemGroupTag = "<ItemGroup>";
            var closingItemGroupTag = "</ItemGroup>";

            var sb = new StringBuilder();
            sb.AppendLine(itemGroupTag);

            foreach (var folder in folders)
            {
                sb.AppendLine($"    <Folder Include=\"{folder.Replace("\\", "/")}\" />");
            }

            sb.AppendLine(closingItemGroupTag);
            sb.AppendLine("");
            sb.AppendLine(itemGroupTag);

            // แทรกโฟลเดอร์ใน ItemGroup
            var updatedContent = csprojContent.Replace(itemGroupTag, sb.ToString().Trim());
            File.WriteAllText(csprojPath, updatedContent);
        }
    }
}
