using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Omnius;
using Omnius.Clients;
using UiPath.SmartData.DataContracts.Dom;
using UiPath.SmartData.DataContracts.Taxonomy;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Linq;
using Omnius;

namespace Tests
{
    [TestClass]
    public class OmniusParserTests
    {
        [TestMethod]
        public void givenWordWithId_r1_l1_w1_parserConvertsItToResultValueCorrectly()
        {
            var taxonomy = DeserializeJsonFromResources<DocumentTaxonomy>("_taxonomy.json");
            var fieldsById = taxonomy.DocumentTypes[0].Fields.ToDictionary(x => x.FieldId, x => x);
            var xmlIdsToFieldIdsMapping = DeserializeJsonFromResources<Dictionary<string, string>>("xmlIdToFieldId.json");
            var documentInfo = new OmniusFormsXml2UipathConverter.DocumentInfo
            {
                DocumentID = "randomDocId",
                Path = "RandomFilePath",
                Pages = new Page[] {
                    PageWithIndexAndSize(0,100,100),
                    PageWithIndexAndSize(0,100,100)
                }
            };

            var parserUnderTest = new OmniusFormsXml2UipathConverter(new ValuePostProcessor(), fieldsById, xmlIdsToFieldIdsMapping);

            var actual = parserUnderTest.ConvertPagexml(documentInfo, ReadFileFromResources("omnius-response.xml"), 0);

            Assert.AreEqual(ReadFileFromResources("expectedExtractions.json"), JsonConvert.SerializeObject(actual));
        }

        public Page PageWithIndexAndSize(int Index, int width, int height)
        {
            return new Page
            {
                IndexInText = 0,
                PageIndex = 0,
                Size = new float[4] { 0f, 0f, 1f, 1f }
            };
        }

        public T DeserializeJsonFromResources<T>(string relativePath)
        {
            return JsonConvert.DeserializeObject<T>(ReadFileFromResources(relativePath));
        }

        public String ReadFileFromResources(string relativePath)
        {
            return File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "Resources", relativePath));
        }
    }
}
