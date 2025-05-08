using DotnetKickstarter.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetKickstarter.Application.Interfaces
{
    internal interface IMapData
    {
        AppConfigWrapper MapApplicationSettings();
    }
}
