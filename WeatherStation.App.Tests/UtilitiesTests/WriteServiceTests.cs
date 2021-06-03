using System.Threading.Tasks;
using WeatherStation.App.Utilities;
using Xunit;
using System.IO.Abstractions.TestingHelpers;

namespace WeatherStation.App.Tests.UtilitiesTests
{
    public class TxtWriteServiceTests
    {
        private string _fileName = "textfile";

        [Fact]
        public async Task CreateFile_ValidCall()
        {
            var mockFileSystem = new MockFileSystem();
            var service = new TxtWriteService(mockFileSystem,"", _fileName);

            await service.CreateFile();

            Assert.True(mockFileSystem.File.Exists($"{_fileName}.txt"));
        }

        [Fact]
        public async Task CreateFile_FileAlreadyExists_OverridesOldFile()
        {
            var mockFileSystem = new MockFileSystem();
            var service = new TxtWriteService(mockFileSystem,"", _fileName);
            mockFileSystem.File.AppendAllText($"{_fileName}.txt", "123");


            await service.CreateFile();

            var file = mockFileSystem.File.ReadAllLines($"{_fileName}.txt");
            Assert.Empty(file);
        }

        [Fact]
        public async Task WriteFromString_FileDoesNotExists_CreatesNewFile()
        {
            var mockFileSystem = new MockFileSystem();
            var service = new TxtWriteService(mockFileSystem,"", _fileName);

            await service.WriteFromString("");

            Assert.True(mockFileSystem.File.Exists($"{_fileName}.txt"));
        }

        [Fact]
        public async Task WriteFromString_EmptyFileExists_AddsNewText()
        {
            var mockFileSystem = new MockFileSystem();
            var service = new TxtWriteService(mockFileSystem,"", _fileName);
            mockFileSystem.File.Create($"{_fileName}.txt");

            await service.WriteFromString("test");

            var file = mockFileSystem.File.ReadAllLines($"{_fileName}.txt");
            Assert.Equal("test",file[0]);
        }

        [Fact]
        public async Task WriteFromString_FileContainsText_AddsNewText()
        {
            var mockFileSystem = new MockFileSystem();
            mockFileSystem.File.AppendAllText($"{_fileName}.txt", "123");
            var service = new TxtWriteService(mockFileSystem,"", _fileName);

            await service.WriteFromString("321");

            var file = mockFileSystem.File.ReadAllLines($"{_fileName}.txt");
            Assert.Equal("123321",file[0]);
        }

    }
}