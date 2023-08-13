using System;
using System.Collections.Generic;

namespace EBusXmlParser.XmlModels
{
    public class XMLJourney
    {
        public int position { get; set; }
        public int Name { get; set; }
        // each journey has multiple stages
        public List<XMLStage> Stages { get; set; }
        public int id_Journey { get; set; }
        public int id_Duty { get; set; }
        public int id_Module { get; set; }
        public string str_RouteID { get; set; }
        public int int2_JourneyID { get; set; }
        public int int2_Direction { get; set; }
        public DateTime dat_JourneyStartDate { get; set; }
        public DateTime dat_JourneyStartTime { get; set; }
        public DateTime dat_JourneyStopDate { get; set; }
        public DateTime dat_JourneyStopTime { get; set; }
        public DateTime dat_TrafficDate { get; set; }
        public int int4_Distance { get; set; }
        public int int4_Traveled { get; set; }
        public int int4_JourneyRevenue { get; set; }
        public int int4_JourneyTickets { get; set; }
        public int int4_JourneyPasses { get; set; }
        public int int4_JourneyNonRevenue { get; set; }
        public int int4_JourneyTransfer { get; set; }
        public DateTime dat_RecordMod { get; set; }
        public int id_BatchNo { get; set; }
        public int byt_IeType { get; set; }
        public DateTime dat_JourneyMoveTime { get; set; }
        public DateTime dat_JourneyArrivalTime { get; set; }
        public int int4_GPSDistance { get; set; }
    }
}
