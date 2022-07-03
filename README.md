# DocumentSimilarity

DocumentSimilarity project is a web based plagiarism detection system for word documents using information retrieval based methods. I used the document similarity analysis and tf-idf technique as a term weighting method which is the main model in our system. This system is implemented using Lucene.Net and ASP.Net MCV.

#  Solution 
System used two pre-processing steps.
1. in the first step system pre-processes the document once it is imported to the system in order to extract different sections of it. The extracted information are temporary and keep in memory. in order to save the extracted information permanently on the disk, aد XML template is used. 

1. After extracting all sections of the imported document, system pre-processes the content of each section by doing normalization, lemmatization and tokenization operations. To perform lemmatizating, first the sentences are separated in each section and after that the tokens are separated in each sentence. Then for each token lemmatization action is performed. Finally, the tokens are merged in the same order as they were separated to create the initial sentences.

<p align="center">
  <img src="https://user-images.githubusercontent.com/92206600/177039692-72be2d83-d45a-4698-96bb-947f9672c058.png">
</p>

After the pre-processing steps, a document is created in the document database, and each section is added as a field to that, moreover the document is indexed by the lucene.net search engine.

When we need to search in the document database, the pre-processing is also done on the query created in the system, similar to the step of adding a new document to the database.

# system Functionality
* **Add a new document** : By using this feature, the user will be able to upload new documents in the system. In order to display, search and participate in the similarity calculation, documents must be loaded in the system in advance.

* **Index documents** : The system automatically performs indexing on the documents after uploading. Indexing is done in order to perform future searches and obtain similar documents.

* **Display documents**: By using this feature, the user will be able to view the documents indexed in the document repository.

* **Delete the document**: By using this feature, the user will be able to delete any of the displayed documents that are indexed in the document repository. Documents that are deleted from the database will also be physically deleted from the memory where they are stored.

* **Search functionality (advanced)**: By using this feature, the user will be able to search for the desired documents based on specific filters among the documents indexed in the document repository. The search function includes simple search and advanced search. In the advanced search, the user will be able to limit the scope of his search with various filters.

* **Create a new user account**: By using this feature, the user will be able to create a new user account in the system. In order to use other features of the system, the end user must have a valid user account.

* **Log into the system**: Using this feature, the user will be able to log into the system using the username and password that he obtained when creating a new account. In order to use other features of the system, it is necessary for the end user to log into the system.

* **Log out**: Using this feature, the user will be able to log out from the system after logging in.

* **Change password**: Using this feature, the user will be able to change the password he got when creating a new account after logging into the system.

* **Calculate the degree of similarity between new and existing documents**: By using this feature, the user will be able to obtain the degree of similarity between his document and the documents in the document repository.

# System Architecture
this system is implemented using layered architectural style. The layers of system and components of each layer are demonstrated below.

<p align="center">
  <img width="500" height="600" src="https://user-images.githubusercontent.com/92206600/177034042-1e7c6cc0-5b1b-4031-b6ae-525ad4441d99.png">
</p>
