using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Web;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using IO.Swagger.Model;

namespace Microsoft.Teams.Samples.HelloWorld.Web.Dialogs
{
    [Serializable]
    public class QuestionDialog : IDialog<object>
    {

        private const string ExplorerOption = "Musician Explorer";
        private const string SearchOption = "Musician Search";



        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            //Show options whatever users chat
            var userAAD = context.Activity.From.Properties["aadObjectId"].ToString();
            QuestionRequesterModel questionRequest = new QuestionRequesterModel(new System.Guid(userAAD));
            var question = await Microsoft.Teams.Samples.HelloWorld.Web.Controllers.MessagesController.triviaApi.TriviaGetQuestionAsync(questionRequest);


            PromptDialog.Choice(context, this.AfterMenuSelection, question.QuestionOptions, question.Text);
        }

        //After users select option, Bot call other dialogs
        private async Task AfterMenuSelection(IDialogContext context, IAwaitable<object> result)
        {
            var optionSelected = await result;

            context.Call(new QuestionDialog(), this.ResumeAfterOptionDialog);

        }

        //This function is called after each dialog process is done
        private async Task ResumeAfterOptionDialog(IDialogContext context, IAwaitable<object> result)
        {
            //This means  MessageRecievedAsync function of this dialog (PromptButtonsDialog) will receive users' messeges
            context.Wait(MessageReceivedAsync);
        }
    }
}