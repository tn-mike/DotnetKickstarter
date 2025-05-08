using DotnetKickstarter.Application.DTOs;
using DotnetKickstarter.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DotnetKickstarter.Infrastructure.Utility
{
    public class MapData : IMapData
    {
        public AppConfigWrapper MapApplicationSettings()
        {
            var json = File.ReadAllText("appsettings.json");
            var config = JsonSerializer.Deserialize<AppConfigWrapper>(json);
            if (config == null)
            {
                throw new Exception("Error Map Application Settings.");
            }

            config.AppplicationKickstar.jenkinsAppName = config.AppplicationKickstar.solutionName.ToLower();
            config.AppplicationKickstar.jenkinsAppPath = config.AppplicationKickstar.solutionName;

            return config;
        }
    }
}
