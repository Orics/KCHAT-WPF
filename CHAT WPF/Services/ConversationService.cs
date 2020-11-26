using CHAT_WPF.Models;
using Firebase.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CHAT_WPF.Services
{
    public class ConversationService : Service
    {
        public static Dictionary<string, ConversationModel> GetConversations()
        {
            if (!string.IsNullOrEmpty(UserID))
            {
                var conversations = Client.Get("Conversations").ResultAs<Dictionary<string, ConversationModel>>()
                                          .Where(c => c.Value.Members.Any(m => m.Value.ID == UserID))
                                          .ToDictionary(c => c.Key, c => c.Value);

                return conversations;
            }
            return null;
        }

        public static KeyValuePair<string, ConversationModel> GetConversationById(string ConversationID)
        {

            var conv = Client.Get("Conversations").ResultAs<Dictionary<string, ConversationModel>>()
                             .Where(c => c.Key == ConversationID).FirstOrDefault();

            return conv;
        }

        public static Dictionary<string, SummaryUserModel> GetUsersEnteringOfConversation(string ConversationID)
        {
            var users = Client.Get("Conversations/" + ConversationID + "/Members").ResultAs<Dictionary<string, SummaryUserModel>>()
                              .Where(m => m.Value.IsEntering == true)
                              .ToDictionary(c => c.Key, c => c.Value);
            return users;
        }

        public static bool ChangeTitleOfConversation(string ConversationID, string Title)
        {
            if (!string.IsNullOrEmpty(ConversationID))
            {
                var rsp =  Client.Set("Conversations/"+ ConversationID +"/Title", Title);
            }
            return true;
        }

        public static bool ChangeAvatarOfConversation(string ConversationID, string ImgPath)
        {
            var stream = File.Open("D:\\non-male.png", FileMode.Open);
            MemoryStream ms = new MemoryStream();
            stream.CopyTo(ms);
            var imgbase64 = Convert.ToBase64String(ms.GetBuffer());
            ConversationService.Client.Set("Conversations/" + ConversationID + "/Avatar/", imgbase64);
            MessageBox.Show("img uploaded");
            return true;
        }

        public static bool AddMemberToConversation(string ConversationID, string UserID)
        {
            if (!string.IsNullOrEmpty(ConversationID) && !string.IsNullOrEmpty(UserID))
            {
                var user = UserService.GetUserById(UserID);
                Client.Push(
                    path: "Conversations/" + ConversationID + "/Members",
                    data: user
                );
            }
            return true;
        }

        public static string SendMessageToConversation(string conversationID, MessageModel message)
        {
            var rsp = Client.Push("Conversations/" + conversationID + "/Messages", message);
            

            if (rsp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return rsp.Result.name;
            }
            else
            {
                return null;
            }
        }

    }
}