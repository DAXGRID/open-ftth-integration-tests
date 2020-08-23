using System;
using System.Linq;
using System.Threading;
using DemoDataBuilder.Builders;
using GoCommando;
using Microsoft.Extensions.Logging;
using OpenFTTH.Events.RouteNetwork;
using OpenFTTH.EventWatchAndFetch;
using OpenFTTH.TestNetworkSeeder.Datastores;
using OpenFTTH.TestNetworkSeeder.Tests;
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

        public void Run()
        {
            var loggerFactory = LoggerFactory.Create(builder => 
            builder.AddConsole().SetMinimumLevel(LogLevel.Debug)
            );

            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(new Serilog.Core.LoggingLevelSwitch(Serilog.Events.LogEventLevel.Verbose))
            .WriteTo.Console()
            .CreateLogger();

            try {

                var routeNetworkDatastore = new RouteNetworkDatastore(PostgresConnectionString);

                var startMarker = Guid.NewGuid();
                var endMarker = Guid.NewGuid();

                var routeNetworkBuilder = new RouteNetworkBuilder();
                routeNetworkBuilder.Run(Nodefilename, SegmentFilename, routeNetworkDatastore, startMarker, endMarker);

                var graph = routeNetworkBuilder.RouteGraph;

                using var eventFetcher = new WaitForAndFetchEvents<RouteNetworkEvent>(loggerFactory, KafkaServer, RouteNetworkTopicName);
      
                long timeoutMs = 1000 * 60 * 1; // Wait 1 minute, before giving up recieving route network events from topic

                bool timedOut = eventFetcher.WaitForEvents(
                    start => start.WorkTaskMrid.Equals(startMarker),
                    stop => stop.WorkTaskMrid.Equals(endMarker),
                    timeoutMs
                    ).Result;


                var events = eventFetcher.Events.ToList();

                // Check if event fetcher timed out
                if (timedOut)
                {
                    LogErrorAndThrowException($"Seeding of test network failed. Timeout ({timeoutMs} ms) exceded waiting for events to arrive on route network topic.");
                }

                // Check if GDB integrator has put the right amount of events in the route network event topic
                if (events.Count != (graph.Nodes.Count + graph.Edges.Count))
                {
                    LogErrorAndThrowException($"Seeding of test network failed. {(graph.Nodes.Count + graph.Edges.Count)} number of nodes and routes were inserted into Postgres. Expected the same amount of events inserted into the route network topic by GDB integrator, but got {events.Count} events from topic!");
                }


                bool someTestFailed = false;

                // Check that all properties we add to postgres is added to events etc.
                if (!new CheckEventProperties().Run(graph, events))
                    someTestFailed = true;


                // Check if event fetcher timed out
                if (someTestFailed)
                {
                    LogErrorAndThrowException($"Seeding of test network failed. Please search the log for errors resulting from integration tests.");
                }


            }
            catch (Exception ex)
            {
                Log.Error("Seeding of test network failed. Unhandled exception: " + ex.Message, ex);
                throw ex;
            }

        }

        private void LogErrorAndThrowException(string message)
        {
            Log.Error(message);
            throw new GoCommandoException(message);
        }
    }
}
