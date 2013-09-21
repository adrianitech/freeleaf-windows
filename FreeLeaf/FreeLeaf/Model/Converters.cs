using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace FreeLeaf.Model
{
    public class cont : StyleSelector
    {
        public Style Style1 { get; set; }
        public Style Style2 { get; set; }
        public Style Style3 { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is MusicFileItem) return Style1;
            if (item is PictureFileItem) return Style3;
            return Style2;
        }
    }

    public class ColorListItemTemplateSelector: DataTemplateSelector
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
            return !(bool)value;
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

    public class NegateBoolConverter : IValueConverter
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

    public class FileSizeConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double length = (long)value;
            if (length == 0) return string.Empty;

            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;

            while (length >= 1024 && order + 1 < sizes.Length)
            {
                length = length / (double)1024;
                order++;
            }

            return string.Format("{0:0.##} {1}", length, sizes[order]);
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}
