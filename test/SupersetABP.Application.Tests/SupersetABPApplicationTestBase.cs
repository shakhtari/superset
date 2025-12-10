using Volo.Abp.Modularity;

namespace SupersetABP;

public abstract class SupersetABPApplicationTestBase<TStartupModule> : SupersetABPTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
