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
using RestSharp;
using WeatherStation.Library;
using WeatherStation.Library.Interfaces;
using WeatherStation.Library.Repositories;
using WeatherStation.Library.Repositories.AccuWeather;
using WeatherStation.Library.Repositories.OpenWeatherMap;
using WeatherStation.Library.Repositories.Weatherbit;

namespace WeatherStation.Services
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc();
            services.AddSingleton(CreateRepositoryDictionary());
            services.AddSingleton(CreateGeocodingRepository());
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
                    "",
                    dateProvider,
                    accuRestClient, "").Result;

            return repository;
        }

        private IWeatherRepositoryStore CreateWeatherbitRepository(IDateProvider dateProvider)
        {
            var restClient = new RestClient(" http://api.weatherbit.io/v2.0/");
            var repository = new WeatherbitRepositoryStore(
                restClient,
                AppApiKeys.WeatherbitApiKey,
                new Coordinates(0,0),
                dateProvider, "");

            return repository;
        }

        private IWeatherRepositoryStore CreateOpenWeatherMapRepository(IDateProvider dateProvider)
        {
            var restClient = new RestClient("https://api.openweathermap.org/data/2.5/onecall");
            var repository = new OpenWeatherMapRepositoryStore(
                AppApiKeys.OpenWeatherMapApiKey,
                dateProvider,
                restClient,
                "",
                new Coordinates(0, 0));

            return repository;
        }

        private IGeocodingRepository CreateGeocodingRepository()
        {
            var geocodingRestClient = new RestClient("http://api.positionstack.com/v1/");
            var repository = new PositionstackGeocodingRepository(geocodingRestClient, AppApiKeys.PositionstackApiKey);

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
            });
        }
    }
}