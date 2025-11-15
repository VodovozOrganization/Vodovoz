using System;
using Gtk;
using QS.Navigation;
using QSOrmProject;
using Vodovoz.Domain;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;

namespace Vodovoz.MainMenu.JournalsMenu.Helpers
{
	/// <summary>
	/// Создатель меню Справочники - Помощники
	/// </summary>
	public class HelpersMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;

		public HelpersMenuItemCreator(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}

		/// <inheritdoc/>
		public override MenuItem Create()
		{
			var helpersMenuItem = _concreteMenuItemCreator.CreateMenuItem("Помощники");
			var helpersMenu = new Menu();
			helpersMenuItem.Submenu = helpersMenu;
		
			helpersMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Шаблоны комментариев", OnCommentTemplatesPressed));
			helpersMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Шаблоны комментариев для штрафов", OnFineCommentTemplatesPressed));
			helpersMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Шаблоны комментариев для премий", OnPremiumCommentTemplatesPressed));
			helpersMenu.Add(
				_concreteMenuItemCreator.CreateMenuItem("Настройка текстов пуш-уведомлений", OnPushNotificationTextSettingsPressed));

			return helpersMenuItem;
		}
		
		/// <summary>
		/// Шаблоны комментариев
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[Obsolete("Старый диалог, заменить")]
		private void OnCommentTemplatesPressed(object sender, ButtonPressEventArgs e)
		{
			OrmReference refWin = new OrmReference(typeof(CommentTemplate));
			Startup.MainWin.TdiMain.AddTab(refWin);
		}

		/// <summary>
		/// Шаблоны комментариев для штрафов
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[Obsolete("Старый диалог, заменить")]
		private void OnFineCommentTemplatesPressed(object sender, ButtonPressEventArgs e)
		{
			OrmReference refWin = new OrmReference(typeof(FineTemplate));
			Startup.MainWin.TdiMain.AddTab(refWin);
		}

		/// <summary>
		/// Шаблоны комментариев для премий
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnPremiumCommentTemplatesPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<PremiumTemplateJournalViewModel>(null, OpenPageOptions.IgnoreHash);
		}
		
		/// <summary>
		/// Настройка текстов пуш-уведомлений
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnPushNotificationTextSettingsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<OnlineOrderNotificationSettingJournalViewModel>(null, OpenPageOptions.IgnoreHash);
		}
	}
}
