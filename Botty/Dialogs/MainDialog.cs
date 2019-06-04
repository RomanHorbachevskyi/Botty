// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.3.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using Microsoft.Recognizers.Text.Number;

namespace Botty.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        protected readonly IConfiguration Configuration;
        protected readonly ILogger Logger;
        protected bool BookHotel;

        public MainDialog(IConfiguration configuration, ILogger<MainDialog> logger)
            : base(nameof(MainDialog))
        {
            Configuration = configuration;
            Logger = logger;
            BookHotel=false;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>)));
            AddDialog(new BookingTourDialog());
            AddDialog(new BookingHotelDialog());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                ActStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //if (string.IsNullOrEmpty(Configuration["LuisAppId"]) || string.IsNullOrEmpty(Configuration["LuisAPIKey"]) || string.IsNullOrEmpty(Configuration["LuisAPIHostName"]))
            //{
            //    await stepContext.Context.SendActivityAsync(
            //        MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file."), cancellationToken);

            //    return await stepContext.NextAsync(null, cancellationToken);
            //}
            //else
            //{
            //    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("What can I help you with today?\nSay something like \"Book a flight from Paris to Berlin on March 22, 2020\"") }, cancellationToken);
            //}


            //return await stepContext.NextAsync(null, cancellationToken);

            //return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("What do you want to book,\nsome tour or hotel?") }, cancellationToken);
            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text("What do you want to book,\nsome tour or hotel?"),
                Choices = new[] { new Choice { Value = "Tour" }, new Choice { Value = "Hotel" } }.ToList()
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Call LUIS and gather any potential booking details. (Note the TurnContext has the response to the prompt.)
            ////var bookingDetails = stepContext.Result != null
            ////        ?
            ////    await LuisHelper.ExecuteLuisQuery(Configuration, Logger, stepContext.Context, cancellationToken)
            ////        : new BookingTourDetails();

            // In this sample we only have a single Intent we are concerned with. However, typically a scenario
            // will have multiple different Intents each corresponding to starting a different child Dialog.

            // Run the BookingDialog giving it whatever details we have from the LUIS call, it will fill out the remainder.
            //return await stepContext.BeginDialogAsync(nameof(BookingHotelDialog), bookingDetails, cancellationToken);

            //var bookingHotelDetails = (BookingHotelDetails) stepContext.Options;
            //var bookingTourDetails = (BookingTourDetails)stepContext.Options;
            //if (bookingHotelDetails.)
            //{

            //}


            if (stepContext.Result.ToString().ToLower().Contains("tour"))
            {
                BookHotel = false;
            }
            else if (stepContext.Result.ToString().ToLower().Contains("hotel"))
            {
                BookHotel = true;
            }
            
            if (BookHotel)
            {
                var bookingDetails = new BookingHotelDetails();
                return await stepContext.BeginDialogAsync(nameof(BookingHotelDialog), bookingDetails, cancellationToken);
            }
            else
            {
                var bookingDetails = new BookingTourDetails();
                return await stepContext.BeginDialogAsync(nameof(BookingTourDialog), bookingDetails, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // If the child dialog ("BookingDialog") was cancelled or the user failed to confirm, the Result here will be null.
            if (stepContext.Result != null)
            {
                var result = (BookingTourDetails)stepContext.Result;

                // Now we have all the booking details call the booking service.

                // If the call to the booking service was successful tell the user.

                var timeProperty = new TimexProperty(result.DepartureDate);
                var travelDateMsg = timeProperty.ToNaturalLanguage(DateTime.Now);
                var msg = $"I have you booked to {result.ArrivalCountry} from " +
                          $"{result.DepartureCountry}, {result.DepartureCity} on {travelDateMsg}";
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Thank you."), cancellationToken);
            }
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
