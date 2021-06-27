using System;
using System.Windows.Data;

namespace Encrypter.Transform
{
    public class ContentPanelValueTransform : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo cultureInfo)
        {
            if ((bool)value)
            {
                return "Hide Account Information";
            }
            else
            {
                return "Show Account Information";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo cultureInfo)
        {
            return Binding.DoNothing;
        }
    }
}
