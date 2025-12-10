using Microsoft.Extensions.Localization;
using SupersetABP.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace SupersetABP;

[Dependency(ReplaceServices = true)]
public class SupersetABPBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<SupersetABPResource> _localizer;

    public SupersetABPBrandingProvider(IStringLocalizer<SupersetABPResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
