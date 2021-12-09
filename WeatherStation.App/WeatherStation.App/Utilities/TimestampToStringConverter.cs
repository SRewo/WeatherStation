using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace WeatherStation.App.Utilities
{
    public class TimestampToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var timestamp = (Google.Protobuf.WellKnownTypes.Timestamp)value;
            var date = timestamp.ToDateTime();
            return date.ToString((string)parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var datetime = (DateTime)value;
            return Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(datetime);
        }
    }
}
