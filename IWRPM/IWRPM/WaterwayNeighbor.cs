using IWRPM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace IWRPM
{
    /// WaterwayNeighbor 航线规划路径邻接对象
    /// <summary>
    /// </summary>
    public class WaterwayNeighbor
    {
        public string nextWaterwayNodeID;
        public string toNextWaterwayNodeLinkID;
        public double toNextWaterwayNodeCost;

        public WaterwayNeighbor()
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }

        public WaterwayNeighbor(string _nextWaterwayNodeID, string _toNextWaterwayNodeLinkID, double _toNextWaterwayNodeCost)
        {
            this.nextWaterwayNodeID = _nextWaterwayNodeID;
            this.toNextWaterwayNodeLinkID = _toNextWaterwayNodeLinkID;
            this.toNextWaterwayNodeCost = _toNextWaterwayNodeCost;
        }
    }
}