using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using RestSharp;
using WeatherStation.Library;
using WeatherStation.Library.Interfaces;
using WeatherStation.Library.Repositories;
using WeatherStation.Library.Repositories.AccuWeather;
using WeatherStation.Library.Repositories.OpenWeatherMap;
using WeatherStation.Library.Repositories.Weatherbit;
using WeatherStation.Services.Services;

namespace WeatherStation.Services
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly Coordinates _coordinates;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
            _coordinates = CreateCoordinatesFromConfig(configuration);
        }

        private Coordinates CreateCoordinatesFromConfig(IConfiguration configuration)
        {
            double latitude, longitude;
            double.TryParse(configuration["Preferences:Latitude"], out latitude);
            double.TryParse(configuration["Preferences:Longitude"], out longitude);
            return new Coordinates(latitude, longitude);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc();
            services.AddSingleton(CreateRepositoryDictionary());
            services.AddSingleton(CreateMapper());
            services.AddGrpcReflection();
        }

        private IMapper CreateMapper()
        {
            var profile = new ServiceMapperProfile();
            var configuration = new MapperConfiguration(opt =>
                opt.AddProfile(profile));
            return new Mapper(configuration);
        }

        private IDictionary<Repositories, IWeatherRepositoryStore> CreateRepositoryDictionary()
        {
            var dateProvider = new DateProvider();
            var dictionary = new Dictionary<Repositories, IWeatherRepositoryStore>();

            AddRepositoriesToDictionary(dictionary, dateProvider);

            return dictionary;
        }

        private void AddRepositoriesToDictionary(IDictionary<Repositories, IWeatherRepositoryStore> dictionary, IDateProvider dateProvider)
        {
            dictionary.Add(Repositories.Accuweather, CreateAccuWeatherRepository(dateProvider));
            dictionary.Add(Repositories.Weatherbit, CreateWeatherbitRepository(dateProvider));
            dictionary.Add(Repositories.Openweathermap, CreateOpenWeatherMapRepository(dateProvider));
        }


        private IWeatherRepositoryStore CreateAccuWeatherRepository(IDateProvider dateProvider)
        {
            var accuRestClient = new RestClient("http://dataservice.accuweather.com");

            var repository = AccuWeatherRepositoryStore.FromCityCode(
                    AppApiKeys.AccuWeatherApiKey,
                    _configuration["Preferences:AccuweatherCityCode"],
                    dateProvider,
                    accuRestClient, _configuration["Preferences:Language"]).Result;

            return repository;
        }

        private IWeatherRepositoryStore CreateWeatherbitRepository(IDateProvider dateProvider)
        {
            var restClient = new RestClient(" http://api.weatherbit.io/v2.0/");
            var repository = new WeatherbitRepositoryStore(
                restClient,
                AppApiKeys.WeatherbitApiKey,
                _coordinates,
                dateProvider, _configuration["Preferences:Language"]);

            return repository;
        }

        private IWeatherRepositoryStore CreateOpenWeatherMapRepository(IDateProvider dateProvider)
        {
            var restClient = new RestClient("https://api.openweathermap.org/data/2.5/onecall");
            var repository = new OpenWeatherMapRepositoryStore(
                AppApiKeys.OpenWeatherMapApiKey,
                dateProvider,
                restClient,
                _configuration["Preferences:Language"],
                _coordinates);

            return repository;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<WeatherService>();

                endpoints.MapGet("/",
                    async context =>
                    {
                        await context.Response.WriteAsync(
                            "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                    });
                endpoints.MapGrpcReflectionService();
            });
        }
    }
}