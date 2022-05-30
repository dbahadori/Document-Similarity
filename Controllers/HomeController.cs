
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DocumentSimilarity.BLL;
using DocumentSimilarity.Models;
using PagedList;
using System.Collections;
using DocumentSimilarity.Models.Services;
using Lucene.Net.QueryParsers;
using System.Drawing;
using System.IO;
using Novacode;
using DocumentSimilarity.BLL.Services;
using NLog;

namespace DocumentSimilarity.Controllers
{

    public class HomeController : Controller
    {
        public const int RecordsPerPage = 10;
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public HomeController()
        {
            ViewBag.RecordsPerPage = RecordsPerPage;
        }



        [HttpGet]
        public ActionResult Index()
        {

          //adding document to system
            /*   AddDocument("document1.doc",BaseFormat.WORD);
               AddDocument("document3.doc",BaseFormat.WORD);
               AddDocument("document5.doc",BaseFormat.WORD);
               AddDocument("document6.doc",BaseFormat.WORD);
               AddDocument("document4.doc",BaseFormat.WORD);
               AddDocument("document2.doc",BaseFormat.WORD);
               AddDocument("doc4.doc", BaseFormat.WORD);
               SearchEngine.AddDocument("doc3.doc", BaseFormat.WORD);*/

            Session["counter"] = 0;


            return View();
        }


          [HttpPost]
        public JsonResult UploadFile(string Operation)
        {
            bool isSavedSuccessfully = true;
            string fName = "";
            try
            {
                foreach (string fileName in Request.Files)
                {
                    HttpPostedFileBase file = Request.Files[fileName];
                    //Save file content goes here
                    fName = file.FileName;
                    if (file != null && file.ContentLength > 0)
                    {

                        var originalDirectory = new DirectoryInfo(Server.MapPath(@"~/Documents/Word/test/"));

                        string pathString = System.IO.Path.Combine(originalDirectory.ToString(), "imagepath");

                        var fileName1 = Path.GetFileName(file.FileName);

                        bool isExists = System.IO.Directory.Exists(pathString);

                        if (!isExists)
                            System.IO.Directory.CreateDirectory(pathString);

                        var path = string.Format("{0}\\{1}", pathString, file.FileName);
                        file.SaveAs(path);

                    }

                }

            }
            catch (Exception ex)
            {
                isSavedSuccessfully = false;
            }
              string init="<img style='height:160px;width:160px' src='/Images/header.png' class='file-preview-image'>";
              
            if (isSavedSuccessfully)
            {
                var j=Json(new { initialPreview = init, initialPreviewConfig = new { caption = "test", width = "200px", url = "Home/DeleteDirectory", key = "header" },append=true });
                return j;
                    
            }
            else
            {
                return Json(new { Message = "Error in saving file" });
            }
        }
          
        [HttpPost]
          public ActionResult DeleteDirectory()
          {
              var originalDirectory = new DirectoryInfo(Server.MapPath(@"~/Documents/Word/test"));



              System.IO.Directory.Delete(originalDirectory.ToString(),true);

              return Json(new {Message="deleted"});
          }

        [HttpGet]
        [Authorize]
        public ActionResult UploadDocument()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult UploadDocument(ResearchDocument model,  List<HttpPostedFileBase> file, string FileType)
        {
            SavingAndIndexingNewDocument(model, file, FileType, false);
            return RedirectToAction("Display");
        }

        [Authorize]
        public ActionResult AdvanceSearch()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult AdvanceSearch(string OverallContenttxt, string Bodytxt, string Bodycombo, string Titletxt, string Titlecombo, string Authorstxt, string Authorscombo, string Keywordstxt, string Keywordscombo,
            string Yeartxt, string Yearcombo, string Abstracttxt, string Abstractcombo, string Authorsradio, string Keywordsradio, string Yearsradio, string Abstractradio)
        {
            SearchService searchService = new SearchService();
            IEnumerable<ResearchDocument> resultDocuments = new List<ResearchDocument>();
            string viewPage = "AdvanceSearchDisplay";
            if (OverallContenttxt != null)
            {
                resultDocuments = searchService.SectionSearch(OverallContenttxt, SectionField.All, SearchMode.AtLeastOneTerm, 200);
                viewPage = "DocumentsDisplay";
            }
            else
            {
                if (Bodytxt != null)
                {
                    resultDocuments = searchService.SectionSearch(Bodytxt, SectionField.Body, (SearchMode)Convert.ToInt32(Bodycombo), 200);
                }
                else
                {
                    Dictionary<SectionField, string> sectionfields = new Dictionary<SectionField, string>();
                    sectionfields.Add(SectionField.Title, Titletxt);
                    List<Search> searchModels = new List<Models.Search>();

                    if (Titletxt != null)
                        searchModels.Add(new Search() { Field = SectionField.Title, value = Titletxt, Mode = (SearchMode)Convert.ToInt32(Titlecombo), Slop = 0, occur = Lucene.Net.Search.Occur.MUST });
                    if (Authorstxt != null)
                        searchModels.Add(new Search() { Field = SectionField.Authors, value = Authorstxt, Mode = (SearchMode)Convert.ToInt32(Authorscombo), Slop = 0, occur = Authorsradio == "SHOULD" ? Lucene.Net.Search.Occur.SHOULD : Lucene.Net.Search.Occur.MUST });
                    if (Keywordstxt != null)
                        searchModels.Add(new Search() { Field = SectionField.Keywords, value = Keywordstxt, Mode = (SearchMode)Convert.ToInt32(Keywordscombo), Slop = 0, occur = Keywordsradio == "SHOULD" ? Lucene.Net.Search.Occur.SHOULD : Lucene.Net.Search.Occur.MUST });
                    if (Yeartxt != null)
                        searchModels.Add(new Search() { Field = SectionField.Year, value = Yeartxt, Mode = (SearchMode)Convert.ToInt32(Yearcombo), Slop = 0, occur = Yearsradio == "SHOULD" ? Lucene.Net.Search.Occur.SHOULD : Lucene.Net.Search.Occur.MUST });
                    if (Abstracttxt != null)
                        searchModels.Add(new Search() { Field = SectionField.Abstract, value = Abstracttxt, Mode = (SearchMode)Convert.ToInt32(Abstractcombo), Slop = 0, occur = Abstractradio == "SHOULD" ? Lucene.Net.Search.Occur.SHOULD : Lucene.Net.Search.Occur.MUST });
                    resultDocuments = searchService.ModelSearch(searchModels, 200);

                }
            }
           


            LoadSearchedDocumentsToSession(resultDocuments);

            Dictionary<string, int> docwithID = new Dictionary<string, int>();
            var id = 1;
            if (resultDocuments != null && resultDocuments.Count() > 0)
            {
                foreach (var d in resultDocuments)
                {
                    docwithID.Add(d.Name, id++);
                }
                Session["docID"] = docwithID;
            }

            return View(viewPage, resultDocuments);
        }
        public ActionResult Search(string authorName, string title, string abstractString, string keywords, string body, int? pageNum)
        {
            authorName = "";
            title = "";
            Session["counter"] = Convert.ToInt32(Session["counter"]) + 1;
            keywords = "";
            body = "";

            Timer timer = new Timer();
            timer.Start();
            string searchString = abstractString;
            ViewBag.SearchString = abstractString;

            // 
            //  query indexing and updating
            //
           /* #region query indexing and updating

            List<Author> auts = new List<Author>();
            Author aut = new Author();
            aut.Name = authorName;
            auts.Add(aut);

            IndexQuery("TempQuery-" + Convert.ToInt32(Session["counter"]), title, auts, abstractString, keywords, body);
            if ( Convert.ToInt32(Session["counter"])>1)
            deleteDocument("TempQuery-" + (Convert.ToInt32(Session["counter"]) - 1).ToString());
            #endregion */


            string interval = timer.Stop().ToString();
            string searchtime = "زمان جستجو (" + interval + " ثانیه)";
            ViewBag.SearchTime = searchtime;


            //paging with scroll
            pageNum = pageNum ?? 0;
            ViewBag.IsEndOfRecords = false;
            if (Request.IsAjaxRequest())
            {
                var documentsDictionary = GetRecordsForPage(pageNum.Value);
                ViewBag.IsEndOfRecords = (documentsDictionary.Any()) && ((pageNum.Value * RecordsPerPage) >= documentsDictionary.Last().Key);
                return PartialView("_DocumnentRow", documentsDictionary);
            }
            else
            {
                //
                // query string preprocessing
                //
                #region query preprocessing
                List<string> queryparams = new List<string>();
                List<string> query = new List<string>();
                Preprocessing preprocessing=new BLL.Preprocessing ();
                queryparams.Add(abstractString);

               // queryparams = Preprocessing.RemovingSpecificChar(queryparams, new char[] { '،', '؛', '.', ',', ';', ':', '/' ,'\\','@','#','$','%'});
                queryparams=Preprocessing.RemovingStopWords(queryparams);
                queryparams = preprocessing.QueryNormalizer(queryparams);
                queryparams = preprocessing.QueryLemmatizer(queryparams);

                searchString = queryparams[0];

                #endregion

                LoadSearchedDocumentsToSession(null);
                ViewBag.Documents = GetRecordsForPage(pageNum.Value);
                return View("Search");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchString"></param>
        public void LoadSearchedDocumentsToSession(IEnumerable<ResearchDocument> documents)
        {
            //var documents = documentService.ListSearchedDocuments(searchString, SectionField.Abstract, 100,3, SearchMode.ExactlyPhrase,QueryParser.Operator.AND);
            int documentIndex = 1;
            if (documents != null)
            {
                Session["Documents"] = documents.ToDictionary(x => documentIndex++, x => x);
                ViewBag.TotalNumberDocuments = documents.Count();
            }
            else
            {
                Dictionary<int, ResearchDocument> nulldoc = new Dictionary<int, ResearchDocument>();
                Session["Documents"] = nulldoc;
            }


        }
        public Dictionary<int, ResearchDocument> GetRecordsForPage(int pageNum)
        {


            Dictionary<int, ResearchDocument> documents = (Session["Documents"] as Dictionary<int, ResearchDocument>);

            int from = (pageNum * RecordsPerPage);
            int to = from + RecordsPerPage;


            return documents
                .Where(x => x.Key > from && x.Key <= to)
                .OrderBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.Value);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [Authorize]
        [HttpGet]
        public ActionResult Display()
        {
                SearchService searchService = new SearchService();
                IEnumerable<ResearchDocument> resultDocuments = new List<ResearchDocument>();
                resultDocuments=searchService.GetAllDocuments();
                LoadSearchedDocumentsToSession(resultDocuments);
                Dictionary<string, int> docwithID = new Dictionary<string, int>();
                var id = 1;
                foreach (var d in resultDocuments)
                {
                    docwithID.Add(d.Name, id++);
                }
                Session["docID"] = docwithID;

                return View("DocumentsDisplay", resultDocuments);
        }


        [HttpPost]
        [Authorize]
        public ActionResult Display(string Searchtxt, string SearchFilterCombo)
        {
            SearchService searchService = new SearchService();
            IEnumerable<ResearchDocument> resultDocuments = new List<ResearchDocument>();
            switch (SearchFilterCombo)
            {
                case "AtLeastOneWords": resultDocuments=searchService.SectionSearch(Searchtxt, SectionField.All, SearchMode.AtLeastOneTerm, 200);
                    break;
                case "AllWords": resultDocuments = searchService.SectionSearch(Searchtxt, SectionField.All, SearchMode.AllTerms, 200);
                    break;
                case "Phrase": resultDocuments = searchService.SectionSearch(Searchtxt, SectionField.All, SearchMode.ExactlyPhrase, 200);
                    break;
            }
                LoadSearchedDocumentsToSession(resultDocuments);
                Dictionary<string, int> docwithID = new Dictionary<string, int>();
                var id = 1;
                foreach (var d in resultDocuments)
                {
                    docwithID.Add(d.Name, id++);
                }
                Session["docID"] = docwithID;
                return View("DocumentsDisplay", resultDocuments);
        }

        [HttpGet]
        [Authorize]
        public ActionResult Similarity()
        {
            ViewBag.FormHeader = "تشخیص میزان درصد تشابه ما بین اسناد ";

            return View();
        }

          [Authorize]
        [HttpPost]
        public ActionResult Similarity(ResearchDocument model, HttpPostedFileBase file, string FileType, int? pageNum, string[] SimilarityFilterCombo, bool MustBeIndexCheckBox, string SimilarityMode)
        {
            ViewBag.Title = "تشخیص میزان درصد تشابه ما بین اسناد ";
            ViewBag.IconCSSClass = "fa-files-o";
            bool insertoToDB=false;
            Timer timer = new Timer();
            timer.Start();
            string searchString = model.Abstract;
            ViewBag.SearchString = model.Abstract;
            // calculate searching time
            string interval = timer.Stop().ToString();
            string searchtime = "زمان جستجو (" + interval + " ثانیه)";
            ViewBag.SearchTime = searchtime;
            //paging with scroll
            pageNum = pageNum ?? 0;
            ViewBag.IsEndOfRecords = false;
            if (Request.IsAjaxRequest())
            {
                var documentsDictionary = GetRecordsForPage(pageNum.Value);
                ViewBag.IsEndOfRecords = (documentsDictionary.Any()) && ((pageNum.Value * RecordsPerPage) >= documentsDictionary.Last().Key);
                return PartialView("_Similarity DocumnentRow", documentsDictionary);
            }
            else
            {

                SimilarityService similaritySerive = new SimilarityService();
                ResearchDocument basedResearchdocument = new ResearchDocument();
                IEnumerable<ResearchDocument> resultDocuments = new List<ResearchDocument>();
                List<ResearchDocument> savedResearchdocuments = new List<ResearchDocument>();
                //research document that must be compare
                savedResearchdocuments = SavingAndIndexingNewDocument(model, new List<HttpPostedFileBase>() { file }, FileType, false);
                basedResearchdocument = savedResearchdocuments[0];
                //detect similar document
                List<SectionField> sectionFieldQuery= new List<SectionField>();
                //if user want to detect similarity of document overall
                if (SimilarityFilterCombo.Count() == 5 || SimilarityFilterCombo.Contains("All") || SimilarityFilterCombo.Count() == 4)
                    resultDocuments = similaritySerive.PearToPearSimilarity(basedResearchdocument, 200);
                else
                {
                    foreach(string s in SimilarityFilterCombo)
                     sectionFieldQuery.Add((SectionField)Enum.Parse(typeof(SectionField),s)) ;
                    resultDocuments = similaritySerive.SectionSimilarity(sectionFieldQuery,basedResearchdocument, 200);

                }
                //check for indexing the document that must be compare 
                if (!MustBeIndexCheckBox) SearchEngine.DeleteIndex(basedResearchdocument.Name);




                //delete base file from results
                resultDocuments=(from r in (resultDocuments.ToList()) where r.Name != Preprocessing.ConvertDocumentName(file.FileName) select r).ToList();
                   
                LoadSearchedDocumentsToSession(resultDocuments);
                ViewBag.Documents = GetRecordsForPage(pageNum.Value);
                return View("SimilarityDisplay");
            }
            
        }

        
        public ActionResult test()
        {
            return View();

        }

        [HttpPost]
        public ActionResult test(string ttt)
        {
            var x = ttt;
            return View();

        }


        private void deleteDocument(string name)
        {

            bool res = SearchEngine.DeleteIndex(name);


        }

        private List<ResearchDocument> SavingAndIndexingNewDocument(ResearchDocument model, List<HttpPostedFileBase> files, string FileType, bool create)
        {
            List<ResearchDocument> savedResearchDocumentList = new List<ResearchDocument>();
            try
            {
            foreach (var file in files)
            {
                string path = "";
                string fileNmae = Path.GetFileName(file.FileName);
                ResearchDocument savedResearchDocument = new ResearchDocument();

                //*********** saving and indexing new document ********
               
                    if (file != null && file.ContentLength > 0)
                    {
                        String FileExtn = System.IO.Path.GetExtension(file.FileName).ToLower();
                        if (!(FileExtn == ".doc" || FileExtn == ".docx"))
                        {
                            ViewBag.error = "فقط فایل های با پسوند docx , doc  را می توانید بارگزاری کنید!";
                        }
                        else
                        {
                            if (FileType == "Structural")
                            {
                                path = Server.MapPath(@"~/Documents/Word/Structural/" + fileNmae);

                                // 1. word document saving
                                file.SaveAs(path);

                                //2. indexing new document using word document



                                savedResearchDocument = SearchEngine.AddDocument(fileNmae, BaseFormat.WORD, StructuralType.Structural);

                                //  SearchEngine.UpdateIndex(fileNmae,savedResearchDocument);




                            }
                            else
                                if (FileType == "Unstructural")
                                {
                                    path = Server.MapPath(@"~/Documents/Word/Unstructural/" + fileNmae);

                                    List<Author> aut = model.Authors;
                                    Preprocessing preprocessing = new Preprocessing();
                                    XMLEngine xmlEngine = new XMLEngine();

                                    // 1. word document saving
                                    file.SaveAs(path);

                                    //2. document creation
                                    ResearchDocument reDocument = preprocessing.WordSectionsExtraction(fileNmae, model, StructuralType.Unstructural);


                                    //4. indexing new document using model
                                    SearchEngine.AddDocument(reDocument);

                                    savedResearchDocument = reDocument;
                                }

                        }

                    }
                savedResearchDocumentList.Add(savedResearchDocument);
                }
                    //**************
            return savedResearchDocumentList;
                }

                catch (Exception e)
                {
                    return new List<ResearchDocument>();
                }
            
            
            
        }

        [Authorize]
        [AuthorizeAdmin]
        public ActionResult Configuration()
        {
            return View();
        }
        public string DeleteData(int id){
            var name = ((Dictionary<string, int>)Session["docID"]).FirstOrDefault(x => x.Value == id).Key;
            if (SearchEngine.DeleteIndex(name))
            {
                ((Dictionary<string, int>)Session["docID"]).Remove(name);
                return "ok";
            }
                
            else return "Error in delete operation";
        }
        public string UpdateData(int id, string value, int? rowId,
               int? columnPosition, int? columnId, string columnName) { return "error"; }
        public int AddData(string name, string address, string town, int? country) { return -1; }

        [Authorize]
        public FileResult Download(string docName, string strType)
        {string contentType="";
            if(docName.Contains("-docx"))
            {
                contentType="application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            }
            else
            {
                if(docName.Contains("-doc"))
                {
                   contentType="application/msword";
                }
            }

            return File(Server.MapPath(@"~/Documents/Word/" +strType+"/"+Preprocessing.ConvertDocumentName( docName)),contentType,Preprocessing.ConvertDocumentName( docName));
        }
    }
}