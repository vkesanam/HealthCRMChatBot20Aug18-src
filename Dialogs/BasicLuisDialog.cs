using System;
using System.Configuration;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;

namespace Microsoft.Bot.Sample.LuisBot
{
    // For more information about this template visit http://aka.ms/azurebots-csharp-luis
    [Serializable]
    public class BasicLuisDialog : LuisDialog<object>
    {
        public BasicLuisDialog() : base(new LuisService(new LuisModelAttribute(
            ConfigurationManager.AppSettings["LuisAppId"], 
            ConfigurationManager.AppSettings["LuisAPIKey"], 
            domain: ConfigurationManager.AppSettings["LuisAPIHostName"])))
        {
        }

        string customerName;
        string complaint;
        string email;
        string phone;
        string servicename;
        string appointmentdate;
        string hospitalname;
        string doctorname;
        private string name;

        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowLuisResult(context, result);
        }

        // Go to https://luis.ai and create a new intent, then train/publish your luis app.
        // Finally replace "Greeting" with the name of your newly created intent in the following handler
        [LuisIntent("Greeting")]
        public async Task GreetingIntent(IDialogContext context, LuisResult result)
        {
            //await this.ShowLuisResult(context, result);
            if (customerName == null)
            {
                string message = "Glad to talk to you. Welcome to Virtual Customer Service.";
                //await context.PostAsync(message);

                PromptDialog.Text(
                context: context,
                resume: CustomerNameFromGreeting,
                prompt: "May i know your Name please?",
                retry: "Sorry, I don't understand that.");
            }
            else
            {
                string message = "Tell me " + customerName + ". How i can help you?";
                await context.PostAsync(message);
            }
        }
        public async Task CustomerNameFromGreeting(IDialogContext context, IAwaitable<string> result)
        {
            string response = await result;
            customerName = response;

            string message = "Thanks " + customerName + ".Tell me. How i can help you?";
            await context.PostAsync(message);
        }

        [LuisIntent("Cancel")]
        public async Task CancelIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowLuisResult(context, result);
        }

        [LuisIntent("Help")]
        public async Task HelpIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowLuisResult(context, result);
        }

        private async Task ShowLuisResult(IDialogContext context, LuisResult result) 
        {
            await context.PostAsync($"You have reached {result.Intents[0].Intent}. Welcome to CRM: {result.Query}");
            context.Wait(MessageReceived);
        }
    }
}