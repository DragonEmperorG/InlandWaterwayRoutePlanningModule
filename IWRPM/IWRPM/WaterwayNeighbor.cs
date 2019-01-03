using IWRPM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace IWRPM
{
    /// WaterwayNeighbor 航线规划路径邻接对象
    /// 可以进行相应的扩展，属于基础的数据结构
    /// <summary>
    /// </summary>
    public class WaterwayNeighbor
    {
        // 当前节点 的 邻节点的ID
        public string nextWaterwayNodeID;
        // 当前节点 连接 邻节点的拓扑线ID
        public string toNextWaterwayNodeLinkID;
        // 当前节点 到 邻节点的开销
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