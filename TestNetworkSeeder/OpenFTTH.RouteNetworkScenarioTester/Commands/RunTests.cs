using System;
using System.Linq;
using System.Threading;
using GoCommando;
using Microsoft.Extensions.Logging;
using OpenFTTH.Events.RouteNetwork;
using OpenFTTH.EventWatchAndFetch;
using OpenFTTH.RouteNetworkScenarioTester.Tests;
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

            bool someTestFailed = false;

            try
            {
                if (!new Scenario01(loggerFactory, PostgresConnectionString, KafkaServer, RouteNetworkTopicName).Run())
                    someTestFailed = true;
            }
            catch (Exception ex)
            {
                Log.Error("Some route network integration tests failed.  Unhandled exception: " + ex.Message, ex);
                throw ex;
            }

            // If any test failed throw an exception
            if (someTestFailed)
            {
                LogErrorAndThrowException($"Some route network integration tests failed. Please search the log for errors resulting from integration tests.");
            }


        }

        private void LogErrorAndThrowException(string message)
        {
            Log.Error(message);
            throw new GoCommandoException(message);
        }
    }
}
