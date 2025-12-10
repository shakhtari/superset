using SupersetABP.Localization;
using Volo.Abp.Application.Services;

namespace SupersetABP;

/* Inherit your application services from this class.
 */
public abstract class SupersetABPAppService : ApplicationService
{
    protected SupersetABPAppService()
    {
        LocalizationResource = typeof(SupersetABPResource);
    }
}
