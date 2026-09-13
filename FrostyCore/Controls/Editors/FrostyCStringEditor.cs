using Frosty.Controls;
using System.Windows;
using System.Windows.Data;
using Frosty.Sdk.Interfaces;

namespace Frosty.Core.Controls.Editors;

public class FrostyCStringEditor : FrostyTypeEditor<FrostyCStringControl>
{
    public FrostyCStringEditor()
    {
        ValueProperty = FrostyCStringControl.ValueProperty;
        BindingMode = BindingMode.TwoWay;
        NotifyOnTargetUpdated = true;
    }
}

public class FrostyCStringControl : FrostyEllipsedTextBox
{
    #region -- Properties --

    #region -- Value --
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(IPrimitive), typeof(FrostyCStringControl), new FrameworkPropertyMetadata(null));
    public IPrimitive Value
    {
        get => (IPrimitive)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
    #endregion

    #endregion

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        GotFocus += FrostyCStringControl_GotFocus;
        LostFocus += FrostyCStringControl_LostFocus;
        TargetUpdated += FrostyCStringControl_TargetUpdated;

        ShowStringDisplay();
    }

    private void FrostyCStringControl_LostFocus(object sender, RoutedEventArgs e)
    {
        string oldStr = (string)Value.ToActualType();
        string newStr = Text;

        if(!oldStr.Equals(newStr))
            Value.FromActualType(newStr);

        ShowStringDisplay();
        //e.Handled = true;
    }

    private void FrostyCStringControl_GotFocus(object sender, RoutedEventArgs e)
    {
        Text = (string)Value.ToActualType();
        ToolTip = null;
        SelectAll();
        //e.Handled = true;
    }

    public void ShowStringDisplay()
    {
        string value = (string)Value.ToActualType();
        /*if (value.StartsWith("ID_"))
        {
            Text = LocalizedStringDatabase.Current.GetString(value);
            ToolTip = value;
        }
        else*/
            Text = value;
    }

    private void FrostyCStringControl_TargetUpdated(object sender, DataTransferEventArgs e)
    {
        Text = (string)Value.ToActualType();
        ShowStringDisplay();
    }
}