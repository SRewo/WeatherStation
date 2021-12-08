using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Net.Client;

namespace WeatherStation.Services.Tests.Integration
{
    public class WeatherServiceTestFixture : IAsyncDisposable
    {
        public GrpcChannel GrpcChannel { get; }

        private WebApplicationFactory<Startup> _webApplicationFactory;

        public WeatherServiceTestFixture()
        {
            _webApplicationFactory = new WebApplicationFactory<Startup>();
            var client = _webApplicationFactory.CreateClient();
            var options = new GrpcChannelOptions()
            {
                HttpClient = client,
            };
            GrpcChannel = GrpcChannel.ForAddress(client.BaseAddress, options);

        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }
}
