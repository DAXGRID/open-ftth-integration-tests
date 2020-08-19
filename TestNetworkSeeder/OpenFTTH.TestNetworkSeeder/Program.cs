using GoCommando;

namespace Beverage
{
    [Banner(@"------------------------------
OpenFTTH Test Network Seeder
------------------------------")]
    [SupportImpersonation]
    class Program
    {
        static void Main()
        {
            Go.Run();
        }
    }
}