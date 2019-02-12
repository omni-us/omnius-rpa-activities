using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using UiPath.Shared.Activities;
using UiPath.SmartData.DataContracts.Dom;
using UiPath.SmartData.DataContracts.Results;
using UiPath.SmartData.DataContracts.Taxonomy;
using Newtonsoft.Json;
using System.IO;

namespace Omnius.Activities
{
    public class OmniusXmlParserActivity : AsyncTaskCodeActivity
    {
        public InArgument<string> Path { get; set; }
        public InArgument<Document> InitalDom { get; set; }
        public InArgument<string> OmniusResponse { get; set; }
        public InArgument<string> Text { get; set; }

        public InArgument<string> TaxonomyFile { get; set; }
        public InArgument<string> OmniusIdToUIPathFieldIdFile { get; set; }

        public OutArgument<ExtractionResult> Extraction { get; set; }
        public OutArgument<DocumentTaxonomy> Taxonomy { get; set; }

        protected override async Task<Action<AsyncCodeActivityContext>> ExecuteAsync(AsyncCodeActivityContext context, CancellationToken cancellationToken)
        {
            var dom = InitalDom.Get(context);
            var page = dom.Pages.First();
            var extractedText = Text.Get(context);
            var omniusResponse = OmniusResponse.Get(context);
            var xmlIdTofieldId = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(OmniusIdToUIPathFieldIdFile.Get(context)));
            var taxonomy = JsonConvert.DeserializeObject<DocumentTaxonomy>(File.ReadAllText(TaxonomyFile.Get(context)));
            var fieldsByID = taxonomy.DocumentTypes[0].Fields.ToDictionary(x => x.FieldId, x => x);
            var textLength = extractedText?.Length ?? 0;
            var documentInfo = new OmniusFormsXml2UipathConverter.DocumentInfo
            {
                DocumentID = dom.DocumentId,
                Path = Path.Get(context),
                Pages = dom.Pages
            };

            var xmlConverter = new OmniusFormsXml2UipathConverter(new ValuePostProcessor(), fieldsByID, xmlIdTofieldId);
            var res = xmlConverter.ConvertPagexml(documentInfo, omniusResponse, textLength);
            return x =>
            {
                Extraction.Set(x, res);
                Taxonomy.Set(x, taxonomy);
            };
        }


    }



}
