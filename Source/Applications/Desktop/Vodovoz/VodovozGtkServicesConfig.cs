using System;
using Vodovoz.Infrastructure.Services;
using QS.Permissions;
using Vodovoz.Infrastructure.Journal;
using QS.Services;
using QS.Project.Services.GtkUI;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Permissions;
using QS.Project.Services;
using QS.Validation;
using Vodovoz.Services;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories;
using QS.DomainModel.UoW;
using QSProjectsLib;

namespace Vodovoz
{
	public static class VodovozGtkServicesConfig
	{
		public static IEmployeeService EmployeeService { get; set; }
		public static IRepresentationEntityPicker RepresentationEntityPicker { get; set; }

		public static void CreateVodovozDefaultServices()
		{
			ServicesConfig.InteractiveService = new GtkInteractiveService();
			ServicesConfig.ValidationService = ServicesConfig.ValidationService;
			ServicesConfig.UserService = new UserService(LoadCurrentUser());
			EmployeeService = new EmployeeService(ServicesConfig.UnitOfWorkFactory, ServicesConfig.UserService);

			IRepresentationJournalFactory journalFactory = new PermissionControlledRepresentationJournalFactory();
			RepresentationEntityPicker = new RepresentationEntityPickerGtk(journalFactory);

			PermissionsSettings.ConfigureEntityPermissionFinder(new Vodovoz.Domain.Permissions.EntitiesWithPermissionFinder());

			//пространство имен специально прописано чтобы при изменениях не было случайного совпадения с валидатором из QS
			var entityPermissionValidator =
				new Vodovoz.Domain.Permissions.EntityPermissionValidator(ServicesConfig.UnitOfWorkFactory, new EmployeeRepository(), new PermissionRepository(), ServicesConfig.UnitOfWorkFactory);
			var presetPermissionValidator =
				new Vodovoz.Domain.Permissions.HierarchicalPresetPermissionValidator(ServicesConfig.UnitOfWorkFactory, new EmployeeRepository(), new PermissionRepository());
			var permissionService = new PermissionService(entityPermissionValidator, presetPermissionValidator);
			PermissionsSettings.PermissionService = permissionService;
			PermissionsSettings.CurrentPermissionService = new CurrentPermissionServiceAdapter(permissionService, ServicesConfig.UserService);
		}

		private static User LoadCurrentUser()
		{
			using(var uow = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot())
			{
				return uow.GetById<User>(QSMain.User.Id);
			}
		}
	}
}
