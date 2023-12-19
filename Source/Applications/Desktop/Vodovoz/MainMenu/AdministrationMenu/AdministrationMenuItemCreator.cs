using System;
using Gtk;
using QS.Project.Domain;
using QSOrmProject;
using QSProjectsLib;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.StoredResources;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.BaseParameters;
using Vodovoz.ViewModels.Journals.JournalViewModels.Sale;
using Vodovoz.ViewModels.Journals.JournalViewModels.Security;
using Vodovoz.ViewModels.Journals.JournalViewModels.Users;

namespace Vodovoz.MainMenu.AdministrationMenu
{
	public class AdministrationMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;
		private readonly AdminServiceMenuItemCreator _adminServiceMenuItemCreator;

		public AdministrationMenuItemCreator(
			ConcreteMenuItemCreator concreteMenuItemCreator,
			AdminServiceMenuItemCreator adminServiceMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
			_adminServiceMenuItemCreator = adminServiceMenuItemCreator ?? throw new ArgumentNullException(nameof(adminServiceMenuItemCreator));
		}

		public MenuItem Create()
		{
			var adminMenuItem = _concreteMenuItemCreator.CreateMenuItem("Администрирование");
			var adminMenu = new Menu();
			adminMenuItem.Submenu = adminMenu;

			adminMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Типы документов", OnTypesOfEntitiesPressed));
			adminMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Пользователи", OnUsersPressed));
			adminMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Роли пользователей", OnUsersRolesPressed));
			adminMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Зарегистрированные RM", OnRegisteredRMPressed));
			adminMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Параметры", OnParametersPressed));
			adminMenu.Add(_adminServiceMenuItemCreator.Create());
			adminMenu.Add(CreateSeparatorMenuItem());
			adminMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Шаблоны документов", OnDocTemplatesPressed));
			adminMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Географические группы", OnGeographicGroupsPressed));
			adminMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Изображения", OnImagesPressed));
			
			return adminMenuItem;
		}
		
		/// <summary>
		/// Типы документов
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[Obsolete("Старый диалог, заменить")]
		private void OnTypesOfEntitiesPressed(object sender, ButtonPressEventArgs e)
		{
			if(QSMain.User.Admin)
			{
				Startup.MainWin.TdiMain.OpenTab(
					OrmReference.GenerateHashName<TypeOfEntity>(),
					() => new OrmReference(typeof(TypeOfEntity)));
			}
		}

		/// <summary>
		/// Пользователи
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnUsersPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<UsersJournalViewModel>(null);
		}

		/// <summary>
		/// Роли пользователей
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnUsersRolesPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<UserRolesJournalViewModel>(null);
		}

		/// <summary>
		/// Зарегистрированные RM
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnRegisteredRMPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<RegisteredRMJournalViewModel>(null, QS.Navigation.OpenPageOptions.IgnoreHash);
		}

		/// <summary>
		/// Параметры
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnParametersPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<BaseParametersViewModel>(null);
		}
		
		/// <summary>
		/// Шаблоны документов
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[Obsolete("Старый диалог, заменить")]
		private void OnDocTemplatesPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				OrmReference.GenerateHashName<DocTemplate>(),
				() => new OrmReference(typeof(DocTemplate)));
		}

		/// <summary>
		/// Географические группы
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnGeographicGroupsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<GeoGroupJournalViewModel>(null);
		}

		/// <summary>
		/// Изображения
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[Obsolete("Старый диалог, заменить")]
		private void OnImagesPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				OrmReference.GenerateHashName<StoredResource>(),
				() => new OrmReference(typeof(StoredResource))
			);
		}
	}
}
