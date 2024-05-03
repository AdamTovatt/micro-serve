using System.Text;

namespace MicroServe
{
    /// <summary>
    /// Used to serve files from a directory.
    /// </summary>
    public class FileServer
    {
        private const string optionsFileName = "file-server-config.json";

        public FileServerOptions Options { get; set; }

        private Dictionary<string, ServedContent> servedContents;
        private string path;

        private FileServer(string path)
        {
            this.path = path;
            servedContents = new Dictionary<string, ServedContent>();

            Options = GetInitializedOptions();
        }

        /// <summary>
        /// Will create a new file server with the provided path as the root directory to serve files from.
        /// </summary>
        /// <param name="path">The path where the files to serve are.</param>
        /// <returns></returns>
        public static FileServer CreateNew(string path)
        {
            FileServer server = new FileServer(path);

            server.InitializeServedContent();

            return server;
        }

        /// <summary>
        /// Will map the requests from the provided web application to be handled by this file server.
        /// </summary>
        /// <param name="webApplication"></param>
        public void MapRequestSource(WebApplication webApplication)
        {
            RouteGroupBuilder api = webApplication.MapGroup("");
            api.MapGet("/{*path}", async (string? path, HttpRequest request) => await GetResponseAsync(path, request));
        }

        public void InitializeOptions(string? path)
        {
            Options = GetInitializedOptions(path);
        }

        public FileServerOptions GetInitializedOptions(string? fileName = null)
        {
            if (fileName == null)
                fileName = optionsFileName;

            string pathToUse = $"{path}/{fileName}";
            FileServerOptions? options = FileServerOptions.FromPath(pathToUse);

            if (options == null)
            {
                options = new FileServerOptions();
                options.ToPath(pathToUse);
            }

            return options;
        }

        /// <summary>
        /// Will initialize the content in the path that the file server is serving from.
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void InitializeServedContent()
        {
            if (!Directory.Exists(path))
                throw new Exception($"A path to a directory that does not exist was provided: {path}");

            if(servedContents.Count != 0)
                servedContents.Clear();

            InitializeServedContentInDirectory(path);
        }

        private void InitializeServedContentInDirectory(string path, string? subDirectoryPath = null)
        {
            // first load the files
            foreach (string file in Directory.GetFiles(path))
            {
                string fileName = Path.GetFileName(file);
                string fileExtension = Path.GetExtension(file);
                string relativeFilePath = subDirectoryPath == null ? fileName : $"{subDirectoryPath}/{fileName}";
                byte[] bytes = File.ReadAllBytes(file);

                if (servedContents.ContainsKey(relativeFilePath))
                    throw new ArgumentException($"Duplicate file names found! {file}");

                servedContents.Add(relativeFilePath, new ServedContent(fileName, fileExtension, relativeFilePath, file, this));
            }

            // now handle the directories
            foreach (string directoryPath in Directory.GetDirectories(path))
            {
                string newSubDirectoryPath = subDirectoryPath == null ? Path.GetFileName(directoryPath) : $"{subDirectoryPath}/{Path.GetFileName(directoryPath)}";
                InitializeServedContentInDirectory(directoryPath, newSubDirectoryPath);
            }
        }

        public async Task<IResult> GetResponseAsync(string? path, HttpRequest request)
        {
            return await GetContentAsync(path, Options.RedirectHost(RefineHostString(request.Host.Value)));
        }

        public async Task<IResult> GetContentAsync(string? path, string? host = null)
        {
            path ??= "index.html";

            if (Options.PrefixPathWithHost && host != null) // prefix the path with the host (if enabled)
            {
                path = $"{host}/{path}";
            }

            if (!servedContents.ContainsKey(path)) // first try to match the path as is
            {
                string pathWithHtml = $"{path}.html"; // if not found, try to match the path with .html

                if (!servedContents.ContainsKey(pathWithHtml))
                {
                    return await Get404PageAsync(); // if still not found, return 404
                }

                path = pathWithHtml;
            }

            ServedContent content = servedContents[path];

            return await content.ToResultAsync();
        }

        private async Task<IResult> Get404PageAsync()
        {
            if (servedContents.TryGetValue(Options.NotFoundPagePath, out ServedContent? servedContent))
                return await servedContent.ToResultAsync();

            return CreateTextResult("404 not found");
        }

        private IResult CreateTextResult(string text)
        {
            return Results.File(Encoding.UTF8.GetBytes(text), contentType: "text/plain");
        }

        private string RefineHostString(string hostString)
        { 
            if(string.IsNullOrEmpty(hostString))
                return hostString;

            return hostString.Replace(":", "").Replace(".", "");
        }
    }
}
