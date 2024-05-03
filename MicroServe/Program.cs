namespace MicroServe
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
                throw new ArgumentException("No run time arguments where provided! A run time argument with the path to a folder to server must be provided!");

            FileServer fileServer = FileServer.CreateNew(args[0]);

            WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);

            WebApplication app = builder.Build();

            fileServer.MapRequestSource(app);

            app.Run();
        }
    }
}
