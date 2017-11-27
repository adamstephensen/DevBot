using Microsoft.Bot.Builder.Dialogs;
using QnAMakerDialog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;

namespace LuisBot.Dialogs
{
    [Serializable]
    [QnAMakerService("2eb9aeee53b84bcf98bbf9d41b2c9220", "50a63d42-b55f-4049-a32e-c033b4dfd9ec")]
    public class QNADialog : QnAMakerDialog<object>
    {

        public override async Task NoMatchHandler(IDialogContext context, string originalQueryText)
        {
            await context.Forward(new RootLuisDialog(),ResumeAfterOptionDialogAsync,context.Activity, System.Threading.CancellationToken.None);
        }

        private async Task ResumeAfterOptionDialogAsync(IDialogContext context, IAwaitable<object> result)
        {
            context.Done<object>(null);
        }

        [QnAMakerResponseHandler(50)]
        public async Task LowScoreHandler(IDialogContext context, string originalQueryText, QnAMakerResult result) 
        {
            await context.PostAsync($"I've found an answer that might help... {result.Answer}.");
            context.Wait(MessageReceived);
        }


    }
}