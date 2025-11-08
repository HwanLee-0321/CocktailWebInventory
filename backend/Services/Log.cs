using System.Runtime.CompilerServices;

namespace CocktailWebApplication.Services
{
    public static class Log
    {
        public static void Error(string message, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "")
        {
            string fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
            Console.WriteLine($"[{fileName}] {memberName}: {message}");
        }
    }
}
