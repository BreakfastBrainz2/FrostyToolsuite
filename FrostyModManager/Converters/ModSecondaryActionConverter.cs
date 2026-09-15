using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using FrostyModManager.Windows;

namespace FrostyModManager.Converters;

public class ModSecondaryActionConverter : IValueConverter
{
    private static readonly ImageSource BlankSource = null;
    private static readonly ImageSource SecondaryActionAddSource = new ImageSourceConverter().ConvertFromString("pack://application:,,,/FrostyModManager;component/Images/SecondaryActionAdd.png") as ImageSource;

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
                return SecondaryActionAddSource;
        }

        return BlankSource;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
