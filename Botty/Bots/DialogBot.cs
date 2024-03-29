// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.3.0

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Botty.Bots
{
    // This IBot implementation can run any type of Dialog. The use of type parameterization is to allows multiple different bots
    // to be run at different endpoints within the same project. This can be achieved by defining distinct Controller types
    // each with dependency on distinct IBot types, this way ASP Dependency Injection can glue everything together without ambiguity.
    // The ConversationState is used by the Dialog system. The UserState isn't, however, it might have been used in a Dialog implementation,
    // and the requirement is that all BotState objects are saved at the end of a turn.
    public class DialogBot<T> : ActivityHandler where T : Dialog
    {
        protected readonly Dialog Dialog;
        protected readonly BotState ConversationState;
        protected readonly BotState UserState;
        protected readonly ILogger Logger;

        public DialogBot(ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger)
        {
            ConversationState = conversationState;
            UserState = userState;
            Dialog = dialog;
            Logger = logger;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occured during the turn.
            await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await UserState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Running dialog with Message Activity.");

            

            if (turnContext.Activity.GetType().GetProperty("ChannelData") != null)
            {
                var channelData = JObject.Parse(turnContext.Activity.ChannelData.ToString());
                if (channelData.ContainsKey("postBack"))
                {
                    var token = JToken.Parse(turnContext.Activity.ChannelData.ToString());
                    string recievedData = string.Empty;
                    string DepartureCountry = string.Empty;
                    string DepartureCity = string.Empty;
                    string ArrivalCountry = string.Empty;
                    string DepartureDate = string.Empty;
                    string ArrivalDate = string.Empty;
                    string NumberOfAdults = string.Empty;
                    string NumberOfChildrenYounger12 = string.Empty;

                    await turnContext.SendActivityAsync(turnContext.Activity.Value.ToString(), cancellationToken:cancellationToken);

                    //if (System.Convert.ToBoolean(token["postBack"].Value<string>()))
                    {
                        JToken commandToken = JToken.Parse(turnContext.Activity.Value.ToString());
                        string command = commandToken["action"].Value<string>();

                        if (command.ToLowerInvariant() == "datasender")
                        {
                            
                            //recievedData = commandToken["choiceset"].Value<string>();
                            DepartureCountry = commandToken["DepartureCountry"].Value<string>();
                            DepartureCity = commandToken["DepartureCity"].Value<string>();
                            ArrivalCountry = commandToken["ArrivalCountry"].Value<string>();
                            DepartureDate = commandToken["DepartureDate"].Value<string>();
                            ArrivalDate = commandToken["ArrivalDate"].Value<string>();
                            NumberOfAdults = commandToken["NumberOfAdults"].Value<string>();
                            NumberOfChildrenYounger12 = commandToken["NumberOfChildrenYounger12"].Value<string>();
                            await turnContext.SendActivityAsync($"You Selected {DepartureCountry}, " +
                                                                $"{DepartureCity}, " +
                                                                $"{ArrivalCountry}, " +
                                                                $"{DepartureDate}, " +
                                                                $"{ArrivalDate}, " +
                                                                $"{NumberOfAdults}, " +
                                                                $"{NumberOfChildrenYounger12}, ",
                                cancellationToken: cancellationToken);
                            await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
                            await UserState.SaveChangesAsync(turnContext, false, cancellationToken);
                            
                        }
                        else
                        {
                            // Run the Dialog with the new message Activity.
                            //await Dialog.Run(turnContext, ConversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
                        }
                    }
                }
                else
                {
                    // Run the Dialog with the new message Activity.
                    //await Dialog.Run(turnContext, ConversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
                }
            }
            else
            {
                // Run the Dialog with the new message Activity.
                //await Dialog.Run(turnContext, ConversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
            }


            // Run the Dialog with the new message Activity.
            await Dialog.Run(turnContext, ConversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
        }
    }
}
