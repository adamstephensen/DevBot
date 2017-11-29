using formflow.FormFlow;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace LuisBot.Services
{
    [Serializable]
    public class Commons
    {
        public async Task ResumeAfterCreateIssueFormOption(IDialogContext context, IAwaitable<CreateGitIssue> result)
        {
            CreateGitIssue order = null;
            try
            {
                order = await result;
            }
            catch (OperationCanceledException)
            {
                await context.PostAsync("You canceled the bug!");
                return;
            }


            if (order != null)
            {
                string message = order.Message.ToString();
               

                CallLogicApp(order);
                await context.PostAsync("Issue has been logged.");

            }
            else
            {
                await context.PostAsync("Form returned empty response!");
            }

            context.Done<object>(null);


        }
        private void CallLogicApp(CreateGitIssue result)
        {
            using (WebClient client = new WebClient())
            {
                // get the lead details
                var myLead = result;
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(CreateGitIssue));
                MemoryStream memoryStream = new MemoryStream();
                serializer.WriteObject(memoryStream, myLead);
                var jsonObject = Encoding.Default.GetString(memoryStream.ToArray());

                var webClient = new WebClient();
                webClient.Headers[HttpRequestHeader.ContentType] = "application/json";

                // our function key

                // the url for our Azure Function
                var serviceUrl = "https://prod-16.australiasoutheast.logic.azure.com:443/workflows/0e5a184df04145dcb144391b66a38ce8/triggers/manual/paths/invoke?api-version=2016-10-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=J_duvr8lhjYevbk22DeUCMTVn0Marc5Fr_JxVgyQYPU";

                // upload the data using Post mehtod
                string response = webClient.UploadString(serviceUrl, jsonObject);
            }
        }

        

        private void SearchGithub(string result)
        {
            using (WebClient client = new WebClient())
            {
                // get the lead details
                var myLead = result;
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(CreateGitIssue));
                MemoryStream memoryStream = new MemoryStream();
                serializer.WriteObject(memoryStream, myLead);
                var jsonObject = Encoding.Default.GetString(memoryStream.ToArray());

                var webClient = new WebClient();
                webClient.Headers[HttpRequestHeader.ContentType] = "application/json";

                // our function key

                var serviceUrl = "https://api.github.com/search/code?q=" + result + "+in:file+language:js+repo:jquery/jquery";
                
                // upload the data using Post mehtod
                string response = webClient.UploadString(serviceUrl, jsonObject);
            }
        }

        public async Task AddMessageToQueueAsync(string message)
        {
            // Retrieve storage account from connection string.
            var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["AzureWebJobsStorage"]); 

            // Create the queue client.
            var queueClient = storageAccount.CreateCloudQueueClient();

            // Retrieve a reference to a queue.
            var queue = queueClient.GetQueueReference("bot-queue");

            // Create the queue if it doesn't already exist.
            await queue.CreateIfNotExistsAsync();

            // Create a message and add it to the queue.
            var queuemessage = new CloudQueueMessage(message);
            await queue.AddMessageAsync(queuemessage);
        }
    }
}