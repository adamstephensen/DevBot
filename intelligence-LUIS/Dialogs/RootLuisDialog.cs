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



        [LuisIntent("Find Contributors")]
        public async Task FindContributors(IDialogContext context, LuisResult result)
        {
            string message = $"You want to find contibutors: '{result.Query}'. List results here";

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
            context.Done<object>(null);
        }

        [LuisIntent("Create Issue")]
        public async Task CreateIssue(IDialogContext context, LuisResult result)
        {
            string message = $"You want to find Create and Issue for this repo: '{result.Query}'. Trigger Form flow";
            await context.PostAsync(message);
            var myform = new FormDialog<CreateGitIssue>(new CreateGitIssue(), CreateGitIssue.BuildEnquiryForm, FormOptions.PromptInStart, null);
            context.Call<CreateGitIssue>(myform, new Commons().ResumeAfterCreateIssueFormOption);


            

//            context.Wait(this.MessageReceived);

        }

        private async Task ResumeAfterFormOption(IDialogContext context, IAwaitable<CreateGitIssue> result)
        {
            context.Done<object>(null);

        }

        [LuisIntent("Where is")]
        public async Task Search(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var message = await activity;
            //await context.PostAsync($"Finding the information you need for: '{message.Text}'...");

            var hotelsQuery = new HotelsQuery();

            EntityRecommendation cityEntityRecommendation;

            if (result.TryFindEntity(EntityPersonName, out cityEntityRecommendation))
            {
                cityEntityRecommendation.Type = "Destination";
                
            }
            
            var hotelsFormDialog = new FormDialog<HotelsQuery>(hotelsQuery, this.BuildHotelsForm, FormOptions.PromptInStart, result.Entities);

            context.Call(hotelsFormDialog, this.ResumeAfterHotelsFormDialog);
        }
        /*
        [LuisIntent("ShowHotelsReviews")]
        public async Task Reviews(IDialogContext context, LuisResult result)
        {
            EntityRecommendation hotelEntityRecommendation;

            if (result.TryFindEntity(EntityHotelName, out hotelEntityRecommendation))
            {
                await context.PostAsync($"Looking for reviews of '{hotelEntityRecommendation.Entity}'...");

                var resultMessage = context.MakeMessage();
                resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                resultMessage.Attachments = new List<Attachment>();

                for (int i = 0; i < 5; i++)
                {
                    var random = new Random(i);
                    ThumbnailCard thumbnailCard = new ThumbnailCard()
                    {
                        Title = this.titleOptions[random.Next(0, this.titleOptions.Count - 1)],
                        Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Mauris odio magna, sodales vel ligula sit amet, vulputate vehicula velit. Nulla quis consectetur neque, sed commodo metus.",
                        Images = new List<CardImage>()
                        {
                            new CardImage() { Url = "https://upload.wikimedia.org/wikipedia/en/e/ee/Unknown-person.gif" }
                        },
                    };

                    resultMessage.Attachments.Add(thumbnailCard.ToAttachment());
                }

                await context.PostAsync(resultMessage);
            }

            context.Wait(this.MessageReceived);
        }*/

        [LuisIntent("Authorized")]
        public async Task WhosAuthorized(IDialogContext context, LuisResult result)
        {
            PactDB _db = new Model.PactDB();

            if (_db.Workers.Count() == 0)
            {
                await context.PostAsync($"There are no workers in your Active Directory");
            }
            else
            {
                List<Worker> authWorkers = new List<Worker>();
                List<Worker> ActiveAuthWorkers = new List<Worker>();
                foreach (Worker w in _db.Workers.ToList())
                {
                    if (w.AuthLevel > 2)
                    {
                        authWorkers.Add(w);
                        foreach (WorkerInRoom wir in _db.WorkerInRooms.ToList())
                        {
                            if (wir.WorkerName.Equals(w.Name))
                            {
                                ActiveAuthWorkers.Add(w);
                            }
                        }

                    }
                }

                await context.PostAsync($"{authWorkers.Count()} people are authorized:");

                if (authWorkers.Count() != 0)
                {
                    var resultMessage = context.MakeMessage();
                    resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                    resultMessage.Attachments = new List<Attachment>();


                    foreach (Worker hotel in authWorkers)
                    {

                        HeroCard heroCard = new HeroCard()
                        {
                            Title = hotel.Name,
                            Subtitle = $"Role: {hotel.Role}",
                            Text = $"Authentication Level: {hotel.AuthLevel}",
                            Buttons = new List<CardAction>()
                        {
                            new CardAction()
                            {
                                Title = $"Call {hotel.MobileNo}",
                                Type = ActionTypes.OpenUrl,
                                Value = "skype:+61" + hotel.MobileNo +"?call"
                            }
                        }
                        };

                        resultMessage.Attachments.Add(heroCard.ToAttachment());

                    }
                    await context.PostAsync(resultMessage);

                    if (ActiveAuthWorkers.Count() != 0)
                    {
                        await context.PostAsync("I've detect that the following workers are active at the moment: ");

                        var activeMessage = context.MakeMessage();
                        activeMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                        activeMessage.Attachments = new List<Attachment>();


                        foreach (Worker hotel in ActiveAuthWorkers)
                        {

                            HeroCard heroCard = new HeroCard()
                            {
                                Title = hotel.Name,
                                Subtitle = $"Role: {hotel.Role}",
                                Text = $"Authentication Level: {hotel.AuthLevel}",
                                Buttons = new List<CardAction>()
                        {
                            new CardAction()
                            {
                                Title = $"Call {hotel.MobileNo}",
                                Type = ActionTypes.OpenUrl,
                                Value = "skype:+61" + hotel.MobileNo +"?call"
                            }
                        }
                            };

                            activeMessage.Attachments.Add(heroCard.ToAttachment());

                        }
                        await context.PostAsync(activeMessage);
                    }

                }
                context.Wait(this.MessageReceived);
            }
        }

        [LuisIntent("Activated")] 
        public async Task WhosActive(IDialogContext context, LuisResult result)
        {
            PactDB _db = new Model.PactDB();

            if (_db.WorkerInRooms.Count() == 0)
            {
                await context.PostAsync($"Looks like everyone has gone home for the day. No active workers found");
            }
            else
            {
                await context.PostAsync($"I found {_db.WorkerInRooms.Count()} people:");

                var resultMessage = context.MakeMessage();
                resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                resultMessage.Attachments = new List<Attachment>();


                foreach (WorkerInRoom hotel in _db.WorkerInRooms.ToList())
                {
                    Worker worker = new Worker();
                    foreach (Worker work in _db.Workers.ToList())
                    {
                        if (work.Name.Equals(hotel.WorkerName))
                        {
                            worker = work;
                            break;
                        }
                    }
                    HeroCard heroCard = new HeroCard()
                    {
                        Title = hotel.WorkerName,
                        Subtitle = $"Role: {worker.Role}",
                        Text = $"Authentication Level: {worker.AuthLevel}",
                        Buttons = new List<CardAction>()
                        {
                            new CardAction()
                            {
                                Title = $"Call {worker.MobileNo}",
                                Type = ActionTypes.OpenUrl,
                                Value = "skype:+61" + worker.MobileNo +"?call"
                            }
                        }
                    };

                    resultMessage.Attachments.Add(heroCard.ToAttachment());

                }
                await context.PostAsync(resultMessage);
            }
            context.Wait(this.MessageReceived);
        }

        [LuisIntent("Help")]
        public async Task Help(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Try asking me things like 'Find me a worker?', 'Who is active?' or 'Who is authorized to use a hammer?'");

            context.Wait(this.MessageReceived);
        }

        private IForm<HotelsQuery> BuildHotelsForm()
        {
            OnCompletionAsyncDelegate<HotelsQuery> processHotelsSearch = async (context, state) =>
            {
                var message = "Searching all workshops";
                if (!string.IsNullOrEmpty(state.Destination))
                {
                    message += $" for {state.Destination}...";
                }
                else if (!string.IsNullOrEmpty(state.AirportCode))
                {
                    message += $" near {state.AirportCode.ToUpperInvariant()} airport...";
                }

                await context.PostAsync(message);
            };

            return new FormBuilder<HotelsQuery>()
                .Field(nameof(HotelsQuery.Destination), (state) => string.IsNullOrEmpty(state.AirportCode))
                .Field(nameof(HotelsQuery.AirportCode), (state) => string.IsNullOrEmpty(state.Destination))
                .OnCompletion(processHotelsSearch)
                .Build();
        }

        private async Task ResumeAfterHotelsFormDialog(IDialogContext context, IAwaitable<HotelsQuery> result)
        {
            try
            {
                var searchQuery = await result;
                
                PactDB _db = new Model.PactDB();


                string workerFound="";
                foreach (WorkerInRoom hotel in _db.WorkerInRooms.ToList())
                {
                    if (hotel.WorkerName.ToLower().Trim().Equals(searchQuery.Destination.Trim()))
                    {
                        workerFound = hotel.WorkerName;
                        break;
                    }
                }

                if (workerFound.Equals(""))
                {
                    await context.PostAsync($"Sorry, I couldn't find " +searchQuery.Destination);
                }

                else 
                {
                    await context.PostAsync(searchQuery.Destination + " was found in Workshop 1 (AKA booth area)");
                }
                /*else
                { 

                    var resultMessage = context.MakeMessage();
                    resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                    resultMessage.Attachments = new List<Attachment>();


                    foreach (WorkerInRoom hotel in _db.WorkerInRooms.ToList())
                    {
                        Worker worker = new Worker();
                        foreach (Worker work in _db.Workers.ToList())
                        {
                            if (work.Name.Equals(hotel.WorkerName))
                            {
                                worker = work;
                                break;
                            }
                        }
                        HeroCard heroCard = new HeroCard()
                        {
                            Title = hotel.WorkerName,
                            Subtitle = $"Role: {worker.Role}",
                            Text = $"Authentication Level: {worker.AuthLevel}",
                            Images = new List<CardImage>()
                         {
                             new CardImage() { Url = "https://placeholdit.imgix.net/~text?txtsize=35&txt=Hotel+{i}&w=500&h=260" }
                         },
                            Buttons = new List<CardAction>()
                        {
                            new CardAction()
                            {
                                Title = $"Call {worker.MobileNo}",
                                Type = ActionTypes.OpenUrl,
                                Value = "https://www.bing.com/search?q=hotels+in"
                            }
                        }
                        };

                        resultMessage.Attachments.Add(heroCard.ToAttachment());
                    }
                    
                }

                /*var hotels = await this.GetHotelsAsync(searchQuery);

                await context.PostAsync($"I found {hotels.Count()} hotels:");

                var resultMessage = context.MakeMessage();
                resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                resultMessage.Attachments = new List<Attachment>();

                foreach (var hotel in hotels)
                {
                    HeroCard heroCard = new HeroCard()
                    {
                        Title = hotel.Name,
                        Subtitle = $"{hotel.Rating} starts. {hotel.NumberOfReviews} reviews. From ${hotel.PriceStarting} per night.",
                        Images = new List<CardImage>()
                        {
                            new CardImage() { Url = hotel.Image }
                        },
                        Buttons = new List<CardAction>()
                        {
                            new CardAction()
                            {
                                Title = "More details",
                                Type = ActionTypes.OpenUrl,
                                Value = $"https://www.bing.com/search?q=hotels+in+" + HttpUtility.UrlEncode(hotel.Location)
                            }
                        }
                    };

                    resultMessage.Attachments.Add(heroCard.ToAttachment());
                }*/

                //await context.PostAsync(resultMessage);
            }
            catch (FormCanceledException ex)
            {
                string reply;

                if (ex.InnerException == null)
                {
                    reply = "You have canceled the operation.";
                }
                else
                {
                    reply = $"Oops! Something went wrong :( Technical Details: {ex.InnerException.Message}";
                }

                await context.PostAsync(reply);
            }
            finally
            {
                context.Done<object>(null);
            }
        }

        private async Task<IEnumerable<Hotel>> GetHotelsAsync(HotelsQuery searchQuery)
        {
            var hotels = new List<Hotel>();

            // Filling the hotels results manually just for demo purposes
            for (int i = 1; i <= 5; i++)
            {
                var random = new Random(i);
                Hotel hotel = new Hotel()
                {
                    Name = $"{searchQuery.Destination ?? searchQuery.AirportCode} Hotel {i}",
                    Location = searchQuery.Destination ?? searchQuery.AirportCode,
                    Rating = random.Next(1, 5),
                    NumberOfReviews = random.Next(0, 5000),
                    PriceStarting = random.Next(80, 450),
                    Image = $"https://placeholdit.imgix.net/~text?txtsize=35&txt=Hotel+{i}&w=500&h=260"
                };

                hotels.Add(hotel);
            }

            hotels.Sort((h1, h2) => h1.PriceStarting.CompareTo(h2.PriceStarting));

            return hotels;
        }
    }
}
