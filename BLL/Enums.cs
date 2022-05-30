using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DocumentSimilarity.BLL
{
    public enum SearchFieldMode { MultipleField, OneField };
    public enum SectionField { Title, Abstract, Keywords, Name, Authors, Body, Year,All };
    public enum BaseFormat { XML,WORD};
    public enum StructuralType { Structural, Unstructural };
    public enum SearchMode { AtLeastOneTerm,AllTerms,ExactlyPhrase,SamePhrase }

}