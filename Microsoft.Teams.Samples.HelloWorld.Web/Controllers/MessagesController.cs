using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Text;
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

using BotAuth.AADv2;
using BotAuth.Dialogs;
using BotAuth.Models;
using BotAuth;

namespace Microsoft.Teams.Samples.HelloWorld.Web.Controllers
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        public static TriviaApi triviaApi = null;

        

        [HttpPost]
        public async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {

            if (triviaApi == null)
                triviaApi = new IO.Swagger.Api.TriviaApi();

            bool addedBot = false;
            
            if (activity.Type == "conversationUpdate")
            {
               
                //check if bot is added
                for (int i = 0; i < activity.MembersAdded.Count; i++)
                {
                    if (activity.MembersAdded[i].Id == activity.Recipient.Id)
                    {
                        addedBot = true;
                        break;
                    }
                }               

            }

            using (var connector = new ConnectorClient(new Uri(activity.ServiceUrl)))
            {

                //register in TriviaApi
                if (addedBot)
                {
                    var members = await connector.Conversations.GetConversationMembersAsync(activity.Conversation.Id);
                    var teamid = activity.GetChannelData<TeamsChannelData>().Team.Id;
                    var memberList = new List<TeamRosterMemberModel>();

                    foreach (var member in members)
                    {
                        memberList.Add(new TeamRosterMemberModel(member.Id, member.Name));
                    }

                    TeamRosterModel teamRoster = new TeamRosterModel(teamid, memberList);

                    var response = await triviaApi.TriviaRegisterTeamAsync(teamRoster);

                    return new HttpResponseMessage(HttpStatusCode.Accepted);
                }



                if (activity.IsComposeExtensionQuery())
                {
                    var response = MessageExtension.HandleMessageExtensionQuery(connector, activity);
                    return response != null
                        ? Request.CreateResponse<ComposeExtensionResponse>(response)
                        : new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                if (activity.Type == ActivityTypes.Message)
                {
                       
                    await Conversation.SendAsync(activity, () => new QuestionDialog());

                    return new HttpResponseMessage(HttpStatusCode.Accepted);
                }

                return new HttpResponseMessage(HttpStatusCode.Accepted);
            }
        }


        
    }
}
