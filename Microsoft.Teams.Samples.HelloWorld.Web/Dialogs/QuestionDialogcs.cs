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