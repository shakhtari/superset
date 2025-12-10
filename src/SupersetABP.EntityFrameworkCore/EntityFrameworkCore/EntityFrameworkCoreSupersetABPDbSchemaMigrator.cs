using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SupersetABP.Data;
using Volo.Abp.DependencyInjection;

namespace SupersetABP.EntityFrameworkCore;

public class EntityFrameworkCoreSupersetABPDbSchemaMigrator
    : ISupersetABPDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreSupersetABPDbSchemaMigrator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the SupersetABPDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<SupersetABPDbContext>()
            .Database
            .MigrateAsync();
    }
}
