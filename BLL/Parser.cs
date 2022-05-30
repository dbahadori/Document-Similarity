using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DocumentSimilarity.Models;
using NHazm;

namespace DocumentSimilarity.BLL
{
    //this class parse word document that contains body of text (research) as well as title and keywords 
    public class Parser
    {
        public Microsoft.Office.Interop.Word.Document ReadWordDoc(string path, Microsoft.Office.Interop.Word.Application wordApp)
        {

            // File Path
            string strFilePath = path;

            // Create obj filename to pass it as paremeter in open 
            object objFile = strFilePath;
            object objNull = System.Reflection.Missing.Value;
            object objReadOnly = true;

            //Open Document
            Microsoft.Office.Interop.Word.Document Doc = wordApp.Documents.Open(ref objFile, ref objNull, ref objReadOnly,
                ref objNull, ref objNull, ref objNull, ref objNull, ref objNull, ref objNull, ref objNull, ref objNull,
                ref objNull, ref objNull, ref objNull, ref objNull);

            return Doc;
        }
        public string GetSubString(string strSource, string[] startPattern, string[] endPattern)
        {
            Normalizer normalizer = new Normalizer(true, true, true);

            int start = -1, end = -1;
            List<int> startTemp = new List<int> (), endtemp = new List<int> ();
            string newString = null;

            //
            // normalization
            //
            #region normalization

            strSource = strSource.Replace("\r", "**");


            //normalize the content of source 
            strSource = normalizer.Run(strSource);
            strSource = Preprocessing.RemovingSpecificChar(strSource, new char[] { '¬' });
            strSource = strSource.Replace("**", "\r");

            //normalize the startPatterns
            for (int j = 0; j < startPattern.Length; j++)
            {
                startPattern[j] = normalizer.Run(startPattern[j]);
               
            }

            //normalize the endPatterns
            for (int j = 0; j < endPattern.Length; j++)
            {
                endPattern[j] = normalizer.Run(endPattern[j]);
            }

            #endregion

            //
            // start and end index
            //
            #region start and end index
            //specify the index of all start patthern
            foreach (string s in startPattern)
            {
                if (strSource.Contains(s))
                {
                    startTemp.Add(strSource.IndexOf(s, 0) + s.Length);
                    
                }
            }

            if (startTemp != null && startTemp.Count() > 0)
            {
                start = startTemp.Min();
            }
            //specify the index of all end pattern
            if (start != -1 && endPattern.Contains("one line after"))
            {
                endtemp.Add(strSource.IndexOf('\r', start));

            }
            foreach (string s in endPattern)
            {
                
                if (strSource.Contains(s))
                {
                    endtemp.Add(strSource.IndexOf(s, 0));
                }

            }

           
            if(endtemp!=null&&endtemp.Count()>0){
               end = endtemp.Min();
            }
            
            #endregion 

            if (end == -1 && start!=-1 && endPattern.Contains("end of document")) end = strSource.Length;

            

            //seperate the requested string form source string
            if (end != -1 && start != -1 && start<end)
            {
                newString = strSource.Substring(start, (end - start));
                newString = newString.Trim();
            }
            else
            {
                newString = "";
            }

            if (newString != null && newString != "")
            {
                newString = RemoveFirstSpecChar(newString, new char[] { ':', ';' });
            }

            return newString;
        }
        public string RemoveFirstSpecChar(string strSource, char[] chars)
        {
            foreach (char c in chars)
            {
                if (strSource.Trim().IndexOf(c) == 0)
                {
                    strSource = strSource.Remove(strSource.IndexOf(c), 1);
                    break;
                }

            }
            return strSource;
        }
        public string GetSection(Microsoft.Office.Interop.Word.Document document, string[] startPattern, string[] endPattern)
        {
            string section = null;
            try
            {
                section = GetSubString(document.Content.Text, startPattern, endPattern);
            }
            catch (Exception ex) { throw ex; }

            return section;
        }

        public string TotalContent(Microsoft.Office.Interop.Word.Document document)
        {
            Normalizer normalizer = new Normalizer(true, true, true);

            

            string content = document.Content.Text;
            content = content.Replace("\r", "**");


            //normalize the content of source 
            content = normalizer.Run(content);

            content = content.Replace("**", "\r");

            return content;
        }



    }
}