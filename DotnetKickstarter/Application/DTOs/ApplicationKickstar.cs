using DotnetKickstarter.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetKickstarter.Application.DTOs
{
    public class AppConfigWrapper
    {
        public ApplicationKickstar AppplicationKickstar { get; set; } = new ApplicationKickstar();
    }

    public class ApplicationKickstar : ConfigSetting
    {
        public string jenkinsAppName { get; set; } = string.Empty;
        public string jenkinsAppPath { get; set; } = string.Empty;
    }
}
