using DotnetKickstarter.Application.DTOs;
using DotnetKickstarter.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotnetKickstarter.Infrastructure.Utility
{
    public class StructureFile : IStructureFile
    {
        public void CreateStructure(ApplicationKickstar input)
        {
			try
			{
                string[] folders;
                if (input.isCleanArchitecture)
                {
                    folders = CreateCleanArchitectureFolders(input.srcPath);
                }
                else
                {
                    folders = CreateLayerArchitectureFolders(input.srcPath);
                }

                string csprojPath = Path.Combine(input.srcPath, $"{input.solutionName}.csproj");
                AddFoldersToCsproj(csprojPath, folders);

                CopyAndModifyMainDTO(input.solutionName, input.srcPath, input.isCleanArchitecture);
                CopyAndModifyHealthCheckResponseWriter(input.solutionName, input.srcPath, input.isCleanArchitecture);
            }
			catch (Exception ex)
			{
                throw new Exception("Error create structure. " + ex.Message);
			}
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
                "Presentation/Extensions",
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

        private void CopyAndModifyMainDTO(string solutionName, string srcPath, bool isCleanArchitecture)
        {
            string executableLocation = Assembly.GetExecutingAssembly().Location;
            string executableDirectory = Path.GetDirectoryName(executableLocation);
            string sourceFilePath = "Infrastructure\\Templates\\Class\\MainDTO.txt";
            sourceFilePath = Path.Combine(executableDirectory, sourceFilePath);

            if (!File.Exists(sourceFilePath))
            {
                Console.WriteLine($"Source file not found: {sourceFilePath}");
                return;
            }

            string fileContent = File.ReadAllText(sourceFilePath);

            string newNamespace = isCleanArchitecture
                ? $"{solutionName}.Application.DTOs"
                : $"{solutionName}.Models";
            fileContent = fileContent.Replace("{{NAMESPACE}}", newNamespace);

            string destinationPath = isCleanArchitecture
                ? Path.Combine(srcPath, "Application/DTOs", "MainDTO.cs")
                : Path.Combine(srcPath, "Models", "MainModel.cs");

            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);

            File.WriteAllText(destinationPath, fileContent);
            Console.WriteLine($"File copied and modified: {destinationPath}");
        }

        private void CopyAndModifyHealthCheckResponseWriter(string solutionName, string srcPath, bool isCleanArchitecture)
        {
            string executableLocation = Assembly.GetExecutingAssembly().Location;
            string executableDirectory = Path.GetDirectoryName(executableLocation);
            string sourceFilePath = "Infrastructure\\Templates\\Class\\HealthCheckResponseWriter.txt";
            sourceFilePath = Path.Combine(executableDirectory, sourceFilePath);

            if (!File.Exists(sourceFilePath))
            {
                Console.WriteLine($"Source file not found: {sourceFilePath}");
                return;
            }

            string fileContent = File.ReadAllText(sourceFilePath);

            string newNamespace = isCleanArchitecture
                ? $"{solutionName}.Presentation.Extensions"
                : $"{solutionName}.APIHelper";
            fileContent = fileContent.Replace("{{NAMESPACE}}", newNamespace);

            string destinationPath = isCleanArchitecture
                ? Path.Combine(srcPath, "Presentation/Extensions", "HealthCheckResponseWriter.cs")
                : Path.Combine(srcPath, "APIHelper", "HealthCheckResponseWriter.cs");

            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);

            File.WriteAllText(destinationPath, fileContent);
            Console.WriteLine($"File copied and modified: {destinationPath}");
        }
    }
}
