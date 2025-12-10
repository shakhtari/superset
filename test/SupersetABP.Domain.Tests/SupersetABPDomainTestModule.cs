using Volo.Abp.Modularity;

namespace SupersetABP;

[DependsOn(
    typeof(SupersetABPDomainModule),
    typeof(SupersetABPTestBaseModule)
)]
public class SupersetABPDomainTestModule : AbpModule
{

}
