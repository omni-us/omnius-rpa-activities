using Omnius;
using Omnius.Parsers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UiPath.SmartData.DataContracts.Dom;
using UiPath.SmartData.DataContracts.Results;
using UiPath.SmartData.DataContracts.Taxonomy;
using static Omnius.Parsers.RelativeRectangle;

namespace Omnius
{
    public class OmniusFormsXml2UipathConverter
    {
        public class DocumentInfo
        {
            public String DocumentID { get; set; }
            public String Path { get; set; }
            public Page[] Pages { get; set; }

        }
        private ValuePostProcessor _valueConverters;
        private Dictionary<String, Field> _fieldsByID;
        private Dictionary<string, string> _xelemIdsToFieldIdsMap;

        public OmniusFormsXml2UipathConverter(ValuePostProcessor valueConverters, Dictionary<String, Field> fieldsById, Dictionary<String, String> xelemIdToFieldId)
        {
            _valueConverters = valueConverters;
            _fieldsByID = fieldsById;
            _xelemIdsToFieldIdsMap = xelemIdToFieldId;
        }

        public ExtractionResult ConvertPagexml(DocumentInfo documentInfo, string pagexml, int extractedTextlength)
        {
            var dataPoints = ExtractResultDataPoints(documentInfo, pagexml);
            var res = new ExtractionResult
            {
                DocumentId = documentInfo.DocumentID,
                ContentType = "pdf",
                ResultsVersion = 0,
                ResultsDocument = new ResultsDocument
                {
                    DocumentTypeId = "pdf", //has to correspond to taxonomy document typeId
                    DocumentTypeName = "pdf",
                    DocumentCategory = "pdf",
                    DocumentGroup = "pdf",
                    DocumentTypeDataVersion = 0,
                    DataVersion = 0,
                    DocumentTypeField = AxiomaticResultValue("Insurance"),

                    Bounds = new ResultsDocumentBounds
                    {
                        PageCount = documentInfo.Pages.Length,
                        StartPage = 0,
                        TextLength = extractedTextlength,
                        TextStartIndex = 0
                    },
                    Fields = dataPoints
                },
            };

            return res;
        }

        private ResultsDataPoint[] ExtractResultDataPoints(DocumentInfo documentInfo, string pagexml)
        {
            var pagesWithWords = XDocument.Parse(pagexml).FindNodes(ByTag("page"));

            var extractedDatapoints = new List<ResultsDataPoint>();
            for (int i = 0; i < pagesWithWords.Count; i++)
            {
                var page = pagesWithWords[i];
                var datapointsOnCurrentPage = page.FindNodes(ByTag("TextLine"))
                    .Where(x => _xelemIdsToFieldIdsMap.ContainsKey(x.ID()))
                    .Select(x => ConvertXmlRectToDataPoint(x, page, _xelemIdsToFieldIdsMap[x.ID()], i, documentInfo.Pages[i]));
                extractedDatapoints.AddRange(datapointsOnCurrentPage);
            }

            return extractedDatapoints.ToArray();
        }

        public ResultsDataPoint ConvertXmlRectToDataPoint(XContainer xmlRectangle, XElement xmlPage, string fieldId, int pageIndex, Page uipathPage)
        {
            var uiPageWith = uipathPage.Size[2];
            var uiPageHeight = uipathPage.Size[3];
            var textEquivTag = xmlRectangle.FindImmediateChildren(ByTag("TextEquiv")).Single();
            var coordsTag = xmlRectangle.FindImmediateChildren(ByTag("coords")).Single();
            var xmlPageWidth = int.Parse(xmlPage.Attribute("imageWidth").Value);
            var xmlPageHeight = int.Parse(xmlPage.Attribute("imageHeight").Value);
            var extractedText = textEquivTag.FindSingleNode(ByTag("unicode")).Value;
            var confidence = (float)Double.Parse(textEquivTag.Attribute("conf").Value);
            var relativePoints2d = CoordsToPoints(coordsTag);
            var relativeRectangle2D = RelativeRectangle.FromPageXml(xmlPageWidth, xmlPageHeight, relativePoints2d);
            var topLeftWidthHeight = relativeRectangle2D.ToTopLeftWidthHeight(uipathPage.Size[2], uipathPage.Size[3]);
            var valueConverter = _valueConverters.PostprocessFor(_fieldsByID[fieldId]);
            var value = valueConverter(extractedText);
            if (value.IsNullOrWhiteSpace()) {
                return new ResultsDataPoint
                {
                    IsMissing = true,
                    FieldName = fieldId,
                    FieldId = fieldId
                };
            }
            var dataPoint = new ResultsDataPoint
            {
                IsMissing = false,
                FieldName = fieldId,
                FieldId = fieldId,
                Values = new ResultsValue[]{ new ResultsValue {
                            Confidence = confidence,
                            OcrConfidence = confidence,                            
                            Value = value,
                            Reference = new ResultsContentReference{
                                TextLength=0,
                                TextStartIndex=0,
                                Tokens=new ResultsValueTokens[]{ new ResultsValueTokens {
                                    Page = pageIndex,
                                    PageWidth = uiPageWith,
                                    PageHeight = uiPageHeight,
                                    Boxes = new float[1][]{ topLeftWidthHeight },
                                    TextLength = 0,
                                    TextStartIndex = 0,
                                } },
                            }
                      }}
            };
            return dataPoint;
        }

        private List<Point2D> CoordsToPoints(XElement coords)
        {
            return coords.Attribute("points").Value.Split(' ').Select(x =>
            {
                var xNy = x.Split(',');
                return new Point2D
                {
                    X = float.Parse(xNy[0]),
                    Y = float.Parse(xNy[1])
                };
            }).ToList();
        }

        public ResultsValue AxiomaticResultValue(string value)
        {
            return new ResultsValue
            {
                Confidence = 1,
                OcrConfidence = 1,
                Value = value,
                Reference = new ResultsContentReference
                {
                    TextLength = 0,
                    TextStartIndex = 0,
                    Tokens = new ResultsValueTokens[] { }
                }
            };
        }

        public static Func<XElement, bool> ByTag(string tagName)
        {
            return x => x.Name.LocalName.EqualsIgnoreCase(tagName);
        }

        private Func<XElement, bool> ById(string key)
        {
            return x => key.Equals(x.ID());
        }

        public String Identity(String input)
        {
            return input;
        }
    }

}
