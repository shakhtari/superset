using SupersetABP.Samples;
using Xunit;

namespace SupersetABP.EntityFrameworkCore.Domains;

[Collection(SupersetABPTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<SupersetABPEntityFrameworkCoreTestModule>
{

}
