namespace MicroServe
{
    public class LoadedContent
    {
        public string Name { get; set; }
        public string FileExtension { get; set; }
        public string RelativePath { get; set; }
        public string AbsolutePath { get; set; }

        private byte[]? cachedBytes;

        public string ContentType
        {
            get
            {
                return FileExtension switch
                {
                    ".html" => "text/html",
                    ".css" => "text/css",
                    ".js" => "text/javascript",
                    ".json" => "application/json",
                    ".jpg" => "image/jpeg",
                    ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".gif" => "image/gif",
                    ".svg" => "image/svg+xml",
                    ".ico" => "image/x-icon",
                    _ => "text/plain",
                };
            }
        }

        public LoadedContent(string name, string fileExtension, string relativePath, string absolutePath)
        {
            Name = name;
            RelativePath = relativePath;
            FileExtension = fileExtension;
            AbsolutePath = absolutePath;
        }

        public async Task<byte[]> GetBytes()
        {
            if (cachedBytes != null)
                return cachedBytes;

            return cachedBytes = await File.ReadAllBytesAsync(AbsolutePath);
        }

        public async Task<IResult> ToResultAsync()
        {
            return Results.File(await GetBytes(), contentType: ContentType);
        }
    }
}
