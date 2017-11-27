namespace LuisBot
{
    using System;
    using Microsoft.Bot.Builder.FormFlow;

    [Serializable]
    public class HotelsQuery
    {
        //[Prompt("Who are you looking for? {&}")]
        [Prompt("Who are you looking for?")]
        [Optional]
        public string Destination { get; set; }

        [Prompt("Near which Airport")]
        [Optional]
        public string AirportCode { get; set; }
    }
}