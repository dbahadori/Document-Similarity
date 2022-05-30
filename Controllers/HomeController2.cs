using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DocumentSimilarity.BLL;
using Microsoft.Office.Interop.Word;
using DocumentSimilarity.Models;
using NHazm;

namespace DocumentSimilarity.Controllers
{
    public class HomeController2 : Controller
    {
        public ActionResult Index()
        {
            
            Preprocessing preproc = new Preprocessing();
            ResearchDocument redoc= preproc.WordSectionsExtraction("travel4.doc");
            ResearchDocument redocAfnor= preproc.SectoinsNormalizer(redoc);
            ResearchDocument redocAfnorAndLem= preproc.SectionLemmatizer(redocAfnor);

            ViewBag.ant = "عنوان :"+redocAfnorAndLem.Title+" کلمات کلیدی : "+redocAfnorAndLem.Keywords.FirstOrDefault()+" نویسنده: "+redocAfnorAndLem.Authors.FirstOrDefault().Name+" چکیده: "+redocAfnorAndLem.Abstract;
            
            return View();
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
    }
}