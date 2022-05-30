using DocumentSimilarity.Models;
using Lucene.Net.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DocumentSimilarity.BLL
{
    public class AuthorBLL
    {
        /// <summary>
        /// returnd a list of author object that are created from author section of word document
        /// </summary>
        /// <param name="section">author section of word documnet</param>
        /// <returns>list of Author object</returns>
        public static List<Author> GetAuthorsFromSection(string section)
        {
            
            List<Author> Authors = new List<Author>();
           string[] authorsName = section.Split(new string[] { ",", ";" ,"،","؛","-","_"}, StringSplitOptions.RemoveEmptyEntries);
           foreach (string name in authorsName)
           {
               Author author = new Author();
               author.Name = name;
               Authors.Add(author);
           }
           return Authors;
        }

        /// <summary>
        /// returns an author object that is created from an author lucene field
        /// </summary>
        /// <param name="filed">lucene author filed that contains author properties, (name, filed, degree) order is important.</param>
        /// <returns>an author object</returns>
        public static Author GetAuthorFromField(Field filed)
        {
            Author author = new Author();
            string[] aut = filed.StringValue.Split(new string[] { ",", ";" }, StringSplitOptions.None);
            author.Name = aut[0];
            author.Field = aut[1];
            author.Degree = aut[2];

            return author;
        }
    }
}