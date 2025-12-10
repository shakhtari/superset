using Volo.Abp.Ui.Branding;
using Volo.Abp.DependencyInjection;
using Microsoft.Extensions.Localization;
using SupersetABP.Localization;

namespace SupersetABP.Web;

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
