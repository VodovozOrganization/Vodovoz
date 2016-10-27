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
using Vodovoz.Domain.Employees;
using QSProjectsLib;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ChatWidget : TdiTabBase, IChatCallbackObserver
	{
		private int showMessagePeriod = 2;
		private bool isActive;
		private TextTagTable textTags;
		private Employee currentEmployee;
		private IUnitOfWorkGeneric<ChatClass> chatUoW;

		static IChatService getChatService()
		{
			return new ChannelFactory<IChatService>(
				new BasicHttpBinding(), 
				"http://vod-srv.qsolution.ru:9000/ChatService").CreateChannel();
		}

		public ChatWidget(int chatId)
		{
			this.Build();
			textTags = buildTagTable();
			HandleSwitchIn = OnSwitchIn;
			HandleSwitchOut = OnSwitchOut;
			configure(chatId);
			GtkScrolledWindow1.SizeAllocated += (object o, SizeAllocatedArgs args) => {
				if (GtkScrolledWindow1.Vadjustment.Value == 0)
					scrollToEnd();
			};
			textViewChat.ModifyFont(Pango.FontDescription.FromString(".SF NS Text 14"));
		}

		private void updateLastReadedMessage () {
			var lastReaded = LastReadedRepository.GetLastReadedMessageForEmloyee(chatUoW, chatUoW.Root, currentEmployee);
			if (lastReaded == null)
			{
				lastReaded = new LastReadedMessage();
				lastReaded.Chat = chatUoW.Root;
				lastReaded.Employee = currentEmployee;
			}
			lastReaded.LastDateTime = DateTime.Now;
			chatUoW.Save(lastReaded);
			chatUoW.Commit();
		}

		private void configure(int chatId)
		{
			chatUoW = UnitOfWorkFactory.CreateForRoot<ChatClass>(chatId);
			currentEmployee = EmployeeRepository.GetEmployeeForCurrentUser(chatUoW);
			if (currentEmployee == null)
			{
				MessageDialogWorks.RunErrorDialog("Ваш пользователь не привязан к сотруднику. Невозможно открыть чат.");
				this.Destroy();
				return;
			}
			if (!ChatCallbackObservable.IsInitiated)
				ChatCallbackObservable.CreateInstance(currentEmployee.Id);
			ChatCallbackObservable.GetInstance().AddObserver(this);
			var lastReaded = LastReadedRepository.GetLastReadedMessageForEmloyee(chatUoW, chatUoW.Root, currentEmployee);
			if (lastReaded != null)
			{
				var readDays = (DateTime.Today - lastReaded.LastDateTime.Date).Days;
				if (showMessagePeriod < readDays)
					showMessagePeriod = readDays;
			} else {
				var readDays = (DateTime.Today - currentEmployee.DateOfCreate.Date).Days;
				if (showMessagePeriod < readDays)
					showMessagePeriod = readDays;
			}
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

		public override void Destroy()
		{			
			if (ChatCallbackObservable.IsInitiated)
				ChatCallbackObservable.GetInstance().RemoveObserver(this);
			base.Destroy();
		}

		protected void OnButtonSendClicked(object sender, EventArgs e)
		{
			if (String.IsNullOrWhiteSpace(textViewMessage.Buffer.Text))
				return;
			if (chatUoW.Root.ChatType == ChatType.DriverAndLogists)
			{
				if (currentEmployee != null)
					getChatService().SendMessageToDriver(
						currentEmployee.Id,
						chatUoW.Root.Driver.Id,
						textViewMessage.Buffer.Text
					);
				textViewMessage.Buffer.Text = String.Empty;
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
			updateTitle();
			ChatCallbackObservable.GetInstance().NotifyChatUpdate(chatUoW.Root.Id, this);
			scrollToEnd();
		}

		private void scrollToEnd() {
			TextIter ti = textViewChat.Buffer.GetIterAtLine(textViewChat.Buffer.LineCount-1);
			TextMark tm = textViewChat.Buffer.CreateMark("eot", ti,false);
			textViewChat.ScrollToMark(tm, 0, false, 0, 0);
		}

		private void updateTitle() {
			if (!isActive)
			{
				var newMessagesCount = LastReadedRepository.GetLastReadedMessagesCountForEmployee(chatUoW, chatUoW.Root, currentEmployee);
				if (newMessagesCount > 0)
				{
					if (chatUoW.Root.ChatType == ChatType.DriverAndLogists)
						this.TabName = String.Format("Чат ({0}) + {1}", chatUoW.Root.Driver.ShortName, newMessagesCount);
				}
				else
				{
					if (chatUoW.Root.ChatType == ChatType.DriverAndLogists)
						this.TabName = String.Format("Чат ({0})", chatUoW.Root.Driver.ShortName);
				}
			}
			else
			{
				if (chatUoW.Root.ChatType == ChatType.DriverAndLogists)
					this.TabName = String.Format("Чат ({0})", chatUoW.Root.Driver.ShortName);
				updateLastReadedMessage();
			}
		}

		private void OnSwitchIn(ITdiTab tabFrom) {
			updateLastReadedMessage();
			ChatCallbackObservable.GetInstance().NotifyChatUpdate(chatUoW.Root.Id, this);
			isActive = true;
			updateTitle();
		}

		private void OnSwitchOut(ITdiTab tabTo) {
			isActive = false;
		}

		private Dictionary<string, string> usersColors = new Dictionary<string, string>();

		private TextTagTable buildTagTable()
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

		private string getUserTag(string userName)
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

		#region IChatCallbackObserver implementation

		public void HandleChatUpdate()
		{
			updateChat();
		}

		public int? ChatId
		{
			get
			{
				if (chatUoW != null && chatUoW.Root != null)
					return chatUoW.Root.Id;
				return -1;
			}
		}

		public uint? RequestedRefreshInterval { get { return 5000; } }
		#endregion
	}
}

