using Microsoft.Extensions.Logging;
using OpenFTTH.Events.Core.Infos;
using OpenFTTH.Events.RouteNetwork;
using OpenFTTH.Events.RouteNetwork.Infos;
using OpenFTTH.Test.RouteNetworkDatastore;
using OpenFTTH.Test.Util;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenFTTH.RouteNetworkScenarioTester.Tests
{
    public class Scenario01 : ScenarioBase
    {

        public Scenario01(ILoggerFactory loggerFactory, string postgresConnectionString, string kafkaServer, string routeNetworkTopicName) 
            : base(loggerFactory, postgresConnectionString, kafkaServer, routeNetworkTopicName)
        {

        }

        /// <summary>
        /// Returns true if test went well (no errors)
        /// </summary>
        /// <returns></returns>
        public bool Run()
        {
            Log.Information($"Scenario 1 test begun.");

            DateTime startTime = DateTime.Now;

            Guid startMarker = Guid.NewGuid();

            bool allTestsWentOk = true;

            RouteSegmentRecord routeSegment = new RouteSegmentRecord()
            {
                Id = Guid.NewGuid(),
                WorkTaskMrid = startMarker,
                ApplicationName = "ScenarioTester",
                ApplicationInfo = "Scenario 1 happy case",
                DeleteMe = false,
                MarkAsDeleted = false,
                Username = "ScenarioTester",
                Geometry = GeoJsonConversionHelper.ConvertFromLineGeoJson("[[539632.709067166,6177928.15],[539718.634229065,6177984.82],[539816.442658036,6178004.93]]"),
                RouteSegmentInfo = new RouteSegmentInfo(RouteSegmentKindEnum.Underground, "50 cm", "90 cm"),
                LifecycleInfo = new LifecycleInfo(DeploymentStateEnum.InService, DateTime.Now, DateTime.Now),
                MappingInfo = new MappingInfo(MappingMethodEnum.LandSurveying, "10 cm", "20 cm", DateTime.Now, "Surveyed with GPS"),
                NamingInfo = new NamingInfo("Route segment", "I'm an underground route segment"),
                SafetyInfo = new SafetyInfo("no danger", "might contain gophers"),
            };

            _routeNetworkDatastore.InsertRouteSegment(routeSegment);

            var events = WaitForEvents(startMarker);

            if (events.Count != 3)
            {
                allTestsWentOk = false;
                Log.Error($"Expected 3 events, but got {events.Count}");
                return Fail();
            }
            
            if (allTestsWentOk)
            {
                // Event 1 must be route node added
                if (events[0] is RouteNodeAdded)
                {
                    var routeNodeAdded = events[0] as RouteNodeAdded;

                    allTestsWentOk = CheckApplicationName(routeNodeAdded, "GDB_INTEGRATOR");
                    allTestsWentOk = CheckCmdType(routeNodeAdded, "NewRouteSegmentDigitized");
                    allTestsWentOk = CheckEventId(routeNodeAdded);
                    allTestsWentOk = CheckEventType(routeNodeAdded, "RouteNodeAdded");
                    allTestsWentOk = CheckEventTimestamp(routeNodeAdded, startTime);
                    allTestsWentOk = CheckThatIsLastEventInCmdIsFalse(routeNodeAdded);
                    allTestsWentOk = CheckNodeId(routeNodeAdded);
                    allTestsWentOk = CheckThatWorkTaskMridIsTransferedToEvent(routeNodeAdded, routeSegment.WorkTaskMrid);
                    allTestsWentOk = CheckThatUserNameIsTransferedToEvent(routeNodeAdded, routeSegment.Username);
                }
                else
                    return Fail($"Expected that the first event was a route node added event, but got a: {events[0].GetType().Name}");

            }




            if (allTestsWentOk)
                Log.Information($"Scenario 1 test ended with success.");
            else
                return Fail();

            return allTestsWentOk;
        }

      
    }
}
