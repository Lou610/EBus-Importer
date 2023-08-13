using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EbusImport.Classes
{
    public class StageDetails : JoureyDetails
    {
        public int StageID { get; set; }
        public int StagePosition { get; set; }
        public int boardingStageID { get; set; }
    }
}
