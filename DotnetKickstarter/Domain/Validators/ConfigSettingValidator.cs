using DotnetKickstarter.Domain.Entities;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetKickstarter.Domain.Validators
{
    public class ConfigSettingValidator : AbstractValidator<ConfigSetting>
    {
        public ConfigSettingValidator()
        {
            RuleFor(x => x.basePath)
                .NotEmpty().WithMessage("Base path is required.")
                .Must(Directory.Exists).WithMessage("Base path must be a valid existing directory.");

            RuleFor(x => x.solutionName)
                .NotEmpty().WithMessage("Solution name is required.")
                .Matches(@"^[a-zA-Z0-9_.]+$").WithMessage("Solution name must not contain special characters.");

            RuleFor(x => x.dotnetVersion)
                .NotEmpty().WithMessage("Dotnet version is required.")
                .Must(v => new[] { "net7.0", "net8.0", "net9.0" }.Contains(v))
                .WithMessage("Dotnet version must be one of: net7.0, net8.0, net9.0");

            RuleFor(x => x.jenkinsProduct)
                .NotEmpty().WithMessage("Jenkins product is required.")
                .MaximumLength(50);

            RuleFor(x => x.jenkinsNamespace)
                .NotEmpty().WithMessage("Jenkins namespace is required.")
                .Matches("^[a-z0-9-]+$").WithMessage("Namespace must be lowercase, numbers, or hyphens only.");
        }
    }
}
