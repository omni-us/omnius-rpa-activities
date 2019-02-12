# omni:us - RPA activities

## UiPath Integration
### Omnius Watch Folder Activity
Monitors a folder every second, and outputs the latest file moved to that folder when such event occurs.
* Input:
  * Directory: String, the path to the hot folder.
* Output:
  * FilePath: String, the path to the latest file modified.


### Omnius Api Activity
Executes a call to our backend API for a single document.
* Input:
  * CustomHeaders: String, The list of custom headers. Use '|' to separate different headers, and ':' to separate key/value of a single header.
  * FilePath: String, Read only parameter pointing to the file to be processed.
  * Host: String, The host of the backend.
  * Password: String,
  * Username: String,
  * Resource: String, The resource to which the processed files are to be saved. 
* Output:
  * OmniusExtraction: String, The api response in xml fromat.
  
### Omnius Xml Parser Activity
Transforms the omnius api response into a uipath 'Extraction'.
* Input:
  * InitialDom: Document, The initial dom with the correct display page size.
  * OmniusIdToUiPathFieldIdFile: String, The file path to a dictionary with keys consisting of the xml element ID that we need to extract and values the equivalent taxonomy field ID;.
  * OmniusResponse: String, the output of our backend API.
  * Path: String, The path of the file being processed.
  * TaxonomyFile: String, The path of a serialized taxonomy.
  * Text: String, The text representation of the document.
* Output 
  * Taxonomy: DocumentTaxonomy, The taxonomy for this document.
  * Extraction: ExtractionResult, The extracted data in a validation station presentable format.

### Screenshots
![standard workflow](/images/standard_workflow.png?raw=true)
