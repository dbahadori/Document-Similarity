using DocumentSimilarity.Models;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;
using NHazm;
using Lucene.Net.Search.Highlight;
using Lucene.Net.Analysis.Fa;
using Lucene.Net.Search.Spans;
using System.Threading.Tasks;
using Lucene.Net.Search.Similar;
using Lucene.Net.Search.Vectorhighlight;


namespace DocumentSimilarity.BLL
{
    

    public class SearchEngine
    {
        // 
        // private properties
        // 
        #region private properties

        private static Directory directory;
        private static readonly Lucene.Net.Util.Version _version = Lucene.Net.Util.Version.LUCENE_30;
        private static string _luceneDirectory = HttpContext.Current.Server.MapPath(@"~/Documents/LuceneDirectory");
        private static IndexWriter writer;
        #endregion

        //
        // private methods
        //
        #region private methods


        private static int GetLuceneDocumentNumber(ResearchDocument document)
        {
            directory = FSDirectory.Open(_luceneDirectory);
            IndexSearcher _searcher = new IndexSearcher(directory ,true);

            var analyzer = new StandardAnalyzer(_version,Preprocessing.GetStopWords());
            var parser = new QueryParser(_version, "Name", analyzer);
            var query = parser.Parse("\""+document.Name+"\"");
            var doc = _searcher.Search(query, 1);
            if (doc.TotalHits == 0)
            {
                return 0;
            }
            return doc.ScoreDocs[0].Doc;
        }

        /// <summary>
        /// Create query for searching in indexed data base on one field
        /// </summary>
        /// <param name="querystring"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        private static Query QueryCreator(string querystring, SectionField field, QueryParser.Operator _operator)
        {
            Query query;
            var analyzer = new StandardAnalyzer(_version, Preprocessing.GetStopWords());
            QueryParser qp = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, field.ToString(), analyzer);
            qp.DefaultOperator =_operator;
            if (field == SectionField.All) query = AllFieldQueryCreator(querystring, _operator);
            else {
                if (querystring == null)
                    querystring = " ";
                query = qp.Parse(querystring);
            }

            return query;
        }

        /// <summary>
        /// Create query for searching in indexed data base on multiple field
        /// </summary>
        /// <param name="field_query"></param>
        /// <param name="_operator"></param>
        /// <returns></returns>
        private static Query MultiFieldQueryCreator(Dictionary<SectionField, string> field_query, QueryParser.Operator _operator)
        {

            Query query;
            BooleanQuery bQuery = new BooleanQuery();
            var analyzer = new StandardAnalyzer(_version, Preprocessing.GetStopWords());
            Occur occur = Occur.SHOULD;
           Normalizer normalizer = new Normalizer();

            switch (_operator)
            {
                case QueryParser.Operator.OR: occur = Occur.SHOULD;
                    break;
                case QueryParser.Operator.AND: occur = Occur.MUST;
                    break;
            }

            foreach (KeyValuePair<SectionField, string> k in field_query)
            {
                QueryParser qp = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, k.Key.ToString(), analyzer);
                if (!(k.Value == null || k.Value == ""))
                    // query = qp.Parse(normalizer.Run(Preprocessing.RemovingSpecificChar(k.Value, new char[] { '،', '؛', '.', ',', ';', ':', '/', '\\', '@', '#','*','?','*','-','+','^','!','~', '$', '%',')','(','<','>','|','}','{',']','[' })));
                    query = qp.Parse(k.Value);
                else
                    query = qp.Parse("empty");
                    
                bQuery.Add(query,occur);
            }

            return bQuery;
        }
        private static Query SimilarityQuery(ResearchDocument doc)
        {
            directory = FSDirectory.Open(_luceneDirectory);
            IndexSearcher searcher = new IndexSearcher(directory);
            
            var docNum = GetLuceneDocumentNumber(doc);
           
            if (docNum == 0)
                 
                return null;

            var analyzer = new StandardAnalyzer(_version,Preprocessing.GetStopWords());
            var reader = searcher.IndexReader;

            var moreLikeThis = new MoreLikeThis(reader);
            moreLikeThis.Analyzer=analyzer;
            moreLikeThis.SetFieldNames(new[] { "Title", "Body", "Abstract","Keywords","Authors","Year" });
            moreLikeThis.MinDocFreq=1;
            moreLikeThis.MinTermFreq=1;
            moreLikeThis.Boost=true;

            return moreLikeThis.Like(docNum);
        }
        private static Query MultiFieldQueryCreator(string queryString, List<SectionField> fields, QueryParser.Operator _operator)
        {
            Query query;


            var analyzer = new StandardAnalyzer(_version, Preprocessing.GetStopWords());
            MultiFieldQueryParser qp = new MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_30, (from f in fields select f.ToString()).ToArray(), analyzer);
            qp.DefaultOperator = _operator;
            query = qp.Parse(queryString);
            return query;
        }
        private static Query AllFieldQueryCreator(string queryString, QueryParser.Operator _operator)
        {
            Query query;
            string [] fields={"Abstract","Title","Body","Keywords","Authors","Year"};

            var analyzer = new StandardAnalyzer(_version, Preprocessing.GetStopWords());
            MultiFieldQueryParser qp = new MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_30,fields, analyzer);
            qp.DefaultOperator = _operator;
            query = qp.Parse(queryString);
            return query;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="phrase"></param>
        /// <param name="field"></param>
        /// <param name="slop"></param>
        /// <returns></returns>
        private static PhraseQuery PhraseQueryCreator(string phrase, SectionField field, int slop)
        {
            PhraseQuery query = new PhraseQuery();
            Parser parser = new Parser();
            Preprocessing preprocessing=new Preprocessing ();

            query.Slop = slop;

            if (phrase != null && phrase != "")
            {
                phrase = parser.RemoveFirstSpecChar(phrase, new char[] { ':', ';' });
                phrase = preprocessing.ContentNormalizer(phrase);
                phrase = preprocessing.RemovingStopWords(phrase);
                phrase = preprocessing.SectionLemmatizer(phrase);
            }
            
            var terms = phrase.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string word in terms)
            {
                query.Add(new Term(field.ToString(), word));
            }

            return query;
        }

        private static PhraseQuery PhraseQueryCreator(Dictionary<SectionField, string> field_phrase, int slop)
        {
            PhraseQuery query = new PhraseQuery();
            query.Slop = slop;


            foreach (KeyValuePair<SectionField, string> kv in field_phrase)
            {
                var terms = kv.Value.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string word in terms)
                {
                    query.Add(new Term(kv.Key.ToString(), word));
                }
            }

            return query;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="phrase"></param>
        /// <param name="fields"></param>
        /// <param name="slop"></param>
        /// <returns></returns>
        private static PhraseQuery PhraseQueryCreator(string phrase, List<SectionField> fields, int slop)
        {

            return PhraseQueryCreator(phrase, fields[0], slop); 
        }

        /// <summary>
        /// Create DB documents and indexes them
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="researchDocuments"></param>
        /// <returns></returns>
        private static List<Lucene.Net.Documents.Document> LuceneDocumentCreator(List<ResearchDocument> researchDocuments)
        {


            List<Lucene.Net.Documents.Document> luceneDocList = new List<Lucene.Net.Documents.Document>();


            foreach (ResearchDocument rd in researchDocuments)
            {
                Lucene.Net.Documents.Document doc = new Lucene.Net.Documents.Document();

                foreach (var prop in rd.GetType().GetProperties())
                {
                    // add each of research document properties to lucene document

                    if (!(prop.Name == "Authors") && !(prop.Name == "Score"))
                    doc.Add(new Field(prop.Name, prop.GetValue(rd,null).ToString(), Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.YES));
                    
                }

                // add authors property to lucene document 

                foreach (Author aut in rd.Authors)
                {
                    doc.Add(new Field("Authors", aut.Name + " , " + aut.Field + " , " + aut.Degree, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.YES));
                }

                luceneDocList.Add(doc);
            }

           

            return luceneDocList;
        }
        private static Lucene.Net.Documents.Document LuceneDocumentCreator(ResearchDocument researchDocument)
        {
            Lucene.Net.Documents.Document doc = new Lucene.Net.Documents.Document();

            foreach (var prop in researchDocument.GetType().GetProperties())
            {
                // add each of research document properties to lucene document

                if (!(prop.Name == "Authors") && !(prop.Name == "Score"))
                    doc.Add(new Field(prop.Name, prop.GetValue(researchDocument, null).ToString(), Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS));
            }

            // add authors property to lucene document 

            foreach (Author aut in researchDocument.Authors)
            {
                doc.Add(new Field("Authors", aut.Name + " , " + aut.Field + " , " + aut.Degree, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS));
            }
            
            return doc;
        }
       
        #endregion

        //
        // public methods
        //
        #region public methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static ResearchDocument ConvertLuceneDoctoResearchDocument(Lucene.Net.Documents.Document doc)
        {
            ResearchDocument researchDocument = new ResearchDocument();
            ResearchDocumentBLL researchDocumentBLL = new ResearchDocumentBLL();
            List<Author> authors = new List<Author>();

            //craete an author object for each lucene author field and add it to author list
            foreach (Field f in doc.GetFields("Authors"))
            {
                authors.Add(AuthorBLL.GetAuthorFromField(f));
            }


            return researchDocumentBLL.ResearchDocCreator(doc.GetField("Title").StringValue, authors, doc.GetField("Keywords").StringValue, doc.GetField("Body").StringValue, doc.GetField("Name").StringValue, doc.GetField("Abstract").StringValue, doc.GetField("Year").StringValue, doc.GetField("Type").StringValue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static Lucene.Net.Documents.Document ConvertResearchDocumenttoLuceneDoc(ResearchDocument doc)
        {
            return LuceneDocumentCreator(doc);
        }
        public static List<Lucene.Net.Documents.Document> ConvertResearchDocumenttoLuceneDoc(List<ResearchDocument> docs)
        {
            return LuceneDocumentCreator(docs);
        }
        public static void Close()
        {
            writer.Dispose();
        }


        /// <summary>
        /// index a list of ResearchDocument objects to lucene search engine
        /// </summary>
        /// <param name="documents">a list of ResearchDocument objects</param>
        /// <returns></returns>
        public static Boolean CreateIndex(List<ResearchDocument> documents)
        {
            try
            {
                directory = FSDirectory.Open(_luceneDirectory);
                var analyzer = new StandardAnalyzer(_version, Preprocessing.GetStopWords());
                // writer = new IndexWriter(directory, analyzer,true, IndexWriter.MaxFieldLength.UNLIMITED);
                foreach (var ldoc in ConvertResearchDocumenttoLuceneDoc(documents))
                    writer.AddDocument(ldoc);
                writer.Commit();
                writer.Dispose();
                writer.Dispose();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        /// <summary>
        /// index one ResearchDocument object to lucene search engine
        /// </summary>
        /// <param name="document">ResearchDocument object</param>
        /// <returns></returns>
        public static Boolean CreateIndex(ResearchDocument document)
        {
            try
            {
                directory = FSDirectory.Open(_luceneDirectory);
                var analyzer = new StandardAnalyzer(_version, Preprocessing.GetStopWords());
                 writer = new IndexWriter(directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED);
                writer.AddDocument(ConvertResearchDocumenttoLuceneDoc(document));
                writer.Commit();
                writer.Dispose();
                writer.Dispose();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public static Boolean DeleteIndex(string docName)
        {
            try
            {
                //new*********
                var analyzer = new StandardAnalyzer(_version, Preprocessing.GetStopWords());
                var parser = new QueryParser(_version, "Name", analyzer);
                var query = parser.Parse("\"" +docName + "\"");
                //**********
                var directory = FSDirectory.Open(_luceneDirectory);
                using (var indexWriter = new IndexWriter(directory, analyzer,false,  mfl: IndexWriter.MaxFieldLength.UNLIMITED))
                {
                   var b= SearchEngine.IsIndexd(docName);
                   // indexWriter.DeleteDocuments(new Term("Name", "\"" + Preprocessing.ConvertDocumentName(docName) + "\""));
                    indexWriter.DeleteDocuments(query);
                    indexWriter.Commit();
                    
                    indexWriter.Dispose();
                    directory.Dispose();
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public static Boolean IsIndexd(string docName)
        {

            directory = FSDirectory.Open(_luceneDirectory);
            if (directory.ListAll().Count() > 0)
            {
                IndexSearcher _searcher = new IndexSearcher(directory, true);

                var analyzer = new StandardAnalyzer(_version, Preprocessing.GetStopWords());
                var parser = new QueryParser(_version, "Name", analyzer);

                var query = parser.Parse("\"" + docName + "\"");
                var doc = _searcher.Search(query, 1);
                if (doc.TotalHits <= 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
            

            
        }

        public static bool UpdateIndex(string docName, ResearchDocument newDoc)
        {
            directory = FSDirectory.Open(_luceneDirectory);

            var analyzer = new StandardAnalyzer(_version, Preprocessing.GetStopWords());
            var parser = new QueryParser(_version, "Name", analyzer);
            var query = parser.Parse("\"" + docName + "\"");
            ISet<Term> terms=new HashSet<Term>();
            query.ExtractTerms(terms);
            
            using (var indexWriter = new IndexWriter(directory, analyzer, mfl: IndexWriter.MaxFieldLength.UNLIMITED))
            {

               // indexWriter.UpdateDocument(new Term("Name", "\"" + Preprocessing.ConvertDocumentName(docName) + "\""), newDoc);
                if (terms.Count > 0)
                {
                    Term t = terms.ElementAt<Term>(0);
                    
                    indexWriter.UpdateDocument(t, ConvertResearchDocumenttoLuceneDoc(newDoc));
                }
                else return false;
                
                indexWriter.Commit();
                indexWriter.Dispose();
                directory.Dispose();

                
            }
            return true;
        }

        /// <summary>
        /// update if document is indexed already
        /// </summary>
        /// <param name="docName"></param>
        /// <param name="newDoc"></param>
        public static void AddIndex(string docName, ResearchDocument newDoc)
        {
            directory = FSDirectory.Open(_luceneDirectory);
            var analyzer = new StandardAnalyzer(_version, Preprocessing.GetStopWords());
            var parser = new QueryParser(_version, "Name", analyzer);
            var query = parser.Parse("\"" + docName + "\"");


            using (var Writer = new IndexWriter(directory, analyzer, mfl: IndexWriter.MaxFieldLength.UNLIMITED))
            {

               
               // Writer.DeleteDocuments(query);
                Writer.AddDocument(ConvertResearchDocumenttoLuceneDoc(newDoc));
                Writer.Commit();

                Writer.Dispose();
                directory.Dispose();
            }
        }
        
        /// <summary>
        /// create a new index for new document even it is indexd already
        /// </summary>
        /// <param name="docName"></param>
        /// <param name="newDoc"></param>
        
        public static List<ResearchDocument> SimilarityDetection(ResearchDocument document,int _hits)
        {

            var query=SimilarityQuery(document);
            int HITS_PER_PAGE = _hits;
            TopScoreDocCollector collector = TopScoreDocCollector.Create(HITS_PER_PAGE, true);
            IndexSearcher searcher = new IndexSearcher(IndexReader.Open(directory, true));
            List<ResearchDocument> similarResearchDocumentList=new List<ResearchDocument> ();
            searcher.Search(query, collector);
            ScoreDoc[] hits = collector.TopDocs().ScoreDocs;
            float baseScore = 0;
            foreach (ScoreDoc sc in hits)
            {
                if (sc.Score > baseScore)
                {
                    baseScore = sc.Score;
                }
            }
            foreach (ScoreDoc sc in hits)
            {
                int docId = sc.Doc;
                Lucene.Net.Documents.Document doc = searcher.Doc(docId);

                float a = (sc.Score / baseScore);
                float b = a * 100;

                int percent = (int)(b);
                string[] dupDoc=(from s in similarResearchDocumentList where s.Name == doc.GetField("Name").StringValue select s.Name).ToArray();

               
                
                
                //determinig authors of document
                List<Author> authorList=new List<Author> ();
                string[] authors = Preprocessing.RemovingSpecificChar(doc.GetField("Authors").StringValue, new char[] { ',', ';' }).Split(new[] { " " },
                    StringSplitOptions.RemoveEmptyEntries);
                foreach(string autName in authors)
                         authorList=new List<Author>(){new Author(){Name=autName}};

                if (dupDoc.Length <= 0)
                {
                    similarResearchDocumentList.Add(new ResearchDocument()
                    {
                        Name = doc.GetField("Name").StringValue,
                        Title = doc.GetField("Title").StringValue,
                        Abstract = doc.GetField("Abstract").StringValue
                        ,Score=percent,
                        Body = doc.GetField("Body").StringValue,
                        Year = doc.GetField("Year").StringValue,
                        Authors = authorList,
                        Keywords = doc.GetField("Keywords").StringValue
                    });
                }
                    


            }
            return similarResearchDocumentList;
        }

        /// <summary>
        /// search in indexed documents based on one field and return list of research document name with their similarity percent   
        /// </summary>
        /// <param name="querystring">the search string entered by user </param>
        /// <param name="field">field that search operation is based on it</param>
        /// <param name="hits">hits per page for resault search</param>
        /// <returns> returns a list of ResearchDocument object that match to search operation</returns>

        public static SortedDictionary<string, float> SearchWithQueryParser(Dictionary<SectionField, string> field_queryString, int _hits, QueryParser.Operator _operator)
        {
            directory = FSDirectory.Open(_luceneDirectory);
            SortedDictionary<string, float> docDictionary = new SortedDictionary<string, float>();
            Query query = null;

            query = MultiFieldQueryCreator(field_queryString, _operator);
            
            int HITS_PER_PAGE = _hits;
            TopScoreDocCollector collector = TopScoreDocCollector.Create(HITS_PER_PAGE, true);
            IndexSearcher searcher = new IndexSearcher(IndexReader.Open(directory, true));

            searcher.Search(query, collector);
            ScoreDoc[] hits = collector.TopDocs().ScoreDocs;
            float baseScore = 0;
            foreach (ScoreDoc sc in hits)
            {
                if (sc.Score > baseScore)
                {
                    baseScore = sc.Score;
                }
            }
            foreach (ScoreDoc sc in hits)
            {
                int docId = sc.Doc;
                Lucene.Net.Documents.Document doc = searcher.Doc(docId);
                
                float a = (sc.Score / baseScore);
                float b = a * 100;

                int percent = (int)(b);

                if (!docDictionary.ContainsKey(doc.GetField("Name").StringValue))
                docDictionary.Add(doc.GetField("Name").StringValue, percent);
                

            }
            return docDictionary;
        }

        public static SortedDictionary<string, float> SearchWithQueryParser(string queryString, SectionField field, int _hits, QueryParser.Operator _operator)
        {
            directory = FSDirectory.Open(_luceneDirectory);
            SortedDictionary<string, float> docDictionary = new SortedDictionary<string, float>();
            Query query = null;

            query = QueryCreator(queryString,field, _operator);

            int HITS_PER_PAGE = _hits;
            TopScoreDocCollector collector = TopScoreDocCollector.Create(HITS_PER_PAGE, true);
            IndexSearcher searcher = new IndexSearcher(IndexReader.Open(directory, true));

            searcher.Search(query, collector);
            ScoreDoc[] hits = collector.TopDocs().ScoreDocs;
            float baseScore = 0;
            foreach (ScoreDoc sc in hits)
            {
                if (sc.Score > baseScore)
                {
                    baseScore = sc.Score;
                }
            }
            foreach (ScoreDoc sc in hits)
            {
                int docId = sc.Doc;
                Lucene.Net.Documents.Document doc = searcher.Doc(docId);

                float a = (sc.Score / baseScore);
                float b = a * 100;

                int percent = (int)(b);

                if (!docDictionary.ContainsKey(doc.GetField("Name").StringValue))
                docDictionary.Add(doc.GetField("Name").StringValue, percent);


            }
            return docDictionary;
        }

        public static SortedDictionary<string, float> SearchWithModel(List<Search> searchModels, int _hits)
        {
            directory = FSDirectory.Open(_luceneDirectory);
            SortedDictionary<string, float> docDictionary = new SortedDictionary<string, float>();
            Query query = null;
            BooleanQuery BQuery = new BooleanQuery ();
            foreach (var sm in searchModels)
            {
                switch (sm.Mode)
                {
                    case SearchMode.AtLeastOneTerm: query = QueryCreator(sm.value, sm.Field, QueryParser.OR_OPERATOR);
                        break;
                    case SearchMode.AllTerms: query = QueryCreator(sm.value, sm.Field, QueryParser.AND_OPERATOR);
                        break;
                    case SearchMode.ExactlyPhrase: query = PhraseQueryCreator(sm.value, sm.Field, 0);
                        break;
                }
                
                BQuery.Add(query, sm.occur);
            }

            int HITS_PER_PAGE = _hits;
            TopScoreDocCollector collector = TopScoreDocCollector.Create(HITS_PER_PAGE, true);
            IndexSearcher searcher = new IndexSearcher(IndexReader.Open(directory, true));
            FastVectorHighlighter fvHighlighter = new FastVectorHighlighter(true, true);

            searcher.Search(BQuery, collector);
            ScoreDoc[] hits = collector.TopDocs().ScoreDocs;
            float baseScore = 0;
            foreach (ScoreDoc sc in hits)
            {
                if (sc.Score > baseScore)
                {
                    baseScore = sc.Score;
                }
            }
           
           // Query q= qp.Parse("+Authors:\"بهادری\"");
            //Query q = qp.Parse(searchModels[0].value);
            //TopDocs tdocs = searcher.Search(BQuery, 10);
        /*    for (int i = 0; i < tdocs.ScoreDocs.Length; i++)
            {
                string bestfragment =
fvHighlighter.GetBestFragment(fvHighlighter.GetFieldQuery(query),
searcher.IndexReader, tdocs.ScoreDocs[i].Doc, "Authors", 20);
                  string abs = bestfragment;
                  string bestfragment2 =
  fvHighlighter.GetBestFragment(fvHighlighter.GetFieldQuery(BQuery),
  searcher.IndexReader, tdocs.ScoreDocs[i].Doc, "Abstract", 20);
                  string abs2 = bestfragment;

            }

*/
                foreach (ScoreDoc sc in hits)
                {
                    int docid = sc.Doc;
                    Lucene.Net.Documents.Document doc = searcher.Doc(docid);

                    string Authorsbestfragment =
fvHighlighter.GetBestFragment(fvHighlighter.GetFieldQuery(BQuery),
searcher.IndexReader, docid, "Authors", 20);
                    string Abstractbestfragment =
fvHighlighter.GetBestFragment(fvHighlighter.GetFieldQuery(BQuery),
searcher.IndexReader, docid, "Abstract", 20);
                  
                    float a = (sc.Score / baseScore);
                    float b = a * 100;

                    int percent = (int)(b);

                    if (!docDictionary.ContainsKey(doc.GetField("Name").StringValue))
                        docDictionary.Add(doc.GetField("Name").StringValue, percent);


                }
            return docDictionary;
        }

        /// <summary>
        /// search in indexed documents based on multiple field
        /// </summary>
        /// <param name="querystring">the search string entered by user</param>
        /// <param name="field">fields that search operation is based on them</param>
        /// <param name="hits">hits per page for resault search</param>
        /// <returns>returns a list of document name  that match to search operation</returns>
        public static SortedDictionary<string, float> SearchWithQueryParser(string querystring, List<SectionField> fields, int _hits, QueryParser.Operator _operator)
        {
            SortedDictionary<string, float> docDictionary = new SortedDictionary<string, float>();
            directory = FSDirectory.Open(_luceneDirectory);
            List<string> nameList = new List<string>();
            Query query = null;

            query = MultiFieldQueryCreator(querystring, fields, _operator);

            int HITS_PER_PAGE = _hits;
            TopScoreDocCollector collector = TopScoreDocCollector.Create(HITS_PER_PAGE, true);
            IndexSearcher searcher = new IndexSearcher(IndexReader.Open(directory, true));

            //**************

            //************




            searcher.Search(query, collector);
            ScoreDoc[] hits = collector.TopDocs().ScoreDocs;
            float baseScore = 0;
            foreach (ScoreDoc sc in hits)
            {
                if (sc.Score > baseScore)
                {
                    baseScore = sc.Score;
                }
            }
            foreach (ScoreDoc sc in hits)
            {
                int docId = sc.Doc;
                Lucene.Net.Documents.Document doc = searcher.Doc(docId);

                float a = (sc.Score / baseScore);
                float b = a * 100;

                int percent = (int)(b);

                if (!docDictionary.ContainsKey(doc.GetField("Name").StringValue))
                docDictionary.Add(doc.GetField("Name").StringValue, percent);


            }
            return docDictionary;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="querystring"></param>
        /// <param name="field"></param>
        /// <param name="_hits"></param>
        /// <param name="slop"></param>
        /// <returns></returns>
        public static SortedDictionary<string, float> SearchWithPhraseQuery(string querystring, List<SectionField> fields, int _hits, int slop, QueryParser.Operator _operator)
        {
            directory = FSDirectory.Open(_luceneDirectory);
            SortedDictionary<string, float> docDictionary = new SortedDictionary<string, float>();



           PhraseQuery query = PhraseQueryCreator(querystring, fields, slop);

            int HITS_PER_PAGE = _hits;
            TopScoreDocCollector collector = TopScoreDocCollector.Create(HITS_PER_PAGE, true);
            IndexSearcher searcher = new IndexSearcher(IndexReader.Open(directory, true));
            searcher.Search(query, collector);
            ScoreDoc[] hits = collector.TopDocs().ScoreDocs;

            
            

            foreach (ScoreDoc sc in hits)
            {
                int docId = sc.Doc;
                Lucene.Net.Documents.Document doc = searcher.Doc(docId);

                // inverse length normalization
                Explanation expl = searcher.Explain(query, sc.Doc);
                string desc = expl.Description.ToString();
                Explanation[] expl2 = expl.GetDetails();
                /*float lengthNorm=expl2[2].Value;
                string d = expl2[2].Description;
                if (d.Contains("fieldNorm"))
                {
                    bool flag = true;
                }*/

                //calculate tf
                Similarity simi = new DefaultSimilarity();
                


                //calculate idf
                IndexReader reader = IndexReader.Open(directory, true);
                    float docfreq = reader.DocFreq(new Term("Abstract", querystring));
               float maxdoc = reader.MaxDoc;
               float idf = 1.0f + Convert.ToSingle(Math.Log((maxdoc / (docfreq + 1.0f)), Math.E));

                
                var termfreq=reader.GetTermFreqVector(sc.Doc, "Abstract");
                string [] terms=termfreq.GetTerms();
                int[] locations = termfreq.GetTermFrequencies();
                
                float a = (sc.Score );//lengthNorm;
                float b = a;
                var index=Array.IndexOf(terms,querystring);
                var frq=0;
                if (index != -1)
                {
                     frq = locations[index];
                }
                


                int percent = (int)(b);

                var simiidf = simi.Idf(Convert.ToInt32(docfreq), Convert.ToInt32(maxdoc));
                var simitf = simi.Tf(frq);

                DefaultSimilarity defsimi = new DefaultSimilarity();
                var defsimiidf = defsimi.Idf(Convert.ToInt32(docfreq), Convert.ToInt32(maxdoc));
                var defsimitf = defsimi.Tf(Convert.ToSingle(frq));

                if (!docDictionary.ContainsKey(doc.GetField("Name").StringValue))
                docDictionary.Add(doc.GetField("Name").StringValue, percent);


            }
            
           

            return docDictionary;
        }

        public static SortedDictionary<string, float> SearchWithPhraseQuery(Dictionary<SectionField, string> field_phrase, int _hits, int slop, QueryParser.Operator _operator)
        {
            directory = FSDirectory.Open(_luceneDirectory);
            SortedDictionary<string, float> docDictionary = new SortedDictionary<string, float>();



            PhraseQuery query = PhraseQueryCreator(field_phrase, slop);

            int HITS_PER_PAGE = _hits;
            TopScoreDocCollector collector = TopScoreDocCollector.Create(HITS_PER_PAGE, true);
            IndexSearcher searcher = new IndexSearcher(IndexReader.Open(directory, true));
            searcher.Search(query, collector);
            ScoreDoc[] hits = collector.TopDocs().ScoreDocs;




            foreach (ScoreDoc sc in hits)
            {
                int docId = sc.Doc;
                Lucene.Net.Documents.Document doc = searcher.Doc(docId);

                // inverse length normalization
                Explanation expl = searcher.Explain(query, sc.Doc);
                string desc = expl.Description.ToString();
                Explanation[] expl2 = expl.GetDetails();
                /*float lengthNorm=expl2[2].Value;
                string d = expl2[2].Description;
                if (d.Contains("fieldNorm"))
                {
                    bool flag = true;
                }*/

              /*  //calculate tf
                Similarity simi = new DefaultSimilarity();



                //calculate idf
                IndexReader reader = IndexReader.Open(directory, true);
                float docfreq = reader.DocFreq(new Term("Abstract", querystring));
                float maxdoc = reader.MaxDoc;
                float idf = 1.0f + Convert.ToSingle(Math.Log((maxdoc / (docfreq + 1.0f)), Math.E));


                var termfreq = reader.GetTermFreqVector(sc.Doc, "Abstract");
                string[] terms = termfreq.GetTerms();
                int[] locations = termfreq.GetTermFrequencies();

                float a = (sc.Score);//lengthNorm;
                float b = a;
                var index = Array.IndexOf(terms, querystring);
                var frq = 0;
                if (index != -1)
                {
                    frq = locations[index];
                }



                int percent = (int)(b);

                var simiidf = simi.Idf(Convert.ToInt32(docfreq), Convert.ToInt32(maxdoc));
                var simitf = simi.Tf(frq);

                DefaultSimilarity defsimi = new DefaultSimilarity();
                var defsimiidf = defsimi.Idf(Convert.ToInt32(docfreq), Convert.ToInt32(maxdoc));
                var defsimitf = defsimi.Tf(Convert.ToSingle(frq));

                docDictionary.Add(doc.GetField("Name").StringValue, percent);*/


            }



            return docDictionary;
        }


        public static SortedDictionary<string, float> SearchWithPhraseQuery(string querystring, SectionField field, int _hits, int slop, QueryParser.Operator _operator)
        {
            directory = FSDirectory.Open(_luceneDirectory);
            SortedDictionary<string, float> docDictionary = new SortedDictionary<string, float>();



            Query query = PhraseQueryCreator(querystring, field, slop);

            int HITS_PER_PAGE = _hits;
            TopScoreDocCollector collector = TopScoreDocCollector.Create(HITS_PER_PAGE, true);
            IndexSearcher searcher = new IndexSearcher(IndexReader.Open(directory, true));
            searcher.Search(query, collector);
            ScoreDoc[] hits = collector.TopDocs().ScoreDocs;

            foreach (ScoreDoc sc in hits)
            {
                int docId = sc.Doc;
                Lucene.Net.Documents.Document doc = searcher.Doc(docId);
                float percent = ((sc.Score) / hits.First().Score) * 100;

                if (!docDictionary.ContainsKey(doc.GetField("Name").StringValue))
                docDictionary.Add(doc.GetField("Name").StringValue, percent);
            }

            return docDictionary;
        }

        public static SortedDictionary<string, float> SearchWithPhraseQueryForTest(string querystring, SectionField field, int _hits, int slop, QueryParser.Operator _operator)
        {
            directory = FSDirectory.Open(_luceneDirectory);
            SortedDictionary<string, float> docDictionary = new SortedDictionary<string, float>();



            PhraseQuery query = PhraseQueryCreator(querystring, field, slop);

            int HITS_PER_PAGE = _hits;
            TopScoreDocCollector collector = TopScoreDocCollector.Create(HITS_PER_PAGE, true);
            IndexSearcher searcher = new IndexSearcher(IndexReader.Open(directory, true));
            searcher.Search(query, collector);
            ScoreDoc[] hits = collector.TopDocs().ScoreDocs;




            foreach (ScoreDoc sc in hits)
            {
                int docId = sc.Doc;
                Lucene.Net.Documents.Document doc = searcher.Doc(docId);

                // inverse length normalization
                Explanation expl = searcher.Explain(query, sc.Doc);
                string desc = expl.Description.ToString();
                Explanation[] expl2 = expl.GetDetails();
                /*float lengthNorm=expl2[2].Value;
                string d = expl2[2].Description;
                if (d.Contains("fieldNorm"))
                {
                    bool flag = true;
                }*/

                //calculate tf
                Similarity simi = new DefaultSimilarity();



                //calculate idf
                IndexReader reader = IndexReader.Open(directory, true);
                float docfreq = reader.DocFreq(new Term("Abstract", querystring));
                float maxdoc = reader.MaxDoc;
                float idf = 1.0f + Convert.ToSingle(Math.Log((maxdoc / (docfreq + 1.0f)), Math.E));


                var termfreq = reader.GetTermFreqVector(sc.Doc, "Abstract");
                string[] terms = termfreq.GetTerms();
                int[] locations = termfreq.GetTermFrequencies();

                float a = (sc.Score);//lengthNorm;
                float b = a;
                var index = Array.IndexOf(terms, querystring);
                var frq = 0;
                if (index != -1)
                {
                    frq = locations[index];
                }



                int percent = (int)(b);

                var simiidf = simi.Idf(Convert.ToInt32(docfreq), Convert.ToInt32(maxdoc));
                var simitf = simi.Tf(frq);

                DefaultSimilarity defsimi = new DefaultSimilarity();
                var defsimiidf = defsimi.Idf(Convert.ToInt32(docfreq), Convert.ToInt32(maxdoc));
                var defsimitf = defsimi.Tf(Convert.ToSingle(frq));

                if (!docDictionary.ContainsKey(doc.GetField("Name").StringValue))
                docDictionary.Add(doc.GetField("Name").StringValue, percent);


            }



            return docDictionary;
        }

        /// <summary>
        /// returns name of all indexed documents
        /// </summary>
        /// <returns>list of all indexed documents</returns>
        public static List<string> GetAllDocumentName()
        {
            directory = FSDirectory.Open(_luceneDirectory);
            if (directory.ListAll().Count() > 0)
            {
                List<string> documentNameList = new List<string>();
                IndexReader reader = IndexReader.Open(directory, true);
                var j = reader.NumDocs();
                for (int i = 0; i <j ; i++)
                {
                   
                    if (reader.IsDeleted(i))
                    {
                        j++;
                        continue;
                    }
                     
                    Lucene.Net.Documents.Document doc = reader.Document(i);
                    documentNameList.Add(doc.GetField("Name").StringValue);

                }

                reader.Dispose();
                return documentNameList;
            }
            else {
                return new List<string>();
            }
           
        }

        public static ResearchDocument AddDocument(string docname, BaseFormat format, ResearchDocument documentModel, StructuralType strtype)
        {
            Preprocessing preprocessingObject = new Preprocessing();
            ResearchDocument researchDocument = new ResearchDocument();
            

            XMLEngine xmlEngine = new XMLEngine();





            switch (format)
            {
                case BaseFormat.XML: researchDocument = preprocessingObject.CreateResearchDocFromXML(docname);
                    break;
                case BaseFormat.WORD: researchDocument = preprocessingObject.WordSectionsExtraction(docname, documentModel, strtype);
                    break;

            }

            //
            // indexing
            //
            #region indexing

            var searchengine = new SearchEngine();

            if (IsIndexd(researchDocument.Name))
                UpdateIndex(researchDocument.Name, researchDocument);
            else
                CreateIndex(researchDocument);
           

            #endregion

            return researchDocument;
        }
        public static ResearchDocument AddDocument(string docname, BaseFormat format, StructuralType strtype)
        {
            Preprocessing preprocessingObject = new Preprocessing();
            ResearchDocument researchDocument = new ResearchDocument();


            XMLEngine xmlEngine = new XMLEngine();





            switch (format)
            {
                case BaseFormat.XML: researchDocument = preprocessingObject.CreateResearchDocFromXML(docname);
                    break;
                case BaseFormat.WORD: researchDocument = preprocessingObject.WordSectionsExtraction(docname, strtype);
                    break;

            }

            //
            // indexing
            //
            #region indexing

            var searchengine = new SearchEngine();
            if (IsIndexd(researchDocument.Name))
                UpdateIndex(researchDocument.Name, researchDocument);
            else
                CreateIndex(researchDocument);

            #endregion

            return researchDocument;
        }
        public static bool AddDocument(ResearchDocument researchDocument)
        {

            // indexing the document

            var searchengine = new SearchEngine();
            if (IsIndexd(researchDocument.Name))
                UpdateIndex(researchDocument.Name, researchDocument);
            else
                CreateIndex(researchDocument);
            return true;
        }
        public static Query QueryOR(List<Query> queries)
        {
            BooleanQuery bquery = new BooleanQuery();
         foreach(Query q in queries )
            bquery.Add(q, Occur.SHOULD);

         return bquery;
        }
        public static Query QueryOR(Query Q1,Query Q2)
        {
            BooleanQuery bquery = new BooleanQuery();
            
                bquery.Add(Q1, Occur.SHOULD);
                bquery.Add(Q2, Occur.SHOULD);
            return bquery;
        }
        public static Query QueryAND(List<Query> queries)
        {
            BooleanQuery bquery = new BooleanQuery();
            foreach (Query q in queries)
                bquery.Add(q, Occur.MUST);

            return bquery;
        }
        public static Query QueryAND(Query Q1, Query Q2)
        {
            BooleanQuery bquery = new BooleanQuery();

            bquery.Add(Q1, Occur.MUST);
            bquery.Add(Q2, Occur.MUST);
            return bquery;
        }

        #endregion
        

        

       

       
    }
}