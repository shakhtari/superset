using System.Threading.Tasks;

namespace SupersetABP.Data;

public interface ISupersetABPDbSchemaMigrator
{
    Task MigrateAsync();
}
