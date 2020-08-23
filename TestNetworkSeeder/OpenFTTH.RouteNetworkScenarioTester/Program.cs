using GoCommando;
using System;

namespace OpenFTTH.RouteNetworkScenarioTester
{
    [Banner(@"------------------------------
OpenFTTH Route Network Scenario Tester
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
