using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DocumentSimilarity.Models;
using DocumentSimilarity.Models.Services;
using Lucene.Net.QueryParsers;
using NLog;

namespace DocumentSimilarity.BLL.Services
{
    public class SimilarityService
    {
        private const QueryParser.Operator DefaultOperator = QueryParser.Operator.OR;
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public IEnumerable<ResearchDocument> SectionSimilarityOfTwoDocument(string sectionA, string sectionB)
        {

            List<ResearchDocument> result = new List<ResearchDocument>();
            return result;
        }
        public IEnumerable<ResearchDocument> SectionSimilarity(string section, SectionField sectionName, int hits)
        {
            IEnumerable<ResearchDocument> similarityResult = new List<ResearchDocument>();
            ResearchDocumenService documentService = new ResearchDocumenService();
            similarityResult=documentService.ListSearchedDocuments( section,sectionName, hits, 0, SearchMode.AtLeastOneTerm, DefaultOperator);
            return similarityResult;
        }
        public IEnumerable<ResearchDocument> SectionSimilarity(List<SectionField> sectionFielf,ResearchDocument document, int hits)
        {
            IEnumerable<ResearchDocument> similarityResult = new List<ResearchDocument>();
            ResearchDocumenService documentService = new ResearchDocumenService();
            Dictionary<SectionField, string> sectionFieldQuery = new Dictionary<SectionField, string>();
            List<Author> authors = new List<Author>();
            string authorsName = "";
            authors = document.Authors;

            foreach (var a in authors)
                authorsName += a.Name+",";

            foreach (SectionField s in sectionFielf)
            {
                if (s.ToString() == "Authors")
                    sectionFieldQuery.Add(s, authorsName);
                else
                {
                    foreach (var prop in document.GetType().GetProperties())
                    {
                        if (prop.Name == s.ToString()) sectionFieldQuery.Add(s, prop.GetValue(document, null).ToString());
                    }
                }
            }
         
            similarityResult= documentService.ListSearchedDocuments(sectionFieldQuery, hits, 0, SearchMode.AtLeastOneTerm, DefaultOperator);
            return similarityResult;
        }
        public IEnumerable<ResearchDocument> BodySimilarity(string body, int hits)
        {
            IEnumerable<ResearchDocument> similarityResult = new List<ResearchDocument>();
            ResearchDocumenService documentService = new ResearchDocumenService();
            similarityResult = documentService.ListSearchedDocuments(new Dictionary<SectionField, string> { { SectionField.Body, body } }, hits, 0, SearchMode.AtLeastOneTerm, DefaultOperator);
            return similarityResult;
        }
        public IEnumerable<ResearchDocument> PearToPearSimilarity(Dictionary<SectionField, string> sectionNameValue, int hits)
        {
            IEnumerable<ResearchDocument> similarityResult = new List<ResearchDocument>();
            ResearchDocumenService documentService = new ResearchDocumenService();
            similarityResult = documentService.ListSearchedDocuments(sectionNameValue, hits, 0, SearchMode.AtLeastOneTerm, DefaultOperator);
            return similarityResult;
        }
        public IEnumerable<ResearchDocument> PearToPearSimilarity(ResearchDocument document, int hits)
        {
            /*  IEnumerable<ResearchDocument> similarityResult = new List<ResearchDocument>();
            ResearchDocumenService documentService = new ResearchDocumenService();
             Dictionary<SectionField, string> sectionNameValue = new Dictionary<SectionField, string>();
             string authors="";
             sectionNameValue.Add(SectionField.Abstract, document.Abstract);

             sectionNameValue.Add(SectionField.Body, document.Body);

             sectionNameValue.Add(SectionField.Title, document.Title);

             sectionNameValue.Add(SectionField.Keyword, document.Keywords);

             sectionNameValue.Add(SectionField.Year, document.Year);
            
             foreach (Author aut in document.Authors)
                 authors+= aut.Name ;
            
             sectionNameValue.Add(SectionField.Authors, authors);

             similarityResult = documentService.ListSearchedDocuments(sectionNameValue, hits, 0, SearchMode.AtLeastOneTerm, DefaultOperator);
             return similarityResult;*/

            //using MoreLikeThis

            int k = 42;
            int l = 100;

            logger.Trace("Sample trace message, k={0}, l={1}", k, l);
            logger.Debug("Sample debug message, k={0}, l={1}", k, l);
            logger.Info("Sample informational message, k={0}, l={1}", k, l);
            logger.Warn("Sample warning message, k={0}, l={1}", k, l);
            logger.Error("Sample error message, k={0}, l={1}", k, l);
            logger.Fatal("Sample fatal error message, k={0}, l={1}", k, l);
            logger.Log(LogLevel.Info, "Sample informational message, k={0}, l={1}", k, l);


            return SearchEngine.SimilarityDetection(document, hits);

        }
        
    }
}