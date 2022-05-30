using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DocumentSimilarity.BLL;
using DocumentSimilarity.Models;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;

namespace DocumentSimilarity.Models
{
    public class Search
    {
        public SectionField Field { get; set; }
        public string value { get; set; }
        public Occur occur { get; set; }
        public SearchMode Mode { get; set; }
        public int Slop { get; set; }
    }
}