using System.Text;

namespace MicroServe
{
    public class FileServer
    {
        private Dictionary<string, LoadedContent> loadedFiles;
        private string path;

        private FileServer(string path)
        {
            this.path = path;
            loadedFiles = new Dictionary<string, LoadedContent>();
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

            foreach (string file in Directory.GetFiles(path))
            {
                string fileName = Path.GetFileName(file);
                string fileExtension = Path.GetExtension(file);
                byte[] bytes = File.ReadAllBytes(file);

                if (loadedFiles.ContainsKey(fileName))
                    throw new ArgumentException($"Duplicate file names found! {file}");

                loadedFiles.Add(fileName, new LoadedContent(fileName, fileExtension, bytes));
            }
        }

        public async Task<IResult> GetContentAsync(string path)
        {
            await Task.CompletedTask;

            if (!loadedFiles.ContainsKey(path))
            {
                string pathWithHtml = $"{path}.html";

                if (!loadedFiles.ContainsKey(pathWithHtml))
                    return CreateTextResult("404 not found");

                path = pathWithHtml;
            }

            LoadedContent content = loadedFiles[path];

            return Results.File(content.Bytes, contentType: content.ContentType);
        }

        private IResult CreateTextResult(string text)
        {
            return Results.File(Encoding.UTF8.GetBytes(text), contentType: "text/plain");
        }
    }
}
