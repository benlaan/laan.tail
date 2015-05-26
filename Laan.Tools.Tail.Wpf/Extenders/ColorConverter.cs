using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;

namespace Laan.Tools.Tail.Extenders
{
    [ValueConversion(typeof(Int32), typeof(Brush))]
    public class ColorConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var colors = new[]
            {
                new { Level =  0, Color = Colors.Transparent },
                new { Level =  1, Color = Colors.GreenYellow },
                new { Level = 10, Color = Colors.MediumSeaGreen },
                new { Level = 20, Color = Colors.Green },
                new { Level = 30, Color = Colors.Orange },
                new { Level = 45, Color = Colors.OrangeRed },
                new { Level = 60, Color = Colors.Red }
            };

            var level = (int)value;
            
            var least = colors
                .Reverse()
                .SkipWhile(color => color.Level > level)
                .FirstOrDefault();

            var colorLevel = least ?? colors.First();

            return new SolidColorBrush(colorLevel.Color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
