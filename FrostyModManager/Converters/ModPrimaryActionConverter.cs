using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using FrostyModManager.Windows;

namespace FrostyModManager.Converters;

public class ModPrimaryActionConverter : IValueConverter
{
    private static ImageSource blankSource = null;
    private static ImageSource primaryActionModifySource = new ImageSourceConverter().ConvertFromString("pack://application:,,,/FrostyModManager;component/Images/PrimaryActionModify.png") as ImageSource;
    private static ImageSource primaryActionReplaceSource = new ImageSourceConverter().ConvertFromString("pack://application:,,,/FrostyModManager;component/Images/PrimaryActionReplace.png") as ImageSource;
    private static ImageSource primaryActionAddSource = new ImageSourceConverter().ConvertFromString("pack://application:,,,/FrostyModManager;component/Images/PrimaryActionAdd.png") as ImageSource;
    private static ImageSource primaryActionMergeSource = new ImageSourceConverter().ConvertFromString("pack://application:,,,/FrostyModManager;component/Images/PrimaryActionMerge.png") as ImageSource;

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
                case ModPrimaryActionType.None: return blankSource;
                case ModPrimaryActionType.Modify: return (index == mri.FirstModToModifyIndex) ? primaryActionModifySource : primaryActionReplaceSource;
                case ModPrimaryActionType.Add: return (index == mri.FirstModToModifyIndex) ? primaryActionAddSource : primaryActionReplaceSource;
                case ModPrimaryActionType.Merge: return (index == mri.FirstModToModifyIndex) ? primaryActionModifySource : primaryActionMergeSource;
            }
        }

        return blankSource;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
