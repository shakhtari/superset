using SupersetABP.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace SupersetABP.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class SupersetABPController : AbpControllerBase
{
    protected SupersetABPController()
    {
        LocalizationResource = typeof(SupersetABPResource);
    }
}
