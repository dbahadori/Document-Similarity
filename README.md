# DocumentSimilarity

DocumentSimilarity project is a web based plagiarism detection system for word documents using information retrieval based methods. I used the document similarity analysis and tf-idf technique as a term weighting method which is the main model in our system. This system is implemented using Lucene.Net and ASP.Net MCV.

#  Solution 
System used two preprocessing steps
in the first step system preprocesses the document once it is imported to the system in order to extract different sections of it. The extracted information are temporary and keep in memory. in order to save the extracted information permanently on the disk, a XML template is used. 
After extracting all segments of the imported document, system preprocesses the content of each segment by doing normalization, lemmitization and tokenization actions.

To perform lemmitizing, we first separate the sentences in each section and also separate the tokens in each sentence. Then for each token we perform lemmitization action. Finally, we merge the tokens in the same order as they were separated to create the initial sentences.

![pre1](https://user-images.githubusercontent.com/92206600/177039692-72be2d83-d45a-4698-96bb-947f9672c058.png)

preprocessing on centents of each extracted segment of document 

![pre2](https://user-images.githubusercontent.com/92206600/177039695-6cdb4b2d-e394-4e67-bb15-0ab4d3fe555c.png)

# Layers of System Architecture
this system is implemented using layered architectural style. The layers of system and components of each layers is demonstrated in below.

![layers of system architecture](https://user-images.githubusercontent.com/92206600/177034042-1e7c6cc0-5b1b-4031-b6ae-525ad4441d99.png)



