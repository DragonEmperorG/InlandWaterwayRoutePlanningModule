using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace IWRPM
{
    /// <summary>
    /// 定义通航船舶对象
    /// 基础数据结构
    /// </summary>
    public class WaterwayVehicle
    {
        //船舶标识
        public string vehicleID;
        //船舶载重吨位
        public float deadWeightTonnage;
        //总长
        public float lengthOverall;
        //型宽
        public float mouldedBreadth;
        //型深
        public float mouldedDepth;
        //满载吃水
        public float loadedDraft;
        //空载水面线以上高度
        public float freeboardHeight;

        public WaterwayVehicle()
        {
        }

        public WaterwayVehicle(
            string _vehicleID,
            float _deadWeightTonnage,
            float _lengthOverall,
            float _mouldedBreadth,
            float _mouldedDepth,
            float _loadedDraft,
            float _freeboardHeight
        )
        {
            this.vehicleID = _vehicleID;
            this.deadWeightTonnage = _deadWeightTonnage;
            this.lengthOverall = _lengthOverall;
            this.mouldedBreadth = _mouldedBreadth;
            this.mouldedDepth = _mouldedDepth;
            this.loadedDraft = _loadedDraft;
            this.freeboardHeight = _freeboardHeight;
        }
    }
}