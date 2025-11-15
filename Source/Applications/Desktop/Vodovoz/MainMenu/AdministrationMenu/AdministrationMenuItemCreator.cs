using System;
using Gtk;
using QS.Project.Domain;
using QS.Project.Services;
using QSOrmProject;
using QSProjectsLib;
using Vodovoz.Core.Domain.StoredResources;
using Vodovoz.Domain.Client;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.BaseParameters;
using Vodovoz.ViewModels.Journals.JournalViewModels.Sale;
using Vodovoz.ViewModels.Journals.JournalViewModels.Security;
using Vodovoz.ViewModels.Journals.JournalViewModels.Users;

namespace Vodovoz.MainMenu.AdministrationMenu
{
	/// <summary>
	/// Создатель меню Администрирование
	/// </summary>
	public class AdministrationMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;
		private readonly AdminServiceMenuItemCreator _adminServiceMenuItemCreator;
		private MenuItem _adminMenuItem;
		private MenuItem _usersMenuItem;
		private MenuItem _docTemplatesMenuItem;
		private MenuItem _registeredRmMenuItem;

		public AdministrationMenuItemCreator(
			ConcreteMenuItemCreator concreteMenuItemCreator,
			AdminServiceMenuItemCreator adminServiceMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
			_adminServiceMenuItemCreator = adminServiceMenuItemCreator ?? throw new ArgumentNullException(nameof(adminServiceMenuItemCreator));
		}

		/// <inheritdoc/>
		public override MenuItem Create()
		{
			_adminMenuItem = _concreteMenuItemCreator.CreateMenuItem("Администрирование");
			var adminMenu = new Menu();
			_adminMenuItem.Submenu = adminMenu;

			AddFirstSection(adminMenu);
			adminMenu.Add(CreateSeparatorMenuItem());
			AddSecondSection(adminMenu);

			Configure();
			
			return _adminMenuItem;
		}

		#region FirstSection

		private void AddFirstSection(Menu adminMenu)
		{
			adminMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Типы документов", OnTypesOfEntitiesPressed));

			_usersMenuItem = _concreteMenuItemCreator.CreateMenuItem("Пользователи", OnUsersPressed);
			adminMenu.Add(_usersMenuItem);
			adminMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Роли пользователей", OnUsersRolesPressed));

			_registeredRmMenuItem = _concreteMenuItemCreator.CreateMenuItem("Зарегистрированные RM", OnRegisteredRMPressed);
			adminMenu.Add(_registeredRmMenuItem);
			
			adminMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Параметры", OnParametersPressed));
			adminMenu.Add(_adminServiceMenuItemCreator.Create());
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

		#endregion
		
		#region SecondSection
        
        private void AddSecondSection(Menu adminMenu)
        {
        	_docTemplatesMenuItem = _concreteMenuItemCreator.CreateMenuItem("Шаблоны документов", OnDocTemplatesPressed);
        	adminMenu.Add(_docTemplatesMenuItem);
        	adminMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Географические группы", OnGeographicGroupsPressed));
        	adminMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Изображения", OnImagesPressed));
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

        #endregion

		private void Configure()
		{
			var admin = QSMain.User.Admin;
			var userCanManageRegisteredRMs =
				ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("user_can_manage_registered_rms");

			_registeredRmMenuItem.Visible = userCanManageRegisteredRMs;
			_adminMenuItem.Sensitive = admin;
			_usersMenuItem.Sensitive = admin;
			_docTemplatesMenuItem.Visible = admin;
		}
	}
}
