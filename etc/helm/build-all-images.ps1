./build-image.ps1 -ProjectPath "../../src/SupersetABP.DbMigrator/SupersetABP.DbMigrator.csproj" -ImageName supersetabp/dbmigrator
./build-image.ps1 -ProjectPath "../../src/SupersetABP.Web/SupersetABP.Web.csproj" -ImageName supersetabp/web
./build-image.ps1 -ProjectPath "../../src/SupersetABP.HttpApi.Host/SupersetABP.HttpApi.Host.csproj" -ImageName supersetabp/httpapihost
./build-image.ps1 -ProjectPath "../../src/SupersetABP.AuthServer/SupersetABP.AuthServer.csproj" -ImageName supersetabp/authserver
