using Volo.Abp.Modularity;

namespace SupersetABP;

[DependsOn(
    typeof(SupersetABPApplicationModule),
    typeof(SupersetABPDomainTestModule)
)]
public class SupersetABPApplicationTestModule : AbpModule
{

}
