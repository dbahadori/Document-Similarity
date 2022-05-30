using DocumentSimilarity.Models;
using NHazm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Office.Interop.Word;
using Lucene.Net.Analysis.Fa;

namespace DocumentSimilarity.BLL
{
    public class Preprocessing
    {
        private static string _stopWords = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath(@"~/Documents/StopWords/PersianStopWords.txt"),System.Text.Encoding.UTF8);


        //
        // public methods
        //
        #region

        /// <summary>
        /// parsing word document and extract sections from it
        /// </summary>
        /// <param name="documentname"></param>
        /// <returns></returns>
        public ResearchDocument WordSectionsExtraction(string documentname, StructuralType type)
        {
                object objNull = System.Reflection.Missing.Value;
                XMLEngine xmlEngine = new XMLEngine();
                string path = "";
                switch (type)
                {
                    case StructuralType.Structural: path = HttpContext.Current.Server.MapPath(@"~/Documents/Word/Structural/" + documentname);
                        break;
                    case StructuralType.Unstructural: path = HttpContext.Current.Server.MapPath(@"~/Documents/Word/Unstructural/" + documentname);
                        break;
                }

                //create a parser object for parsing word document and extract specified section of content from it (title, authors, keywords, abstract and body)
                Parser parser = new Parser();
                Microsoft.Office.Interop.Word.Application wordApp = new Microsoft.Office.Interop.Word.Application();
                Microsoft.Office.Interop.Word.Document doc = parser.ReadWordDoc(path, wordApp);
                try
                {

                //
                //Extracting Sections 
                //

                    #region Extracting Sections

                    //extract title from word document
                    string titleSection = parser.GetSection(doc, new string[] { "عنوان", "موضوع", "خلاصه طرح تحقیقاتی" }, new string[] { "نویسنده", "نویسندگان", "نگارش", "مجری طرح" });

                    //extract authors from word document
                    string authorSection = parser.GetSection(doc, new string[] { "نویسنده", "نویسندگان", "نگارش", "مجری طرح" }, new string[] { "ناظر", "استاد", "اساتید", "سال", "بهمن", "اسفند", "فروردین", "اردیبهشت", "مهر", "آبان", "آدز", "دی ماه", "دیماه", "تیر", "مرداد", "خرداد", "شهریور", "پاییز", "زمستان", "بهار", "تابستان", "چکیده" });

                    //extract abstract from word document
                    string abstractSection = parser.GetSection(doc, new string[] { "چکیده", "چكيده" }, new string[] { "کلمات کلیدی", "کلمه های کلیدی", "واژه های کلیدی", "واژه‌های کلیدی", "واژههای کلیدی" });

                    //extract keywords from word document
                    string keywordSection = parser.GetSection(doc, new string[] { "کلمات کلیدی", "کلمه های کلیدی", "واژه های کلیدی", "واژه‌های کلیدی", "واژههای کلیدی" }, new string[] { "فهرست", "عناوین", "فهرست عناوین", "مقدمه", "تشریح و بیان مساله" });

                    //extract body from word document
                    string bodySection = parser.GetSection(doc, new string[] { "مقدمه" }, new string[] { "end of document" });

                    //extract year from word document
                    string yearSection = parser.GetSection(doc, new string[] { "سال" }, new string[] { "مقدمه", "چکیده", "one line after" });

                    //if the parser could not extract body section 
                    if (bodySection == null || bodySection == "")
                        bodySection = parser.TotalContent(doc);

                    #endregion


                //
                // Create ResearchDocument Object from Word Document
                //

                #region Create ResearchDocument Object from Word Document

                ResearchDocumentBLL researchDocumentBLL = new ResearchDocumentBLL();

                //A ResearchDocument object that will be passed to xml engine
                ResearchDocument documentFromWord = researchDocumentBLL.ResearchDocCreator(titleSection, AuthorBLL.GetAuthorsFromSection(authorSection), keywordSection, bodySection, ConvertDocumentName(documentname), abstractSection, yearSection,type.ToString());

                #endregion

                //
                // Create intermediate XML document
                //
                #region Create intermediate XML document

                //create a xml file that contains all documents with seperate sections as xml tags
                xmlEngine.XMLResearchDocumentCreator(documentFromWord, ConvertDocumentName(documentname));

                #endregion

                //
                // pereprocessing (Lemmatizing, Stop Words, Specific Char Removing)
                //
       #region pereprocessing (Lemmatizing, Stop Words, Specific Char Removing)




                //removing stop words from document
                ResearchDocument resDocAfterStopFiltering = ResearchDocumentStopWordsRemoving(documentFromWord);

                // normalization process has been done in section extracting process

                ResearchDocument resDocAfterLemmatization = ResearchDocumentLemmatizer(resDocAfterStopFiltering);


                #endregion




               

                // close document and Quit Word Application

                ((_Document)doc).Close(ref objNull, ref objNull, ref objNull);
                ((_Application)wordApp).Quit(ref objNull, ref objNull, ref objNull);

                return resDocAfterLemmatization;
            }
            catch(Exception e){

                ((_Document)doc).Close(ref objNull, ref objNull, ref objNull);
                ((_Application)wordApp).Quit(ref objNull, ref objNull, ref objNull);
                return null;
            }
        }


        //
        // called in unstructural document similarity detection process
        //

        /// <summary>
        /// 
        /// </summary>
        /// <param name="documentname"></param>
        /// <param name="documentModel"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public ResearchDocument WordSectionsExtraction(string documentname, ResearchDocument documentModel, StructuralType type)
        {

            object objNull = System.Reflection.Missing.Value;
            XMLEngine xmlEngine = new XMLEngine();
            string path = "";

            switch (type)
            {
                case StructuralType.Structural: path = HttpContext.Current.Server.MapPath(@"~/Documents/Word/Structural/" + documentname);
                    break;
                case StructuralType.Unstructural: path = HttpContext.Current.Server.MapPath(@"~/Documents/Word/Unstructural/" + documentname);
                    break;
            }


            //create a parser object for parsing word document and extract specified section of content from it (title, authors, keywords, abstract and body)
            Parser parser = new Parser();

            Microsoft.Office.Interop.Word.Application wordApp = new Microsoft.Office.Interop.Word.Application();

            Microsoft.Office.Interop.Word.Document doc = parser.ReadWordDoc(path, wordApp);

            try
            {

                //
                //Extracting Sections 
                //

                #region Extracting Sections

                //extract title from word document
                string titleSection = parser.GetSection(doc, new string[] { "عنوان", "موضوع", "خلاصه طرح تحقیقاتی" }, new string[] { "نویسنده", "نویسندگان", "نگارش", "مجری طرح" });

                //extract authors from word document
                string authorSection = parser.GetSection(doc, new string[] { "نویسنده", "نویسندگان", "نگارش", "مجری طرح" }, new string[] { "ناظر", "استاد", "اساتید", "سال", "بهمن", "اسفند", "فروردین", "اردیبهشت", "مهر", "آبان", "آدز", "دی ماه", "دیماه", "تیر", "مرداد", "خرداد", "شهریور", "پاییز", "زمستان", "بهار", "تابستان", "چکیده" });

                //extract abstract from word document
                string abstractSection = parser.GetSection(doc, new string[] { "چکیده", "چكيده" }, new string[] { "کلمات کلیدی", "کلمه های کلیدی", "واژه های کلیدی", "واژه‌های کلیدی", "واژههای کلیدی" });

                //extract keywords from word document
                string keywordSection = parser.GetSection(doc, new string[] { "کلمات کلیدی", "کلمه های کلیدی", "واژه های کلیدی", "واژه‌های کلیدی", "واژههای کلیدی" }, new string[] { "فهرست", "عناوین", "فهرست عناوین", "مقدمه", "تشریح و بیان مساله" });

                //extract body from word document
                string bodySection = parser.GetSection(doc, new string[] { "مقدمه" }, new string[] { "منابع", "مراجع", "مأخذ", "end of document" });

                //extract year from word document
                string yearSection = parser.GetSection(doc, new string[] { "سال" }, new string[] { "مقدمه", "چکیده", "one line after" });
                #endregion


                //
                // Create ResearchDocument Object from Word Document
                //

                #region Create ResearchDocument Object from Word Document

                //replace user entry instead of document section if user enter content for each section or document does not have proper are null 
                if (documentModel.Title != null && documentModel.Title != "")
                {
                    titleSection = documentModel.Title;
                    titleSection = ContentNormalizer(titleSection);
                }

                if (documentModel.Abstract != null && documentModel.Abstract != "")
                {
                    abstractSection = documentModel.Abstract;
                    abstractSection = ContentNormalizer(abstractSection);
                }

                if (documentModel.Authors != null && documentModel.Authors[0].Name != "" && documentModel.Authors[0].Name != null)
                {
                    authorSection = documentModel.Authors[0].Name;
                    authorSection = ContentNormalizer(authorSection);
                }

                if (documentModel.Keywords != null && documentModel.Keywords != "")
                {
                    keywordSection = documentModel.Keywords;
                    keywordSection = ContentNormalizer(keywordSection);
                }

                if (documentModel.Year != null && documentModel.Year != "")
                {
                    yearSection = documentModel.Year;
                    yearSection = ContentNormalizer(yearSection);
                }


                //if the parser could not extract body section 
                if (bodySection == null || bodySection == "")
                    bodySection = parser.TotalContent(doc);

                ResearchDocumentBLL researchDocumentBLL = new ResearchDocumentBLL();

                //A ResearchDocument object that will be passed to xml engine
                ResearchDocument documentFromWord = researchDocumentBLL.ResearchDocCreator(titleSection, AuthorBLL.GetAuthorsFromSection(authorSection), keywordSection, bodySection, ConvertDocumentName(documentname), abstractSection, yearSection,type.ToString());

                #endregion

                //
                // Create intermediate XML document
                //
                #region Create intermediate XML document

                //create a xml file that contains all documents with seperate sections as xml tags
                xmlEngine.XMLResearchDocumentCreator(documentFromWord, ConvertDocumentName(documentname));

                #endregion
                //
                // pereprocessing (Lemmatizing, Stop Words, Specific Char Removing)
                //
                #region pereprocessing (Lemmatizing, Stop Words, Specific Char Removing)




                //removing stop words from document
                ResearchDocument resDocAfterStopFiltering = ResearchDocumentStopWordsRemoving(documentFromWord);

                // normalization process has been done in section extracting process

                ResearchDocument resDocAfterLemmatization = ResearchDocumentLemmatizer(resDocAfterStopFiltering);


                #endregion




                

                // close document and Quit Word Application

                ((_Document)doc).Close(ref objNull, ref objNull, ref objNull);
                ((_Application)wordApp).Quit(ref objNull, ref objNull, ref objNull);

                return resDocAfterLemmatization;
            }
            catch (Exception e)
            {

                ((_Document)doc).Close(ref objNull, ref objNull, ref objNull);
                ((_Application)wordApp).Quit(ref objNull, ref objNull, ref objNull);
                return null;
            }
        }
        public string WordSectionExtraction(string documentname, SectionField sectionFiled, StructuralType type)
        {

            object objNull = System.Reflection.Missing.Value;
            XMLEngine xmlEngine = new XMLEngine();
            string section = " ";

            string path = "";

            switch (type)
            {
                case StructuralType.Structural: path = HttpContext.Current.Server.MapPath(@"~/Documents/Word/Structural/" + documentname);
                    break;
                case StructuralType.Unstructural: path = HttpContext.Current.Server.MapPath(@"~/Documents/Word/Unstructural/" + documentname);
                    break;
            }



            //create a parser object for parsing word document and extract specified section of content from it (title, authors, keywords, abstract and body)
            Parser parser = new Parser();

            Microsoft.Office.Interop.Word.Application wordApp = new Microsoft.Office.Interop.Word.Application();

            Microsoft.Office.Interop.Word.Document doc = parser.ReadWordDoc(path, wordApp);
            

            try
            {
                //
                //Extracting Sections 
                //
                
                #region Extracting Sections
                switch (sectionFiled)
                {
                    case SectionField.Abstract:
                        //extract abstract from word document
                        section = parser.GetSection(doc, new string[] { "چکیده", "چكيده" }, new string[] { "کلمات کلیدی", "کلمه های کلیدی", "واژه های کلیدی", "واژه‌های کلیدی", "واژههای کلیدی" });
                        break;
                    case SectionField.Authors:
                        //extract authors from word document
                        section = parser.GetSection(doc, new string[] { "نویسنده", "نویسندگان", "نگارش", "مجری طرح" }, new string[] { "ناظر", "استاد", "اساتید", "سال", "بهمن", "اسفند", "فروردین", "اردیبهشت", "مهر", "آبان", "آدز", "دی", "تیر", "مرداد", "خرداد", "شهریور", "پاییز", "زمستان", "بهار", "تابستان", "چکیده" });
                        break;
                    case SectionField.Keywords:
                        //extract keywords from word document
                        section = parser.GetSection(doc, new string[] { "کلمات کلیدی", "کلمه های کلیدی", "واژه های کلیدی", "واژه‌های کلیدی", "واژههای کلیدی" }, new string[] { "فهرست", "عناوین", "فهرست عناوین", "مقدمه", "تشریح و بیان مساله" });
                        break;
                    case SectionField.Title:
                        //extract title from word document
                        section = parser.GetSection(doc, new string[] { "عنوان", "موضوع" }, new string[] { "نویسنده", "نویسندگان", "نگارش" });
                        break;

                    case SectionField.Body:
                        //extract body from word document
                        section = parser.GetSection(doc, new string[] { "مقدمه" }, new string[] { "منابع", "مراجع", "مأخذ", "end of document" });
                        break;
                    case SectionField.Year:
                        //extract year from word document
                        section = parser.GetSection(doc, new string[] { "سال" }, new string[] { "مقدمه", "چکیده", "one line after" });
                        break;
                }


                #endregion


                //
                // pereprocessing (Lemmatizing, Stop Words, Specific Char Removing)
                //
                #region pereprocessing (Lemmatizing, Stop Words, Specific Char Removing)


                //removing stop words from document
                string sectionAfterStopFiltering = RemovingStopWords(section);

                // normalization process has been done in section extracting process

                string sectionDocAfterLemmatization = SectionLemmatizer(sectionAfterStopFiltering);


                #endregion

                // close document and Quit Word Application

                ((_Document)doc).Close(ref objNull, ref objNull, ref objNull);
                ((_Application)wordApp).Quit(ref objNull, ref objNull, ref objNull);

                return sectionDocAfterLemmatization;
            }
            catch (Exception e)
            {

                ((_Document)doc).Close(ref objNull, ref objNull, ref objNull);
                ((_Application)wordApp).Quit(ref objNull, ref objNull, ref objNull);
                return null;
            }
        }

        /// <summary>
        /// parsing xml file that contain word document sections
        /// </summary>
        /// <param name="documentname"></param>
        /// <returns></returns>
        public ResearchDocument CreateResearchDocFromXML(string documentname)
        {
            XMLEngine xmlEngine=new XMLEngine();

            //A ResearchDocument that create using xml engine
            ResearchDocument documentsFromXML = new ResearchDocument();

            //read document sections from xml file
            documentsFromXML = xmlEngine.XMLResearchDocumentReader(documentname);



            //
            // pereprocessing (Lemmatizing, Stop Words, Specific Char Removing)
            //
            #region pereprocessing (Lemmatizing, Stop Words, Specific Char Removing)


            //normalization process
            ResearchDocument resDocAfterNormalization = ResearchDocumentNormalizer(documentsFromXML);

            //removing stop words from document
            ResearchDocument resDocAfterStopFiltering = ResearchDocumentStopWordsRemoving(resDocAfterNormalization);

            // lemmatization process

            ResearchDocument resDocAfterLemmatization = ResearchDocumentLemmatizer(resDocAfterStopFiltering);


            #endregion


            return resDocAfterLemmatization;
        }

       /// <summary>
       /// normalizing the content of extracted sections
       /// </summary>
       /// <param name="document"></param>
       /// <returns></returns>
        public ResearchDocument ResearchDocumentNormalizer(ResearchDocument document)
        {
            ResearchDocument normalizedDocument = new ResearchDocument();

            // using normalizer class from NHazm library to normalize content of sections
            Normalizer normalizer = new Normalizer(true, true, true);

            foreach (var prop in document.GetType().GetProperties())
            {
                // add each of research document properties to lucene document

                if (!(prop.Name == "Authors") && !(prop.Name == "Score"))
                {
                    normalizedDocument.GetType().GetProperty(prop.Name).SetValue(normalizedDocument, normalizer.Run(prop.GetValue(document, null).ToString()));
                }
            }


            normalizedDocument.Score = 0.0f;

            //
            // normalizing the authors section
            //

            #region Authors Normalizing

            List<Author> auts = new List<Author>();
            foreach (Author aut in document.Authors)
            {
                auts.Add(AuthorNormalizer(aut));
            }
            normalizedDocument.Authors = auts;
            #endregion

            return normalizedDocument;
        }

        /// <summary>
        /// lemmatizering the content of sections (properties of ResearchDocument object )
        /// </summary>
        /// <param name="document">ResearchDocument object</param>
        /// <returns>lemmatized ResearchDocument object </returns>
        public ResearchDocument ResearchDocumentLemmatizer(ResearchDocument document)
        {
            ResearchDocument lemmatizedDocument = new ResearchDocument();
            foreach (var prop in document.GetType().GetProperties())
            {
                // add each of research document properties to lucene document

                if (!(prop.Name == "Authors") && !(prop.Name == "Score")&&!(prop.Name=="Name"))
                {
                    lemmatizedDocument.GetType().GetProperty(prop.Name).SetValue(lemmatizedDocument, SectionLemmatizer(prop.GetValue(document, null).ToString()));
                }
            }

            lemmatizedDocument.Authors = document.Authors;
            lemmatizedDocument.Score = 0.0f;
            lemmatizedDocument.Name = document.Name;
            return lemmatizedDocument;
        }
        public  ResearchDocument ResearchDocumentStopWordsRemoving(ResearchDocument document)
        {
            ResearchDocument StopWordsFilteredDocument = new ResearchDocument();
            WordTokenizer wordTokenizer = new WordTokenizer(HttpContext.Current.Server.MapPath(@"~/Data/verbs.dat"));

            foreach (var prop in document.GetType().GetProperties())
            {
                // add each of research document properties to lucene document

                if (!(prop.Name == "Authors") && !(prop.Name == "Score"))
                {
                   StopWordsFilteredDocument.GetType().GetProperty(prop.Name).SetValue(StopWordsFilteredDocument,RemovingStopWords(prop.GetValue(document, null).ToString()));
                }
            }

            StopWordsFilteredDocument.Authors = document.Authors;
            StopWordsFilteredDocument.Score = 0.0f;

            return StopWordsFilteredDocument;
        }

        public  string SectionLemmatizer(string section)
        {
            string lemmatizedSection ="";
            
                Lemmatizer lemmatizer = new Lemmatizer(HttpContext.Current.Server.MapPath(@"~/Data/words.dat"), HttpContext.Current.Server.MapPath(@"~/Data/verbs.dat"), false);
                WordTokenizer wordTokenizer = new WordTokenizer(HttpContext.Current.Server.MapPath(@"~/Data/verbs.dat"));
           string sectiona=RemovingSpecificChar(section, new char[] { '،', '؛', '.', ',', ';', ':', '/', '\\', '@', '#','*','?','*','-','+','^','!','~', '$', '%',')','(','<','>','|','}','{',']','[' });
                //tokenizing each section
                List<string> tokens = wordTokenizer.Tokenize(sectiona);
            int counter=0;
                //lemmatizing each tokens
                foreach (string t in tokens)
                {

                    counter++;
                    if (t.Length > 3)
                    {
                        string lemm = lemmatizer.Lemmatize(t);
                        if (lemm.Length < 1)
                        {
                            continue;
                        }

                        if (lemm.Contains('#'))
                        {
                            List<string> conj = lemmatizer.Conjugations(lemm);
                            lemm = conj[8];
                        }
                        if (counter == 50)
                        {
                            var test = "";
                        }
                        if (counter != 1)
                        {
                            lemmatizedSection += " " + lemm;
                        }
                        else
                            lemmatizedSection += lemm;
                    }
                    
                }
            
                return lemmatizedSection;
        }

        public  List<string> SectionLemmatizer(List<string> sections)
        {
            string lemmatizedSection = "";
            List<string> lemmatizedSectionList = new List<string>();
            Lemmatizer lemmatizer = new Lemmatizer(HttpContext.Current.Server.MapPath(@"~/Data/words.dat"), HttpContext.Current.Server.MapPath(@"~/Data/verbs.dat"), false);
            WordTokenizer wordTokenizer = new WordTokenizer(HttpContext.Current.Server.MapPath(@"~/Data/verbs.dat"));

            foreach (string sec in sections)
            {
                string seca = RemovingSpecificChar(sec, new char[] { '،', '؛', '.', ',', ';', ':', '/', '\\', '@', '#', '*', '?', '*', '-', '+', '^', '!', '~', '$', '%', ')', '(', '<', '>', '|', '}', '{', ']', '[' });

                //tokenizing each section
                List<string> tokens = wordTokenizer.Tokenize(seca);
                int counter = 0;
                //lemmatizing each tokens
                foreach (string t in tokens)
                {
                    counter++;
                    if (t.Length > 3)
                    {
                        string lemm = lemmatizer.Lemmatize(t);
                        if (lemm.Length < 1)
                        {
                            continue;
                        }
                        if (lemm.Contains('#'))
                        {
                            List<string> conj = lemmatizer.Conjugations(lemm);
                            lemm = conj[8];
                        }

                        if (counter != 1)
                        {
                            lemmatizedSection += " " + lemm;
                        }
                        else
                            lemmatizedSection += lemm;
                    }
                    lemmatizedSectionList.Add(lemmatizedSection);
                }
            }
           

            return lemmatizedSectionList;
        }

        /// <summary>
        ///  normalizing the query strign parameters
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public  List<string> QueryNormalizer(List<string> parameters)
        {
            List<string> normalizedParams = new List<string>();
            string normalizedParam="";
            foreach (string p in parameters)
            {
                Normalizer normalizer = new Normalizer();
                normalizedParam = normalizer.Run(p);
                normalizedParams.Add(normalizedParam);
            }
            return normalizedParams;
        }

        /// <summary>
        ///  lemmatizing the query strign parameters
        /// </summary>
        /// <param name="parameters"> the parameters (fields) of query string </param>
        /// <returns></returns>
        public  List<string> QueryLemmatizer(List<string> parameters)
        {
            List<string> lemmatizedParams = new List<string>();
            foreach (string p in parameters)
            {
                Lemmatizer lemmatizer = new Lemmatizer(HttpContext.Current.Server.MapPath(@"~/Data/words.dat"), HttpContext.Current.Server.MapPath(@"~/Data/verbs.dat"),false);
                WordTokenizer wordTokenizer=new WordTokenizer (HttpContext.Current.Server.MapPath(@"~/Data/verbs.dat"));
                string lemmatizedParam="";
                
                //tokenizing each parameter
                List<string> tokens=wordTokenizer.Tokenize(p);
                int counter=0;
                //lemmatizing each tokens
                foreach(string t in tokens)
                {
                    counter++;
                    string lemm=lemmatizer.Lemmatize(t);
                    if (lemm.Contains('#'))
                    {
                        List<string> conj = lemmatizer.Conjugations(lemm);
                        lemm = conj[8];
                    }
                    if (counter != 1)
                    {
                        lemmatizedParam += " " + lemm;
                    }
                    else 
                       lemmatizedParam+= lemm;
                    
                     
                }
                
                lemmatizedParams.Add(lemmatizedParam);
            }
            return lemmatizedParams;
        }
        

        public  string RemovingStopWords(string content)
        {
           
            string[] exceptionsList = GetStopWords().ToList().ToArray();

            
            string[] wordList = content.Split(' ');

            string final = null;
            var result = wordList.Except(exceptionsList).ToArray();
            final = String.Join(" ", result);

            return final;
        }
        public static List<string> RemovingStopWords(List<string> contents)
        {

            string[] exceptionsList = GetStopWords().ToList().ToArray();
            string[] wordList = { };
            List<string> filteredContent = new List<string>();

            foreach (string content in contents)
            {
                wordList = content.Split(' ');
                string final = null;
                var result = wordList.Except(exceptionsList).ToArray();
                final = String.Join(" ", result);
                filteredContent.Add(final);
            }

            

            return filteredContent;
        }
        public static List<string> RemovingSpecificChar(List<string> contents, char[] chars)
        {
            List<string> filteredContent = new List<string>();

            foreach (string content in contents)
            {
                var tempContent = content;
                foreach (char c in chars)
                {
                        while(tempContent.IndexOf(c)>=0)
                            tempContent = tempContent.Remove(tempContent.IndexOf(c), 1);
                }

                filteredContent.Add(tempContent);
            }

            return filteredContent;
        }
        public static string RemovingSpecificChar(string content, char[] chars)
        {
                foreach (char c in chars)
                {
                    while (content.IndexOf(c) >= 0)
                        content = content.Remove(content.IndexOf(c), 1);
                }
            return content;
        }
        
        public static HashSet<string> GetStopWords()
        {
            string[] stopwords = { };
            HashSet<string> set=new HashSet<string> ();
            stopwords = _stopWords.Split(new[] { "\r" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in stopwords)
                 set.Add(s);
            return set;
        }
        
        #endregion

        //
        // private methods
        //
        #region private methods

        /// <summary>
        /// convert document name from name.doc/docx to name-doc/docx
        /// </summary>
        /// <param name="documentName">document name with .doc or .docx extension</param>
        /// <returns></returns>
        public static string ConvertDocumentName(string docuName)
        {
            string name = null;
            if (docuName.Contains(".doc") || docuName.Contains(".docx"))
            {
                switch (docuName.Substring(docuName.LastIndexOf('.')))
                {
                    case ".doc": name = docuName.Substring(0, docuName.LastIndexOf('.')) + "-doc"; name.Trim(new char[] { '.' });
                        break;
                    case ".docx": name = docuName.Substring(0, docuName.LastIndexOf('.')) + "-docx"; name.Trim(new char[] { '.' });
                        break;
                }

            }
            else
            {
                if (docuName.Contains("-doc") || docuName.Contains("-docx"))
                {
                    switch (docuName.Substring(docuName.LastIndexOf('-')))
                    {
                        case "-doc": name = docuName.Substring(0, docuName.LastIndexOf('-')) + ".doc"; name.Trim(new char[] { '-' });
                            break;
                        case "-docx": name = docuName.Substring(0, docuName.LastIndexOf('-')) + ".docx"; name.Trim(new char[] { '-' });
                            break;
                    }
                }
                else
                    return docuName;
            }
                
            
            return name;
        }

        public string ContentNormalizer(string content)
        {
            Normalizer normalizer = new Normalizer(true, true, true);

            
            content = content.Replace("\r", "**");


            //normalize the content of source 
            content = normalizer.Run(content);

            content = content.Replace("**", "\r");

            return content;
        }

        /// <summary>
        /// normalizing the author properties
        /// </summary>
        /// <param name="author"></param>
        /// <returns></returns>
        private Author AuthorNormalizer(Author author)
        {
            Author normalizeAuthor = new Author();
            Normalizer normalizer = new Normalizer(true, true, true);

            if (author.Degree == null) author.Degree = "تعیین نشده است";

            if (author.Field == null) author.Field = "تعیین نشده است";


            normalizeAuthor.Degree = normalizer.Run(author.Degree);
            normalizeAuthor.Field = normalizer.Run(author.Field);
            normalizeAuthor.Name = normalizer.Run(author.Name);

            return normalizeAuthor;
        }

        #endregion
        
    }
}