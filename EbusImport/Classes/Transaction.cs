using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EBusXmlParser.XmlModels
{
    public class XMLTransaction
    {
        public int position { get; set; }
        public int id_Trans { get; set; }
        public int id_Stage { get; set; }
        public int id_Journey { get; set; }
        public int id_Duty { get; set; }
        public int id_Module { get; set; }
        public string str_LocationCode { get; set; }
        public int int2_BoardingStageID { get; set; }
        public int int2_AlightingStageID { get; set; }
        public int int2_Class { get; set; }
        public int int4_Revenue { get; set; }
        public int int4_NonRevenue { get; set; }
        public int int2_TicketCount { get; set; }
        public int int2_PassCount { get; set; }
        public int int2_Transfers { get; set; }
        public DateTime dat_TransDate { get; set; }
        public DateTime dat_TransTime { get; set; }
        public DateTime str_SerialNumber { get; set; }
        public int int4_RevenueBal { get; set; }
        public int int4_TripBal { get; set; }
        public int int2_AnnulCount { get; set; }
        public int int4_AnnulCash { get; set; }
        public int id_SCTrans { get; set; }
    }
}
