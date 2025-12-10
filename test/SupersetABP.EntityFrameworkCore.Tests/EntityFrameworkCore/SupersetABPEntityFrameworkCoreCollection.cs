using Xunit;

namespace SupersetABP.EntityFrameworkCore;

[CollectionDefinition(SupersetABPTestConsts.CollectionDefinitionName)]
public class SupersetABPEntityFrameworkCoreCollection : ICollectionFixture<SupersetABPEntityFrameworkCoreFixture>
{

}
