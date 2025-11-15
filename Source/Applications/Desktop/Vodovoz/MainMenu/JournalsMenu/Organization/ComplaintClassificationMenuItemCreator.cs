using System;
using Gtk;
using QS.Navigation;
using Vodovoz.ViewModels.Journals.FilterViewModels.Complaints;
using Vodovoz.ViewModels.Journals.JournalViewModels.Complaints;

namespace Vodovoz.MainMenu.JournalsMenu.Organization
{
	/// <summary>
	/// Создатель меню Справочники - Наша организация - Классификация рекламаций
	/// </summary>
	public class ComplaintClassificationMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;

		public ComplaintClassificationMenuItemCreator(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}

		/// <inheritdoc/>
		public override MenuItem Create()
		{
			var complaintClassificationMenuItem = _concreteMenuItemCreator.CreateMenuItem("Классификация рекламаций");
			var complaintClassificationMenu = new Menu();
			complaintClassificationMenuItem.Submenu = complaintClassificationMenu;

			complaintClassificationMenu.Add(_concreteMenuItemCreator.CreateMenuItem(
				"Объекты рекламаций", OnActionComplaintObjectActivated));
			complaintClassificationMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Виды рекламаций", OnActionComplaintKindActivated));
			complaintClassificationMenu.Add(
				_concreteMenuItemCreator.CreateMenuItem("Детализация видов рекламаций", OnActionComplaintDetalizationJournalActivated));

			return complaintClassificationMenuItem;
		}
		
		/// <summary>
		/// Объекты рекламаций
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnActionComplaintObjectActivated(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager
				.OpenViewModel<ComplaintObjectJournalViewModel, Action<ComplaintObjectJournalFilterViewModel>>(
					null,
					filter => filter.HidenByDefault = true);
		}

		/// <summary>
		/// Виды рекламаций
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnActionComplaintKindActivated(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<ComplaintKindJournalViewModel, Action<ComplaintKindJournalFilterViewModel>>(
				null,
				filter =>
				{
					filter.HidenByDefault = true;
				});
		}

		/// <summary>
		/// Детализация видов рекламаций
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnActionComplaintDetalizationJournalActivated(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<ComplaintDetalizationJournalViewModel>(null, OpenPageOptions.IgnoreHash);
		}
	}
}
