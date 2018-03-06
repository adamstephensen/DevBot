namespace LuisBot
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Configuration;
    using System.Web.Http;
    using Dialogs;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Services;
    using Newtonsoft.Json;
    using LuisBot.Model;
    using Newtonsoft.Json.Linq;

    [BotAuthentication]
    public class MessagesController : ApiController
    {
        private static readonly bool IsSpellCorrectionEnabled = bool.Parse(WebConfigurationManager.AppSettings["IsSpellCorrectionEnabled"]);

        private readonly BingSpellCheckService spellService = new BingSpellCheckService();

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                await Conversation.SendAsync(activity, () => new RootDialog());
            }
            else if (activity.Type == ActivityTypes.Event)
            {
                IEventActivity triggerEvent = activity;
                var message = JsonConvert.DeserializeObject<Message>(((JObject)triggerEvent.Value).GetValue("Message").ToString());
                var messageactivity = (Activity)message.RelatesTo.GetPostToBotMessage();

                var client = new ConnectorClient(new Uri(messageactivity.ServiceUrl));
                var triggerReply = messageactivity.CreateReply();
                triggerReply.Text = "I am very sorry to interupt but I thought I would let you know that your build has finished! Great success!";
                await client.Conversations.ReplyToActivityAsync(triggerReply);
            }
            else
            {
                this.HandleSystemMessage(activity);
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}