using Lxy.HttpUtils.App;

namespace Lxy.HttpUtilsTests
{
    internal class Startup
    {
        static Startup()
        {
            Program.Main(["--urls=https://localhost:5200", "--start=Start"]);
        }

        public static void Run()
        {
        }
    }
}