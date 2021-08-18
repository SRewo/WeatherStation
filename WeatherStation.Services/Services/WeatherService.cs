using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using WeatherStation.Library.Interfaces;

namespace WeatherStation.Services.Services
{
    public class WeatherService : Weather.WeatherBase
    {
        private readonly ILogger<WeatherService> _logger;
        private readonly IDictionary<Repositories, IWeatherRepositoryStore> _repositories;
        private readonly IMapper _mapper;

        public WeatherService(ILogger<WeatherService> logger, IDictionary<Repositories, IWeatherRepositoryStore> repositories, IMapper mapper)
        {
            _logger = logger;
            _repositories = repositories;
            _mapper = mapper;
        }

        public override Task<ListReply> GetRepositoryList(ListRequest request, ServerCallContext context)
        {
            var repositoriesEnumerable = Enum.GetValues(typeof(Repositories)).Cast<Repositories>();

            return Task.FromResult(new ListReply()
            {
                ListOfRepositories = {repositoriesEnumerable}
            });
        }

        public override async Task<InfoReply> GetRepositoryInfo(InfoRequest request, ServerCallContext context)
        {
            var repository = await GetRepositoryFromDictionary(request.Repository);

            return _mapper.Map<InfoReply>(repository);
        }

        private Task<IWeatherRepositoryStore> GetRepositoryFromDictionary(Repositories repositoryEnum)
        {
            if (!_repositories.ContainsKey(repositoryEnum))
                throw new RpcException(new Status(StatusCode.OutOfRange,
                    $"Service does not have implementation of repository {repositoryEnum}"));

            var repository = _repositories[repositoryEnum];

            return Task.FromResult(repository);
        }

        public override async Task<CurrentWeatherReply> GetCurrentWeather(WeatherRequest request, ServerCallContext context)
        {
            try
            {
                return await CreateCurrentWeatherReply(request);
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, $"{ex.GetType()}: {ex.Message}"));
            }

        }

        private async Task<CurrentWeatherReply> CreateCurrentWeatherReply(WeatherRequest request)
        {
            var repository = await GetRepositoryFromDictionary(request.Repository);
            var weather = await repository.CurrentWeatherRepository.GetWeatherDataFromRepository(); var data = weather.First(); 
            var result = new CurrentWeatherReply()
            {
                Weather = _mapper.Map<WeatherMessage>(data)
            };

            return result;
        }

        public override async Task<ForecastsReply> GetDailyForecasts(WeatherRequest request, ServerCallContext context)
        {
            try
            {
                return await PrepareDailyForecastsReply(request);
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, $"{ex.GetType()}: {ex.Message}"));
            }

        }

        private async Task<ForecastsReply> PrepareDailyForecastsReply (WeatherRequest request)
        {
            var repository = await GetRepositoryAndCheckIfContainsDailyForecasts(request);

            var forecasts = await repository.DailyForecastsRepository.GetWeatherDataFromRepository();
            var messageEnumerable = _mapper.Map<IEnumerable<WeatherMessage>>(forecasts);
            var result = new ForecastsReply()
            {
                Forecasts = { messageEnumerable }
            };

            return result;
        }

        private async Task<IWeatherRepositoryStore> GetRepositoryAndCheckIfContainsDailyForecasts(WeatherRequest request)
        {
            var repository = await GetRepositoryFromDictionary(request.Repository);
            if (!repository.ContainsDailyForecasts)
                throw new RpcException(new Status(StatusCode.OutOfRange,
                    $"Repository {request.Repository} does not have daily forecasts."));

            return repository;
        }

        public override async Task<ForecastsReply> GetHourlyForecasts(WeatherRequest request, ServerCallContext context)
        {
            try
            {
                return await PrepareHourlyForecastReply(request);
            }
            catch (RpcException)
            {
                throw;
            }catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, $"{ex.GetType()}: {ex.Message}"));
            }
        }

        private async Task<ForecastsReply> PrepareHourlyForecastReply(WeatherRequest request)
        {
            var repository = await GetHourlyForecastsRepository(request);
            var forecasts = await repository.GetWeatherDataFromRepository();
            var weatherMessageList = _mapper.Map<IEnumerable<WeatherMessage>>(forecasts);

            return new ForecastsReply{Forecasts = {weatherMessageList}};
        }

        private async Task<IWeatherRepository> GetHourlyForecastsRepository(WeatherRequest request)
        {
            var repositoryStore = await GetRepositoryFromDictionary(request.Repository);
            if(!repositoryStore.ContainsHourlyForecasts)
                throw new RpcException(new Status(StatusCode.OutOfRange,
                    $"Repository {request.Repository} does not have hourly forecasts."));

            return repositoryStore.HourlyForecastsRepository;
        }
    }

}
