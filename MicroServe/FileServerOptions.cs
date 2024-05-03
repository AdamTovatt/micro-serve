using System.Text.Json;
using System.Text.Json.Serialization;

namespace MicroServe
{
    public class FileServerOptions
    {
        /// <summary>
        /// The path to the file that will be served when a file is not found.
        /// </summary>
        [JsonPropertyName("notFoundPagePath")]
        public string NotFoundPagePath { get; set; } = "404.html";

        /// <summary>
        /// The maximum size of a file that will be cached in memory.
        /// </summary>
        [JsonPropertyName("maxCachedFileSize")]
        public int MaxCachedFileSize { get; set; } = 1024 * 5;

        /// <summary>
        /// If the path should be prefixed with the host name. Should be true if multiple sites are being served from the same server.
        /// </summary>
        [JsonPropertyName("prefixPathWithHost")]
        public bool PrefixPathWithHost { get; set; } = true;

        /// <summary>
        /// Will try to load an options object from a path, returning null if the file does not exist.
        /// </summary>
        /// <param name="path">The path to load from.</param>
        /// <returns>An options object if loadable or null if not.</returns>
        public static FileServerOptions? FromPath(string path)
        {
            if (!File.Exists(path))
                return null;

            return FromJson(File.ReadAllText(path));
        }

        /// <summary>
        /// Will save the options object to a file.
        /// </summary>
        /// <param name="path">The path to save the options object to.</param>
        public void ToPath(string path)
        {
            File.WriteAllText(path, ToJson());
        }

        private static FileServerOptions FromJson(string json)
        {
            FileServerOptions? result = JsonSerializer.Deserialize<FileServerOptions>(json);

            if (result == null)
                throw new Exception("Could not deserialize the provided json to a FileServerOptions object.");

            return result;
        }

        private string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
