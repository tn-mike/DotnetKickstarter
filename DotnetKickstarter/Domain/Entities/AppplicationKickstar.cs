using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetKickstarter.Domain.Entities
{
    public class ConfigSetting
    {
        public string basePath { get; set; } = string.Empty;
        public string solutionName { get; set; } = string.Empty;
        public bool includeTestProject { get; set; } = true;
        public string dotnetVersion { get; set; } = string.Empty;
        public bool disableHttps { get; set; } = true;
        public bool generateCiCdFiles { get; set; } = true;
        public bool isCleanArchitecture { get; set; } = true;

        public string jenkinsProduct { get; set; } = string.Empty;
        public string jenkinsNamespace { get; set; } = string.Empty;


        //public bool IsValidDotnetVersion()
        //{
        //    return !string.IsNullOrEmpty(dotnetVersion) && dotnetVersion.StartsWith("net");
        //}

        //public void Validate()
        //{
        //    if (!IsValidDotnetVersion())
        //    {
        //        throw new Exception($"Invalid .NET version: {dotnetVersion}");
        //    }
        //}
    }
}
