using System;
using System.Collections.Generic;
using System.Linq;
using Chat;
using Gtk;
using QSOrmProject;
using QSProjectsLib;
using Vodovoz.Domain.Employees;
using Vodovoz.Repository;
using Vodovoz.Repository.Chat;
using ChatClass = Vodovoz.Domain.Chat.Chat;
using Vodovoz.Domain.Chat;
using Gamma.Utilities;
using QSTDI;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class UnreadedMessagesWidget : Gtk.Bin, IChatCallbackObserver
	{
		private IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot();
		private Employee currentEmployee;
		private bool accessToLigisticChat;
		private int unreadedMessagesCount = 0;
		private Menu menu;
		private Dictionary<MenuItem, int> MenuItems;
		private TdiNotebook mainTab;

		public UnreadedMessagesWidget()
		{
			this.Build();
			currentEmployee = EmployeeRepository.GetEmployeeForCurrentUser(uow);
			if (currentEmployee == null)
			{
				this.Sensitive = false;
				return;
			}
			accessToLigisticChat = QSMain.User.Permissions["logistican"];

			if (!ChatCallbackObservable.IsInitiated)
				ChatCallbackObservable.CreateInstance(currentEmployee.Id);
			ChatCallbackObservable.GetInstance().AddObserver(this);
			HandleChatUpdate();
			MainClass.TrayIcon.PopupMenu += (o, args) => {
				if (menu == null || menu.Children.Count() == 0)
					return;
				if (!menu.Visible)
					menu.Popup(null, null, null, 0, Gtk.Global.CurrentEventTime);
				else {
					menu.Cancel();
				}
			};
		}

		public TdiNotebook MainTab { set { mainTab = value; } }


		public override void Destroy()
		{
			ChatCallbackObservable.GetInstance().RemoveObserver(this);
			base.Destroy();
		}

		void Position (Menu menu, out int x, out int y, out bool push_in)
		{
			int gdkX, gdkY;
			GdkWindow.GetOrigin (out gdkX, out gdkY);

			x = this.Allocation.X + gdkX;
			y = this.Allocation.Bottom + gdkY;
			if (GdkWindow.Screen.Height < y + menu.Requisition.Height)
				y = this.Allocation.Top + gdkY - menu.Requisition.Height;
			push_in = true;

			if (Allocation.Width > menu.Requisition.Width)
				menu.WidthRequest = Allocation.Width;
		}

		protected void OnEventboxLabelButtonPressEvent (object o, ButtonPressEventArgs args)
		{
			if (args.Event.Button == 1)
			{
				if (menu == null)
					return;

				menu.Popup(null, null, Position, 0, Gtk.Global.CurrentEventTime);
			}
		}

		#region IChatCallbackObserver implementation

		public void HandleChatUpdate()
		{
			var unreadedMessages = ChatMessageRepository.GetUnreadedChatMessages(uow, currentEmployee, accessToLigisticChat);
			unreadedMessagesCount = unreadedMessages.Sum(x => x.Value);
			if (unreadedMessagesCount > 0)
			{
				MainClass.TrayIcon.Blinking = true;
				MainClass.TrayIcon.Tooltip = String.Format("У вас {0} {1}", unreadedMessagesCount, RusNumber.Case(unreadedMessagesCount,
					"непрочитанное сообщение", 
					"непрочитанных сообщения", 
					"непрочитанных сообщений"));
				labelUnreadedMessages.Markup = String.Format("<b><span size=\"large\" foreground=\"red\">+ {0}</span></b>", unreadedMessagesCount);
				labelUnreadedMessages.TooltipMarkup = String.Format("<b><span size=\"large\" foreground=\"red\">У вас {0} {1}.</span></b>", 
					unreadedMessagesCount,
					RusNumber.Case(unreadedMessagesCount,
						"непрочитанное сообщение", 
						"непрочитанных сообщения", 
						"непрочитанных сообщений"));
			}
			else
			{
				MainClass.TrayIcon.Blinking = false;
				labelUnreadedMessages.Markup = String.Format("<b><span size=\"large\">0</span></b>");
				MainClass.TrayIcon.Tooltip = labelUnreadedMessages.TooltipMarkup = "У вас нет непрочитанных сообщений.";
			}

			menu = new Gtk.Menu ();
			MenuItems = new Dictionary<MenuItem, int> ();
			MenuItem item;

			foreach (var chat in unreadedMessages) {
				var chatUoW = UnitOfWorkFactory.CreateForRoot<ChatClass>(chat.Key);
				string chatType = chatUoW.Root.ChatType.GetEnumTitle();;
				string chatName;
				switch (chatUoW.Root.ChatType)
				{
					case (ChatType.DriverAndLogists):
						chatName = chatUoW.Root.Driver.ShortName;
						break;
					default:
						chatType = "Неизвестный тип чата";
						chatName = "";
						break;	
				}
				item = new MenuItem(String.Format("{0} {1}: + {2} {3}",
					chatType, chatName, chat.Value, 
					RusNumber.Case(chat.Value, "сообщение", "сообщения", "сообщений")));
				item.Activated += MenuItemActivated;;
				MenuItems.Add (item, chat.Key);
				menu.Add (item);
			}
			menu.ShowAll ();
		}

		void MenuItemActivated (object sender, EventArgs e)
		{
			var selectedChat = MenuItems[(MenuItem)sender];
			mainTab.OpenTab(ChatWidget.GenerateHashName(selectedChat),
				() => new ChatWidget(selectedChat)
			);
		}

		public int? ChatId { get { return null; } }

		public uint? RequestedRefreshInterval { get { return null; }	}

		#endregion
	}
}

