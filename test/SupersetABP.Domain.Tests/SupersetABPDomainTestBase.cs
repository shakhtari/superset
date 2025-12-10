using Volo.Abp.Modularity;

namespace SupersetABP;

/* Inherit from this class for your domain layer tests. */
public abstract class SupersetABPDomainTestBase<TStartupModule> : SupersetABPTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
