using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherStation.App.ViewModels;
using Xunit;

namespace WeatherStation.App.Tests.ViewModelsTests
{
    public class MasterPageViewModelTests : IClassFixture<MasterPageVMTestsFixture>
    {
        private MasterPageVMTestsFixture _fixture;

        public MasterPageViewModelTests(MasterPageVMTestsFixture fixture) => _fixture = fixture;

        [Fact]
        public async Task GetItems_ValidCall_ReturnsArray()
        {
            var model = _fixture.CreateViewModel();

            var array = await model.CreateMenuItems();

            _ = Assert.IsAssignableFrom<Array>(array);
            Assert.Equal(3, array.Count());
        }

        [Fact]
        public async Task GetItems_ValidCall_Contains_ResultRepositoryEnum()
        {
            var model = _fixture.CreateViewModel();

            var result = await model.CreateMenuItems();

            Assert.NotEmpty(result);
            Assert.IsType<Repositories>(result[0].Parameters["repository"]);
        }
    }
}
