using Volo.Abp.Settings;

namespace SupersetABP.Settings;

public class SupersetABPSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(SupersetABPSettings.MySetting1));
    }
}
