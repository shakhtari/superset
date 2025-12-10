using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace SupersetABP.Web.Pages;

public class IndexModel : SupersetABPPageModel
{
    public void OnGet()
    {

    }

    public async Task OnPostLoginAsync()
    {
        await HttpContext.ChallengeAsync("oidc");
    }
}
