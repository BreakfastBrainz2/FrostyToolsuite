using System.Globalization;
using System.Windows.Data;
using FrostyModManager.Windows;

namespace FrostyModManager.Converters;

public class ModPrimaryActionTooltipConverter : IValueConverter
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
            switch (modAction.PrimaryAction)
            {
                case ModPrimaryActionType.None: return null;
                case ModPrimaryActionType.Modify: return (index == mri.FirstModToModifyIndex) ? "Resource is initially modified by this mod" : "Resource is replaced by this mod";
                case ModPrimaryActionType.Add: return (index == mri.FirstModToModifyIndex) ? "Resource is initially added by this mod" : "Resource is replaced by this mod";
                case ModPrimaryActionType.Merge: return (index == mri.FirstModToModifyIndex) ? "Resource is initially modified by this mod" : "Resource is merged by this mod";
            }
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}