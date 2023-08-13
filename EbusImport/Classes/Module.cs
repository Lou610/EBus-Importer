using System;
using System.Collections.Generic;
namespace EBusXmlParser.XmlModels
{
    //define module class with properties which need to be saved to DB
    public class XMLModule
    {

        // each module can have multiple duties
     
        public List<XMLWaybill> WayBills { get; set; }

        public int id_Module { get; set; }
        public string str_LocationCode { get; set; }
        public int int4_ModuleID { get; set; }
        public int int4_SignOnID { get; set; }
        public int int4_OnReaderID { get; set; }
        public DateTime dat_SignOnDate { get; set; }
        public DateTime dat_SignOnTime { get; set; }
        public int int4_OffReaderID { get; set; }
        public DateTime dat_SignOffDate { get; set; }
        public DateTime dat_SignOffTime { get; set; }
        public DateTime dat_TrafficDate { get; set; }
        public DateTime dat_ModuleOutDate { get; set; }
        public DateTime dat_ModuleOutTime { get; set; }
        public int int4_HdrModuleRevenue { get; set; }
        public int int4_HdrModuleTickets { get; set; }
        public int int4_HdrModulePasses { get; set; }
        public int int4_ModuleRevenue { get; set; }
        public int int4_ModuleTickets { get; set; }
        public int int4_ModulePasses { get; set; }
        public int int4_ModuleNonRevenue { get; set; }
        public int int4_ModuleTransfer { get; set; }
        public DateTime dat_ImportStamp { get; set; }
        public DateTime dat_RecordMod { get; set; }
        public int int4_ImportModuleKey { get; set; }
        public int id_BatchNo { get; set; }
        public int byt_IeType { get; set; }
        public int byt_ModuleType { get; set; }
    }
}