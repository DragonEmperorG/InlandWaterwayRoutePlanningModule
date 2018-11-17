using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace IWRPM
{
    public class WaterwayVehicle
    {
        public string vehicleID;
        public float deadWeightTonnage;
        public float lengthOverall;
        public float mouldedBreadth;
        public float mouldedDepth;
        public float loadedDraft;
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