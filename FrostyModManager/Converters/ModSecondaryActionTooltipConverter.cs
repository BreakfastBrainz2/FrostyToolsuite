using System.Globalization;
using System.Windows.Data;
using FrostyModManager.Windows;

namespace FrostyModManager.Converters;

public class ModSecondaryActionTooltipConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        ModResourceInfo mri = (ModResourceInfo)value;
        List<ModAction> mods = (List<ModAction>)mri.Mods;
        string modName = (string)parameter;

        int index = mods.FindIndex((ModAction a) => a.Name == modName);
        if (index != -1)
        {
            var modAction = mods[index];
            if (modAction.SecondaryAction == ModSecondaryActionType.AddToBundle)
                return "Resource is added to other bundle(s) by this mod";
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}