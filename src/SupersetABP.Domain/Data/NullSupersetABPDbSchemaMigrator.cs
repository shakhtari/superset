using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace SupersetABP.Data;

/* This is used if database provider does't define
 * ISupersetABPDbSchemaMigrator implementation.
 */
public class NullSupersetABPDbSchemaMigrator : ISupersetABPDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
