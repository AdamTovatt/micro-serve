namespace MicroServe
{
    public class ServedContent
    {
        public string Name { get; set; }
        public string FileExtension { get; set; }
        public string RelativePath { get; set; }
        public string AbsolutePath { get; set; }

        private byte[]? cachedBytes;
        private FileServer owningServer;

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

        public ServedContent(string name, string fileExtension, string relativePath, string absolutePath, FileServer owningServer)
        {
            Name = name;
            RelativePath = relativePath;
            FileExtension = fileExtension;
            AbsolutePath = absolutePath;
            this.owningServer = owningServer;
        }

        public async Task<byte[]> GetBytes()
        {
            if (cachedBytes != null)
                return cachedBytes;

            byte[] bytes = await File.ReadAllBytesAsync(AbsolutePath);
            
            if(bytes.Length < owningServer.Options.MaxCachedFileSize)
                cachedBytes = bytes;

            return bytes;
        }

        public async Task<IResult> ToResultAsync()
        {
            return Results.File(await GetBytes(), contentType: ContentType);
        }
    }
}
