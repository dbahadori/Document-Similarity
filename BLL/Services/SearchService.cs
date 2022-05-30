using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DocumentSimilarity.Models;
using DocumentSimilarity.Models.Services;
using Lucene.Net.QueryParsers;

namespace DocumentSimilarity.BLL
{
    public class SearchService
    {
        private const QueryParser.Operator DefaultOperator = QueryParser.Operator.OR;
        public IEnumerable<ResearchDocument> SectionSearch(string section, SectionField sectionName,SearchMode mode ,int hits)
        {
            IEnumerable<ResearchDocument> similarityResult = new List<ResearchDocument>();
            ResearchDocumenService documentService = new ResearchDocumenService();

            if(mode==SearchMode.AllTerms)
            similarityResult = documentService.ListSearchedDocuments(section, sectionName, hits, 0, mode, QueryParser.Operator.AND);
            else 
                similarityResult = documentService.ListSearchedDocuments(section, sectionName, hits, 0, mode, DefaultOperator);

            return similarityResult;
        }
        public IEnumerable<ResearchDocument> BodySearch(string body, int hits)
        {
            IEnumerable<ResearchDocument> similarityResult = new List<ResearchDocument>();
            ResearchDocumenService documentService = new ResearchDocumenService();
            similarityResult = documentService.ListSearchedDocuments(new Dictionary<SectionField, string> { { SectionField.Body, body } }, hits, 0, SearchMode.AtLeastOneTerm, DefaultOperator);
            return similarityResult;
        }
        public IEnumerable<ResearchDocument> ModelSearch(List<Search> models, int hits)
        {
             IEnumerable<ResearchDocument> similarityResult = new List<ResearchDocument>();
            ResearchDocumenService documentService = new ResearchDocumenService();

            similarityResult = documentService.ListSearchedDocuments(models,hits);
           
            return similarityResult;
        
        }
        public IEnumerable<ResearchDocument> GetAllDocuments()
        {
            IEnumerable<ResearchDocument> Result = new List<ResearchDocument>();
            ResearchDocumenService documentService = new ResearchDocumenService();
            Result = documentService.ListAllDocuments();
            return Result;
        }
    }
}