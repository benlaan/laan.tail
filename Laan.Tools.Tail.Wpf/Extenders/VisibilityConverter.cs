using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using System.Windows;

namespace Laan.Tools.Tail.Win.Extenders
{
    public class VisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool isVisible = (bool)value;

            return (isVisible ? Visibility.Visible : Visibility.Collapsed);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((Visibility)value) == Visibility.Visible;
        }

        #endregion
    }
}
