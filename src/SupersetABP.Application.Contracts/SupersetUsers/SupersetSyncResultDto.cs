using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupersetABP.SupersetUsers
{
    public class SupersetSyncResultDto
    {
        public int TotalUsers { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public List<string> SyncedUsers { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }
}
