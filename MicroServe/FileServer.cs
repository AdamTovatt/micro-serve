﻿using System.Text;

namespace MicroServe
{
    public class FileServer
    {
        public string NotFoundPagePath { get; set; } = "404.html";

        private Dictionary<string, ServedContent> servedContents;
        private string path;

        private FileServer(string path)
        {
            this.path = path;
            servedContents = new Dictionary<string, ServedContent>();
        }

        public static FileServer CreateNew(string path)
        {
            FileServer server = new FileServer(path);

            server.InitializeServedContent();

            return server;
        }

        public void InitializeServedContent()
        {
            if (!Directory.Exists(path))
                throw new Exception($"A path to a directory that does not exist was provided: {path}");

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

                servedContents.Add(relativeFilePath, new ServedContent(fileName, fileExtension, relativeFilePath, file));
            }

            // now handle the directories
            foreach (string directoryPath in Directory.GetDirectories(path))
            {
                string newSubDirectoryPath = subDirectoryPath == null ? Path.GetFileName(directoryPath) : $"{subDirectoryPath}/{Path.GetFileName(directoryPath)}";
                InitializeServedContentInDirectory(directoryPath, newSubDirectoryPath);
            }
        }

        public async Task<IResult> GetContentAsync(string? path, HttpRequest request)
        {
            path ??= "index.html";

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
            if (servedContents.TryGetValue(NotFoundPagePath, out ServedContent? servedContent))
                return await servedContent.ToResultAsync();

            return CreateTextResult("404 not found");
        }

        private IResult CreateTextResult(string text)
        {
            return Results.File(Encoding.UTF8.GetBytes(text), contentType: "text/plain");
        }
    }
}
