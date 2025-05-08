using DotnetKickstarter.Application.DTOs;
using DotnetKickstarter.Application.Interfaces;
using DotnetKickstarter.Domain.Validators;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DotnetKickstarter.Application.Services
{
    internal class ProcessService : IProcessService
    {
        private readonly IMapData mapData;
        private readonly IStructureFile structureFile;

        public ProcessService(IMapData mapData, IStructureFile structureFile) 
        {
            this.mapData = mapData;
            this.structureFile = structureFile;
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
                    return "Cancel generate project by user";
                }

                //confirm = AppInformation(input, "Are you sure????? (y/n): ");
                //if (!confirm)
                //{
                //    return "Cancel generate project by user";
                //}

                ValidateConfigSetting(config);

                input.solutionPath = Path.Combine(input.basePath, input.solutionName);
                input.srcPath = Path.Combine(input.solutionPath, "src", input.solutionName);
                input.testPath = Path.Combine(input.solutionPath, "tests", "UnitTest", $"{input.solutionName}.UnitTest");

                Console.WriteLine("Continuing with project generation...");

                CreateSolution(input);
                structureFile.CreateStructure(input);

                Console.WriteLine("Do you open your project folder? (y/n): ");
                var isOpen = Console.ReadLine()?.Trim().ToLower();
                if (isOpen == "y")
                {
                    OpenSolutionFolder(input.solutionPath);
                }

                return "Success generate project";
            }
            catch (Exception ex)
            {
                return "Failed generate project because " + ex.Message;
            }
        }

        //string solutionPath = Path.Combine(input.basePath, input.solutionName);
        //string srcPath = Path.Combine(solutionPath, "src", input.solutionName);
        //string testPath = Path.Combine(solutionPath, "tests", "UnitTest", $"{input.solutionName}.UnitTest");
        private void CreateSolution(ApplicationKickstar input)
        {
            Directory.CreateDirectory(input.solutionPath);
            Directory.SetCurrentDirectory(input.solutionPath);

            RunCLI("dotnet", $"new sln -n {input.solutionName}");
            Directory.CreateDirectory(Path.Combine(input.solutionPath, "src"));

            if (input.includeTestProject)
                Directory.CreateDirectory(Path.Combine(input.solutionPath, "tests", "UnitTest"));

            string httpsFlag = input.disableHttps ? "--no-https" : "";
            RunCLI("dotnet", $"new webapi -n {input.solutionName} --framework {input.dotnetVersion} {httpsFlag} -o \"{input.srcPath}\"");
            RunCLI("dotnet", $"sln add \"{Path.Combine(input.srcPath, $"{input.solutionName}.csproj")}\"");

            RunCLI("dotnet", $"add \"{Path.Combine(input.srcPath, $"{input.solutionName}.csproj")}\" package FluentValidation");
            
            RunCLI("dotnet", $"add \"{Path.Combine(input.srcPath, $"{input.solutionName}.csproj")}\" package Serilog");
            RunCLI("dotnet", $"add \"{Path.Combine(input.srcPath, $"{input.solutionName}.csproj")}\" package Serilog.Sinks.Console");

            if (input.includeTestProject)
            {
                RunCLI("dotnet", $"new xunit -n {input.solutionName}.UnitTest --framework {input.dotnetVersion} -o \"{input.testPath}\"");
                RunCLI("dotnet", $"sln add \"{Path.Combine(input.testPath, $"{input.solutionName}.UnitTest.csproj")}\"");
                RunCLI("dotnet", $"add \"{Path.Combine(input.testPath, $"{input.solutionName}.UnitTest.csproj")}\" reference \"{Path.Combine(input.srcPath, $"{input.solutionName}.csproj")}\"");
            }

            Console.WriteLine("Project generation complete.");
        }

        private void RunCLI(string fileName, string arguments)
        {
            try
            {
                Console.WriteLine($"> {fileName} {arguments}");
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
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
            catch (Exception ex)
            {
            }
        }

        private bool AppInformation(ApplicationKickstar input, string confirmMessage)
        {
            Console.WriteLine("\n========== Review Configuration ==========");
            Console.WriteLine($"Base Path:             {input.basePath}");
            Console.WriteLine($"Solution Name:         {input.solutionName}");
            Console.WriteLine($"Dotnet Version:        {input.dotnetVersion}");
            Console.WriteLine($"Include Unit Test:     {input.includeTestProject}");
            Console.WriteLine($"Disable HTTPS:         {input.disableHttps}");
            Console.WriteLine($"Generate CI/CD Files:  {input.generateCiCdFiles}");
            Console.WriteLine($"Clean Architecture:    {input.isCleanArchitecture}");
            Console.WriteLine($"Jenkins App Name:      {input.jenkinsAppName}");
            Console.WriteLine($"Jenkins App Path:      {input.jenkinsAppPath}");
            Console.WriteLine($"Jenkins Product:       {input.jenkinsProduct}");
            Console.WriteLine($"Jenkins Namespace:     {input.jenkinsNamespace}");
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

        private void ValidateConfigSetting(AppConfigWrapper config)
        {
            var validator = new ConfigSettingValidator();
            ValidationResult result = validator.Validate(config.AppplicationKickstar);

            if (!result.IsValid)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.ErrorMessage));
                throw new Exception($"Validation failed: {errors}");
            }
        }

        private void OpenSolutionFolder(string solutionPath)
        {
            try
            {
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    // สำหรับ Windows
                    Process.Start("explorer", solutionPath);
                }
                else if (Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    // สำหรับ macOS หรือ Linux
                    Process.Start("open", solutionPath); // macOS
                }
                else if (Environment.OSVersion.Platform == PlatformID.MacOSX)
                {
                    Process.Start("open", solutionPath); // macOS
                }
                else
                {
                    Console.WriteLine("Opening solution folder is not supported on this platform.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to open solution folder: {ex.Message}");
            }
        }
    }
}
