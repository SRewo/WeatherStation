using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Google.Api;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using WeatherStation.Library;
using WeatherStation.Library.Interfaces;

namespace WeatherStation.Services.Services
{
    public class WeatherService : WeatherStation.Services.Weather.WeatherBase
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

        public override async Task<CurrentWeatherReply> GetCurrentWeather(CurrentWeatherRequest request, ServerCallContext context)
        {
            var repository = await GetRepositoryFromDictionary(request.Repository);

            var weather = await repository.CurrentWeatherRepository.
                GetWeatherDataFromRepository();
            var data = weather.First();
            var result = new CurrentWeatherReply()
            {
                Repository = request.Repository,
                Weather = _mapper.Map<WeatherMessage>(data)
            };

            return result;
        }
    }
}
