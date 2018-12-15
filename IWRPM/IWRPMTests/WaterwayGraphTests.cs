using Microsoft.VisualStudio.TestTools.UnitTesting;
using IWRPM;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IWRPM.Tests
{
    [TestClass()]
    public class WaterwayGraphTests
    {
        [TestMethod()]
        public void LoadWaterwayLinkDatasetsTest()
        {
            var waterwayGraphTest = new WaterwayGraph();

            var waterLinkShapefilePath = Path.Combine(waterwayGraphTest._shpfileDatasetsPath, "WaterwayLinkCGCS2000.shp");
            var waterwayLinkDictionaryTest = waterwayGraphTest.LoadWaterwayLinkDatasets(waterLinkShapefilePath);

            ShapefileDataReader waterwayLinkShapefileDataReader = new ShapefileDataReader(waterLinkShapefilePath, GeometryFactory.Default);
            DbaseFileHeader waterwayLinkDbaseFileHeader = waterwayLinkShapefileDataReader.DbaseHeader;

            Assert.AreEqual(waterwayLinkDictionaryTest.Count, waterwayLinkDbaseFileHeader.NumRecords);
        }

        [TestMethod()]
        public void WaterwayNodePropertySelfConfirmLITWLIContain()
        {
            var waterwayGraphTest = new WaterwayGraph();
            waterwayGraphTest.LoadWaterwayNetworkDatasets();

            var waterwayNodeDictionaryTest = waterwayGraphTest.m_dicWaterwayNode;
            var waterwayLinkDictionaryTest = waterwayGraphTest.m_dicWaterwayLink;

            var waterwayNodePropertyNotSelfConfirmList = new List<string>();
            foreach (var item in waterwayNodeDictionaryTest)
            {
                var waterwayNodeTemp = item.Value;
                foreach (var link in waterwayNodeTemp.waterLinkInList)
                {
                    if (!waterwayLinkDictionaryTest.ContainsKey(link))
                    {
                        waterwayNodePropertyNotSelfConfirmList.Add(waterwayNodeTemp.waterNodeID);
                    }
                }

            }

            Assert.AreEqual(waterwayNodePropertyNotSelfConfirmList.Count, 0);
        }

        [TestMethod()]
        public void WaterwayNodePropertySelfConfirmLITWLOContain()
        {
            var waterwayGraphTest = new WaterwayGraph();
            waterwayGraphTest.LoadWaterwayNetworkDatasets();

            var waterwayNodeDictionaryTest = waterwayGraphTest.m_dicWaterwayNode;
            var waterwayLinkDictionaryTest = waterwayGraphTest.m_dicWaterwayLink;

            var waterwayNodePropertyNotSelfConfirmList = new List<string>();
            foreach (var item in waterwayNodeDictionaryTest)
            {
                var waterwayNodeTemp = item.Value;
                foreach (var link in waterwayNodeTemp.waterLinkOutList)
                {
                    if (!waterwayLinkDictionaryTest.ContainsKey(link))
                    {
                        waterwayNodePropertyNotSelfConfirmList.Add(waterwayNodeTemp.waterNodeID);
                    }
                }

            }

            Assert.AreEqual(waterwayNodePropertyNotSelfConfirmList.Count, 0);
        }

        [TestMethod()]
        public void WaterwayNodePropertySelfConfirmNUMWLI2LITWLI()
        {
            var waterwayGraphTest = new WaterwayGraph();

            var waterLinkShapefilePath = Path.Combine(waterwayGraphTest._shpfileDatasetsPath, "waterwayNodeCGCS2000.shp");
            var waterwayNodeDictionaryTest = waterwayGraphTest.LoadWaterwayNodeDatasets(waterLinkShapefilePath);

            var waterwayNodePropertyNotSelfConfirmList = new List<string>();
            foreach (var item in waterwayNodeDictionaryTest)
            {
                var waterwayNodeTemp = item.Value;
                if (waterwayNodeTemp.waterLinkInNumber != waterwayNodeTemp.waterLinkInList.Length)
                {
                    waterwayNodePropertyNotSelfConfirmList.Add(waterwayNodeTemp.waterNodeID);
                }

            }

            Assert.AreEqual(waterwayNodePropertyNotSelfConfirmList.Count, 0);
        }

        [TestMethod()]
        public void WaterwayNodePropertySelfConfirmNUMWLO2LITWLO()
        {
            var waterwayGraphTest = new WaterwayGraph();

            var waterLinkShapefilePath = Path.Combine(waterwayGraphTest._shpfileDatasetsPath, "waterwayNodeCGCS2000.shp");
            var waterwayNodeDictionaryTest = waterwayGraphTest.LoadWaterwayNodeDatasets(waterLinkShapefilePath);

            var waterwayNodePropertyNotSelfConfirmList = new List<string>();
            foreach (var item in waterwayNodeDictionaryTest)
            {
                var waterwayNodeTemp = item.Value;
                if (waterwayNodeTemp.waterLinkOutNumber != waterwayNodeTemp.waterLinkOutList.Length)
                {
                    waterwayNodePropertyNotSelfConfirmList.Add(waterwayNodeTemp.waterNodeID);
                }

            }

            Assert.AreEqual(waterwayNodePropertyNotSelfConfirmList.Count, 0);
        }

        [TestMethod()]
        public void WaterwayLinkPropertySelfConfirmONEWAY2TRFWAW()
        {
            var waterwayGraphTest = new WaterwayGraph();

            var waterLinkShapefilePath = Path.Combine(waterwayGraphTest._shpfileDatasetsPath, "WaterwayLinkCGCS2000.shp");
            var waterwayLinkDictionaryTest = waterwayGraphTest.LoadWaterwayLinkDatasets(waterLinkShapefilePath);
            var onewayTrafficDirectionEnum = new int[2] { 1, -1 };

            var waterwayLinkPropertyNotSelfConfirmList = new List<string>();
            foreach (var item in waterwayLinkDictionaryTest)
            {
                var waterwayLinkTemp = item.Value;
                if (waterwayLinkTemp.oneWay == true)
                {
                    if (!onewayTrafficDirectionEnum.Contains(waterwayLinkTemp.trafficDirection))
                    {
                        waterwayLinkPropertyNotSelfConfirmList.Add(waterwayLinkTemp.waterLinkID);
                    }
                }

            }

            Assert.AreEqual(waterwayLinkPropertyNotSelfConfirmList.Count, 0);
        }

        [TestMethod()]
        public void WaterwayLinkPropertySelfConfirmUWNCODAndDWNCODContain()
        {
            var waterwayGraphTest = new WaterwayGraph();
            waterwayGraphTest.LoadWaterwayNetworkDatasets();

            var waterwayNodeDictionaryTest = waterwayGraphTest.m_dicWaterwayNode;
            var waterwayLinkDictionaryTest = waterwayGraphTest.m_dicWaterwayLink;

            var waterwayLinkPropertyNotSelfConfirmList = new List<string>();
            foreach (var item in waterwayLinkDictionaryTest)
            {
                var waterwayLinkTemp = item.Value;
                if (!waterwayNodeDictionaryTest.ContainsKey(waterwayLinkTemp.upStreamWaterNodeID) || !waterwayNodeDictionaryTest.ContainsKey(waterwayLinkTemp.downStreamWaterNodeID))
                {
                    waterwayLinkPropertyNotSelfConfirmList.Add(waterwayLinkTemp.waterLinkID);
                }

            }

            Assert.AreEqual(waterwayLinkPropertyNotSelfConfirmList.Count, 0);
        }

        [TestMethod()]
        public void WaterwayLinkPropertySelfConfirmBRGCODContain()
        {
            var waterwayGraphTest = new WaterwayGraph();
            waterwayGraphTest.LoadWaterwayNetworkDatasets();
            var waterwayRoutePlannerTest = new WaterwayRoutePlanner();
            var waterwayNodeDictionaryTest = waterwayGraphTest.m_dicWaterwayNode;
            var waterwayLinkDictionaryTest = waterwayGraphTest.m_dicWaterwayLink;

            var waterwayLinkPropertyNotSelfConfirmList = new List<string>();
            foreach (var item in waterwayLinkDictionaryTest)
            {
                var waterwayLinkTemp = item.Value;
                //if (waterwayLinkTemp.waterLinkID == "HTMSD-0013-HTMSD-0011")
                //{
                //    Console.WriteLine(waterwayLinkTemp.waterLinkID);
                //}
                if (waterwayLinkTemp.bridgeNumber >= 1)
                {
                    var bridgeReferenceList = waterwayLinkTemp.bridgeReference.Split(',');
                    foreach (var bridge in bridgeReferenceList)
                    {
                        if (!waterwayNodeDictionaryTest.ContainsKey(bridge))
                        {
                            waterwayLinkPropertyNotSelfConfirmList.Add(waterwayLinkTemp.waterLinkID);
                        }
                        else
                        {
                            var bridgeCoordinate = waterwayNodeDictionaryTest[bridge].waterNodeCoordinate;
                            var bridgeUpStreamCoordinate = waterwayNodeDictionaryTest[waterwayLinkTemp.upStreamWaterNodeID].waterNodeCoordinate;
                            var bridgeDownStreamCoordinate = waterwayNodeDictionaryTest[waterwayLinkTemp.downStreamWaterNodeID].waterNodeCoordinate;
                            var bridgeMidStreamCoordinateLongtitude = (bridgeUpStreamCoordinate[0] + bridgeDownStreamCoordinate[0]) / 2.0;
                            var bridgeMidStreamCoordinateLatitude = (bridgeUpStreamCoordinate[1] + bridgeDownStreamCoordinate[1]) / 2.0;
                            var bridgeMidStreamCoordinate = new double[2] { bridgeMidStreamCoordinateLongtitude, bridgeMidStreamCoordinateLatitude };

                            var distanceBridge2MidStream = waterwayRoutePlannerTest.GreatCircleDistance(bridgeCoordinate, bridgeMidStreamCoordinate);
                            var distanceUpStream2DownStream = waterwayRoutePlannerTest.GreatCircleDistance(bridgeUpStreamCoordinate, bridgeDownStreamCoordinate);

                            if (!((distanceBridge2MidStream*2)< distanceUpStream2DownStream))
                            {
                                waterwayLinkPropertyNotSelfConfirmList.Add(waterwayLinkTemp.waterLinkID);
                            }
                        }
                    }                    
                }

            }

            Assert.AreEqual(waterwayLinkPropertyNotSelfConfirmList.Count, 0);
        }

        [TestMethod()]
        public void WaterwayLink2NodeSpatialConsistency()
        {
            var waterwayGraphTest = new WaterwayGraph();
            waterwayGraphTest.LoadWaterwayNetworkDatasets();
            var waterwayRoutePlannerResponseTest = new WaterwayRoutePlannerResponse();
            var epsilon = WaterwayRoutePlannerResponse.epsilon;
            var waterwayNodeDictionaryTest = waterwayGraphTest.m_dicWaterwayNode;
            var waterwayLinkDictionaryTest = waterwayGraphTest.m_dicWaterwayLink;

            var waterwayLinkPropertyNotSelfConfirmList = new List<string>();
            foreach (var item in waterwayLinkDictionaryTest)
            {
                var waterwayLinkTemp = item.Value;
                if (waterwayLinkTemp.waterLinkID == "HTMSD-0017-HTMSD-0013")
                {
                    Console.WriteLine(waterwayLinkTemp.waterLinkID);
                }
                var linkUpStreamNodeCoordinate = waterwayNodeDictionaryTest[waterwayLinkTemp.upStreamWaterNodeID].waterNodeCoordinate;
                var linkDownStreamNodeCoordinate = waterwayNodeDictionaryTest[waterwayLinkTemp.downStreamWaterNodeID].waterNodeCoordinate;

                var linkGeometryLength = waterwayLinkTemp.channelGeometry.Length;
                var linkIndexStartCoordinate = new double[2] { waterwayLinkTemp.channelGeometry[0].X, waterwayLinkTemp.channelGeometry[0].Y };
                var linkIndexEndCoordinate = new double[2] { waterwayLinkTemp.channelGeometry[linkGeometryLength - 1].X, waterwayLinkTemp.channelGeometry[linkGeometryLength - 1].Y };

                var distanceUp2Start = waterwayRoutePlannerResponseTest.GetDistanceByCoordinate(linkUpStreamNodeCoordinate, linkIndexStartCoordinate);
                var distanceDown2End = waterwayRoutePlannerResponseTest.GetDistanceByCoordinate(linkDownStreamNodeCoordinate, linkIndexEndCoordinate);
                var distanceUp2End = waterwayRoutePlannerResponseTest.GetDistanceByCoordinate(linkUpStreamNodeCoordinate, linkIndexEndCoordinate);
                var distanceDown2Start = waterwayRoutePlannerResponseTest.GetDistanceByCoordinate(linkDownStreamNodeCoordinate, linkIndexStartCoordinate);

                var isSpatialConsistency = false;

                if ((distanceUp2Start < epsilon) && (distanceDown2End < epsilon))
                {
                    isSpatialConsistency = true;
                }
                else
                {
                    if ((distanceUp2End < epsilon) && (distanceDown2Start < epsilon))
                    {
                        isSpatialConsistency = true;
                    }
                }

                if (!isSpatialConsistency)
                {
                    waterwayLinkPropertyNotSelfConfirmList.Add(waterwayLinkTemp.waterLinkID);
                }

            }

            Assert.AreEqual(waterwayLinkPropertyNotSelfConfirmList.Count, 0);
        }


    }
}