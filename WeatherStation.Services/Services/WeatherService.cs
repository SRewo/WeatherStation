using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using WeatherStation.Library.Interfaces;

namespace WeatherStation.Services.Services
{
    public class WeatherService : WeatherStation.Services.Weather.WeatherBase
    {
        private readonly ILogger<WeatherService> _logger;
        private readonly IDictionary<Repositories, IWeatherRepositoryStore> _repositories;

        public WeatherService(ILogger<WeatherService> logger, IDictionary<Repositories, IWeatherRepositoryStore> repositories)
        {
            _logger = logger;
            _repositories = repositories;
        }

        public override Task<ListReply> GetRepositoryList(ListRequest request, ServerCallContext context)
        {
            var repositoriesEnumerable = Enum.GetValues(typeof(Repositories)).Cast<Repositories>();

            return Task.FromResult(new ListReply()
            {
                ListOfRepositories = {repositoriesEnumerable}
            });
        }

        public override Task<InfoReply> GetRepositoryInfo(InfoRequest request, ServerCallContext context)
        {
            var repositoryEnum = request.Repository;

            if (!_repositories.ContainsKey(repositoryEnum))
                throw new RpcException(new Status(StatusCode.OutOfRange,
                    $"Service does not have implementation of repository {repositoryEnum}"));

            var repository = _repositories[repositoryEnum];

            return CreateInfoReplayFromRepository(repository);
        }

        private Task<InfoReply> CreateInfoReplayFromRepository(IWeatherRepositoryStore repository)
        {
            var response = new InfoReply()
            {
                ContainsDailyForecasts = repository.ContainsDailyForecasts,
                ContainsHistoricalData = repository.ContainsHistoricalData,
                ContainsHourlyForecasts = repository.ContainsHourlyForecasts,
                RepositoryName = repository.RepositoryName
            };
            return Task.FromResult(response);
        }
    }
}
