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
    public class OmniusWatchFolderActivity : AsyncTaskCodeActivity
    {
        public InArgument<string> Directory { get; set; }
        public OutArgument<string> FilePath { get; set; }
        protected override async Task<Action<AsyncCodeActivityContext>> ExecuteAsync(AsyncCodeActivityContext inputContext, CancellationToken cancellationToken)
        {
            string hotDirectory = Directory.Get(inputContext);
            for (int i = 0; i < 1000; i++)
            {
                var directoryInfo = new DirectoryInfo(hotDirectory);//Assuming Test is your Folder
                var files = directoryInfo.GetFiles("*.pdf").OrderByDescending(x => x.LastWriteTime).ToList(); //Getting Text files
                if (files != null && files.Any())
                {
                    return outputContext => FilePath.Set(outputContext, Path.Combine(hotDirectory, files[0].ToString()));
                }

                Thread.Sleep(2000);
            }
            return x => { };
        }

    }
}