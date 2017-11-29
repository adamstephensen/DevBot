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
using LuisBot.Model;
using Microsoft.Bot.Builder.ConnectorEx;
using Newtonsoft.Json;
using Octokit;

namespace LuisBot.Dialogs
{
    /// <summary>
    /// The root dialog processes the message and generates a response
    /// </summary>
    [Serializable]
    public class SearchCodeDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            /* Wait until the first message is received from the conversation and call MessageReceviedAsync 
             *  to process that message. */
            
            
            context.Wait(this.MessageRecievedAsync);
        }

        public virtual async Task MessageRecievedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var github = new GitHubClient(new ProductHeaderValue("DevBot"));
            var user = await github.User.Get("Bec-Lyons");

            var query = await result;
            if (query.Text.Trim().Equals("go back"))
            {
                context.Done<object>(null);
            }
            else
            {
                await context.PostAsync("Searching for files with content: " + query.Text);
                // Or we can restrict the search to a specific repo
                var request = new SearchCodeRequest(query.Text, "Bec-Lyons", "DevBot");

                var get = await github.Search.SearchCode(request);
                string allfiles = "";
                if (get.Items.Count > 1)
                {
                    foreach (var item in get.Items)
                    {
                        allfiles += "FILE: " + item.Name + " PATH: " + item.Path + "\n\r";
                    }

                    await context.PostAsync("The following files contain the search term: " + query.Text + "\n\r" + allfiles);
                    await context.PostAsync("Type another query or type go back");
                }
                else
                {
                    await context.PostAsync("No files found with search term '" + query.Text + "'.");
                    await context.PostAsync("Try typing another query or go back to main dialog");


                }
            }
        }
    }
}