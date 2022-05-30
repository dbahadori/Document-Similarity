using DocumentSimilarity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DocumentSimilarity.BLL
{
    public class ResearchDocumentBLL
    {
        /// <summary>
        /// Create a ResearchDocument object from sections of document content
        /// </summary>
        /// <param name="title">title section of document</param>
        /// <param name="authors">authors section of document</param>
        /// <param name="keywords">keywords section of document</param>
        /// <param name="body">body section of document</param>
        /// <param name="docName">name of document</param>
        /// <param name="_abstract">abstract section of document</param>
        /// <returns>ResearchDocument object</returns>
        public  ResearchDocument ResearchDocCreator(string title, List<Author> authors, string keywords, string body, string docName, string _abstract,string year,string type)
        {
            //create the root item document
            var doc = new ResearchDocument();
            AuthorBLL authorBLL = new AuthorBLL();
             //specify list of authors from author section                         
             

            doc.Authors = authors;
            doc.Keywords = keywords;
            doc.Title = title;
            doc.Body = body;
            doc.Name = docName;
            doc.Abstract = _abstract;
            doc.Year = year;
            doc.Type = type;

            return doc;
        }
    }
}