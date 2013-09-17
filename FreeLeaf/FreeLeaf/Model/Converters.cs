using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace FreeLeaf.Model
{
    public class DiscoveryListStyleSelector : StyleSelector
    {
        public Style ItemStyle { get; set; }
        public Style ButtonStyle { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            return ((DeviceItem)item).ID == null ? ButtonStyle : ItemStyle;
        }
    }

    public class NegateBooleanConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}
