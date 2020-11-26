using CHAT_WPF.Models;
using CHAT_WPF.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CHAT_WPF
{
    /// <summary>
    /// Interaction logic for MessageContainer.xaml
    /// </summary>
    public partial class MessageContainer : UserControl
    {
        public KeyValuePair<string, ConversationModel> Model { get; set; }

        public MessageContainer()
        {
            InitializeComponent();

            this.ConversationScrollContainer.ScrollToEnd();
        }

        public void Load(KeyValuePair<string, ConversationModel> Model)
        {
            this.Model = Model;
            Load();
            InitializaEventsOnAsyns();
        }

        public void Load()
        {
            this.MessageContainer1.Children.Clear();
            foreach (var message in Model.Value.Messages.OrderBy(m => m.Value.SendTime))
            {
                if (message.Value.UserID == UserService.UserID)
                {
                    this.MessageContainer1.Children.Add(
                       new UserControlMessageSent()
                       {
                           Model = message
                       }
                   );
                }
                else
                {
                    this.MessageContainer1.Children.Add(
                        new UserControlMessageReceived()
                        {
                            Model = message
                        }
                    );
                   
                }
            }
        }

        public void InitializaEventsOnAsyns()
        {
            Service.Client.OnAsync(
            path: "Conversations/" + Model.Key + "/Messages",
            added: (s, args, context) =>
            {
                Model = ConversationService.GetConversationById(Model.Key);
                this.Dispatcher.Invoke(() =>
                {
                    Load();
                });

            },
            removed: (s, args, context) =>
            {
                Model = ConversationService.GetConversationById(Model.Key);
                this.Dispatcher.Invoke(() =>
                {
                    Load();
                });
            }
            );
        }

        private void SendMessage()
        {
            List<MessageFileModel> Files = new List<MessageFileModel>();
            foreach (Upload_file_usercontrol file in MessageFilesContainer.Children)
            {
                Files.Add(new MessageFileModel()
                {
                    ConversationID = file.Model.ConversationID,
                    FileName = file.Model.FileName,
                    DowloadUrl = file.Model.DowloadUrl
                });
            }

            List<MessageFileModel> Images = new List<MessageFileModel>();
            foreach (Upload_image_usercontrol file in MessageImagesContainer.Children)
            {
                Images.Add(new MessageFileModel()
                {
                    ConversationID = file.Model.ConversationID,
                    FileName = file.Model.FileName,
                    DowloadUrl = file.Model.DowloadUrl
                });
            }


            MessageModel message = new MessageModel()
            {
                UserID = Service.UserID,
                SendTime = DateTime.Now,
                Text = new TextRange(ConversationInput.Document.ContentStart, ConversationInput.Document.ContentEnd).Text,
                Files = Files,
                Images = Images,
            };

            ConversationService.SendMessageToConversation(Model.Key, message);
        }



        private void _EventClickSendMessage(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void _KeyDown(object sender, KeyEventArgs e)
        {
            
            
        }

        private void _Event_UploadFileButton_Clicked(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "All Files (*.*)|*.*";
            dialog.FilterIndex = 1;

            if (dialog.ShowDialog() == true)
            {
                var control = new Upload_file_usercontrol(new MessageUploadFileModel()
                {   
                    ConversationID = Model.Key,
                    FileName = System.IO.Path.GetFileName(dialog.FileName),
                    FilePath = dialog.FileName,
                    Source = dialog.OpenFile(),
                });

                MessageFilesContainer.Children.Add(control);
            }
        }

        private void _Event_UploadImageButton_Clicked(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Files | *.jpg; *.jpeg; *.png";
            dialog.FilterIndex = 1;

            if (dialog.ShowDialog() == true)
            {
                var control = new Upload_image_usercontrol(new MessageUploadFileModel()
                {
                    ConversationID = Model.Key,
                    FileName = System.IO.Path.GetFileName(dialog.FileName),
                    FilePath = dialog.FileName,
                    Source = dialog.OpenFile(),
                });

                MessageImagesContainer.Children.Add(control);
            }
        }
    }
}