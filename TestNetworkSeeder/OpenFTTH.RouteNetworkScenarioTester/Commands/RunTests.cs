using System;
using System.Linq;
using System.Threading;
using GoCommando;
using Microsoft.Extensions.Logging;
using OpenFTTH.Events.RouteNetwork;
using OpenFTTH.EventWatchAndFetch;
using OpenFTTH.Test.RouteNetworkDatastore;
using Serilog;

namespace OpenFTTH.RouteNetworkScenarioTester.Commands
{
    [Command("run-tests", group: "test")]
    [Description("Run a bunch of route network test scenarios.")]
    public class BuildFromGeojson : ICommand
    {
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
            builder.AddConsole().SetMinimumLevel(LogLevel.Information)
            );

            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(new Serilog.Core.LoggingLevelSwitch(Serilog.Events.LogEventLevel.Information))
            .WriteTo.Console()
            .CreateLogger();

            try {

                var routeNetworkDatastore = new RouteNetworkDatastore(PostgresConnectionString);

                var startMarker = Guid.NewGuid();
                var endMarker = Guid.NewGuid();
         
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


                bool someTestFailed = false;


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
