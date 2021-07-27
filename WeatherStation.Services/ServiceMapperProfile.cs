using System;
using AutoMapper;
using AutoMapper.Configuration;
using Google.Protobuf.WellKnownTypes;
using WeatherStation.Library;
using WeatherStation.Library.Interfaces;

namespace WeatherStation.Services
{
    public class ServiceMapperProfile : Profile
    {
        public ServiceMapperProfile()
        {
            CreateMap<Library.TemperatureScale, TemperatureScale>();

            CreateMap<Temperature, TemperatureMessage>();

            CreateMap<WeatherData, WeatherMessage>().
                ForMember(x => x.Date, opt => 
                    opt.MapFrom(src => Timestamp.FromDateTime(src.Date.ToUniversalTime())));

            CreateMap<IWeatherRepositoryStore, InfoReply>();

            
        }
    }
}