using IWRPM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace IWRPM
{
    /// <summary>
    /// WaterwayRoutePlannerResponse 的摘要说明
    /// </summary>
    public class WaterwayRoutePlannerResponse
    {
        public static readonly double epsilon = 0.00001;

        public string[] messages { get; set; }
        public string checksum { get; set; } = "dzwAANwIAAA";
        public WaterwayRoutePlannerResponseRoutes routes { get; set; }
        public List<WaterwayRoutePlannerResponseDirection> directions { get; set; } = new List<WaterwayRoutePlannerResponseDirection>();

        public WaterwayRoutePlannerResponse()
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }


        static bool IsSameCoordinate(double[] _traceCoordinate, double[] _comparedCoordinate)
        {
            bool isSameCoordinate = false;
            if (((_traceCoordinate[0] - _comparedCoordinate[0]) + (_traceCoordinate[1] - _comparedCoordinate[1])) < epsilon)
                isSameCoordinate = true;

            return isSameCoordinate;
        }

        public WaterwayRoutePlannerResponse OutputRouteResults(WaterwayGraph waterwayGraph, WaterwayRoutePlanner channelRoutePlanner, string start, string goal)
        {
            if (channelRoutePlanner.isFindOptimalRoute)
            {
                Stack<string> routeResultsStack = new Stack<string>();
                int bridgeNumber = 0;
                int directionsNumber = 1;
                double routeLength = 0.0;
                var startCoordinate = waterwayGraph.m_dicWaterwayNode[start].waterNodeCoordinate;
                var goalCoordinate = waterwayGraph.m_dicWaterwayNode[goal].waterNodeCoordinate;

                var currentResearchElement = goal;
                do
                {
                    routeResultsStack.Push(currentResearchElement);
                    if (waterwayGraph.m_dicWaterwayNode[currentResearchElement].waterNodeType != 3)
                        directionsNumber += 1;

                    var currentResearchLinkElement = channelRoutePlanner.cameFrom[currentResearchElement][1];
                    routeResultsStack.Push(currentResearchLinkElement);

                    routeLength += waterwayGraph.m_dicWaterwayLink[currentResearchLinkElement].channelLength;

                    if (waterwayGraph.m_dicWaterwayLink[currentResearchLinkElement].bridgeNumber != 0)
                        bridgeNumber += waterwayGraph.m_dicWaterwayLink[currentResearchLinkElement].bridgeNumber;
                    currentResearchElement = channelRoutePlanner.cameFrom[currentResearchElement][0];
                }
                while (currentResearchElement != start);
                //routeResultsStack.Push(currentResearchElement);

                WaterwayRoutePlannerResponse waterwayRoutePlannerResponse = new WaterwayRoutePlannerResponse();
                WaterwayRoutePlannerResponseRoutes waterwayRoutePlannerResponseRoutes = new WaterwayRoutePlannerResponseRoutes();
                WaterwayRoutePlannerResponseRoutesFeature waterwayRoutePlannerResponseRoutesFeature = new WaterwayRoutePlannerResponseRoutesFeature();
                List<double[]> waterwayRoutePlannerResponseRoutesFeatureGeometryPath = new List<double[]>();
                waterwayRoutePlannerResponseRoutesFeature.attributes.ObjectID = "1";
                waterwayRoutePlannerResponseRoutesFeature.attributes.Name = waterwayGraph.m_dicWaterwayNode[start].waterNodeName + " - " + waterwayGraph.m_dicWaterwayNode[goal].waterNodeName;
                waterwayRoutePlannerResponseRoutesFeature.attributes.FirstStopID = start;
                waterwayRoutePlannerResponseRoutesFeature.attributes.LastStopID = goal;
                waterwayRoutePlannerResponseRoutesFeature.attributes.StopCount = "2";
                waterwayRoutePlannerResponseRoutesFeature.attributes.Total_TravelTime = "3600.0";
                waterwayRoutePlannerResponseRoutesFeature.attributes.StartTime = "0";
                waterwayRoutePlannerResponseRoutesFeature.attributes.EndTime = "360000";
                waterwayRoutePlannerResponseRoutesFeature.attributes.StartTimeUTC = "30000000";
                waterwayRoutePlannerResponseRoutesFeature.attributes.EndTimeUTC = "30360000";
                waterwayRoutePlannerResponseRoutesFeature.attributes.Shape_Length = routeLength.ToString();

                WaterwayRoutePlannerResponseDirection waterwayRoutePlannerResponseDirection = new WaterwayRoutePlannerResponseDirection();
                WaterwayRoutePlannerResponseDirectionFeature waterwayRoutePlannerResponseDirectionFeatureStart = new WaterwayRoutePlannerResponseDirectionFeature();
                WaterwayRoutePlannerResponseDirectionFeatureString waterwayRoutePlannerResponseDirectionFeatureStringStart = new WaterwayRoutePlannerResponseDirectionFeatureString();
                waterwayRoutePlannerResponseDirectionFeatureStart.attributes.length = 0;
                waterwayRoutePlannerResponseDirectionFeatureStart.attributes.time = "0";
                waterwayRoutePlannerResponseDirectionFeatureStart.attributes.text = "Start at " + waterwayGraph.m_dicWaterwayNode[start].waterNodeName;
                waterwayRoutePlannerResponseDirectionFeatureStart.attributes.ETA = "28800000";
                waterwayRoutePlannerResponseDirectionFeatureStart.attributes.maneuverType = "esriDMTDepart";
                waterwayRoutePlannerResponseDirectionFeatureStart.strings.Add(waterwayRoutePlannerResponseDirectionFeatureStringStart);
                waterwayRoutePlannerResponseDirection.features.Add(waterwayRoutePlannerResponseDirectionFeatureStart);

                WaterwayRoutePlannerResponseDirectionSummaryEnvelope waterwayRoutePlannerResponseDirectionSummaryEnvelope = new WaterwayRoutePlannerResponseDirectionSummaryEnvelope();
                if (startCoordinate[0] >= goalCoordinate[0])
                {
                    waterwayRoutePlannerResponseDirectionSummaryEnvelope.xmax = startCoordinate[0];
                    waterwayRoutePlannerResponseDirectionSummaryEnvelope.xmin = goalCoordinate[0];
                }
                else
                {
                    waterwayRoutePlannerResponseDirectionSummaryEnvelope.xmax = goalCoordinate[0];
                    waterwayRoutePlannerResponseDirectionSummaryEnvelope.xmin = startCoordinate[0];
                }
                if (startCoordinate[1] >= goalCoordinate[1])
                {
                    waterwayRoutePlannerResponseDirectionSummaryEnvelope.ymax = startCoordinate[1];
                    waterwayRoutePlannerResponseDirectionSummaryEnvelope.ymin = goalCoordinate[1];
                }
                else
                {
                    waterwayRoutePlannerResponseDirectionSummaryEnvelope.ymax = goalCoordinate[1];
                    waterwayRoutePlannerResponseDirectionSummaryEnvelope.ymin = startCoordinate[1];
                }

                waterwayRoutePlannerResponseDirection.routeId = "1";
                waterwayRoutePlannerResponseDirection.routeName = waterwayRoutePlannerResponseRoutesFeature.attributes.Name;
                waterwayRoutePlannerResponseDirection.summary.totalLength = waterwayRoutePlannerResponseRoutesFeature.attributes.Shape_Length;
                waterwayRoutePlannerResponseDirection.summary.totalTime = "3660.0";
                waterwayRoutePlannerResponseDirection.summary.totalDriveTime = waterwayRoutePlannerResponseRoutesFeature.attributes.Total_TravelTime;


                int routeResultsStackCount = routeResultsStack.Count;
                var lastRouteTraceNodeCoordinate = startCoordinate;
                var lastDirectionTraceNodeID = start;
                double directionAttributesLengthSum = 0.0;

                for (var i = 0; i < routeResultsStackCount; i += 2)
                {
                    var currentRouteLinkResult = routeResultsStack.Pop();
                    var currentRouteLinkObject = waterwayGraph.m_dicWaterwayLink[currentRouteLinkResult];
                    var currentRouteLinkCoordinatesObject = currentRouteLinkObject.channelGeometry;
                    var currentRouteLinkCoordinatesObjectLength = currentRouteLinkCoordinatesObject.Length;
                    waterwayRoutePlannerResponseRoutesFeatureGeometryPath.Add(lastRouteTraceNodeCoordinate);
                    if (IsSameCoordinate(lastRouteTraceNodeCoordinate, new double[2] { currentRouteLinkCoordinatesObject[0].X, currentRouteLinkCoordinatesObject[0].Y }))
                    {
                        for (var j = 1; j < currentRouteLinkCoordinatesObjectLength - 1; j++)
                        {
                            waterwayRoutePlannerResponseRoutesFeatureGeometryPath.Add(new double[2] { currentRouteLinkCoordinatesObject[j].X, currentRouteLinkCoordinatesObject[j].Y });
                            if (currentRouteLinkCoordinatesObject[j].X > waterwayRoutePlannerResponseDirectionSummaryEnvelope.xmax)
                            {
                                waterwayRoutePlannerResponseDirectionSummaryEnvelope.xmax = currentRouteLinkCoordinatesObject[j].X;
                            }
                            if (currentRouteLinkCoordinatesObject[j].X < waterwayRoutePlannerResponseDirectionSummaryEnvelope.xmin)
                            {
                                waterwayRoutePlannerResponseDirectionSummaryEnvelope.xmin = currentRouteLinkCoordinatesObject[j].X;
                            }
                            if (currentRouteLinkCoordinatesObject[j].Y > waterwayRoutePlannerResponseDirectionSummaryEnvelope.ymax)
                            {
                                waterwayRoutePlannerResponseDirectionSummaryEnvelope.ymax = currentRouteLinkCoordinatesObject[j].Y;
                            }
                            if (currentRouteLinkCoordinatesObject[j].Y < waterwayRoutePlannerResponseDirectionSummaryEnvelope.ymin)
                            {
                                waterwayRoutePlannerResponseDirectionSummaryEnvelope.ymin = currentRouteLinkCoordinatesObject[j].Y;
                            }
                        }
                        lastRouteTraceNodeCoordinate = new double[2] { currentRouteLinkCoordinatesObject[currentRouteLinkCoordinatesObjectLength - 1].X, currentRouteLinkCoordinatesObject[currentRouteLinkCoordinatesObjectLength - 1].Y };
                        if (lastRouteTraceNodeCoordinate[0] > waterwayRoutePlannerResponseDirectionSummaryEnvelope.xmax)
                        {
                            waterwayRoutePlannerResponseDirectionSummaryEnvelope.xmax = lastRouteTraceNodeCoordinate[0];
                        }
                        if (lastRouteTraceNodeCoordinate[0] < waterwayRoutePlannerResponseDirectionSummaryEnvelope.xmin)
                        {
                            waterwayRoutePlannerResponseDirectionSummaryEnvelope.xmin = lastRouteTraceNodeCoordinate[0];
                        }
                        if (lastRouteTraceNodeCoordinate[1] > waterwayRoutePlannerResponseDirectionSummaryEnvelope.ymax)
                        {
                            waterwayRoutePlannerResponseDirectionSummaryEnvelope.ymax = lastRouteTraceNodeCoordinate[1];
                        }
                        if (lastRouteTraceNodeCoordinate[1] < waterwayRoutePlannerResponseDirectionSummaryEnvelope.ymin)
                        {
                            waterwayRoutePlannerResponseDirectionSummaryEnvelope.ymin = lastRouteTraceNodeCoordinate[1];
                        }
                    }
                    else
                    {
                        for (var j = currentRouteLinkCoordinatesObjectLength - 2; j > 0; j--)
                        {
                            waterwayRoutePlannerResponseRoutesFeatureGeometryPath.Add(new double[2] { currentRouteLinkCoordinatesObject[j].X, currentRouteLinkCoordinatesObject[j].Y });
                        }
                        lastRouteTraceNodeCoordinate = new double[2] { currentRouteLinkCoordinatesObject[0].X, currentRouteLinkCoordinatesObject[0].Y };
                    }

                    directionAttributesLengthSum += waterwayGraph.m_dicWaterwayLink[currentRouteLinkResult].channelLength;

                    var currentRouteNodeResult = routeResultsStack.Pop();
                    if (waterwayGraph.m_dicWaterwayNode[currentRouteNodeResult].waterNodeType != 3)
                    {
                        WaterwayRoutePlannerResponseDirectionFeature waterwayRoutePlannerResponseDirectionFeatureTemp = new WaterwayRoutePlannerResponseDirectionFeature();
                        WaterwayRoutePlannerResponseDirectionFeatureString waterwayRoutePlannerResponseDirectionFeatureStringTemp = new WaterwayRoutePlannerResponseDirectionFeatureString();
                        waterwayRoutePlannerResponseDirectionFeatureTemp.attributes.length = directionAttributesLengthSum;
                        waterwayRoutePlannerResponseDirectionFeatureTemp.attributes.time = "0";
                        waterwayRoutePlannerResponseDirectionFeatureTemp.attributes.text = waterwayGraph.m_dicWaterwayNode[lastDirectionTraceNodeID].waterNodeName + " -- " + waterwayGraph.m_dicWaterwayNode[currentRouteNodeResult].waterNodeName;
                        waterwayRoutePlannerResponseDirectionFeatureTemp.attributes.ETA = "28800000";
                        waterwayRoutePlannerResponseDirectionFeatureTemp.attributes.maneuverType = "esriDMTStraight";
                        waterwayRoutePlannerResponseDirectionFeatureTemp.strings.Add(waterwayRoutePlannerResponseDirectionFeatureStringTemp);
                        waterwayRoutePlannerResponseDirection.features.Add(waterwayRoutePlannerResponseDirectionFeatureTemp);

                        directionAttributesLengthSum = 0;
                    }


                }
                WaterwayRoutePlannerResponseDirectionFeature waterwayRoutePlannerResponseDirectionFeatureStop = new WaterwayRoutePlannerResponseDirectionFeature();
                WaterwayRoutePlannerResponseDirectionFeatureString waterwayRoutePlannerResponseDirectionFeatureStringStop = new WaterwayRoutePlannerResponseDirectionFeatureString();
                waterwayRoutePlannerResponseDirectionFeatureStop.attributes.length = 0;
                waterwayRoutePlannerResponseDirectionFeatureStop.attributes.time = "0";
                waterwayRoutePlannerResponseDirectionFeatureStop.attributes.text = "Finish at " + waterwayGraph.m_dicWaterwayNode[goal].waterNodeName;
                waterwayRoutePlannerResponseDirectionFeatureStop.attributes.ETA = "72000000";
                waterwayRoutePlannerResponseDirectionFeatureStop.attributes.maneuverType = "esriDMTStop";
                waterwayRoutePlannerResponseDirectionFeatureStop.strings.Add(waterwayRoutePlannerResponseDirectionFeatureStringStop);
                waterwayRoutePlannerResponseDirection.features.Add(waterwayRoutePlannerResponseDirectionFeatureStop);


                waterwayRoutePlannerResponseRoutesFeature.geometry.paths.Add(waterwayRoutePlannerResponseRoutesFeatureGeometryPath);
                waterwayRoutePlannerResponseRoutes.features.Add(waterwayRoutePlannerResponseRoutesFeature);

                waterwayRoutePlannerResponseDirection.summary.envelope = waterwayRoutePlannerResponseDirectionSummaryEnvelope;
                waterwayRoutePlannerResponse.directions.Add(waterwayRoutePlannerResponseDirection);
                waterwayRoutePlannerResponse.routes = waterwayRoutePlannerResponseRoutes;

                return waterwayRoutePlannerResponse;
            }
            else
            {
                WaterwayRoutePlannerResponse waterwayRoutePlannerResponse = new WaterwayRoutePlannerResponse();
                waterwayRoutePlannerResponse.messages[0] = "Can not reach the destination ...";

                return waterwayRoutePlannerResponse;
            }
        }
    }

    public class WaterwayRoutePlannerResponseRoutes
    {
        public WaterwayRoutePlannerResponseRoutesFieldAliases fieldAliases { get; set; } = new WaterwayRoutePlannerResponseRoutesFieldAliases();
        public string geometryType { get; set; } = "esriGeometryPolyline";
        public WaterwayRoutePlannerResponseRoutesSpatialReference spatialReference { get; set; } = new WaterwayRoutePlannerResponseRoutesSpatialReference();
        public List<WaterwayRoutePlannerResponseRoutesFeature> features { get; set; } = new List<WaterwayRoutePlannerResponseRoutesFeature>();

        public WaterwayRoutePlannerResponseRoutes()
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }
    }


    public class WaterwayRoutePlannerResponseRoutesFieldAliases
    {
        public string ObjectID { get; set; } = "ObjectID";
        public string Name { get; set; } = "Name";
        public string FirstStopID { get; set; } = "FirstStopID";
        public string LastStopID { get; set; } = "LastStopID";
        public string StopCount { get; set; } = "StopCount";
        public string Total_TravelTime { get; set; } = "Total_TravelTime";
        public string StartTime { get; set; } = "StartTime";
        public string EndTime { get; set; } = "EndTime";
        public string StartTimeUTC { get; set; } = "StartTimeUTC";
        public string EndTimeUTC { get; set; } = "EndTimeUTC";
        public string Shape_Length { get; set; } = "Shape_Length";

        public WaterwayRoutePlannerResponseRoutesFieldAliases()
        {

        }
    }


    public class WaterwayRoutePlannerResponseRoutesSpatialReference
    {
        public string wkid { get; set; } = "4326";
        public string latestWkid { get; set; } = "4326";

        public WaterwayRoutePlannerResponseRoutesSpatialReference()
        {

        }
    }


    public class WaterwayRoutePlannerResponseRoutesFeature
    {
        public WaterwayRoutePlannerResponseRoutesFieldAliases attributes { get; set; } = new WaterwayRoutePlannerResponseRoutesFieldAliases();
        public WaterwayRoutePlannerResponseRoutesFeatureGeometry geometry { get; set; } = new WaterwayRoutePlannerResponseRoutesFeatureGeometry();

        public WaterwayRoutePlannerResponseRoutesFeature()
        {

        }
    }


    public class WaterwayRoutePlannerResponseRoutesFeatureGeometry
    {
        public List<List<double[]>> paths { get; set; } = new List<List<double[]>>();

        public WaterwayRoutePlannerResponseRoutesFeatureGeometry()
        {

        }
    }


    public class WaterwayRoutePlannerResponseDirection
    {
        public string routeId { get; set; }
        public string routeName { get; set; }
        public WaterwayRoutePlannerResponseDirectionSummary summary { get; set; } = new WaterwayRoutePlannerResponseDirectionSummary();
        public List<WaterwayRoutePlannerResponseDirectionFeature> features { get; set; } = new List<WaterwayRoutePlannerResponseDirectionFeature>();

        public WaterwayRoutePlannerResponseDirection()
        {

        }
    }

    public class WaterwayRoutePlannerResponseDirectionSummary
    {
        public string totalLength { get; set; }
        public string totalTime { get; set; }
        public string totalDriveTime { get; set; }
        public WaterwayRoutePlannerResponseDirectionSummaryEnvelope envelope { get; set; } = new WaterwayRoutePlannerResponseDirectionSummaryEnvelope();

        public WaterwayRoutePlannerResponseDirectionSummary()
        {

        }
    }

    public class WaterwayRoutePlannerResponseDirectionSummaryEnvelope
    {
        public double xmin { get; set; }
        public double ymin { get; set; }
        public double xmax { get; set; }
        public double ymax { get; set; }
        public WaterwayRoutePlannerResponseRoutesSpatialReference spatialReference { get; set; } = new WaterwayRoutePlannerResponseRoutesSpatialReference();

        public WaterwayRoutePlannerResponseDirectionSummaryEnvelope()
        {

        }
    }

    public class WaterwayRoutePlannerResponseDirectionFeature
    {
        public WaterwayRoutePlannerResponseDirectionFeatureAttributes attributes { get; set; } = new WaterwayRoutePlannerResponseDirectionFeatureAttributes();
        public string compressedGeometry { get; set; } = "";
        public List<WaterwayRoutePlannerResponseDirectionFeatureString> strings { get; set; } = new List<WaterwayRoutePlannerResponseDirectionFeatureString>();

        public WaterwayRoutePlannerResponseDirectionFeature()
        {

        }
    }

    public class WaterwayRoutePlannerResponseDirectionFeatureAttributes
    {
        public double length { get; set; }
        public string time { get; set; }
        public string text { get; set; }
        public string ETA { get; set; }
        public string arriveTimeUTC { get; set; }
        public string maneuverType { get; set; }

        public WaterwayRoutePlannerResponseDirectionFeatureAttributes()
        {

        }
    }

    public class WaterwayRoutePlannerResponseDirectionFeatureString
    {
        public string @string { get; set; } = "12:00 AM";
        public string stringType { get; set; } = "esriDSTEstimatedArrivalTime";

        public WaterwayRoutePlannerResponseDirectionFeatureString()
        {

        }
    }    
}