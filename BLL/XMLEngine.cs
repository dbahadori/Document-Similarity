using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DocumentSimilarity.Models;
using System.Xml.Linq;
using NHazm;
using System.Text.RegularExpressions;

namespace DocumentSimilarity.BLL
{
    public class XMLEngine
    {
        public  Boolean XMLResearchDocumentCreator(ResearchDocument document, string documentName)
        {
            // Create a root node
            
            
            XElement doc = new XElement("Document");

            XElement authors = new XElement("Authors");
            try
            {

                if (document.Authors != null && document.Authors.Count > 0)
                {
                    foreach (var a in document.Authors)
                    {
                        XElement author = new XElement("Author");
                        XElement name = new XElement("Name", a.Name);
                        XElement field = new XElement("Field", a.Field);
                        XElement degree = new XElement("Degree", a.Degree);

                        author.Add(name);
                        author.Add(degree);
                        author.Add(field);

                        authors.Add(author);
                    }
                }

                XElement keywords = new XElement("Keywords", CleanInvalidXmlChars(document.Keywords));
                XElement docname = new XElement("Name", CleanInvalidXmlChars(document.Name));
                XElement title = new XElement("Title", CleanInvalidXmlChars(document.Title));
                XElement abst = new XElement("Abstract", CleanInvalidXmlChars(document.Abstract));
                XElement body = new XElement("Body", CleanInvalidXmlChars(document.Body));
                XElement type = new XElement("Type", CleanInvalidXmlChars(document.Type));
                XElement year = new XElement("Year", CleanInvalidXmlChars(document.Year));


                doc.Add(docname);
                doc.Add(title);
                doc.Add(abst);
                doc.Add(authors);
                doc.Add(keywords);
                doc.Add(body);
                doc.Add(type);
                doc.Add(year);


                doc.Save(HttpContext.Current.Server.MapPath(@"~/Documents/XML/" + documentName + ".xml"));

                return true;
            }
            catch (Exception e)
            {
                System.IO.TextWriter wr = Console.Out;
                wr.Write(e.Message.ToString());
                return false;
            }


            
        }

        public  Boolean XMLResearchDocumentCreator(string Name,string Title, string Abstract, string Body, List<Author> Authors,string Keywords, string Year,string Type)
        {
            // Create a root node


            XElement doc = new XElement("Document");

            XElement authors = new XElement("Authors");


            if (Authors != null && Authors.Count > 0)
            {
                foreach (var a in Authors)
                {
                    XElement author = new XElement("Author");
                    XElement name = new XElement("Name", a.Name);
                    XElement field = new XElement("Field", a.Field);
                    XElement degree = new XElement("Degree", a.Degree);

                    author.Add(name);
                    author.Add(degree);
                    author.Add(field);

                    authors.Add(author);
                }
            }





            XElement keywords = new XElement("Keywords", Keywords);
            XElement docname = new XElement("Name",Name);
            XElement title = new XElement("Title", Title);
            XElement abst = new XElement("Abstract",Abstract);
            XElement body = new XElement("Body",Body);
            XElement year = new XElement("Year", Year);
            XElement type = new XElement("Type", Type);

            
            doc.Add(docname);
            doc.Add(title);
            doc.Add(abst);
            doc.Add(authors);
            doc.Add(keywords);
            doc.Add(body);
            doc.Add(year);
            doc.Add(type);


            doc.Save(HttpContext.Current.Server.MapPath(@"~/Documents/XML/" + Name + ".xml"));

            return true;


        }
        public  ResearchDocument XMLResearchDocumentReader(string docName)
        {
            
             docName=docName.Trim(new char[] { ' ', '‌', '‌', '‌' });
            
            XElement allData = XElement.Load(HttpContext.Current.Server.MapPath(@"~/Documents/XML/" + docName + ".xml"));
            ResearchDocument document = new ResearchDocument();
            
            //normalizing the value of each section 
            Normalizer normalizer = new Normalizer(true,true,true);

            if (allData != null)
            {

                document.Abstract =normalizer.Run(allData.Descendants("Abstract").FirstOrDefault().Value);
                document.Name = docName;
                document.Title = normalizer.Run(allData.Descendants("Title").FirstOrDefault().Value);
                document.Body = normalizer.Run(allData.Descendants("Body").FirstOrDefault().Value);
                document.Keywords = normalizer.Run(allData.Descendants("Keywords").FirstOrDefault().Value);
                document.Type = normalizer.Run(allData.Descendants("Type").FirstOrDefault().Value);
                document.Year = normalizer.Run(allData.Descendants("Year").FirstOrDefault().Value);



                IEnumerable<XElement> auths = allData.Descendants("Authors").FirstOrDefault().Descendants("Author");
                    List<Author> authors = new List<Author>();

                    foreach (XElement a in auths)
                    {
                        Author author = new Author();
                        author.Degree =normalizer.Run(a.Descendants("Degree").FirstOrDefault().Value);
                        author.Field = normalizer.Run(a.Descendants("Field").FirstOrDefault().Value);
                        author.Name = normalizer.Run(a.Descendants("Name").FirstOrDefault().Value);
                        authors.Add(author);
                        
                    }


                    document.Authors = authors;
                  
              
                
            }

            return document;
        }

        public  ResearchDocument XMLResearchDocumentReader(string docName, float score)
        {
            docName = docName.Trim(new char[] { ' ', '‌', '‌', '‌' });

            XElement allData = XElement.Load(HttpContext.Current.Server.MapPath(@"~/Documents/XML/" + docName + ".xml"));
            ResearchDocument document = new ResearchDocument();

            //normalizing the value of each section 
            Normalizer normalizer = new Normalizer(true, true, true);

            if (allData != null)
            {

                document.Abstract = normalizer.Run(allData.Descendants("Abstract").FirstOrDefault().Value);
                document.Name = docName;
                document.Title = normalizer.Run(allData.Descendants("Title").FirstOrDefault().Value);
                document.Body = normalizer.Run(allData.Descendants("Body").FirstOrDefault().Value);
                document.Keywords = normalizer.Run(allData.Descendants("Keywords").FirstOrDefault().Value);
                document.Type = normalizer.Run(allData.Descendants("Type").FirstOrDefault().Value);
                document.Year = normalizer.Run(allData.Descendants("Year").FirstOrDefault().Value);

                IEnumerable<XElement> auths = allData.Descendants("Authors").FirstOrDefault().Descendants("Author");
                List<Author> authors = new List<Author>();

                foreach (XElement a in auths)
                {
                    Author author = new Author();
                    author.Degree = normalizer.Run(a.Descendants("Degree").FirstOrDefault().Value);
                    author.Field = normalizer.Run(a.Descendants("Field").FirstOrDefault().Value);
                    author.Name = normalizer.Run(a.Descendants("Name").FirstOrDefault().Value);
                    authors.Add(author);

                }


                document.Authors = authors;
                document.Score = score;

            }

            return document;
        }

        private static Regex _invalidXMLChars = new Regex(
    @"(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|[\x00-\x08\x0B\x0C\x0E-\x1F\x7F-\x9F\uFEFF\uFFFE\uFFFF]",
    RegexOptions.Compiled);

        /// <summary>
        /// removes any unusual unicode characters that can't be encoded into XML
        /// </summary>
        public static string CleanInvalidXmlChars(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return _invalidXMLChars.Replace(text, "");
        }
        
    }
}




