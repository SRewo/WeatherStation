using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace WeatherStation.Services.Services
{
    public class WeatherService : WeatherStation.Services.WeatherService.WeatherServiceBase
    {
        private readonly ILogger<WeatherService> _logger;

        public WeatherService(ILogger<WeatherService> logger)
        {
            _logger = logger;
        }

        public override Task<ListReply> GetRepositoryList(ListRequest request, ServerCallContext context)
        {
            var enums = Enum.GetValues(typeof(Repositories)).Cast<Repositories>();

            return Task.FromResult(new ListReply()
            {
                ListOfRepositories = {enums}
            });
        }
    }
}
