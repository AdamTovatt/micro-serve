namespace MicroServe
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
                throw new ArgumentException("No run time arguments where provided! A run time argument with the path to a folder to server must be provided!");

            string path = args[0];

            FileServer fileServer = FileServer.CreateNew(path);

            WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);

            WebApplication app = builder.Build();

            RouteGroupBuilder todosApi = app.MapGroup("");
            todosApi.MapGet("/{*path}", async (string? path, HttpRequest request) => await fileServer.GetContentAsync(path, request));

            app.Run();
        }
    }
}
