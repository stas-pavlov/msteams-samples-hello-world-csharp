using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Web;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using IO.Swagger.Model;

using BotAuth.AADv2;
using BotAuth.Dialogs;
using BotAuth.Models;
using BotAuth;

using System.Configuration;
using System.Threading;

namespace Microsoft.Teams.Samples.HelloWorld.Web.Dialogs
{
    [Serializable]
    public class QuestionDialog : IDialog<object>
    {
        private QuestionModel question = null;
      

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }

        public async Task QuastionDialog(IDialogContext context)
        {
            var userID = new System.Guid(context.Activity.From.Properties["aadObjectId"].ToString());
            QuestionRequesterModel questionRequest = new QuestionRequesterModel(userID);
            question = await Microsoft.Teams.Samples.HelloWorld.Web.Controllers.MessagesController.triviaApi.TriviaGetQuestionAsync(questionRequest);

            PromptDialog.Choice(context, this.AfterMenuSelection, question.QuestionOptions, question.Text);

        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;



            // Initialize AuthenticationOptions and forward to AuthDialog for token

            AuthenticationOptions options = new AuthenticationOptions()

            {

                Authority = ConfigurationManager.AppSettings["aad:Authority"],
                ClientId = ConfigurationManager.AppSettings["aad:ClientId"],
                ClientSecret = ConfigurationManager.AppSettings["aad:ClientSecret"],
                Scopes = new string[] { "User.ReadWrite" },
                RedirectUrl = ConfigurationManager.AppSettings["aad:Callback"],
                MagicNumberView = "/magic.html#{0}"
            };

            await context.Forward(new AuthDialog(new MSALAuthProvider(), options), async (IDialogContext authContext, IAwaitable<AuthResult> authResult) =>
            {
                var resultAuth = await authResult;

                //// Use token to call into service
                //var json = await new HttpClient().GetWithAuthAsync(result.AccessToken, "https://graph.microsoft.com/v1.0/me");
                //await authContext.PostAsync($"I'm a simple bot that doesn't do much, but I know your name is {json.Value<string>("displayName")} and your UPN is {json.Value<string>("userPrincipalName")}");
            }, message, CancellationToken.None);

            //Show options whatever users chat
            await QuastionDialog(context);
        }

        //After users select option, Bot call other dialogs
        private async Task AfterMenuSelection(IDialogContext context, IAwaitable<QuestionOptionModel> result)
        {
            var optionSelected = await result;

            var userID = new System.Guid(context.Activity.From.Properties["aadObjectId"].ToString());

            AnswerModel answer = new AnswerModel(userID, question.Id, optionSelected.Id);

            var result2 = await Microsoft.Teams.Samples.HelloWorld.Web.Controllers.MessagesController.triviaApi.TriviaSubmitAnswerAsync(answer);

            if (result2.Correct.Value)
                await context.SayAsync(context.Activity.From.Name + ", you are right!");
            else
                await context.SayAsync(context.Activity.From.Name +", you are wrong!");

            //context.Call(new QuestionDialog(), this.ResumeAfterOptionDialog);
            await QuastionDialog(context);

        }

        //This function is called after each dialog process is done
        private async Task ResumeAfterOptionDialog(IDialogContext context, IAwaitable<object> result)
        {
            //This means  MessageRecievedAsync function of this dialog (PromptButtonsDialog) will receive users' messeges
            context.Wait(MessageReceivedAsync);
        }
    }
}