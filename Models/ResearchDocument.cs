using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DocumentSimilarity.Models
{
    public class ResearchDocument
    {
       
        [Required(AllowEmptyStrings=false,ErrorMessage="لطفا عنوان سند را وارد کنید")]
        [Display(Name = "عنوان")]
        public string Title { get; set; }

        public string Name { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "لطفا چکیده سند را وارد کنید")]
        [Display(Name = "چکیده")]
        [MaxLength(300,ErrorMessage="حداکثر طول متن چکیده 300 می باشد")]
        public string Abstract { get; set; }

        [Required(AllowEmptyStrings = true, ErrorMessage = "لطفا کلمات کلیدی سند را وارد کنید")]
        [Display(Name = "کلمات کلیدی")]
        public string Keywords { get; set; }

        
        [Display(Name = "بدنه")]
        public string Body { get; set; }

        [Required (AllowEmptyStrings = false, ErrorMessage = "لطفا سال گرداوری سند را وارد کنید")]
        [Display(Name = "سال نگارش")]
        public string Year { get; set; }
        public float Score { get; set; }
        public string Type { get; set; }
        
        public List<Author> Authors { get; set; }  
    }
}