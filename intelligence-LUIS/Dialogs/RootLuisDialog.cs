namespace LuisBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.FormFlow;
    using Microsoft.Bot.Builder.Luis;
    using Microsoft.Bot.Builder.Luis.Models;
    using Microsoft.Bot.Connector;
    using LuisBot.Model;
    using formflow.FormFlow;
    using LuisBot.Services;
    using Microsoft.Bot.Builder.ConnectorEx;
    using Newtonsoft.Json;

    [LuisModel("7e80b19f-15a0-4c33-8568-abb1142d556d", "858ddb41c50843ef8e2bbbe5ccb95a3c")]
    [Serializable]
    public class RootLuisDialog : LuisDialog<object>
    {
        private const string EntityGeographyCity = "builtin.geography.city";

        private const string EntityHotelName = "Hotel";

        private const string EntityPersonName = "Person";

        private const string EntityAirportCode = "AirportCode";

        private IList<string> titleOptions = new List<string> { "“Very stylish, great stay, great staff”", "“good hotel awful meals”", "“Need more attention to little things”", "“Lovely small hotel ideally situated to explore the area.”", "“Positive surprise”", "“Beautiful suite and resort”" };
        private string originalQueryText;



        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Sorry, I did not understand '{result.Query}'. Type 'help' if you need assistance.";

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("Queue Build")]
        public async Task QueueBuild(IDialogContext context, LuisResult result)
        {
            string message = $"You want to Queue Build: '{result.Query}'.";

            await context.PostAsync(message);

            var queueMessage = new Message
            {
                RelatesTo = context.Activity.ToConversationReference(),
                Text = "New Queue"
            };
            Commons commons = new Commons();
            // write the queue Message to the queue
            await commons.AddMessageToQueueAsync(JsonConvert.SerializeObject(queueMessage));

            await context.PostAsync("New build has been queued. I'll notify you when it's done");
            await context.PostAsync("What would you like to do next?");

        }

        [LuisIntent("Find Code")]
        public async Task FindCode(IDialogContext context, LuisResult result)
        {
            string message = "You want to find some specific code in this repo. What would you like to search for?";

            await context.PostAsync(message);

            context.Call(new SearchCodeDialog(), ResumeAfterFormOption);
        }


        [LuisIntent("Create Issue")]
        public async Task CreateIssue(IDialogContext context, LuisResult result)
        {
            string message = $"You want to find Create and Issue for this repo: '{result.Query}'. Trigger Form flow";
            await context.PostAsync(message);
            var myform = new FormDialog<CreateGitIssue>(new CreateGitIssue(), CreateGitIssue.BuildEnquiryForm, FormOptions.PromptInStart, null);
            context.Call<CreateGitIssue>(myform, new Commons().ResumeAfterCreateIssueFormOption);

        }

        private async Task ResumeAfterFormOption(IDialogContext context, IAwaitable<object> result)
        {
            context.Done<object>(null);

        }

       
    }
}
