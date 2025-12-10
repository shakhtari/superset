using SupersetABP.Samples;
using Xunit;

namespace SupersetABP.EntityFrameworkCore.Applications;

[Collection(SupersetABPTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<SupersetABPEntityFrameworkCoreTestModule>
{

}
