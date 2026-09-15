using System.Globalization;
using System.Windows.Data;
using Frosty.ModSupport.Interfaces;

namespace FrostyModManager.Converters;

public class ModDescriptionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        IFrostyMod mod = (IFrostyMod)value;
        if (!mod.HasWarnings)
            return mod.ModDetails.Description;

        string desc = "";
        foreach (string warning in mod.Warnings)
            desc += "(WARNING: " + warning + ")\n";
        desc += "\n";
        desc += mod.ModDetails.Description;
        return desc;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
