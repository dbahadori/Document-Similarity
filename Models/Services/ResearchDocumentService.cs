using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DocumentSimilarity.BLL;
using Lucene.Net;
using Lucene.Net.QueryParsers;

namespace DocumentSimilarity.Models.Services
{
    public class ResearchDocumenService
    {
        /// <summary>
        /// Returns a list of all research documents that exist and have been indexed.
        /// </summary>
        /// <returns>list of ResearchDocument object </returns>
        public IEnumerable<ResearchDocument> ListAllDocuments()
        {
            List<ResearchDocument> researchDocumentList=new List<ResearchDocument> ();
            XMLEngine xmlEngine=new XMLEngine ();

            //get all research documents that are indexed
            List<string> docNameList=SearchEngine.GetAllDocumentName();

            if(docNameList.Count<=0)
                return researchDocumentList;

            foreach(string dn in docNameList)
            {
              
                //read and parse related xml file and converrt it to related ResearchDocument object
               ResearchDocument rd= xmlEngine.XMLResearchDocumentReader(dn);
                researchDocumentList.Add(rd);
            }

            IEnumerable<ResearchDocumentResult> result = from d in researchDocumentList
                         from a in d.Authors
                                                         select new ResearchDocumentResult { DocumentName = d.Name, Abstract = d.Abstract, Body = d.Body, Keywords = d.Keywords, Title = d.Title, Year = d.Year, Type = d.Type, AuthorName = a.Name, AuthorDegree = a.Degree, AuthorField = a.Field };
            List<ResearchDocument> DocumentList = CreateDocumentListfromResult(result);
            return DocumentList;
        }



        /// <summary>
        /// Returns a list of research documents that match with search string
        /// </summary>
        /// <param name="searchString">A string that search operation is based on it</param>
        /// <param name="field"></param>
        /// <param name="hits"></param>
        /// <param name="slop"></param>
        /// <param name="searchmode"></param>
        /// <returns>list of ResearchDocument object</returns>
        public IEnumerable<ResearchDocument> ListSearchedDocuments(string searchString, SectionField field, int hits, int slop, SearchMode searchmode, QueryParser.Operator _operator)
        {
            List<string> idList = new List<string>();
            SearchEngine searchEngineObject = new SearchEngine();
            XMLEngine xmlEngine = new XMLEngine();
            SortedDictionary<string,float> ResultDictionary = new SortedDictionary<string,float>();

            List<ResearchDocument> researchDocumentList = new List<ResearchDocument>();


            switch (searchmode)
            {
                case SearchMode.AtLeastOneTerm: ResultDictionary = SearchEngine.SearchWithQueryParser(searchString, field, hits,_operator); 
                    break;
                case SearchMode.ExactlyPhrase: ResultDictionary = SearchEngine.SearchWithPhraseQuery(searchString, field, hits, slop,_operator);
                    break;

            }


            //get all research documents that meet to search operation
            if (ResultDictionary.Count>0)
            {
                foreach (KeyValuePair<string, float> kv in ResultDictionary)
                {
                    //read and parse related xml file and converrt it to related ResearchDocument object
                    ResearchDocument rd = xmlEngine.XMLResearchDocumentReader(kv.Key, kv.Value);
                    researchDocumentList.Add(rd);
                }
            }
            else
            {
                return new List<ResearchDocument>();
            }

            IEnumerable<ResearchDocumentResult> result = from d in researchDocumentList
                                                         from a in d.Authors
                                                         select new ResearchDocumentResult { DocumentName = d.Name, Abstract = d.Abstract, Body = d.Body, Keywords = d.Keywords, Title = d.Title, Year = d.Year, Score = d.Score, Type = d.Type, AuthorName = a.Name, AuthorDegree = a.Degree, AuthorField = a.Field };
            List<ResearchDocument> DocumentList = CreateDocumentListfromResult(result);
            DocumentList=DocumentList.OrderByDescending(i=>i.Score).ToList();
            return DocumentList;
        }
        public IEnumerable<ResearchDocument> ListSearchedDocuments(Dictionary<SectionField, string> field_queryString, int hits, int slop, SearchMode searchmode, QueryParser.Operator _operator)
        {
            List<string> idList = new List<string>();
            SearchEngine searchEngineObject = new SearchEngine();
            XMLEngine xmlEngine = new XMLEngine();
            SortedDictionary<string, float> ResultDictionary = new SortedDictionary<string, float>();

            List<ResearchDocument> researchDocumentList = new List<ResearchDocument>();


            switch (searchmode)
            {
                case SearchMode.AtLeastOneTerm: ResultDictionary = SearchEngine.SearchWithQueryParser(field_queryString, hits, _operator);
                    break;
                case SearchMode.ExactlyPhrase: ResultDictionary = SearchEngine.SearchWithPhraseQuery(field_queryString, hits, slop, _operator);
                    break;


            }


            //get all research documents that meet to search operation
            if (ResultDictionary.Count > 0)
            {
                foreach (KeyValuePair<string, float> kv in ResultDictionary)
                {
                    //read and parse related xml file and converrt it to related ResearchDocument object
                    ResearchDocument rd = xmlEngine.XMLResearchDocumentReader(kv.Key, kv.Value);
                    researchDocumentList.Add(rd);
                }
            }
            else
            {
                return null;
            }

            IEnumerable<ResearchDocumentResult> result = from d in researchDocumentList
                                                         from a in d.Authors
                                                         select new ResearchDocumentResult { DocumentName = d.Name, Abstract = d.Abstract, Body = d.Body, Keywords = d.Keywords, Title = d.Title, Year = d.Year, Score = d.Score, Type = d.Type, AuthorName = a.Name, AuthorDegree = a.Degree, AuthorField = a.Field };
            List<ResearchDocument> DocumentList = CreateDocumentListfromResult(result);
            DocumentList = DocumentList.OrderByDescending(i => i.Score).ToList();
            return DocumentList;
        }

        /// <summary>
        /// for all field
        /// </summary>
        /// <param name="searchString"></param>
        /// <param name="hits"></param>
        /// <param name="slop"></param>
        /// <param name="searchmode"></param>
        /// <returns></returns>
        public IEnumerable<ResearchDocument> ListSearchedDocuments(string searchString, int hits, int slop, SearchMode searchmode,QueryParser.Operator _operator)
        {
            List<string> idList = new List<string>();
            SearchEngine searchEngineObject = new SearchEngine();
            XMLEngine xmlEngine = new XMLEngine();
            SortedDictionary<string, float> ResultDictionary = new SortedDictionary<string, float>();

            List<ResearchDocument> researchDocumentList = new List<ResearchDocument>();


            switch (searchmode)
            {
                case SearchMode.AtLeastOneTerm: ResultDictionary = SearchEngine.SearchWithQueryParser(searchString, SectionField.All, hits,_operator);
                    break;
                case SearchMode.ExactlyPhrase: ResultDictionary = SearchEngine.SearchWithPhraseQuery(searchString, SectionField.All, hits, slop,_operator);
                    break;


            }


            //get all research documents that meet to search operation
            if (ResultDictionary != null)
            {
                foreach (KeyValuePair<string, float> kv in ResultDictionary)
                {
                    //read and parse related xml file and converrt it to related ResearchDocument object
                    ResearchDocument rd = xmlEngine.XMLResearchDocumentReader(kv.Key, kv.Value);
                    researchDocumentList.Add(rd);
                }
            }
            else
            {
                return null;
            }

            IEnumerable<ResearchDocumentResult> result = from d in researchDocumentList
                                                         from a in d.Authors
                                                         select new ResearchDocumentResult { DocumentName = d.Name, Abstract = d.Abstract, Body = d.Body, Keywords = d.Keywords, Title = d.Title, Year = d.Year, Score = d.Score, Type = d.Type, AuthorName = a.Name, AuthorDegree = a.Degree, AuthorField = a.Field };
            List<ResearchDocument> DocumentList = CreateDocumentListfromResult(result);
            DocumentList = DocumentList.OrderByDescending(i => i.Score).ToList();
            return DocumentList;
        }

        /// <summary>
        /// for more than one field
        /// </summary>
        /// <param name="searchString"></param>
        /// <param name="fields"></param>
        /// <param name="hits"></param>
        /// <param name="slop"></param>
        /// <param name="searchmode"></param>
        /// <param name="_operator"></param>
        /// <returns></returns>
        public IEnumerable<ResearchDocument> ListSearchedDocuments(string searchString, List<SectionField> fields, int hits, int slop, SearchMode searchmode, QueryParser.Operator _operator)
        {
            List<string> idList = new List<string>();
            SearchEngine searchEngineObject = new SearchEngine();
            XMLEngine xmlEngine = new XMLEngine();
            SortedDictionary<string, float> ResultDictionary = new SortedDictionary<string, float>();

            List<ResearchDocument> researchDocumentList = new List<ResearchDocument>();


            switch (searchmode)
            {
                case SearchMode.AtLeastOneTerm: ResultDictionary = SearchEngine.SearchWithQueryParser(searchString, fields, hits, _operator);
                    break;
                case SearchMode.ExactlyPhrase: ResultDictionary = SearchEngine.SearchWithPhraseQuery(searchString, fields, hits, slop,_operator);
                    break;


            }


            //get all research documents that meet to search operation
            if (ResultDictionary != null)
            {
                foreach (KeyValuePair<string, float> kv in ResultDictionary)
                {
                    //read and parse related xml file and converrt it to related ResearchDocument object
                    ResearchDocument rd = xmlEngine.XMLResearchDocumentReader(kv.Key, kv.Value);
                    researchDocumentList.Add(rd);
                }
            }
            else
            {
                return null;
            }

            IEnumerable<ResearchDocumentResult> result = from d in researchDocumentList
                                                         from a in d.Authors
                                                         select new ResearchDocumentResult { DocumentName = d.Name, Abstract = d.Abstract, Body = d.Body, Keywords = d.Keywords, Title = d.Title, Year = d.Year, Score = d.Score, Type = d.Type, AuthorName = a.Name, AuthorDegree = a.Degree, AuthorField = a.Field };
            List<ResearchDocument> DocumentList = CreateDocumentListfromResult(result);
            DocumentList = DocumentList.OrderByDescending(i => i.Score).ToList();
            return DocumentList;
        }

        public IEnumerable<ResearchDocument> ListSearchedDocuments(List<Search> searchModel,int hits)
        {
            List<string> idList = new List<string>();
            SearchEngine searchEngineObject = new SearchEngine();
            XMLEngine xmlEngine = new XMLEngine();
            SortedDictionary<string, float> ResultDictionary = new SortedDictionary<string, float>();

            List<ResearchDocument> researchDocumentList = new List<ResearchDocument>();

            ResultDictionary=SearchEngine.SearchWithModel(searchModel, hits);

            //get all research documents that meet to search operation
            if (ResultDictionary.Count > 0)
            {
                foreach (KeyValuePair<string, float> kv in ResultDictionary)
                {
                    //read and parse related xml file and converrt it to related ResearchDocument object
                    ResearchDocument rd = xmlEngine.XMLResearchDocumentReader(kv.Key, kv.Value);
                    researchDocumentList.Add(rd);
                }
            }
            else
            {
                return new List<ResearchDocument>();
            }

            IEnumerable<ResearchDocumentResult> result = from d in researchDocumentList
                                                         from a in d.Authors
                                                         select new ResearchDocumentResult { DocumentName = d.Name, Abstract = d.Abstract, Body = d.Body, Keywords = d.Keywords, Title = d.Title, Year = d.Year, Score = d.Score,Type=d.Type, AuthorName = a.Name, AuthorDegree = a.Degree, AuthorField = a.Field };
            List<ResearchDocument> DocumentList = CreateDocumentListfromResult(result);
            DocumentList = DocumentList.OrderByDescending(i => i.Score).ToList();
            return DocumentList;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        private List<ResearchDocument> CreateDocumentListfromResult(IEnumerable<ResearchDocumentResult> results)
        {
            ResearchDocument researchDocumentObj = new ResearchDocument();
            Hashtable documentHashtable = new Hashtable();

            foreach (ResearchDocumentResult obj in results)
            {
               

                if (!documentHashtable.ContainsKey(obj.DocumentName))
                {
                    researchDocumentObj = new ResearchDocument { Name = obj.DocumentName, Title = obj.Title, Abstract = obj.Abstract, Keywords = obj.Keywords, Body = obj.Body, Year = obj.Year, Authors = new List<Author>(), Score=obj.Score,Type=obj.Type };
                    Author aut = new Author();
                    aut.Name = obj.AuthorName;
                    aut.Field = obj.AuthorField;
                    aut.Degree = obj.AuthorDegree;
                    researchDocumentObj.Authors.Add(aut);
                    documentHashtable.Add(obj.DocumentName, researchDocumentObj);
                }
                else
                {
                    Author aut = new Author();
                    aut.Name = obj.AuthorName;
                    aut.Field = obj.AuthorField;
                    aut.Degree = obj.AuthorDegree;
                    ((ResearchDocument)documentHashtable[obj.DocumentName]).Authors.Add(aut);

                }


            }
            List<ResearchDocument> documentList = documentHashtable.Values.Cast<object>().Cast<ResearchDocument>().ToList();
            return documentList;
        }
    }
}