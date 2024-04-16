using System.Text;

namespace MicroServe
{
    public class FileServer
    {
        private Dictionary<string, byte[]> loadedFiles;
        private string path;

        private FileServer(string path)
        {
            this.path = path;
            loadedFiles = new Dictionary<string, byte[]>();
        }

        public static FileServer CreateNew(string path)
        {
            FileServer server = new FileServer(path);

            server.LoadFiles();
            
            return server;
        }

        public void LoadFiles()
        {
            if (!Directory.Exists(path))
                throw new Exception($"A path to a directory that does not exist was provided: {path}");

            foreach(string file in Directory.GetFiles(path))
            {
                loadedFiles.Add(Path.GetFileNameWithoutExtension(file), File.ReadAllBytes(file));
            }
        }

        public async Task<IResult> GetContent(string path)
        {
            await Task.CompletedTask;

            if (!loadedFiles.ContainsKey(path))
                return CreateTextResult("404 not found");

            return Results.File(Encoding.UTF8.GetBytes("hello world :)"), contentType: "text/plain");
        }

        private IResult CreateTextResult(string text)
        {
            return Results.File(Encoding.UTF8.GetBytes(text), contentType: "text/plain");
        }
    }
}
