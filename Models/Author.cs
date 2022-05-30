using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace DocumentSimilarity.Models
{
    public class Author
    {
        public string WriterID { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "لطفا نام گردآورنده ی سند را وارد کنید")]
        [Display(Name = "نام نگارنده")]
        public string Name { get; set; }
        public string Degree { get; set; }
        public string Field { get; set; }

        //public virtual ICollection<ResearchDocument> ResearchDocuments { get; set; }
    }
}