using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Omnius;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Net;

namespace Omnius.Clients
{
    public class OmniusApiClient
    {
        private static readonly StringContent EmptyBodyHttpContent = new StringContent("");

        public string Host { get; set; }
        public string User { get; set; }
        public string Pass { get; set; }

        public string CustomHeaders { get; set; } = "";
        public string Api { get; set; } = "api/stie/v1.0";
        public string FileFormKey { get; set; } = "inpFile";

        public string UploadStatusJSelector { get; set; } = "envelope.status";
        public string UploadStatusSuccess { get; set; } = "SUCCESS";

        public string DocumentIdJSelector { get; set; } = "payload.document_key.document_key";

        public string DocumentStatusJSelector { get; set; } = "envelope.status";
        public string DocumentStatusProcessing { get; set; } = "QUEUED";
        public string DocumentStatusProcessed { get; set; } = "PROCESSED";

        public string DocumentExtractionJSelector { get; set; } = "payload.value";

        public Action<int> Wait { get; set; } = x => System.Threading.Thread.Sleep(x);


        public string RunSynchroniusExtraction(string resource, Stream file, string originalFileName, int polRequestsCountLimit, int polRequestsDelayInSeconds)
        {
            var uploadMetadata = UploadDocumentToResource(resource, file, originalFileName);
            string status = uploadMetadata.SelectToken(UploadStatusJSelector).ToString();
            if (!UploadStatusSuccess.EqualsIgnoreCase(status))
            {
                throw new HttpRequestException($"Error while uploading document {originalFileName} to resource {resource} : {status}");
            }

            var uuid = uploadMetadata.SelectToken(DocumentIdJSelector).ToString();
            for (int i = 0; i < polRequestsCountLimit; i++)
            {
                var documentMetadata = GetDocument(resource, uuid);
                var documentStatus = documentMetadata.SelectToken(DocumentStatusJSelector).ToString();
                if (DocumentStatusProcessing.EqualsIgnoreCase(documentStatus))
                {
                    Wait(polRequestsDelayInSeconds * 1000);
                    continue;
                }

                if (DocumentStatusProcessed.EqualsIgnoreCase(documentStatus))
                {
                    return documentMetadata.SelectToken(DocumentExtractionJSelector).ToString();
                }

                throw new HttpRequestException($"Error while uploading document {originalFileName} with uuid {uuid} to resource {resource} : {documentStatus}");
            }

            throw new HttpRequestException($"Error while uploading document {originalFileName} with uuid {uuid} to resource {resource} : Maximum poling count exceeded after {polRequestsCountLimit} tries.");
        }

        public JObject UploadDocumentToResource(string resource, Stream file, string originalFileName)
        {
            using (var formData = new MultipartFormDataContent())
            using (HttpContent fileStreamContent = new StreamContent(file))
            {
                formData.Add(fileStreamContent, FileFormKey, originalFileName);
                return POST($"{Api}/{resource}", formData);
            }
        }

        public JObject CreateResource(string resource) => POST($"{Api}/{resource}", EmptyBodyHttpContent);
        public JObject GetResource(string resource) => GET($"{Api}/{resource}");
        public JObject GetDocument(string resource, string uuid) => GET($"{Api}/{resource}/{uuid}");



        private HttpClient CreateAuthorizedClientWithHeaders()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(Host);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Base64Encode($"{User}:{Pass}"));
            CustomHeaders.Split('|')
                      .Select(x => x.Split(':'))
                      .Where(x => x != null && x.Length == 2)
                      .ForEach(keyValPair => client.DefaultRequestHeaders.Add(keyValPair[0], keyValPair[1]));
            return client;
        }

        private JObject GET(String url)
        {
            using (var client = CreateAuthorizedClientWithHeaders())
            {
                var response = client.GetAsync(url).Result;
                JObject json = TryGetResponseAsJSON(response);
                return json;
            }
        }

        private JObject POST(String url, HttpContent content)
        {
            using (var client = CreateAuthorizedClientWithHeaders())
            {
                var response = client.PostAsync(url, content).Result;
                JObject json = TryGetResponseAsJSON(response);
                return json;
            }
        }

        private JObject TryGetResponseAsJSON(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error: {response.StatusCode} {response.ReasonPhrase} {response.RequestMessage}");
            }

            string resonse = response.Content.ReadAsStringAsync().Result;
            return JObject.Parse(resonse);
        }

        private string Base64Encode(string input) => Convert.ToBase64String(Encoding.UTF8.GetBytes(input));

    }
}
