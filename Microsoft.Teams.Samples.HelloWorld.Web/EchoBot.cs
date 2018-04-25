using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;

using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams;
using Microsoft.Bot.Connector.Teams.Models;

using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Teams.Samples.HelloWorld.Web.Dialogs;

namespace Microsoft.Teams.Samples.HelloWorld.Web
{
    public class EchoBot
    {
        public static async Task EchoMessage(ConnectorClient connector, Activity activity)
        {

            var replyText = "Hi, please print 'trivia' to startt!";
          

            var action = activity.GetTextWithoutMentions();
            if (action.ToLower() == "trivia")
            {
                var triviaAPI = Microsoft.Teams.Samples.HelloWorld.Web.Controllers.MessagesController.triviaApi;

                if (triviaAPI != null)
                {
                    var userAAD = activity.From.Properties["aadObjectId"].ToString();

                    QuestionRequesterModel questionRequest = new QuestionRequesterModel(new System.Guid(userAAD));
                    
                    var question = await triviaAPI.TriviaGetQuestionAsync(questionRequest);

                    await Conversation.SendAsync(activity, () => new QuestionDialog());

                }
            }
            else
            { 
                var reply = activity.CreateReply(replyText);
                await connector.Conversations.ReplyToActivityWithRetriesAsync(reply);
            }
        }
    }
}
