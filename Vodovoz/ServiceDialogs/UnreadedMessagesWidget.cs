using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using Gtk;
using QSOrmProject;
using QSProjectsLib;
using QSTDI;
using Vodovoz.Domain.Chats;
using Vodovoz.Domain.Employees;
using Vodovoz.Repository;
using Vodovoz.Repository.Chat;
using VodovozService.Chats;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class UnreadedMessagesWidget : Gtk.Bin, IChatCallbackObserver
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		private IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot();
		private Employee currentEmployee;
		private bool accessToLogisticChat;
		private int unreadedMessagesCount = 0;
		private Menu menu;
		private Dictionary<MenuItem, int> MenuItems;
		private TdiNotebook mainTab;

		public UnreadedMessagesWidget()
		{
			this.Build();

			QSProjectsLib.PerformanceHelper.StartPointsGroup ("Обработки старта виджета сообщений.");

			currentEmployee = EmployeeRepository.GetEmployeeForCurrentUser(uow);
			if (currentEmployee == null)
			{
				this.Sensitive = false;
				return;
			}
			accessToLogisticChat = QSMain.User.Permissions["logistican"];

			QSProjectsLib.PerformanceHelper.AddTimePoint (logger, "Получили сотрудника.");

			if (!ChatCallbackObservable.IsInitiated)
				ChatCallbackObservable.CreateInstance(currentEmployee.Id);
			ChatCallbackObservable.GetInstance().AddObserver(this);

			QSProjectsLib.PerformanceHelper.AddTimePoint (logger, "Создали сервис чата.");

			HandleChatUpdate();

			QSProjectsLib.PerformanceHelper.AddTimePoint (logger, "Обработка чата.");

			MainClass.TrayIcon.PopupMenu += (o, args) => {
				if (menu == null || menu.Children.Count() == 0)
					return;
				if (!menu.Visible)
					menu.Popup(null, null, null, 0, Gtk.Global.CurrentEventTime);
				else {
					menu.Cancel();
				}
			};
			QSProjectsLib.PerformanceHelper.EndPointsGroup ();
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
			var unreadedMessages = ChatMessageRepository.GetUnreadedChatMessages(uow, currentEmployee, accessToLogisticChat);
			unreadedMessagesCount = unreadedMessages.Sum(x => x.UnreadedMessagesTotal);
			var unreadedMessagesAuto = unreadedMessages.Sum (x => x.UnreadedMessagesAuto);
			var unreadedMessagesHuman = unreadedMessages.Sum (x => x.UnreadedMessages);
			if (unreadedMessagesCount > 0)
			{
				MainClass.TrayIcon.Blinking = true;
				MainClass.TrayIcon.Tooltip = String.Format("У вас {0} {1}", unreadedMessagesCount, RusNumber.Case(unreadedMessagesCount,
					"непрочитанное сообщение", 
					"непрочитанных сообщения", 
					"непрочитанных сообщений"));
				var labelText = new List<string> ();
				if(unreadedMessagesAuto > 0)
					labelText.Add(String.Format ("<b><span size=\"large\" foreground=\"blue\">+{0}</span></b>", unreadedMessagesAuto));
				if (unreadedMessagesHuman > 0)
					labelText.Add (String.Format ("<b><span size=\"large\" foreground=\"red\">+{0}</span></b>", unreadedMessagesHuman));
				labelUnreadedMessages.Markup = String.Join(" ", labelText);
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
				string chatName;
				string chatType = chat.ChatType.GetEnumTitle ();
				switch (chat.ChatType)
				{
					case (ChatType.DriverAndLogists):
					chatName = StringWorks.PersonNameWithInitials (chat.EmployeeLastName, chat.EmployeeName, chat.EmployeePatronymic);
						break;
					default:
						chatType = "Неизвестный тип чата";
						chatName = "";
						break;	
				}
				item = new MenuItem(String.Format("{0} {1}: +{2}{4} {3}",
				                                  chatType, chatName, chat.UnreadedMessagesTotal, 
				                                  RusNumber.Case(chat.UnreadedMessagesTotal, "сообщение", "сообщения", "сообщений"),
				                                  chat.UnreadedMessages > 0 ? $"({chat.UnreadedMessages})" : String.Empty
				                                 ));
				item.Activated += MenuItemActivated;

				MenuItems.Add (item, chat.ChatId);
				menu.Add (item);
			}

			menu.Add (new SeparatorMenuItem ());
			item = new MenuItem ("Прочитать все сообщения");
			item.Activated += ItemReadAllMessges_Activated;
			menu.Add (item);
			menu.ShowAll ();
		}

		void ItemReadAllMessges_Activated (object sender, EventArgs e)
		{
			logger.Info ("Обновляем последние обращения к чатам...");
			var localUow = UnitOfWorkFactory.CreateWithoutRoot ();
			var chats = ChatRepository.GetCurrentUserChats (localUow, currentEmployee);
			foreach(var chat in chats)
			{
				chat.UpdateLastReadedTime (currentEmployee);
				localUow.Save (chat);
			}
			localUow.Commit ();
			HandleChatUpdate ();
			logger.Info ("Все чаты прочитаны.");
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

