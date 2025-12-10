using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SupersetABP.SupersetUsers;
using System.Threading.Tasks;

namespace SupersetABP.Web.Pages.SupersetDashboard
{
    public class IndexModel : PageModel
    {
        private readonly ISupersetUserAppService _supersetService;

        public IndexModel(ISupersetUserAppService supersetService)
        {
            _supersetService = supersetService;
        }

        public async Task OnGetAsync()
        {
            // ⭐ Sayfa yüklendiğinde otomatik sync yap
            await _supersetService.SyncAllUsersAsync();
        }
    }
}
