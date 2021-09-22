using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tynamix.ObjectFiller;

namespace WeatherStation.App.Tests
{
    class ReplyFactory
    {
        internal static Task<CurrentWeatherReply> CreateCurrentWeatherReply()
        {
            var message = CreateWeatherMessage();

            var reply = new CurrentWeatherReply
            {
                Weather = message
            };

            return Task.FromResult(reply);
        }

        private static WeatherMessage CreateWeatherMessage()
        {
            var message = new WeatherMessage()
            {
                ChanceOfRain = new IntRange(0, 100).GetValue(),
                Temperature = CreateTemperatureMessage(),
                TemperatureApparent = CreateTemperatureMessage(),
                TemperatureMin = CreateTemperatureMessage(),
                TemperatureMax = CreateTemperatureMessage(),
                Humidity = new IntRange(0, 100).GetValue(),
                Date = new Google.Protobuf.WellKnownTypes.Timestamp(){ Seconds = new LongRange().GetValue() },
                PrecipitationSummary = new FloatRange(0,200).GetValue(),
                WeatherDescription = new MnemonicString(2,4,10).GetValue()
            };
            
            return message;
        }

        private static TemperatureMessage CreateTemperatureMessage()
        {
            var temperature = new TemperatureMessage(){ 
                    Value = new FloatRange(-30, 40).GetValue(),
                    Scale = TemperatureScale.Celsius
                };
            return temperature;
        }

        internal static Task<ForecastsReply> CreateForecastsReply(int count)
        {
            var list = CreateWeatherMessageList(count);

            var reply = new ForecastsReply
            {
                Forecasts = { list }
            };

            return Task.FromResult(reply);
        }

        private static IEnumerable<WeatherMessage> CreateWeatherMessageList(int count)
        {
            var list = new List<WeatherMessage>();
            for(int i = 0; i < count; i++)
                list.Add(CreateWeatherMessage());

            return list;
        }
    }
}
