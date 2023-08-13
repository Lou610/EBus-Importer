using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EBusXmlParser.XmlModels
{
    //way bill info per module, these need to be save into DB
    public class XMLWaybill
    {
        public int position { get; set; }
        public int ModuleID { get; set; }
        public DateTime dat_Start { get; set; }
        public DateTime dat_End { get; set; }
        public int int4_Operator { get; set; }
        public string str8_BusID { get; set; }
        public string str6_EtmID { get; set; }
        public int int4_EtmGrandTotal { get; set; }
        public int int4_Revenue { get; set; }
        public DateTime dat_Match { get; set; }
        public DateTime dat_Actual { get; set; }
        public int Imported_Operator { get; set; }
        public string Imported_BusID { get; set; }
        public string Imported_ETMID { get; set; }
        public int Imported_GT { get; set; }
        public int Imported_Revenue { get; set; }
    }
}
