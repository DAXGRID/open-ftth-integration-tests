using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenFTTH.Test.RouteNetworkDatastore
{
    public class RouteNetworkDatastore
    {
       
        private string _postgresConnectionString;

        public RouteNetworkDatastore(string postgresConnectionString)
        {
            _postgresConnectionString = postgresConnectionString;
        }

        public void InsertRouteNode(RouteNodeRecord routeNode)
        {
            using (var connection = GetConnection())
            {
                var query = @"
                    INSERT INTO route_network.route_node(
                    mrid,
                    coord,
                    work_task_mrid,
                    user_name,
                    application_name,
                    application_info,
                    marked_to_be_deleted,
                    delete_me,
                    lifecycle_deployment_state,
                    lifecycle_installation_date,
                    lifecycle_removal_date,
                    mapping_method,
                    mapping_vertical_accuracy,
                    mapping_horizontal_accuracy,
                    mapping_source_info,
                    mapping_survey_date,
                    safety_classification,
                    safety_remark,
                    routenode_kind,
                    routenode_function,
                    naming_name,
                    naming_description
                    )
                    VALUES(
                    @mrid,
                    ST_GeomFromWKB(@coord, 25832),
                    @workTaskMrid,
                    @username,
                    @applicationName,
                    @applicationInfo,
                    @markAsDeleted,
                    @deleteMe,
                    @lifeCycleDeploymentState,
                    @lifeCycleInstallationDate,
                    @lifeCycleRemovalDate,
                    @mappingMethod,
                    @mappingVerticalAccuracy,
                    @mappingHorizontalAccuracy,
                    @mappingSourceInfo,
                    @mappingSurveyDate,
                    @safetyClassification,
                    @safetyRemark,
                    @routeNodeKind,
                    @routeNodeFunction,
                    @namingName,
                    @namingDescription
                    ) ON CONFLICT ON CONSTRAINT route_node_pkey DO NOTHING;";

                var mappedRouteNode = new
                {
                    mrid = routeNode.Id,
                    coord = routeNode.Coord,
                    workTaskMrId = routeNode.WorkTaskMrid,
                    userName = routeNode.Username,
                    applicationName = routeNode.ApplicationName,
                    applicationInfo = routeNode.ApplicationInfo,
                    markAsDeleted = routeNode.MarkAsDeleted,
                    deleteMe = routeNode.DeleteMe,
                    lifeCycleDeploymentState = routeNode.LifecycleInfo?.DeploymentState?.ToString("g"),
                    lifeCycleInstallationDate = routeNode.LifecycleInfo?.InstallationDate,
                    lifeCycleRemovalDate = routeNode.LifecycleInfo?.RemovalDate,
                    mappingMethod = routeNode.MappingInfo?.Method?.ToString("g"),
                    mappingVerticalAccuracy = routeNode.MappingInfo?.VerticalAccuracy,
                    mappingHorizontalAccuracy = routeNode.MappingInfo?.HorizontalAccuracy,
                    mappingSourceInfo = routeNode.MappingInfo?.SourceInfo,
                    mappingSurveyDate = routeNode.MappingInfo?.SurveyDate,
                    safetyClassification = routeNode.SafetyInfo?.Classification,
                    safetyRemark = routeNode.SafetyInfo?.Remark,
                    routeNodeKind = routeNode.RouteNodeInfo?.Kind?.ToString("g"),
                    routeNodeFunction = routeNode.RouteNodeInfo?.Function?.ToString("g"),
                    namingName = routeNode.NamingInfo?.Name,
                    namingDescription = routeNode.NamingInfo?.Description
                };

                connection.Open();
                connection.Execute(query, mappedRouteNode);
            }
        }

        public void InsertRouteSegment(RouteSegmentRecord routeSegment)
        {
            using (var connection = GetConnection())
            {
                var query = @"
                    INSERT INTO route_network.route_segment(
                    mrid,
                    coord,
                    work_task_mrid,
                    user_name,
                    application_name,
                    application_info,
                    marked_to_be_deleted,
                    delete_me,
                    lifecycle_deployment_state,
                    lifecycle_installation_date,
                    lifecycle_removal_date,
                    mapping_method,
                    mapping_vertical_accuracy,
                    mapping_horizontal_accuracy,
                    mapping_source_info,
                    mapping_survey_date,
                    safety_classification,
                    safety_remark,
                    routesegment_kind,
                    routesegment_height,
                    routesegment_width,
                    naming_name,
                    naming_description
                    )
                    VALUES(
                    @mrid,
                    ST_GeomFromWKB(@coord, 25832),
                    @workTaskMrid,
                    @username,
                    @applicationName,
                    @applicationInfo,
                    @markAsDeleted,
                    @deleteMe,
                    @lifeCycleDeploymentState,
                    @lifeCycleInstallationDate,
                    @lifeCycleRemovalDate,
                    @mappingMethod,
                    @mappingVerticalAccuracy,
                    @mappingHorizontalAccuracy,
                    @mappingSourceInfo,
                    @mappingSurveyDate,
                    @safetyClassification,
                    @safetyRemark,
                    @routeSegmentKind,
                    @routeSegmentHeight,
                    @routeSegmentWidth,
                    @namingName,
                    @namingDescription
                    ) ON CONFLICT ON CONSTRAINT route_segment_pkey DO NOTHING;";

                var mappedRouteSegment = new
                {
                    mrid = routeSegment.Id,
                    coord = routeSegment.Coord,
                    workTaskMrId = routeSegment.WorkTaskMrid,
                    userName = routeSegment.Username,
                    applicationName = routeSegment.ApplicationName,
                    applicationInfo = routeSegment.ApplicationInfo,
                    markAsDeleted = routeSegment.MarkAsDeleted,
                    deleteMe = routeSegment.DeleteMe,
                    lifeCycleDeploymentState = routeSegment.LifecycleInfo?.DeploymentState?.ToString("g"),
                    lifeCycleInstallationDate = routeSegment.LifecycleInfo?.InstallationDate,
                    lifeCycleRemovalDate = routeSegment.LifecycleInfo?.RemovalDate,
                    mappingMethod = routeSegment.MappingInfo?.Method?.ToString("g"),
                    mappingVerticalAccuracy = routeSegment.MappingInfo?.VerticalAccuracy,
                    mappingHorizontalAccuracy = routeSegment.MappingInfo?.HorizontalAccuracy,
                    mappingSourceInfo = routeSegment.MappingInfo?.SourceInfo,
                    mappingSurveyDate = routeSegment.MappingInfo?.SurveyDate,
                    safetyClassification = routeSegment.SafetyInfo?.Classification,
                    safetyRemark = routeSegment.SafetyInfo?.Remark,
                    routeSegmentKind = routeSegment?.RouteSegmentInfo?.Kind?.ToString("g"),
                    routeSegmentHeight = routeSegment.RouteSegmentInfo?.Height,
                    routeSegmentWidth = routeSegment.RouteSegmentInfo?.Width,
                    namingName = routeSegment.NamingInfo?.Name,
                    namingDescription = routeSegment.NamingInfo?.Description
                };

                connection.Open();
                connection.Execute(query, mappedRouteSegment);
            }
        }


        private NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(_postgresConnectionString);
        }
    }
}
