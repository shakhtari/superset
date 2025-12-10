using SupersetABP.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace SupersetABP.Web.Pages;

/* Inherit your Page Model classes from this class.
 */
public abstract class SupersetABPPageModel : AbpPageModel
{
    protected SupersetABPPageModel()
    {
        LocalizationResourceType = typeof(SupersetABPResource);
    }
}
