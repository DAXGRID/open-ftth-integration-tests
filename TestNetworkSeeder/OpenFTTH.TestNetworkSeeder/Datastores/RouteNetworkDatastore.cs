using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenFTTH.TestNetworkSeeder.Datastores
{
    public class RouteNetworkDatastore
    {
        private string _applicationName = "TestNetworkSeeder";
        private string _userName = Environment.UserName;
        private Guid _workTaskId = Guid.Parse("22f110e2-e132-4301-a7df-2f1cb85167e3");
        private string _postgresConnectionString;

        public RouteNetworkDatastore(string postgresConnectionString)
        {
            _postgresConnectionString = postgresConnectionString;
        }

        public void InsertNode(RouteNodeRecord node)
        {
            using (var connection = GetConnection())
            {
                var insertStmt = $@"
                    INSERT INTO route_network.route_node(
                    mrid,
                    coord,
                    work_task_mrid,
                    user_name,
                    application_name,
                    marked_to_be_deleted,
                    delete_me
                    )
                    VALUES(
                    @id,
                    ST_GeomFromWKB(@coord, 25832),
                    '{_workTaskId}',
                    '{_userName}',
                    '{_applicationName}',
                    false,
                    false
                    );";

                connection.Open();
                connection.Execute(insertStmt, node);
            }
        }

        public void InsertSegment(RouteSegmentRecord node)
        {
            using (var connection = GetConnection())
            {
                var insertStmt = $@"
                    INSERT INTO route_network.route_segment(
                    mrid,
                    coord,
                    work_task_mrid,
                    user_name,
                    application_name,
                    marked_to_be_deleted,
                    delete_me
                    )
                    VALUES(
                    @id,
                    ST_GeomFromWKB(@coord, 25832),
                    '{_workTaskId}',
                    '{_userName}',
                    '{_applicationName}',
                    false,
                    false
                    );";

                connection.Open();
                connection.Execute(insertStmt, node);
            }
        }

        private NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(_postgresConnectionString);
        }
    }
}
