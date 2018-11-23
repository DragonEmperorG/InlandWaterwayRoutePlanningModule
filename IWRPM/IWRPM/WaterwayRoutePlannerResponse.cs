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
        public static readonly double epsilon = 0.000001;
        public static readonly double EARTH_RADIUS = 6371004.0;

        public List<string> messages { get; set; } = new List<string>();
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
            double diffrence = Math.Abs((_traceCoordinate[0] - _comparedCoordinate[0]) + (_traceCoordinate[1] - _comparedCoordinate[1]));
            if (diffrence < epsilon)
                isSameCoordinate = true;

            return isSameCoordinate;
        }

        static double Rad(double d)
        {
            return d * Math.PI / 180.0;
        }

        static double Degree(double r)
        {
            return r * 180.0 / Math.PI;
        }

        static string GetDirection(double _azimuth)
        {
            string direction = "北";

            if ((_azimuth > 10) && (_azimuth <= 80))
                direction = "东北";
            if ((_azimuth > 80) && (_azimuth <= 100))
                direction = "东";
            if ((_azimuth > 100) && (_azimuth <= 170))
                direction = "东南";
            if ((_azimuth > 170) && (_azimuth <= 190))
                direction = "南";
            if ((_azimuth > 190) && (_azimuth <= 260))
                direction = "西南";
            if ((_azimuth > 260) && (_azimuth <= 280))
                direction = "西";
            if ((_azimuth > 280) && (_azimuth <= 350))
                direction = "西北";

            return direction;
        }

        static double GetAzimuth(double[] _startCoordinate, double[] _targetCoordinate)
        {
            double azimuth = 0.0;

            var _startLongitudeRad = Rad(_startCoordinate[0]);
            var _startLatitudeRad = Rad(_startCoordinate[1]);
            var _targetLongitudeRad = Rad(_targetCoordinate[0]);
            var _targetLatitudeRad = Rad(_targetCoordinate[1]);

            if (_startLongitudeRad == _targetLongitudeRad)
            {
                if (_targetLatitudeRad < _startLatitudeRad)
                {
                    azimuth = 180.0;
                }
            }
            else
            {
                var _cosC = Math.Cos(Rad(90.0 - _targetCoordinate[1])) * Math.Cos(Rad(90.0 - _startCoordinate[1])) + Math.Sin(Rad(90.0 - _targetCoordinate[1])) * Math.Sin(Rad(90.0 - _startCoordinate[1])) * Math.Cos(_targetLongitudeRad - _startLongitudeRad);
                var _sinC = Math.Sqrt(1.0 - _cosC * _cosC);
                var azimuthRad = Math.Asin(Math.Sin(Rad(90.0 - _targetCoordinate[1])) * Math.Sin(_targetLongitudeRad - _startLongitudeRad) / _sinC);
                azimuth = Degree(azimuthRad);

                if (_startLatitudeRad > _targetLatitudeRad)
                {
                    azimuth = 180.0 - azimuth;
                }
                else
                {
                    if (_startLongitudeRad > _targetLongitudeRad)
                    {
                        azimuth = 360.0 + azimuth;
                    }
                }
            }            

            return azimuth;
        }

        static double GetSteeringAngle(double[] _beforeCoordinate, double[] _currentCoordinate, double[] _afterCoordinate)
        {
            double steeringAngle = 0.0;
            double azimuthB2C = GetAzimuth(_beforeCoordinate, _currentCoordinate);
            double azimuthC2A = GetAzimuth(_currentCoordinate, _afterCoordinate);
            steeringAngle = azimuthC2A - azimuthB2C;
            if (steeringAngle < 0.0)
            {
                steeringAngle = steeringAngle + 360.0;
            }

            return steeringAngle;
        }

        public double GetDistanceByCoordinate(double[] _startCoordinate, double[] _endCoordinate)
        {
            var startLongitudeRad = Rad(_startCoordinate[0]);
            var startLatitudeRad = Rad(_startCoordinate[1]);
            var endLongitudeRad = Rad(_endCoordinate[0]);
            var endLatitudeRad = Rad(_endCoordinate[1]);
            var distance = Math.Acos(Math.Sin(startLatitudeRad) * Math.Sin(endLatitudeRad) + Math.Cos(startLatitudeRad) * Math.Cos(endLatitudeRad) * Math.Cos(startLongitudeRad - endLongitudeRad));
            distance = distance * EARTH_RADIUS;
            return distance;
        }

        public WaterwayRoutePlannerResponse OutputRouteResults(WaterwayGraph waterwayGraph, WaterwayRoutePlanner channelRoutePlanner, string start, string goal)
        {
            if (channelRoutePlanner.isFindOptimalRoute)
            {
                Stack<string> routeResultsStack = new Stack<string>();
                int bridgeNumber = 0;
                int shipLockNumber = 0;
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
                    if (waterwayGraph.m_dicWaterwayLink[currentResearchLinkElement].lockNumber != 0)
                        shipLockNumber += waterwayGraph.m_dicWaterwayLink[currentResearchLinkElement].lockNumber;

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
                waterwayRoutePlannerResponseRoutesFeature.attributes.Bridge_Number = bridgeNumber.ToString();
                waterwayRoutePlannerResponseRoutesFeature.attributes.ShipLock_Number = shipLockNumber.ToString();

                WaterwayRoutePlannerResponseDirection waterwayRoutePlannerResponseDirection = new WaterwayRoutePlannerResponseDirection();
                WaterwayRoutePlannerResponseDirectionFeature waterwayRoutePlannerResponseDirectionFeatureStart = new WaterwayRoutePlannerResponseDirectionFeature();
                WaterwayRoutePlannerResponseDirectionFeatureString waterwayRoutePlannerResponseDirectionFeatureStringStart = new WaterwayRoutePlannerResponseDirectionFeatureString();
                waterwayRoutePlannerResponseDirectionFeatureStart.attributes.length = 0;
                waterwayRoutePlannerResponseDirectionFeatureStart.attributes.time = "0";
                waterwayRoutePlannerResponseDirectionFeatureStart.attributes.text = "Start at " + waterwayGraph.m_dicWaterwayNode[start].waterNodeName;
                waterwayRoutePlannerResponseDirectionFeatureStart.attributes.ETA = 0.0;
                waterwayRoutePlannerResponseDirectionFeatureStart.attributes.maneuverType = "esriDMTDepart";
                waterwayRoutePlannerResponseDirectionFeatureStart.geometry.type = "point";
                waterwayRoutePlannerResponseDirectionFeatureStart.geometry.geom.Add(startCoordinate);
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
                var lastDirectionTraceChannelName = "";
                int directionBridgeSum = 0;
                int directionShipLockSum = 0;
                double directionAttributesLengthSum = 0.0;
                List<string> currentSectionChannelNameList = new List<string>();
                List<string> currentSectionBridgeNameList = new List<string>();
                List<string> currentSectionShipLockNameList = new List<string>();
                var directionGeometry = new WaterwayRoutePlannerResponseDirectionFeatureGeometry();

                for (var i = 0; i < routeResultsStackCount; i += 2)
                {
                    var currentRouteLinkResult = routeResultsStack.Pop();


                    //if (currentRouteLinkResult == "DPSD2XNZ-9001-XJ2DZZ-0010")
                    //{
                    //    Console.WriteLine(currentRouteLinkResult);
                    //}


                    var currentRouteLinkObject = waterwayGraph.m_dicWaterwayLink[currentRouteLinkResult];
                    var currentRouteLinkCoordinatesObject = currentRouteLinkObject.channelGeometry;
                    var currentRouteLinkCoordinatesObjectLength = currentRouteLinkCoordinatesObject.Length;
                    var currentRouteLinkCoordinatesObjectChannelName = currentRouteLinkObject.channelName;
                    var currentRouteLinkCoordinatesObjectBridgeName = currentRouteLinkObject.bridgeName;
                    var currentRouteLinkCoordinatesObjectShipLockName = currentRouteLinkObject.lockName;
                    if (!currentSectionChannelNameList.Contains(currentRouteLinkCoordinatesObjectChannelName))
                    {
                        currentSectionChannelNameList.Add(currentRouteLinkCoordinatesObjectChannelName);
                    }
                    if (currentRouteLinkObject.bridgeNumber != 0)
                    {
                        directionBridgeSum = directionBridgeSum + currentRouteLinkObject.bridgeNumber;
                        if (!currentSectionBridgeNameList.Contains(currentRouteLinkCoordinatesObjectBridgeName))
                        {
                            currentSectionBridgeNameList.Add(currentRouteLinkCoordinatesObjectBridgeName);
                        }
                    }
                    if (currentRouteLinkObject.lockNumber != 0)
                    {
                        directionShipLockSum = directionShipLockSum + currentRouteLinkObject.lockNumber;
                        if (!currentSectionShipLockNameList.Contains(currentRouteLinkCoordinatesObjectShipLockName))
                        {
                            currentSectionShipLockNameList.Add(currentRouteLinkCoordinatesObjectShipLockName);
                        }
                    }

                    //处理该Section的几何属性
                    waterwayRoutePlannerResponseRoutesFeatureGeometryPath.Add(lastRouteTraceNodeCoordinate);
                    directionGeometry.geom.Add(lastRouteTraceNodeCoordinate);
                    if (IsSameCoordinate(lastRouteTraceNodeCoordinate, new double[2] { currentRouteLinkCoordinatesObject[0].X, currentRouteLinkCoordinatesObject[0].Y }))
                    {
                        for (var j = 1; j < currentRouteLinkCoordinatesObjectLength - 1; j++)
                        {
                            waterwayRoutePlannerResponseRoutesFeatureGeometryPath.Add(new double[2] { currentRouteLinkCoordinatesObject[j].X, currentRouteLinkCoordinatesObject[j].Y });
                            directionGeometry.geom.Add(new double[2] { currentRouteLinkCoordinatesObject[j].X, currentRouteLinkCoordinatesObject[j].Y });
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
                            directionGeometry.geom.Add(new double[2] { currentRouteLinkCoordinatesObject[j].X, currentRouteLinkCoordinatesObject[j].Y });
                        }
                        lastRouteTraceNodeCoordinate = new double[2] { currentRouteLinkCoordinatesObject[0].X, currentRouteLinkCoordinatesObject[0].Y };
                    }

                    //处理该Section的线路长度属性
                    directionAttributesLengthSum += waterwayGraph.m_dicWaterwayLink[currentRouteLinkResult].channelLength;

                    var currentRouteNodeResult = routeResultsStack.Pop();
                    if (waterwayGraph.m_dicWaterwayNode[currentRouteNodeResult].waterNodeType != 3)
                    {
                        var lastDirectionTraceWaterwayNode = waterwayGraph.m_dicWaterwayNode[lastDirectionTraceNodeID];
                        var currentDirectionTraceWaterwayNode = waterwayGraph.m_dicWaterwayNode[currentRouteNodeResult];


                        WaterwayRoutePlannerResponseDirectionFeature waterwayRoutePlannerResponseDirectionFeatureTemp = new WaterwayRoutePlannerResponseDirectionFeature();                        
                        waterwayRoutePlannerResponseDirectionFeatureTemp.attributes.length = directionAttributesLengthSum;
                        waterwayRoutePlannerResponseDirectionFeatureTemp.attributes.time = "0";                        
                        waterwayRoutePlannerResponseDirectionFeatureTemp.attributes.maneuverType = "esriDMTStraight";
                        directionGeometry.geom.Add(lastRouteTraceNodeCoordinate);

                        WaterwayRoutePlannerResponseDirectionFeatureString waterwayRoutePlannerResponseDirectionFeatureStringTemp = new WaterwayRoutePlannerResponseDirectionFeatureString();
                        waterwayRoutePlannerResponseDirectionFeatureStringTemp.stringType = "1";
                        waterwayRoutePlannerResponseDirectionFeatureStringTemp.@string = currentSectionChannelNameList[0];

                        if ((lastDirectionTraceWaterwayNode.waterNodeType == 4) && (currentDirectionTraceWaterwayNode.waterNodeType == 4) && (directionBridgeSum >= 1))
                        {
                            waterwayRoutePlannerResponseDirectionFeatureStringTemp.stringType = "4";
                            if (currentSectionBridgeNameList.Count == 0)
                            {
                                waterwayRoutePlannerResponseDirectionFeatureStringTemp.@string = "桥梁名称数据缺失";
                            }
                            else
                            {
                                waterwayRoutePlannerResponseDirectionFeatureStringTemp.@string = currentSectionBridgeNameList[0];
                            }
                            var waterwayRoutePlannerResponseDirectionFeatureStringTempGeomLongitude = (directionGeometry.geom[0][0] + lastRouteTraceNodeCoordinate[0]) / 2.0;
                            var waterwayRoutePlannerResponseDirectionFeatureStringTempGeomLaitude = (directionGeometry.geom[0][1] + lastRouteTraceNodeCoordinate[1]) / 2.0;
                            waterwayRoutePlannerResponseDirectionFeatureStringTemp.geom = new double[2] { waterwayRoutePlannerResponseDirectionFeatureStringTempGeomLongitude, waterwayRoutePlannerResponseDirectionFeatureStringTempGeomLaitude };
                            waterwayRoutePlannerResponseDirectionFeatureStringTemp.length = GetDistanceByCoordinate(directionGeometry.geom[0], waterwayRoutePlannerResponseDirectionFeatureStringTemp.geom);
                        }

                        if ((lastDirectionTraceWaterwayNode.waterNodeType == 5) && (currentDirectionTraceWaterwayNode.waterNodeType == 5) && (directionShipLockSum >= 1))
                        {
                            waterwayRoutePlannerResponseDirectionFeatureStringTemp.stringType = "5";
                            if (currentSectionShipLockNameList.Count == 0)
                            {
                                waterwayRoutePlannerResponseDirectionFeatureStringTemp.@string = "船闸名称数据缺失";
                            }
                            else
                            {
                                waterwayRoutePlannerResponseDirectionFeatureStringTemp.@string = currentSectionShipLockNameList[0];
                            }
                        }

                        if (lastDirectionTraceWaterwayNode.waterNodeType == 2 || (lastDirectionTraceWaterwayNode.waterNodeType == 1 && lastDirectionTraceWaterwayNode.waterLinkOutNumber >= 3))
                        {
                            var lastDirectionFeature = waterwayRoutePlannerResponseDirection.features[waterwayRoutePlannerResponseDirection.features.Count - 1];

                            waterwayRoutePlannerResponseDirectionFeatureStringTemp.stringType = "2";
                            waterwayRoutePlannerResponseDirectionFeatureStringTemp.@string = lastDirectionTraceChannelName + ',' + currentSectionChannelNameList[0];
                        }
                        waterwayRoutePlannerResponseDirectionFeatureTemp.strings.Add(waterwayRoutePlannerResponseDirectionFeatureStringTemp);                        
                        
                        //记录该段feature的方位角值
                        waterwayRoutePlannerResponseDirectionFeatureTemp.attributes.ETA = GetAzimuth(directionGeometry.geom[0], directionGeometry.geom[directionGeometry.geom.Count - 1]);

                        waterwayRoutePlannerResponseDirectionFeatureTemp.attributes.text = GetDirection(waterwayRoutePlannerResponseDirectionFeatureTemp.attributes.ETA);
                        if (waterwayRoutePlannerResponseDirection.features.Count >= 2)
                        {
                            //记录该段feature起始点的转向角
                            var lastDirectionFeature = waterwayRoutePlannerResponseDirection.features[waterwayRoutePlannerResponseDirection.features.Count - 1];
                            var lastDirectionFeatureGemetrySecondLastCoordinate = lastDirectionFeature.geometry.geom[lastDirectionFeature.geometry.geom.Count - 2];
                            var steeringAngle = GetSteeringAngle(lastDirectionFeatureGemetrySecondLastCoordinate, directionGeometry.geom[0], directionGeometry.geom[1]);
                            waterwayRoutePlannerResponseDirectionFeatureTemp.attributes.text = steeringAngle.ToString();
                        }

                        waterwayRoutePlannerResponseDirectionFeatureTemp.geometry = directionGeometry;
                        waterwayRoutePlannerResponseDirection.features.Add(waterwayRoutePlannerResponseDirectionFeatureTemp);

                        //重置direction.feature记录变量
                        directionAttributesLengthSum = 0;
                        directionGeometry = WaterwayRoutePlannerResponseDirectionFeatureGeometry.ConstructNewInstanceFromExistClass(new WaterwayRoutePlannerResponseDirectionFeatureGeometry());
                        lastDirectionTraceNodeID = currentRouteNodeResult;
                        lastDirectionTraceChannelName = currentSectionChannelNameList[0];
                        directionBridgeSum = 0;
                        directionShipLockSum = 0;
                        currentSectionChannelNameList.Clear();
                        currentSectionBridgeNameList.Clear();
                        currentSectionShipLockNameList.Clear();
                    }


                }
                WaterwayRoutePlannerResponseDirectionFeature waterwayRoutePlannerResponseDirectionFeatureStop = new WaterwayRoutePlannerResponseDirectionFeature();
                WaterwayRoutePlannerResponseDirectionFeatureString waterwayRoutePlannerResponseDirectionFeatureStringStop = new WaterwayRoutePlannerResponseDirectionFeatureString();
                waterwayRoutePlannerResponseDirectionFeatureStop.attributes.length = 0;
                waterwayRoutePlannerResponseDirectionFeatureStop.attributes.time = "0";
                waterwayRoutePlannerResponseDirectionFeatureStop.attributes.text = "Finish at " + waterwayGraph.m_dicWaterwayNode[goal].waterNodeName;
                waterwayRoutePlannerResponseDirectionFeatureStop.attributes.ETA = 0.0;
                waterwayRoutePlannerResponseDirectionFeatureStop.attributes.maneuverType = "esriDMTStop";
                waterwayRoutePlannerResponseDirectionFeatureStop.strings.Add(waterwayRoutePlannerResponseDirectionFeatureStringStop);
                waterwayRoutePlannerResponseDirectionFeatureStop.geometry.type = "point";
                waterwayRoutePlannerResponseDirectionFeatureStop.geometry.geom.Add(waterwayGraph.m_dicWaterwayNode[goal].waterNodeCoordinate);
                waterwayRoutePlannerResponseDirection.features.Add(waterwayRoutePlannerResponseDirectionFeatureStop);

                waterwayRoutePlannerResponseRoutesFeatureGeometryPath.Add(lastRouteTraceNodeCoordinate);
                waterwayRoutePlannerResponseRoutesFeature.geometry.paths.Add(waterwayRoutePlannerResponseRoutesFeatureGeometryPath);
                waterwayRoutePlannerResponseRoutes.features.Add(waterwayRoutePlannerResponseRoutesFeature);


                // 临时返回测试用备选方案测试数据
                //var waterwayRoutePlannerResponseRoutesFeatureAlternative = new WaterwayRoutePlannerResponseRoutesFeature();
                //List<double[]> waterwayRoutePlannerResponseRoutesFeatureAlternativeGeometryPath = new List<double[]>();
                //waterwayRoutePlannerResponseRoutesFeatureAlternative.attributes.ObjectID = "2";
                //waterwayRoutePlannerResponseRoutesFeatureAlternative.attributes.Name = "测试备选规划路线";
                //waterwayRoutePlannerResponseRoutesFeatureAlternative.attributes.FirstStopID = "测试起始点";
                //waterwayRoutePlannerResponseRoutesFeatureAlternative.attributes.LastStopID = "测试目标点";
                //waterwayRoutePlannerResponseRoutesFeatureAlternative.attributes.StopCount = "8";
                //waterwayRoutePlannerResponseRoutesFeatureAlternative.attributes.Total_TravelTime = "3600.0";
                //waterwayRoutePlannerResponseRoutesFeatureAlternative.attributes.StartTime = "0";
                //waterwayRoutePlannerResponseRoutesFeatureAlternative.attributes.EndTime = "360000";
                //waterwayRoutePlannerResponseRoutesFeatureAlternative.attributes.StartTimeUTC = "30000000";
                //waterwayRoutePlannerResponseRoutesFeatureAlternative.attributes.EndTimeUTC = "30360000";
                //waterwayRoutePlannerResponseRoutesFeatureAlternative.attributes.Shape_Length = "100000";
                //waterwayRoutePlannerResponseRoutesFeatureAlternative.attributes.Bridge_Number = "10";
                //waterwayRoutePlannerResponseRoutesFeatureAlternative.attributes.ShipLock_Number = "5";
                //waterwayRoutePlannerResponseRoutesFeatureAlternativeGeometryPath.Add(startCoordinate);
                //waterwayRoutePlannerResponseRoutesFeatureAlternativeGeometryPath.Add(goalCoordinate);
                //waterwayRoutePlannerResponseRoutesFeatureAlternative.geometry.paths.Add(waterwayRoutePlannerResponseRoutesFeatureAlternativeGeometryPath);
                //waterwayRoutePlannerResponseRoutes.features.Add(waterwayRoutePlannerResponseRoutesFeatureAlternative);

                //WaterwayRoutePlannerResponseDirection waterwayRoutePlannerResponseDirectionAlternative = new WaterwayRoutePlannerResponseDirection();
                //waterwayRoutePlannerResponseDirectionAlternative.routeId = waterwayRoutePlannerResponseRoutesFeatureAlternative.attributes.ObjectID;
                //waterwayRoutePlannerResponseDirectionAlternative.routeName = waterwayRoutePlannerResponseRoutesFeatureAlternative.attributes.Name;
                //waterwayRoutePlannerResponseDirectionAlternative.summary.totalLength = waterwayRoutePlannerResponseRoutesFeatureAlternative.attributes.Shape_Length;
                //waterwayRoutePlannerResponseDirectionAlternative.summary.totalTime = waterwayRoutePlannerResponseRoutesFeatureAlternative.attributes.Total_TravelTime;
                //waterwayRoutePlannerResponseDirection.summary.totalDriveTime = waterwayRoutePlannerResponseRoutesFeature.attributes.Total_TravelTime;

                //WaterwayRoutePlannerResponseDirectionFeature waterwayRoutePlannerResponseDirectionAlternativeFeatureStart = new WaterwayRoutePlannerResponseDirectionFeature();
                //WaterwayRoutePlannerResponseDirectionFeatureString waterwayRoutePlannerResponseDirectionAlternativeFeatureStringStart = new WaterwayRoutePlannerResponseDirectionFeatureString();
                //waterwayRoutePlannerResponseDirectionAlternativeFeatureStart.attributes.length = 0;
                //waterwayRoutePlannerResponseDirectionAlternativeFeatureStart.attributes.time = "0";
                //waterwayRoutePlannerResponseDirectionAlternativeFeatureStart.attributes.text = "Start at 测试起始点";
                //waterwayRoutePlannerResponseDirectionAlternativeFeatureStart.attributes.ETA = 0.0;
                //waterwayRoutePlannerResponseDirectionAlternativeFeatureStart.attributes.maneuverType = "esriDMTDepart";
                //waterwayRoutePlannerResponseDirectionAlternativeFeatureStart.geometry.type = "point";
                //waterwayRoutePlannerResponseDirectionAlternativeFeatureStart.geometry.geom.Add(startCoordinate);
                //waterwayRoutePlannerResponseDirectionAlternativeFeatureStart.strings.Add(waterwayRoutePlannerResponseDirectionAlternativeFeatureStringStart);
                //waterwayRoutePlannerResponseDirectionAlternative.features.Add(waterwayRoutePlannerResponseDirectionAlternativeFeatureStart);

                //WaterwayRoutePlannerResponseDirectionFeature waterwayRoutePlannerResponseDirectionAlternativeFeatureDirectConnection = new WaterwayRoutePlannerResponseDirectionFeature();
                //WaterwayRoutePlannerResponseDirectionFeatureString waterwayRoutePlannerResponseDirectionAlternativeFeatureStringDirectConnection = new WaterwayRoutePlannerResponseDirectionFeatureString();
                //waterwayRoutePlannerResponseDirectionAlternativeFeatureDirectConnection.attributes.length = 10;
                //waterwayRoutePlannerResponseDirectionAlternativeFeatureDirectConnection.attributes.time = "10";
                //waterwayRoutePlannerResponseDirectionAlternativeFeatureDirectConnection.attributes.text = "End at 测试目标点";
                //waterwayRoutePlannerResponseDirectionAlternativeFeatureDirectConnection.attributes.ETA = 0.0;
                //waterwayRoutePlannerResponseDirectionAlternativeFeatureDirectConnection.attributes.maneuverType = "esriDMTStop";
                //waterwayRoutePlannerResponseDirectionAlternativeFeatureDirectConnection.geometry.type = "polyline";
                //waterwayRoutePlannerResponseDirectionAlternativeFeatureDirectConnection.geometry.geom.Add(startCoordinate);
                //waterwayRoutePlannerResponseDirectionAlternativeFeatureDirectConnection.geometry.geom.Add(goalCoordinate);
                //waterwayRoutePlannerResponseDirectionAlternativeFeatureDirectConnection.strings.Add(waterwayRoutePlannerResponseDirectionAlternativeFeatureStringDirectConnection);
                //waterwayRoutePlannerResponseDirectionAlternative.features.Add(waterwayRoutePlannerResponseDirectionAlternativeFeatureDirectConnection);

                //WaterwayRoutePlannerResponseDirectionFeature waterwayRoutePlannerResponseDirectionAlternativeFeatureGoal = new WaterwayRoutePlannerResponseDirectionFeature();
                //WaterwayRoutePlannerResponseDirectionFeatureString waterwayRoutePlannerResponseDirectionAlternativeFeatureStringGoal = new WaterwayRoutePlannerResponseDirectionFeatureString();
                //waterwayRoutePlannerResponseDirectionAlternativeFeatureGoal.attributes.length = 10;
                //waterwayRoutePlannerResponseDirectionAlternativeFeatureGoal.attributes.time = "10";
                //waterwayRoutePlannerResponseDirectionAlternativeFeatureGoal.attributes.text = "End at 测试目标点";
                //waterwayRoutePlannerResponseDirectionAlternativeFeatureGoal.attributes.ETA = 0.0;
                //waterwayRoutePlannerResponseDirectionAlternativeFeatureGoal.attributes.maneuverType = "esriDMTStop";
                //waterwayRoutePlannerResponseDirectionAlternativeFeatureGoal.geometry.type = "point";
                //waterwayRoutePlannerResponseDirectionAlternativeFeatureGoal.geometry.geom.Add(goalCoordinate);
                //waterwayRoutePlannerResponseDirectionAlternativeFeatureGoal.strings.Add(waterwayRoutePlannerResponseDirectionAlternativeFeatureStringGoal);
                //waterwayRoutePlannerResponseDirectionAlternative.features.Add(waterwayRoutePlannerResponseDirectionAlternativeFeatureGoal);





                waterwayRoutePlannerResponseDirection.summary.envelope = waterwayRoutePlannerResponseDirectionSummaryEnvelope;
                waterwayRoutePlannerResponse.directions.Add(waterwayRoutePlannerResponseDirection);
                //waterwayRoutePlannerResponse.directions.Add(waterwayRoutePlannerResponseDirectionAlternative);
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
        public string Bridge_Number { get; set; } = 0.ToString();
        public string ShipLock_Number { get; set; } = 0.ToString();

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
        public WaterwayRoutePlannerResponseDirectionFeatureGeometry geometry { get; set; } = new WaterwayRoutePlannerResponseDirectionFeatureGeometry();

        public WaterwayRoutePlannerResponseDirectionFeature()
        {

        }
    }

    public class WaterwayRoutePlannerResponseDirectionFeatureAttributes
    {
        public double length { get; set; }
        public string time { get; set; }
        public string text { get; set; }
        public double ETA { get; set; }
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
        public double[] geom { get; set; }
        public double length { get; set; } = 0.0;

        public WaterwayRoutePlannerResponseDirectionFeatureString()
        {

        }
    }

    public class WaterwayRoutePlannerResponseDirectionFeatureGeometry
    {
        public string type { get; set; } = "polyline";
        public List<double[]> geom { get; set; } = new List<double[]>();

        public WaterwayRoutePlannerResponseDirectionFeatureGeometry()
        {

        }

        public static WaterwayRoutePlannerResponseDirectionFeatureGeometry ConstructNewInstanceFromExistClass(WaterwayRoutePlannerResponseDirectionFeatureGeometry _waterwayRoutePlannerResponseDirectionFeatureGeometry)
        {
            var waterwayRoutePlannerResponseDirectionFeatureGeometryNew = new WaterwayRoutePlannerResponseDirectionFeatureGeometry();
            waterwayRoutePlannerResponseDirectionFeatureGeometryNew.type = new string(_waterwayRoutePlannerResponseDirectionFeatureGeometry.type.ToArray());
            for (var i = 0; i < _waterwayRoutePlannerResponseDirectionFeatureGeometry.geom.Count; i++)
            {
                waterwayRoutePlannerResponseDirectionFeatureGeometryNew.geom.Add(new double[2] { _waterwayRoutePlannerResponseDirectionFeatureGeometry.geom[i][0], _waterwayRoutePlannerResponseDirectionFeatureGeometry.geom[i][1] });
            }

            return waterwayRoutePlannerResponseDirectionFeatureGeometryNew;
        }
    }
}