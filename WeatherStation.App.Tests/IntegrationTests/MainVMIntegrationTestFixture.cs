using Docker.DotNet;
using Docker.DotNet.Models;
using Grpc.Core;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity;
using WeatherStation.App.Utilities;
using WeatherStation.Library;
using WeatherStation.Library.Interfaces;
using Xamarin.Essentials.Interfaces;
using Xunit;
using static WeatherStation.App.Weather;

namespace WeatherStation.App.Tests.IntegrationTests
{
    public class MainVMIntegrationTestFixture : IAsyncLifetime
    {
        internal IUnityContainer Container;
        internal DockerClient DockerClient;

        protected virtual void RegisterTypes()
        {
            Container.RegisterType<IDateProvider, DateProvider>();
            Container.RegisterInstance(CreateWeatherClient("192.168.1.101:80"));

            RegisterPreferencesInterface(Container);
            RegisterExceptionHandlingInterface(Container);
        }

        private void RegisterPreferencesInterface(IUnityContainer container)
        {
            var preferencesMock = new Mock<IPreferences>();
            preferencesMock.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<string>())).Returns(string.Empty);
            container.RegisterInstance(preferencesMock.Object);
        }

        private void RegisterExceptionHandlingInterface(IUnityContainer container)
        {
            var exceptionHandlingMock = new Mock<IExceptionHandlingService>();
            exceptionHandlingMock.Setup(x => x.HandleException(It.IsAny<Exception>())).Callback<Exception>(ex => throw ex);
            container.RegisterInstance(exceptionHandlingMock.Object);
        }

        protected virtual WeatherClient CreateWeatherClient(string address)
        {
            var channel = new Channel(address, ChannelCredentials.Insecure);
            return new WeatherClient(channel);
        }

        public async Task StartContainer(string containerName)
        {
            var containerInfo = await DockerClient.Containers.InspectContainerAsync(containerName);
            var startTask = DockerClient.Containers.StartContainerAsync(containerName, new ContainerStartParameters());

            if(!containerInfo.State.Running)
                await startTask.ContinueWith(_ => Thread.Sleep(1000), TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        public async Task InitializeAsync()
        {
            Container = new UnityContainer().AddExtension(new Diagnostic());
            RegisterTypes();
            DockerClient = new DockerClientConfiguration().CreateClient();
            await StartContainer("weatherstation_services_1");
        }

        public Task DisposeAsync()
        {
            Container.Dispose();
            DockerClient.Dispose();
            return Task.CompletedTask;
        }
    }
}
