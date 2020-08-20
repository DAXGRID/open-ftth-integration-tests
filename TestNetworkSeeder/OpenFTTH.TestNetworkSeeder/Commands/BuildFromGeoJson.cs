using System;
using System.Linq;
using System.Threading;
using DemoDataBuilder.Builders;
using GoCommando;
using Microsoft.Extensions.Logging;
using OpenFTTH.Events.RouteNetwork;
using OpenFTTH.EventWatchAndFetch;
using OpenFTTH.TestNetworkSeeder.Datastores;
using Serilog;

namespace OpenFTTH.TestNetworkSeeder.Commands
{
    [Command("build-from-geojson", group: "import")]
    [Description("Import/build network from two geojson files containing instructions how a test network should be built.")]
    public class BuildFromGeojson : ICommand
    {
        [Parameter("nodeFilename")]
        [Description("Node geojson file containing info on what route nodes should be built-")]
        public string Nodefilename { get; set; }

        [Parameter("segmentFilename")]
        [Description("Segment geojson file containing info on what route segments should be built.")]
        public string SegmentFilename { get; set; }

        [Parameter("postgresConnectionString")]
        [Description("Connection string to Postgres/PostGIS database where route network data should be created.")]
        public string PostgresConnectionString { get; set; }

        [Parameter("kafkaServer")]
        [Description("Server name/ip and port of a kafka broker - i.e. openftth:6000")]
        public string KafkaServer { get; set; }

        [Parameter("routeNetworkTopic")]
        [Description("Name of topic where route network events are written.")]
        public string RouteNetworkTopicName { get; set; }
        
        [Parameter("checkEvents")]
        [Description("Whether the events in Kafka should be checked or not.")]
        public bool CheckKafkaEvents { get; set; }


        public void Run()
        {
            var loggerFactory = LoggerFactory.Create(builder => 
            builder.AddConsole().SetMinimumLevel(LogLevel.Debug)
            );

            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(new Serilog.Core.LoggingLevelSwitch(Serilog.Events.LogEventLevel.Verbose))
            .WriteTo.Console()
            .CreateLogger();


            var routeNetworkDatastore = new RouteNetworkDatastore(PostgresConnectionString);

            var routeNetworkBuilder = new RouteNetworkBuilder();
            routeNetworkBuilder.Run(Nodefilename, SegmentFilename, routeNetworkDatastore);

            using var eventFetcher = new WaitForAndFetchEvents<RouteNetworkEvent>(loggerFactory, KafkaServer, RouteNetworkTopicName);

            Guid startRouteNodeGuid = routeNetworkBuilder.RouteGraph.Nodes.Values.ToList().First().Id;

            Guid stopRouteSegmentGuid = routeNetworkBuilder.RouteGraph.Edges.Values.ToList().Last().Id;

            eventFetcher.WaitForEvents(
                start => start is RouteNodeAdded && ((RouteNodeAdded)start).NodeId == startRouteNodeGuid, 
                stop => stop is RouteSegmentAdded && ((RouteSegmentAdded)stop).SegmentId == stopRouteSegmentGuid
                ).Wait();

            Thread.Sleep(50000);



        }
    }
}
