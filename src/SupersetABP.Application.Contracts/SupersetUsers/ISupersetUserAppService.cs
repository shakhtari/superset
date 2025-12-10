using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SupersetABP.SupersetUsers
{
    public interface ISupersetUserAppService : IApplicationService
    {
        Task<SupersetSyncResultDto> SyncAllUsersAsync();
    }
}
