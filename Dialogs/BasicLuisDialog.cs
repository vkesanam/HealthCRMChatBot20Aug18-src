using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;

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
            
             string message = "I'm afraid I cannot help you with that. Please hold on a minute i will transfer to a human agent.";
            await context.PostAsync(message);
        }

        // Go to https://luis.ai and create a new intent, then train/publish your luis app.
        // Finally replace "Greeting" with the name of your newly created intent in the following handler
        [LuisIntent("Greeting")]
        public async Task GreetingIntent(IDialogContext context, LuisResult result)
        {
            //await this.ShowLuisResult(context, result);
            if (customerName == null)
            {
                string message = "Glad to talk to you. Welcome to Health Customer Service.";
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
        [LuisIntent("CASE")]
        public async Task CASE(IDialogContext context, LuisResult result)
        {
            PromptDialog.Text(
            context: context,
            resume: CustomerNameHandler,
            prompt: "What is your complaint/suggestion?",
            retry: "Sorry, I don't understand that.");
        }
        public async Task CustomerNameHandler(IDialogContext context, IAwaitable<string> result)
        {
            string response = await result;
            complaint = response;

            PromptDialog.Text(
            context: context,
            resume: CustomerEmailHandler,
            prompt: "What is the best number to contact you?",
            retry: "Sorry, I don't understand that.");
        }
        public async Task CustomerEmailHandler(IDialogContext context, IAwaitable<string> result)
        {
            string response = await result;
            phone = response;

            PromptDialog.Text(
            context: context,
            resume: FinalResultHandler,
            prompt: "What is your email id?",
            retry: "Sorry, I don't understand that.");
        }
        public async Task FinalResultHandler(IDialogContext context, IAwaitable<string> argument)
        {
            string response = await argument;
            email = response;

            //string refno =CRMCase.CreateCaseRegistration(complaint, customerName, phone, email);
            //string refe= await getCreateCase(context,complaint, customerName, phone, email);
            //if (refno != null)
            //{
            //{Environment.NewLine}Reference Number: {refno},
            await context.PostAsync($@"Thank you for your interest, your request has been logged. Our customer service team will get back to you shortly.
                                    {Environment.NewLine}Your service request  summary:
                                   
                                    {Environment.NewLine}Complaint Title: {complaint},
                                    {Environment.NewLine}Customer Name: {customerName},
                                    {Environment.NewLine}Phone Number: {phone},
                                    {Environment.NewLine}Email: {email}");

            PromptDialog.Confirm(
            context: context,
            resume: AnythingElseHandler,
            prompt: "Is there anything else that I could help?",
            retry: "Sorry, I don't understand that.");
            //}
        }
        public async Task AnythingElseHandler(IDialogContext context, IAwaitable<bool> argument)
        {
            var answer = await argument;
            if (answer)
            {
                await GeneralGreeting(context, null);
            }
            else
            {
                string message = $"Thanks for using I Bot. Hope you have a great day!";
                await context.PostAsync(message);

                //var survey = context.MakeMessage();

                //var attachment = GetSurveyCard();
                //survey.Attachments.Add(attachment);

                //await context.PostAsync(survey);

                context.Done<string>("conversation ended.");
            }
        }
        public virtual async Task GeneralGreeting(IDialogContext context, IAwaitable<string> argument)
        {
            string message = $"Great! What else that can I help you?";
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("STATUS")]
        public async Task CaseStatus(IDialogContext context, LuisResult result)
        {
            PromptDialog.Text(
            context: context,
            resume: CaseRefStatus,
            prompt: "Enter the Case Reference Number.",
            retry: "Didn't get that!");
        }
        public virtual async Task CaseRefStatus(IDialogContext context, IAwaitable<string> result)
        {
            var refno = await result;
            //var status = CRMCase.CaseRefStatus(refno.ToString());
            //if(status!=null)
            //{
            await context.PostAsync("Your complaint/Suggestion status is: In Progress");

            PromptDialog.Confirm(
            context: context,
            resume: AnythingElseHandler,
            prompt: "Is there anything else that I could help?",
            retry: "Sorry, I don't understand what you typing. Please select Yes to check again.");
            //}

        }

        [LuisIntent("APPOINTMENT")]
        public async Task APPOINTMENT(IDialogContext context, LuisResult result)
        {
            PromptDialog.Text(
           context: context,
           resume: AppointmentIssue,
           prompt: "Sure, May i know what is your issue?",
           retry: "Didn't get that!");
        }

        public async Task AppointmentIssue(IDialogContext context, IAwaitable<string> argument)
        {
            PromptDialog.Text(
          context: context,
          resume: AppointmentLocation,
          prompt: "Sorry to hear that, May i know your preferred location?",
          retry: "Didn't get that!");
        }

        public async Task AppointmentLocation(IDialogContext context, IAwaitable<string> argument)
        {
            var selectedCard = await argument;

            var message = context.MakeMessage();

            var attachment = GetSelectedCard(selectedCard);
            message.Attachments.Add(attachment);

            await context.PostAsync(message);


            PromptDialog.Confirm(
             context: context,
             resume: AppointmentDateAndTime,
             prompt: "Do you want me to go ahead book an appointment for you?",
             retry: "Sorry, I don't understand that.");
        }

        public async Task AppointmentDateAndTime(IDialogContext context, IAwaitable<bool> argument)
        {
            var answer = await argument;
            if (answer)
            {
                PromptDialog.Choice(context, ResumeTypeDoctorNamesOptionsAsync,
                     new List<string>()
                     {
                        "The All England Practice",
                        "Al Safa Primary Health Care Center",
                        "Al Mankhool Health Center",
                        "AL Barsha Health Centre",
                        "Nadd Al Hamar Health Center",
                        "Dubai Gynaecology & Fertility Centre",
                        "Zabeel Health Center",
                        "Al Barsha Clinic",
                        "Al Qusais Health Centre (MOH)",
                        "Dubai Diabetes Center",
                        "Medical Fitness Center",
                        "Airport Medical Centre"
                     },
                     "Select one of the Hospitals/Clinics from the list.");
            }
            else
            {
                string message = $"Thanks for using I Bot. Hope you have a great day!";
                await context.PostAsync(message);

                //var survey = context.MakeMessage();

                //var attachment = GetSurveyCard();
                //survey.Attachments.Add(attachment);

                //await context.PostAsync(survey);

                context.Done<string>("conversation ended.");
            }
        }
        public async Task DoctorClinics(IDialogContext context, IAwaitable<string> argument)
        {
            PromptDialog.Choice(context, ResumeAppointmentOptionsAsync,
                 new List<string>()
                 {
                        "Primary Health Appointment",
                        "Fitness appointment",
                        "General chekup appointment"
                 },
                 "Sorry to hear that,Please select the desired service.");
        }
        public async Task ResumeAppointmentOptionsAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var apptname = await argument;
            servicename = apptname;

            PromptDialog.Choice(context, ResumeTypeDoctorNamesOptionsAsync,
                     new List<string>()
                     {
                        "The All England Practice",
                        "Al Safa Primary Health Care Center",
                        "Al Mankhool Health Center",
                        "AL Barsha Health Centre",
                        "Nadd Al Hamar Health Center",
                        "Dubai Gynaecology & Fertility Centre",
                        "Zabeel Health Center",
                        "Al Barsha Clinic",
                        "Al Qusais Health Centre (MOH)",
                        "Dubai Diabetes Center",
                        "Medical Fitness Center",
                        "Airport Medical Centre"
                     },
                     "Select one of the Hospitals/Clinics from the list.");
        }
        public async Task ResumeTypeDoctorNamesOptionsAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var hospital = await argument;
            hospitalname = hospital;

            PromptDialog.Choice(context, ResumeTypeAppointmentOptionsAsync,
                  new List<string>()
                  {
                        "Dr Aikenhead",
                        "Dr Skinner",
                        "Dr Borer",
                        "Dr Les Plack"
                  },
                  "Here are few doctors available. Let us know which doctor you want to meet for treatment?");
        }
        public async Task ResumeTypeAppointmentOptionsAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var hospital = await argument;
            doctorname = hospital;

            PromptDialog.Choice(context, ResumeTypeAppointmentTimingsAsync,
                  new List<string>()
                  {
                         "30th Sep 2018",
                        "7th Oct 2018",
                        "15th Oct 2018",
                        "30th Oct 2018"

                  },
                  "Here are few dates available. Let us know which date you want to make an appointment?");
        }
        public async Task ResumeTypeAppointmentTimingsAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var apptDate = await argument;
            appointmentdate = apptDate;

            PromptDialog.Choice(context, ResumeTypeAppointmentDateOptionsAsync,
                  new List<string>()
                  {
                         "10 AM",
                        "12.30 PM",
                        "3 PM",
                        "5 PM"
                  },
                  "Here are few schedule timings available. Let us know which time you want to make an appointment?");
        }
        public async Task ResumeTypeAppointmentDateOptionsAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var selection = await argument;
            string apptTime = selection;
            if (appointmentdate == "30th Sep 2018")
            {
                await context.PostAsync($@"Thank you for your interest, your appointment has been logged. 
                                    {Environment.NewLine}Your appointment  summary:
                                    {Environment.NewLine}Name : {customerName},
                                    {Environment.NewLine}Hospital : {hospitalname},
                                    {Environment.NewLine}Doctor : {doctorname},
                                    {Environment.NewLine}Appointment Date: {appointmentdate},
                                    {Environment.NewLine}Appointment Time: {apptTime}");

                //MessageBox.Show("mail Send");

                //MailMessage msg = new MailMessage();
                //msg.To.Add(new MailAddress("venkata.kesanam@intertecsys.com", "SomeOne"));
                //msg.From = new MailAddress("threads.support@threadsme.com", "You");
                //msg.Subject = "This is a Test Mail-console";
                //msg.Body = "This is a test message using Exchange OnLine-From CRM";
                //msg.IsBodyHtml = true;



                PromptDialog.Confirm(
             context: context,
             resume: AnythingElseHandler,
             prompt: "Is there anything else that I could help?",
             retry: "Sorry, I don't understand that."
         );
            }
            else if (appointmentdate == "7th Oct 2018")
            {
                await context.PostAsync($@"Thank you for your interest, your appointment has been logged. 
                                  {Environment.NewLine}Your appointment  summary:
                                    {Environment.NewLine}Name : {customerName},
                                   {Environment.NewLine}Hospital : {hospitalname},
                                    {Environment.NewLine}Doctor : {doctorname},
                                        {Environment.NewLine}Appointment Date: {appointmentdate},
                                    {Environment.NewLine}Appointment Time: {apptTime}");

                PromptDialog.Confirm(
             context: context,
             resume: AnythingElseHandler,
             prompt: "Is there anything else that I could help?",
             retry: "Sorry, I don't understand that."
         );

            }
            else if (appointmentdate == "15th Oct 2018")
            {
                await context.PostAsync($@"Thank you for your interest, your appointment has been logged. 
                               {Environment.NewLine}Your appointment  summary:
                                    {Environment.NewLine}Appointment Ref No :DHA-2018-5858,
                                    {Environment.NewLine}Name : {customerName},
                                   {Environment.NewLine}Hospital : {hospitalname},
                                    {Environment.NewLine}Doctor : {doctorname},
                                    {Environment.NewLine}Appointment Date: {appointmentdate},
                                    {Environment.NewLine}Appointment Time: {apptTime}");

                PromptDialog.Confirm(
             context: context,
             resume: AnythingElseHandler,
             prompt: "Is there anything else that I could help?",
             retry: "Sorry, I don't understand that."
         );

            }
            else if (appointmentdate == "30th Oct 2018")
            {
                await context.PostAsync($@"Thank you for your interest, your appointment has been logged. 
                               {Environment.NewLine}Your appointment  summary:
                                    {Environment.NewLine}Name : {customerName},
                                  {Environment.NewLine}Hospital : {hospitalname},
                                    {Environment.NewLine}Doctor : {doctorname},
                                       {Environment.NewLine}Appointment Date: {appointmentdate},
                                    {Environment.NewLine}Appointment Time: {apptTime}");

                PromptDialog.Confirm(
             context: context,
             resume: AnythingElseHandler,
             prompt: "Is there anything else that I could help?",
             retry: "Sorry, I don't understand that."
         );
                //MailMessage mail = new MailMessage();
                //SmtpClient SmtpServer = new SmtpClient("outlook.office365.crm");

                //mail.From = new MailAddress("admin@customercrm.onmicrosoft.com");
                //mail.To.Add("venkata.kesanam@intertecsys.com");
                //mail.Subject = "Appointment Booking";
                //string body = "";
                //body = "<div align='left' style='width:110px; font:12px Arial, Helvetica, sans-serif'>";
                //body = body + "<div style='padding:10px'>";
                //body = body + "Dear <b> Sekhar </b>,<br /><br />";
                //body = body + "Thank you for your interest, your appointment has been logged." + "<br /><br />";
                //body = body + "Your appointment summary:<br /><br />";
                //body = body + "Name: " + customerName + "<br /><br />";
                //body = body + "Appointment Date: " + appointmentdate + "<br /><br />";
                //body = body + "Appointment Time: " + apptTime + "<br /><br />";
                //body = body + "<b> Thank You.</b><br /><br /><br />";
                //body = body + "Thanks & Regards,<br /><br />";
                //body = body + "Health 365 CRM Admin.<br /><br /></div>";


                //mail.Body = body;

                //SmtpServer.Port = 587;
                //SmtpServer.Credentials = new System.Net.NetworkCredential("admin@customercrm.onmicrosoft.com", "Welcome@123");
                //SmtpServer.EnableSsl = true;

                //SmtpServer.Send(mail);
            }

            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;

            //ClientCredentials credentials = new ClientCredentials();
            //credentials.UserName.UserName = "admin@healthchatbot.onmicrosoft.com";
            //credentials.UserName.Password = "Welcome@123";
            //Uri OrganizatinUri = new Uri("https://healthchatbot.api.crm4.dynamics.com/XRMServices/2011/Organization.svc");
            //Uri HomeRealUri = null;
            //using (OrganizationServiceProxy serviceProxy = new OrganizationServiceProxy(OrganizatinUri, HomeRealUri, credentials, null))
            //{
            //    IOrganizationService service = (IOrganizationService)serviceProxy;
            //    Microsoft.Xrm.Sdk.Entity Appointment = new Microsoft.Xrm.Sdk.Entity("appointment");
            //    Appointment["subject"] = "Appointment";
            //    Appointment["scheduledstart"] = Convert.ToDateTime("03/04/2018");
            //    // Appointment[""]
            //    Guid AppointmentId = service.Create(Appointment);
            //}

            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;

            //CrmServiceClient crmConn = new CrmServiceClient("admin@customercrm.onmicrosoft.com", CrmServiceClient.MakeSecureString("Welcome@123"), "EMEA", "orgbfdc0bcb", useUniqueInstance: false, useSsl: true, isOffice365: true);
            //IOrganizationService service = crmConn.OrganizationServiceProxy;

            //Microsoft.Xrm.Sdk.Entity Appointment = new Microsoft.Xrm.Sdk.Entity("appointment");
            //Appointment["subject"] = "Appointment";
            //Guid AppointmentId = service.Create(Appointment);

        }
        public async Task CustomerAppointmentTime(IDialogContext context, IAwaitable<string> result)
        {
            string response = await result;
            appointmentdate = response;

            PromptDialog.Text(
                context: context,
                resume: CustomerAppointmentName,
                prompt: "When you are planning to make an appointment time?",
                retry: "Sorry, I don't understand that."
                        );
        }
        public async Task CustomerAppointmentName(IDialogContext context, IAwaitable<string> result)
        {

            string response = await result;
            string apptTime = response;

            await context.PostAsync($@"Thank you for your interest, your appointment has been logged. 
                                   {Environment.NewLine}Your appointment  summary:
                                    {Environment.NewLine}Name : {customerName},
                                    {Environment.NewLine}Service : {servicename},
                                    {Environment.NewLine}Doctor : {doctorname},
                                    {Environment.NewLine}Hospital : {hospitalname},
                                       {Environment.NewLine}Appointment Date: {appointmentdate},
                                    {Environment.NewLine}Appointment Time: {apptTime}");

            PromptDialog.Confirm(
              context: context,
              resume: AnythingElseHandler,
              prompt: "Is there anything else that I could help?",
              retry: "Sorry, I don't understand that."
          );

        }

        public async Task DisplaySelectedCard(IDialogContext context, IAwaitable<string> result)
        {
            var selectedCard = await result;

            var message = context.MakeMessage();

            var attachment = GetSelectedCard(selectedCard);
            message.Attachments.Add(attachment);

            await context.PostAsync(message);

            PromptDialog.Confirm(
            context: context,
            resume: AnythingElseHandler,
            prompt: "Is there anything else that I could help?",
            retry: "Sorry, I don't understand that.");
            //context.Wait(MessageReceived);
        }
        private static Microsoft.Bot.Connector.Attachment GetSelectedCard(string selectedCard)
        {
            if (selectedCard.Contains("mankhool") || selectedCard.Contains("Mankhool") || selectedCard.Contains("al mankhool") || selectedCard.Contains("Al Mankhool"))
            {
                return GetMankhoolCard();
            }
            else if (selectedCard.Contains("barsha") || selectedCard.Contains("Barsha") || selectedCard.Contains("Al Barsha") || selectedCard.Contains("al barsha"))
            {
                return GetAlBarshaCard();
            }
            else if (selectedCard.Contains("Al Qusais") || selectedCard.Contains("qusais") || selectedCard.Contains("Qusais"))
            {
                return GetQusaisCard();
            }
            else if (selectedCard.Contains("Nad Al Hamar") || selectedCard.Contains("nad al hamar"))
            {
                return GetNadAlHamarCard();
            }
            else if (selectedCard.Contains("Al Safa") || selectedCard.Contains("al safa"))
            {
                return GetAlSafaCard();
            }
            else if (selectedCard.Contains("Al Badaa") || selectedCard.Contains("al badaa"))
            {
                return GetAlBadaaCard();
            }
            return GetDefaultCard();
        }
        private static Microsoft.Bot.Connector.Attachment GetAlBadaaCard()
        {
            var heroCard = new HeroCard
            {
                Title = "",
                Subtitle = "",
                Text = "",
                Images = new List<CardImage> { new CardImage("https://healthcrmchatbot20aug18.azurewebsites.net/AlBadaa.png") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Click to see the map", value: "https://www.google.ae/maps/place/Al+Badaa+Health+Centre/@25.2114413,55.2724694,17z/data=!4m8!1m2!2m1!1sAl+Badaa+Health+Center!3m4!1s0x3e5f42e8b1e728f1:0x9386335357fb7552!8m2!3d25.2120592!4d55.2722835") }
            };

            return heroCard.ToAttachment();
        }
        private static Microsoft.Bot.Connector.Attachment GetAlSafaCard()
        {
            var heroCard = new HeroCard
            {
                Title = "",
                Subtitle = "",
                Text = "",
                Images = new List<CardImage> { new CardImage("https://healthcrmchatbot20aug18.azurewebsites.net/AlSafa.png") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Click to see the map", value: "https://www.google.ae/maps/place/Al+Safa+Primary+Health+Care+Center/@25.1896,55.2362383,17z/data=!3m1!4b1!4m5!3m4!1s0x3e5f4208f522d93f:0x4c43896c0d306972!8m2!3d25.1896!4d55.238427") }
            };

            return heroCard.ToAttachment();
        }
        private static Microsoft.Bot.Connector.Attachment GetNadAlHamarCard()
        {
            var heroCard = new HeroCard
            {
                Title = "",
                Subtitle = "",
                Text = "",
                Images = new List<CardImage> { new CardImage("https://healthcrmchatbot20aug18.azurewebsites.net/NadAlHamar.png") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Click to see the map", value: "https://www.google.ae/maps/place/Nadd+Al+Hamar+Health+Center/@25.199364,55.3745396,16z/data=!4m8!1m2!2m1!1sNad+Al+Hamar+Clinic!3m4!1s0x3e5f6715c4716b27:0xbd16bd0f728ddd3a!8m2!3d25.199364!4d55.378917") }
            };

            return heroCard.ToAttachment();
        }
        private static Microsoft.Bot.Connector.Attachment GetMankhoolCard()
        {
            var heroCard = new HeroCard
            {
                Title = "",
                Subtitle = "",
                Text = "",
                Images = new List<CardImage> { new CardImage("https://healthcrmchatbot20aug18.azurewebsites.net/DMankhool.png") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Click to see the map", value: "https://www.google.ae/maps/dir/''/''/@25.2459475,55.2868377,14.83z/data=!4m8!4m7!1m0!1m5!1m1!1s0x3e5f4321e1e53069:0x218a7bfc58fbcf0e!2m2!1d55.2923198!2d25.2466252") }
            };

            return heroCard.ToAttachment();
        }
        private static Microsoft.Bot.Connector.Attachment GetAlBarshaCard()
        {
            var heroCard = new HeroCard
            {
                Title = "",
                Subtitle = "",
                Text = "",
                Images = new List<CardImage> { new CardImage("https://healthcrmchatbot20aug18.azurewebsites.net/DAlBarsha.png") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Click to see the map", value: "https://www.google.ae/maps/dir/''/AL+Barsha+Health+Centre+-+Al+Barsha+3+-+Dubai/@25.095679,55.1989079,13.7z/data=!4m8!4m7!1m0!1m5!1m1!1s0x3e5f6b8b20ab6a27:0xc41e11236896cc64!2m2!1d55.2024733!2d25.0975157") }
            };

            return heroCard.ToAttachment();
        }
        private static Microsoft.Bot.Connector.Attachment GetDefaultCard()
        {
            var heroCard = new HeroCard
            {
                Title = "",
                Subtitle = "",
                Text = "",
                Images = new List<CardImage> { new CardImage("https://healthcrmchatbot20aug18.azurewebsites.net/DDHAHeadOffice.png") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Click to see the map", value: "https://www.google.ae/maps/dir/''/DHA+HEAD+OFFICE+-+Dubai/@25.243802,55.3158075,13.42z/data=!4m8!4m7!1m0!1m5!1m1!1s0x3e5f5d2967d29a6d:0xcd99cea2aee34140!2m2!1d55.3196429!2d25.2468578") }
            };

            return heroCard.ToAttachment();
        }
        private static Microsoft.Bot.Connector.Attachment GetQusaisCard()
        {
            var heroCard = new HeroCard
            {
                Title = "",
                Subtitle = "",
                Text = "",
                Images = new List<CardImage> { new CardImage("https://healthcrmchatbot20aug18.azurewebsites.net/AlQusais.png") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Click to see the map", value: "https://www.google.ae/maps/dir/''/DHA+HEAD+OFFICE+-+Dubai/@25.243802,55.3158075,13.42z/data=!4m8!4m7!1m0!1m5!1m1!1s0x3e5f5d2967d29a6d:0xcd99cea2aee34140!2m2!1d55.3196429!2d25.2468578") }
            };

            return heroCard.ToAttachment();
        }

        [LuisIntent("LOCATION")]
        public async Task LOCATION(IDialogContext context, LuisResult result)
        {
            EntityRecommendation employeeName;

            string name = string.Empty;

            if (result.TryFindEntity("Location", out employeeName))
            {
                name = employeeName.Entity;

                var message = context.MakeMessage();

                var attachment = GetSelectedCard(name);
                message.Attachments.Add(attachment);

                await context.PostAsync(message);

                PromptDialog.Confirm(
                context: context,
                resume: AnythingElseHandler,
                prompt: "Is there anything else that I could help?",
                retry: "Sorry, I don't understand that.");
            }
            else
            {
                PromptDialog.Text(
                    context: context,
                    resume: DisplaySelectedCard,
                    prompt: "Please let me know your location preference?",
                    retry: "Sorry, I don't understand that."
                            );
            }
            //await context.PostAsync($"You have searched for {name}");
            //context.Wait(this.MessageReceived);


        }
        [LuisIntent("ENQUIRY")]
        public async Task ENQUIRY(IDialogContext context, LuisResult result)
        {
            PromptDialog.Choice(context, ResumeTypeOptionsAsync, new List<string>() { "Primary Healthcare Centers", "Occupational Health Screening", "Medical Fitness", "Health Regulation & Licensing" }, "Let us know what are you interested in?");
        }
        public async Task ResumeTypeOptionsAsync(IDialogContext context, IAwaitable<string> argument)
        {
            PromptDialog.Choice(context, ResumeTypeSubOptionsAsync, new List<string>() { "Appointments", "Search Hospitals", "Suggestions and Complaints", "Status" }, "Let us know what are you interested in?");
        }
        public async Task ResumeTypeSubOptionsAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var selection = await argument;
            if (selection == "Suggestions and Complaints")
            {
                //await CustomerComplaintHandler(context, null);
                PromptDialog.Text(
                 context: context,
                 resume: CustomerNameHandler,
                 prompt: "What is your complaint/suggestion?",
                 retry: "Sorry, I don't understand that the question."
             );
            }
            else if (selection == "Search Hospitals")
            {
                PromptDialog.Text(
                context: context,
                resume: DisplaySelectedCard,
                prompt: "Please let me know your location preference?",
                retry: "Sorry, I don't understand that."
                        );
            }
            else if (selection == "Appointments")
            {
                PromptDialog.Choice(context, ResumeAppointmentOptionsAsync,
                   new List<string>()
                   {
                        "Primary Health Appointment",
                        "Fitness appointment",
                        "General chekup appointment"
                   },
                   "Please select the desired service.");
            }
            else if (selection == "Status")
            {
                PromptDialog.Text(
           context: context,
           resume: CaseRefStatus,
           prompt: "Enter the Case Reference Number.",
           retry: "Didn't get that!");
            }
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