using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using Microsoft.Bot.Builder.FormFlow;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using formflow.FormFlow;
using LuisBot.Services;

namespace LuisBot.Dialogs
{
    /// <summary>
    /// The root dialog processes the message and generates a response
    /// </summary>
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private const string QNAOption = "Ask Questions about Repo";
        private const string FindContributorsOption = "Find Contributors";
        private const string SubmitIssueOption = "Submit an issue";
        private Commons commons;
        public async Task StartAsync(IDialogContext context)
        {
            /* Wait until the first message is received from the conversation and call MessageReceviedAsync 
             *  to process that message. */
             commons = new Commons();
            await context.PostAsync("Hi. I'm A GIT bot. How can I help you today?");
            context.Wait(this.MessageRecievedAsync);
        }

        public virtual async Task MessageRecievedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            /* When MessageReceivedAsync is called, it's passed an IAwaitable<IMessageActivity>. To get the message,
             *  await the result. */
            //Show options whatever users chat
            PromptDialog.Choice(context, this.AfterMenuSelection, new List<string>() { QNAOption, SubmitIssueOption, FindContributorsOption }, "What can I do you for?");
        }

        //After users select option, Bot call other dialogs
        private async Task AfterMenuSelection(IDialogContext context, IAwaitable<string> result)
        {
            var optionSelected = await result;
            switch (optionSelected)
            {
                case QNAOption:
                    await context.PostAsync("Go ahead and ask me your burning questions about this awesome repository.");
                    context.Call(new QNADialog(), ResumeAfterOptionDialogAsync);
                    break;

                case SubmitIssueOption:
                    var myform = new FormDialog<CreateGitIssue>(new CreateGitIssue(), CreateGitIssue.BuildEnquiryForm, FormOptions.PromptInStart, null);
                    context.Call<CreateGitIssue>(myform, commons.ResumeAfterCreateIssueFormOption);
                    break;

            }
        }

        private async Task ResumeAfterOptionDialogAsync(IDialogContext context, IAwaitable<object> result)
        {
            //throw new NotImplementedException();
            await context.PostAsync("Lets start again");
            PromptDialog.Choice(context, this.AfterMenuSelection, new List<string>() { QNAOption, SubmitIssueOption, FindContributorsOption }, "What can I do you for?");


        }

        private async Task ResumeAfterFormOption(IDialogContext context, IAwaitable<CreateGitIssue> result)
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
                
               

                //CallSentimentAzureFunction(myLead);
                await context.PostAsync("Issue has been logged.");

            }
            else
            {
                await context.PostAsync("Form returned empty response!");
            }

            await context.PostAsync("What would you like to do now?");
            context.Wait(MessageRecievedAsync);
        }



    }
}