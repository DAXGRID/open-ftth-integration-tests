using System;
using DemoDataBuilder.Builders;
using GoCommando;
using OpenFTTH.TestNetworkSeeder.Datastores;

namespace OpenFTTH.TestNetworkSeeder.Commands
{
    [Command("build-from-geojson", group: "import")]
    [Description("Build test network from two geojson files containing instructions now the network should be built.")]
    public class BuildFromGeojson : ICommand
    {
        [Parameter("nodeFilename")]
        [Description("Node geojson file containing info on what route nodes should be built")]
        public string Nodefilename { get; set; }

        [Parameter("segmentFilename")]
        [Description("Segment geojson file containing info on what route segments should be built")]
        public string SegmentFilename { get; set; }

        [Parameter("postgresConnectionString")]
        [Description("Connection string to Postgres/PostGIS database where route network data should be imported.")]
        public string PostgresConnectionString { get; set; }


        public void Run()
        {
            var routeNetworkDatastore = new RouteNetworkDatastore(PostgresConnectionString);

            new RouteNetworkBuilder().Run(Nodefilename, SegmentFilename, routeNetworkDatastore);
        }
    }
}
