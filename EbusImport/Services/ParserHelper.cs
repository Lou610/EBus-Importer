using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Data.Linq;
using System.Xml.Linq;
using System.Threading;
using System.Globalization;
using System.ComponentModel;
using System.Data;
using EbusImport.Classes;

namespace EbusImport.Services
{
    public class ParserHelper
    {
        public List<XElement> GetAllJourneyDetails(XDocument doc)
        {
            var d = (from ds in doc.Root.Descendants("StartOfJourney")
                     select ds).Union(from ds in doc.Root.Descendants("EndOfJourney")
                                      select ds);
            return d.ToList();
        }

        public List<XElement> GetAllEndJourneyDetails(XDocument doc)
        {
            var d = from ds in doc.Root.Descendants("EndOfJourney")
                    select ds;

            return d.ToList();
        }

        public List<XElement> GetStageDetails(XDocument doc)
        {
            var allStageDetails = from ds in doc.Root.Descendants("GPS_Stage")
                                  select ds;
            return allStageDetails.ToList();
        }

        public List<XElement> GetTranseDetails(XDocument doc)
        {
            var allStageDetails = from ds in doc.Root.Descendants("TicketSale")
                                  select ds;
            return allStageDetails.ToList();
        }

        public List<XElement> GetMJDetails(XDocument doc)
        {
            var allStageDetails = from ds in doc.Root.Descendants("CombinedSTDTicketSmartcard")
                                  select ds;
            return allStageDetails.ToList();
        }

        public List<XElement> GetAllElements(XDocument doc)
        {
            var allElements = from ds in doc.Root.Elements()
                              select ds;
            return allElements.ToList();
        }

        private void Grouping(string path)
        {
            var doc = XDocument.Load(path);
            List<XElement> StageRersult = GetAllElements(doc);

            DataTable dt = new DataTable();


            foreach (XElement xEle in StageRersult)
            {

            }
        }


        /// <summary>
        /// for any stage we can get journey and duty details here 
        ///IDJourneies, dutID, ModId, istartOfJourneyPos, iEndOfJourneyPos
        /// </summary>
        /// <param name="journeyDt"></param>
        /// <param name="stagePostion"></param>
        /// <returns></returns>
        public JoureyDetails GetAllParentDetailsForStage(List<JoureyDetails> journeyDetails, int stagePostion)
        {
            var res = (from jr in journeyDetails
                       where jr.istartOfJourneyPos < stagePostion && jr.iEndOfJourneyPos > stagePostion
                       select jr).FirstOrDefault();

            return res;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="journeyDt"></param>
        /// <param name="transactionPostion"></param>
        /// <returns></returns>
        public StageDetails GetAllParentDetailsForTransaction(List<StageDetails> stageDetails, int transactionPostion)
        {
            //need to find on boarding stage details
            //imp:just get the onboarding stage for current transaction !!
            var onbStage = stageDetails.Where(s => s.StagePosition < transactionPostion).OrderByDescending(s => s.StagePosition).First();
            return onbStage;
        }

    }
}
