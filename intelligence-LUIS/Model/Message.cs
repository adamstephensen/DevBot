using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LuisBot.Model
{
    public class Message
    {
        public ConversationReference RelatesTo { get; set; }
        public String Text { get; set; }
    }
}