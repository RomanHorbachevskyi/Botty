// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.3.0

using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Botty.Dialogs
{
    public class BookingTourDialog : CancelAndHelpDialog
    {
        public BookingTourDialog()
            : base(nameof(BookingTourDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new AttachmentPrompt(nameof(AttachmentPrompt)));
            AddDialog(new DateResolverDialog());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                //DestinationCountryStepAsync,
                //OriginCountryStepAsync,
                //OriginCityStepAsync,
                //TravelDateStepAsync,
                //NightsNumberStepAsync,
                LoadCardAsync,
                //HandleResponseAsync,
                ConfirmStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> DestinationCountryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var bookingDetails = (BookingTourDetails)stepContext.Options;

            if (bookingDetails.ArrivalCountry == null)
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Where would you like to travel to (country)?") }, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(bookingDetails.ArrivalCountry, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> OriginCountryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var bookingDetails = (BookingTourDetails)stepContext.Options;

            bookingDetails.ArrivalCountry = (string)stepContext.Result;

            if (bookingDetails.DepartureCountry == null)
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("What is country you are traveling from?") }, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(bookingDetails.DepartureCountry, cancellationToken);
            }
        }
        private async Task<DialogTurnResult> OriginCityStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var bookingDetails = (BookingTourDetails)stepContext.Options;

            bookingDetails.DepartureCountry = (string)stepContext.Result;

            if (bookingDetails.DepartureCity == null)
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("What is city you are traveling from?") }, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(bookingDetails.DepartureCity, cancellationToken);
            }
        }
        private async Task<DialogTurnResult> TravelDateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var bookingDetails = (BookingTourDetails)stepContext.Options;

            bookingDetails.DepartureCity = (string)stepContext.Result;

            if (bookingDetails.DepartureDate == null || IsAmbiguous(bookingDetails.DepartureDate))
            {
                return await stepContext.BeginDialogAsync(nameof(DateResolverDialog), bookingDetails.DepartureDate, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(bookingDetails.DepartureDate, cancellationToken);
            }
        }
        private async Task<DialogTurnResult> NightsNumberStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var bookingDetails = (BookingTourDetails)stepContext.Options;

            bookingDetails.DepartureDate = (string)stepContext.Result;

            if (bookingDetails.NumberOfNights == null) //|| IsAmbiguous(bookingDetails.DepartureDate)
            {
                return await stepContext.BeginDialogAsync(nameof(NumberPrompt<byte>), new PromptOptions { Prompt = MessageFactory.Text("How many nights are you planning to be there?") }, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(bookingDetails.NumberOfNights, cancellationToken);
            }
        }


        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var bookingDetails = (BookingTourDetails)stepContext.Options;

            bookingDetails.NumberOfNights = stepContext.Result.ToString();

            var msg = $"Please confirm, I have you traveling to: {bookingDetails.ArrivalCountry} from: " +
                      $"{bookingDetails.DepartureCity}, {bookingDetails.DepartureCountry} on: {bookingDetails.DepartureDate}, " +
                      $"\nnumber of nights {bookingDetails.NumberOfNights}";

            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text(msg) }, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                var bookingDetails = (BookingTourDetails)stepContext.Options;

                return await stepContext.EndDialogAsync(bookingDetails, cancellationToken);
            }
            else
            {
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
        }

        private static bool IsAmbiguous(string timex)
        {
            var timexProperty = new TimexProperty(timex);
            return !timexProperty.Types.Contains(Constants.TimexTypes.Definite);
        }



        private async Task<DialogTurnResult> LoadCardAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var welcomeCard = CreateAdaptiveCardAttachment();
            var response = CreateResponse(stepContext.Context.Activity, welcomeCard);

            //await stepContext.Context.SendActivityAsync(response, cancellationToken);
            return await stepContext.PromptAsync(nameof(TextPrompt),
                new PromptOptions {Prompt = response}, cancellationToken);
            
            await stepContext.ContinueDialogAsync(cancellationToken);
            //var adaptiveCard = File.ReadAllText(@".\Cards\TourBookingCard.json");
            if (stepContext.Context.Responded)
            {
                return await this.ResumeDialogAsync(stepContext, stepContext.Reason, stepContext.Result, cancellationToken);
                
            }

            return await this.ContinueDialogAsync(stepContext, cancellationToken);
            ;
            //return await this.ContinueDialogAsync(stepContext, cancellationToken);

            //return await stepContext.NextAsync((BookingTourDetails)stepContext.Options, cancellationToken);
            ////return await stepContext.PromptAsync(nameof(AttachmentPrompt),
            ////    new PromptOptions{Prompt = response}, cancellationToken);
            // Create the text prompt
            //var opts = new PromptOptions
            //{
            //    Prompt = new Activity
            //    {
            //        Type = ActivityTypes.Message,
            //        Text = "waiting for user input...", // You can comment this out if you don't want to display any text. Still works.
            //    }
            //};

            // Display a Text Prompt and wait for input
            //return await stepContext.PromptAsync(nameof(TextPrompt), opts, cancellationToken);


            //return await stepContext.NextAsync((stepContext.Options as BookingTourDetails), cancellationToken);
            //return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions{Prompt = response}, cancellationToken);

            //if ((bool)stepContext.Result)
            //{
            //    var bookingDetails = (BookingTourDetails)stepContext.Options;

            //    return await stepContext.EndDialogAsync(bookingDetails, cancellationToken);
            //}
            //else
            //{
            //    return await stepContext.EndDialogAsync(null, cancellationToken);
            //}
        }
        private async Task HandleResponseAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            // Do something with step.result
            // Adaptive Card submissions are objects, so you likely need to JObject.Parse(step.result)
            //await stepContext.Context.SendActivityAsync($"INPUT: {stepContext.Result}");
            //return await stepContext.NextAsync();



            var token = JToken.Parse(turnContext.Activity.ChannelData.ToString());
            string recievedData = string.Empty;
            string DepartureCountry = string.Empty;
            string DepartureCity = string.Empty;
            string ArrivalCountry = string.Empty;
            string DepartureDate = string.Empty;
            string ArrivalDate = string.Empty;
            string NumberOfAdults = string.Empty;
            string NumberOfChildrenYounger12 = string.Empty;

            if (System.Convert.ToBoolean(token["postback"].Value<string>()))
            {
                //JToken commandToken = JToken.Parse(turnContext.Activity.Value.ToString());
                //string command = commandToken["action"].Value<string>();

                //if (command.ToLowerInvariant() == "dataselector")
                //{
                //    recievedData = commandToken["choiceset"].Value<string>();
                //}

            }

            await turnContext.SendActivityAsync($"You Selected {recievedData}", cancellationToken: cancellationToken);
        }




        // Create an attachment message response.
        private Activity CreateResponse(IActivity activity, Attachment attachment)
        {
            var response = ((Activity)activity).CreateReply();
            response.Attachments = new List<Attachment>() { attachment };
            return response;
        }

        // Load attachment from file.
        private Attachment CreateAdaptiveCardAttachment()
        {
            // combine path for cross platform support
            string[] paths = { ".", "Cards", "TourBookingCard.json" };
            string fullPath = Path.Combine(paths);
            var adaptiveCard = File.ReadAllText(fullPath);
            //return new Attachment()
            //{
            //    ContentType = "application/vnd.microsoft.card.adaptive",
            //    Content = JsonConvert.DeserializeObject(adaptiveCard),
            //};

            //var card = new AdaptiveCard();
            // Get a JSON-serialized payload
            // Your app will probably get cards from somewhere else :)
            ////var client = new HttpClient("http://adaptivecards.io/payloads/ActivityUpdate.json");
            ////var response = await client.GetAsync(cardUrl);
            var json = adaptiveCard; // await response.Content.ReadAsStringAsync();

            // Parse the JSON 
            AdaptiveCardParseResult result = AdaptiveCard.FromJson(json);

            // Get card from result
            AdaptiveCard card = result.Card;
            
            // Optional: check for any parse warnings
            // This includes things like unknown element "type"
            // or unknown properties on element
            IList<AdaptiveWarning> warnings = result.Warnings;
            return new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = card
            };
        }
    }
}
