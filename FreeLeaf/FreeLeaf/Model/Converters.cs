using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace FreeLeaf.Model
{
    public class FileExplorerItemSelector : StyleSelector
    {
        public Style GenericStyle { get; set; }
        public Style MusicStyle { get; set; }
        public Style PictureStyle { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is MusicFileItem) return MusicStyle;
            if (item is PictureFileItem) return PictureStyle;
            return GenericStyle;
        }
    }

    public class ColorListItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Template1 { get; set; }
        public DataTemplate Template2 { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return ((DeviceItem)item).ID != null ? Template1 : Template2;
        }
    }

    public class NegateBooleanConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return !(value is bool && (bool)value);
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value is bool && (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

    public class NegateBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value is bool && (bool)value ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

    public class FileSizeConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Helper.SizeToString((long)value);
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}
