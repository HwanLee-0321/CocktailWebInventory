using DotNetEnv;
namespace CocktailWebApplication.Models
{
    public static class Settings
    {
        private const string _filePath = ".\\data\\settings.env";

        public static readonly string PAPAGO_API_URL;
        public static readonly string X_NCP_APIGW_API_KEY_ID;
        public static readonly string X_NCP_APIGW_API_KEY;
        public static readonly string OPEN_AI_API;

        static Settings()
        {
            Env.Load(_filePath);

            PAPAGO_API_URL = Environment.GetEnvironmentVariable("PAPAGO_API_URL")!;
            X_NCP_APIGW_API_KEY_ID = Environment.GetEnvironmentVariable("X_NCP_APIGW_API_KEY_ID")!;
            X_NCP_APIGW_API_KEY = Environment.GetEnvironmentVariable("X_NCP_APIGW_API_KEY")!;
            OPEN_AI_API = Environment.GetEnvironmentVariable("OPEN_AI_API")!;
        }
    }
}
