using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DocumentSimilarity.Models
{
    public class ResearchDocumentResult
    {
        public string DocumentName { get; set; }
        public string Title { get; set; }
        public string Year { get; set; }
        public string Abstract { get; set; }
        public string Keywords   { get; set; }
        public string AuthorName { get; set; }
        public string AuthorField { get; set; }
        public string AuthorDegree { get; set; }
        public float Score { get; set; }
        public string Body { get; set; }
        public string Type { get; set; }
    }
}