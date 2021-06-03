using System.Threading.Tasks;
using System.IO.Abstractions;

namespace WeatherStation.App.Utilities{
    public interface IWriteService{
        Task WriteFromString(string content);
        Task CreateFile();
    }

    public class TxtWriteService : IWriteService
    {
        public string FolderPath {get; private set;}
        public string FileName {get; private set;}

        private string _filePath; 
        private IFileSystem _fileSystem;

        public TxtWriteService(IFileSystem fileSystem,string path, string filename)
        {
            FolderPath=path;
            FileName = filename;
            _fileSystem = fileSystem;
            _filePath = _fileSystem.Path.Combine(FolderPath, filename+".txt");
        }

        public async Task WriteFromString(string content)
        {
            if(!_fileSystem.File.Exists(_filePath))
                await CreateFile();

            _fileSystem.File.AppendAllText(_filePath, content);
        }
        public Task CreateFile()
        {
           _fileSystem.File.Create(_filePath);
           return Task.CompletedTask;
        }
    }
}