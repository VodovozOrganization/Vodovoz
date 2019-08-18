using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using Gtk;
using QS.DomainModel.UoW;
using QS.Project.Repositories;
using QS.Tdi.Gtk;
using QS.Utilities;
using QS.Utilities.Text;
using QSProjectsLib;
using Vodovoz.Domain.Chats;
using Vodovoz.Domain.Employees;
using Vodovoz.Repositories.HumanResources;
using Vodovoz.Repository.Chats;
using Chats;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class UnreadedMessagesWidget : Bin, IChatCallbackObserver
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

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

			PerformanceHelper.StartPointsGroup("Обработки старта виджета сообщений.");

			currentEmployee = EmployeeRepository.GetEmployeeForCurrentUser(uow);
			if(currentEmployee == null) {
				this.Sensitive = false;
				return;
			}
			accessToLogisticChat = UserPermissionRepository.CurrentUserPresetPermissions["logistican"];

			PerformanceHelper.AddTimePoint(logger, "Получили сотрудника.");

			if(!ChatCallbackObservable.IsInitiated)
				ChatCallbackObservable.CreateInstance(currentEmployee.Id);
			ChatCallbackObservable.GetInstance().AddObserver(this);

			PerformanceHelper.AddTimePoint(logger, "Создали сервис чата.");

			HandleChatUpdate();

			PerformanceHelper.AddTimePoint(logger, "Обработка чата.");

			MainClass.TrayIcon.PopupMenu += (o, args) => {
				if(menu == null || !menu.Children.Any())
					return;
				if(!menu.Visible)
					menu.Popup(null, null, null, 0, Global.CurrentEventTime);
				else
					menu.Cancel();
			};
			PerformanceHelper.EndPointsGroup();
		}

		public TdiNotebook MainTab {
			set => mainTab = value;
		}

		public override void Destroy()
		{
			ChatCallbackObservable.GetInstance().RemoveObserver(this);
			uow?.Dispose();
			base.Destroy();
		}

		void Position(Menu menu, out int x, out int y, out bool push_in)
		{
			GdkWindow.GetOrigin(out int gdkX, out int gdkY);

			x = this.Allocation.X + gdkX;
			y = this.Allocation.Bottom + gdkY;
			if(GdkWindow.Screen.Height < y + menu.Requisition.Height)
				y = this.Allocation.Top + gdkY - menu.Requisition.Height;
			push_in = true;

			if(Allocation.Width > menu.Requisition.Width)
				menu.WidthRequest = Allocation.Width;
		}

		protected void OnEventboxLabelButtonPressEvent(object o, ButtonPressEventArgs args)
		{
			if(args.Event.Button == 1) {
				if(menu == null)
					return;

				menu.Popup(null, null, Position, 0, Global.CurrentEventTime);
			}
		}

		#region IChatCallbackObserver implementation

		public void HandleChatUpdate()
		{
			var unreadedMessages = ChatMessageRepository.GetUnreadedChatMessages(uow, currentEmployee, accessToLogisticChat);
			unreadedMessagesCount = unreadedMessages.Sum(x => x.UnreadedMessagesTotal);
			var unreadedMessagesAuto = unreadedMessages.Sum(x => x.UnreadedMessagesAuto);
			var unreadedMessagesHuman = unreadedMessages.Sum(x => x.UnreadedMessages);
			if(unreadedMessagesCount > 0) {
				MainClass.TrayIcon.Blinking = true;
				MainClass.TrayIcon.Tooltip = string.Format(
					"У вас {0} {1}", unreadedMessagesCount,
					NumberToTextRus.Case(
						unreadedMessagesCount,
						"непрочитанное сообщение",
						"непрочитанных сообщения",
						"непрочитанных сообщений"
					)
				);
				var labelText = new List<string>();
				if(unreadedMessagesAuto > 0)
					labelText.Add(string.Format("<b><span size=\"large\" foreground=\"blue\">+{0}</span></b>", unreadedMessagesAuto));
				if(unreadedMessagesHuman > 0)
					labelText.Add(string.Format("<b><span size=\"large\" foreground=\"red\">+{0}</span></b>", unreadedMessagesHuman));
				labelUnreadedMessages.Markup = string.Join(" ", labelText);
				labelUnreadedMessages.TooltipMarkup = string.Format(
					"<b><span size=\"large\" foreground=\"red\">У вас {0} {1}.</span></b>",
					unreadedMessagesCount,
					NumberToTextRus.Case(
						unreadedMessagesCount,
						"непрочитанное сообщение",
						"непрочитанных сообщения",
						"непрочитанных сообщений"
					)
				);
			} else {
				MainClass.TrayIcon.Blinking = false;
				labelUnreadedMessages.Markup = string.Format("<b><span size=\"large\">0</span></b>");
				MainClass.TrayIcon.Tooltip = labelUnreadedMessages.TooltipMarkup = "У вас нет непрочитанных сообщений.";
			}

			menu = new Gtk.Menu();
			MenuItems = new Dictionary<MenuItem, int>();
			MenuItem item;

			foreach(var chat in unreadedMessages) {
				string chatName;
				string chatType = chat.ChatType.GetEnumTitle();
				switch(chat.ChatType) {
					case (ChatType.DriverAndLogists):
						chatName = PersonHelper.PersonNameWithInitials(chat.EmployeeLastName, chat.EmployeeName, chat.EmployeePatronymic);
						break;
					default:
						chatType = "Неизвестный тип чата";
						chatName = "";
						break;
				}
				item = new MenuItem(
					string.Format(
						"{0} {1}: +{2}{4} {3}",
						chatType,
						chatName,
						chat.UnreadedMessagesTotal,
						NumberToTextRus.Case(chat.UnreadedMessagesTotal, "сообщение", "сообщения", "сообщений"),
						chat.UnreadedMessages > 0 ? $"({chat.UnreadedMessages})" : string.Empty
					)
				);
				item.Activated += MenuItemActivated;

				MenuItems.Add(item, chat.ChatId);
				menu.Add(item);
			}

			menu.Add(new SeparatorMenuItem());
			item = new MenuItem("Прочитать все сообщения");
			item.Activated += ItemReadAllMessges_Activated;
			menu.Add(item);
			menu.ShowAll();
		}

		void ItemReadAllMessges_Activated(object sender, EventArgs e)
		{
			logger.Info("Обновляем последние обращения к чатам...");
			var localUow = UnitOfWorkFactory.CreateWithoutRoot();
			var chats = ChatRepository.GetCurrentUserChats(localUow, currentEmployee);
			foreach(var chat in chats) {
				chat.UpdateLastReadedTime(currentEmployee);
				localUow.Save(chat);
			}
			localUow.Commit();
			HandleChatUpdate();
			logger.Info("Все чаты прочитаны.");
		}

		void MenuItemActivated(object sender, EventArgs e)
		{
			var selectedChat = MenuItems[(MenuItem)sender];
			mainTab.OpenTab(
				ChatWidget.GenerateHashName(selectedChat),
				() => new ChatWidget(selectedChat)
			);
		}

		public int? ChatId => null;

		public uint? RequestedRefreshInterval => null;

		#endregion
	}
}

