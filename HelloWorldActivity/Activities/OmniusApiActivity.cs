using Omnius.Clients;
using Newtonsoft.Json.Linq;
using System;
using System.Activities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UiPath.Shared.Activities;

namespace Omnius.Activities
{
    public class OmniusApiActivity : AsyncTaskCodeActivity
    {
        public InArgument<string> CustomHeaders { get; set; }
        public InArgument<string> Host { get; set; }
        public InArgument<string> Username { get; set; }
        public InArgument<string> Password { get; set; }
        public InArgument<string> Resource { get; set; }
        public InArgument<string> FilePath { get; set; }
        public InArgument<string> Debug { get; set; }
        public OutArgument<string> OmniusExtraction { get; set; }
        protected override async Task<Action<AsyncCodeActivityContext>> ExecuteAsync(AsyncCodeActivityContext inputContext, CancellationToken cancellationToken)
        {
            var client = new OmniusApiClient
            {
                CustomHeaders = CustomHeaders.Get(inputContext),
                Host = Host.Get(inputContext),
                User = Username.Get(inputContext),
                Pass = Password.Get(inputContext)
            };

            var filePath = FilePath.Get(inputContext);

            var debugPath = Debug.Get(inputContext);
            if (!debugPath.IsNullOrWhiteSpace())
            {
                return outputContext => OmniusExtraction.Set(outputContext, File.ReadAllText(debugPath));
            }
            string res;
            using (Stream fileStream = File.Open(filePath, FileMode.Open))
            {
                res = client.RunSynchroniusExtraction(Resource.Get(inputContext), fileStream, filePath, 42, 10);
            }

            return outputContext => OmniusExtraction.Set(outputContext, res);
        }
    }
}
