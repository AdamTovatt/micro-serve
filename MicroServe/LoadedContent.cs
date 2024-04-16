namespace MicroServe
{
    public class LoadedContent
    {
        public byte[] Bytes { get; set; }
        public string Name { get; set; }
        public string FileExtension { get; set; }

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

        public LoadedContent(string name, string fileExtension, byte[] bytes)
        {
            Name = name;
            Bytes = bytes;
            FileExtension = fileExtension;
        }
    }
}
