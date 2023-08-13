using System;
using System.Collections.Generic;

namespace EBusXmlParser.XmlModels
{
    public class XMLStage
    {
        public int position { get; set; }
        //each stage will have many transactions
        public List<XMLTransaction> Trans { get; set; }
        public int id_Stage { get; set; }
        public int id_Journey { get; set; }
        public int id_Duty { get; set; }
        public int id_Module { get; set; }
        public int int2_StageID { get; set; }
        public DateTime dat_StageDate { get; set; }
        public DateTime dat_StageTime { get; set; }
        public DateTime dat_RecordMod { get; set; }
        public int id_BatchNo { get; set; }
    }
}
