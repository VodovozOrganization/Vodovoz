using System;
using Chat;
using ChatClass = Vodovoz.Domain.Chat.Chat;
using QSTDI;
using Vodovoz.Domain.Chat;
using System.ServiceModel;
using Gtk;
using System.Collections.Generic;
using Vodovoz.Repository;
using QSOrmProject;
using Vodovoz.Repository.Chat;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ChatWidget : TdiTabBase
	{
		TextTagTable textTags;
		int showMessagePeriod = 2;
		IUnitOfWorkGeneric<ChatClass> chatUoW;

		static IChatService getChatService()
		{
			return new ChannelFactory<IChatService>(
				new BasicHttpBinding(), 
				"http://vod-srv.qsolution.ru:9000/ChatService").CreateChannel();
		}

		public ChatWidget()
		{
			this.Build();
			this.TabName = "Чат";
			textTags = buildTagTable();
		}

		public ChatWidget(int chatId) : this()
		{
			Configure(chatId);
		}

		public void Configure(int chatId)
		{
			chatUoW = UnitOfWorkFactory.CreateForRoot<ChatClass>(chatId);
			if (chatUoW.Root.ChatType == ChatType.DriverAndLogists)
				this.TabName = String.Format("Чат ({0})", chatUoW.Root.Driver.ShortName);
			updateChat();
		}

		public static string GenerateHashName(int chatId)
		{
			return String.Format("Dlg_chat_{0}", chatId);
		}

		public override bool CompareHashName(string hashName)
		{
			return GenerateHashName(chatUoW.Root.Id) == hashName;
		}

		protected void OnButtonSendClicked(object sender, EventArgs e)
		{
			if (textViewMessage.Buffer.Text == "")
				return;
			if (chatUoW.Root.ChatType == ChatType.DriverAndLogists)
			{
				var currentUser = EmployeeRepository.GetEmployeeForCurrentUser(chatUoW);
				if (currentUser != null)
					getChatService().SendMessageToDriver(
						currentUser.Id,
						chatUoW.Root.Driver.Id,
						textViewMessage.Buffer.Text
					);
				updateChat();
			}
		}

		protected void OnButtonHistoryClicked(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}

		[GLib.ConnectBefore]
		protected void OnTextViewMessageKeyPressEvent(object o, Gtk.KeyPressEventArgs args)
		{
			if (args.Event.Key == Gdk.Key.Return && !args.Event.State.HasFlag(Gdk.ModifierType.ControlMask))
			{
				buttonSend.Click();
				args.RetVal = true;
			}
		}

		private void updateChat()
		{
			IList<ChatMessage> messages = new List<ChatMessage>();
			messages = ChatMessageRepository.GetChatMessagesForPeriod(chatUoW, chatUoW.Root, showMessagePeriod);
			
			TextBuffer tempBuffer = new TextBuffer(textTags);
			TextIter iter = tempBuffer.EndIter;
			DateTime maxDate = default(DateTime);

			foreach (var message in messages)
			{
				if (message.DateTime.Date != maxDate.Date)
				{ 
					tempBuffer.InsertWithTagsByName(
						ref iter, 
						String.Format("\n{0:D}", message.DateTime.Date), 
						"date");
				}
				tempBuffer.InsertWithTagsByName(
					ref iter, 
					string.Format("\n({0:t}) {1}: ", message.DateTime, message.Sender.ShortName), 
					getUserTag(message.Sender.ShortName));
				tempBuffer.Insert(ref iter, message.Message);
				if (message.DateTime > maxDate)
					maxDate = message.DateTime;
			}
			textViewChat.Buffer = tempBuffer;
		}

		private static Dictionary<string, string> usersColors = new Dictionary<string, string>();

		static TextTagTable buildTagTable()
		{
			TextTagTable textTags = new TextTagTable();
			var tag = new TextTag("date");
			tag.Justification = Justification.Center;
			tag.Weight = Pango.Weight.Bold;
			textTags.Add(tag);
			tag = new TextTag("user1");
			tag.Foreground = "#FF00FF";
			textTags.Add(tag);
			tag = new TextTag("user2");
			tag.Foreground = "#9400D3";
			textTags.Add(tag);
			tag = new TextTag("user3");
			tag.Foreground = "#191970";
			textTags.Add(tag);
			tag = new TextTag("user4");
			tag.Foreground = "#7F0000";
			textTags.Add(tag);
			tag = new TextTag("user5");
			tag.Foreground = "#FF8C00";
			textTags.Add(tag);
			tag = new TextTag("user6");
			tag.Foreground = "#FFA500";
			textTags.Add(tag);
			tag = new TextTag("user7");
			tag.Foreground = "#32CD32";
			textTags.Add(tag);
			tag = new TextTag("user8");
			tag.Foreground = "#3CB371";
			textTags.Add(tag);
			tag = new TextTag("user9");
			tag.Foreground = "#007F00";
			textTags.Add(tag);
			tag = new TextTag("user10");
			tag.Foreground = "#FFFF00";
			textTags.Add(tag);
			return textTags;
		}

		static string getUserTag(string userName)
		{
			if (usersColors.ContainsKey(userName))
				return usersColors[userName];
			else
			{
				string tagName = String.Format("user{0}", usersColors.Count % 10 + 1);
				usersColors.Add(userName, tagName);
				return tagName;
			}
		}
	}
}

