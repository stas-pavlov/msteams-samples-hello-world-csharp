using System.Threading.Tasks;

using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams;

using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;

namespace Microsoft.Teams.Samples.HelloWorld.Web
{
    public class EchoBot
    {
        public static async Task EchoMessage(ConnectorClient connector, Activity activity)
        {

            var action = activity.GetTextWithoutMentions();
            if (action.ToLower() == "trivia")
            {
                var triviaAPI = Microsoft.Teams.Samples.HelloWorld.Web.Controllers.MessagesController.triviaApi;

                if (triviaAPI != null)
                {
                    

                    //var question = triviaAPI.TriviaGetQuestionAsync();
                }
            }

                var reply = activity.CreateReply("You said: " + activity.GetTextWithoutMentions());
                await connector.Conversations.ReplyToActivityWithRetriesAsync(reply);
        }
    }
}
