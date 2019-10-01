using System;
using Vodovoz.Infrastructure.Services;
using QS.Permissions;
using Vodovoz.Infrastructure.Journal;
using QS.Services;
using QS.Project.Services.GtkUI;

namespace Vodovoz
{
	public static class VodovozGtkServicesConfig
	{
		public static IEmployeeService EmployeeService { get; set; }
		public static IRepresentationEntityPicker RepresentationEntityPicker { get; set; }

		public static void CreateVodovozDefaultServices()
		{
			GtkAppServicesConfig.CreateDefaultGtkServices();
			EmployeeService = new EmployeeService();

			IRepresentationJournalFactory journalFactory = new PermissionControlledRepresentationJournalFactory();
			RepresentationEntityPicker = new RepresentationEntityPickerGtk(journalFactory);

			PermissionsSettings.ConfigureEntityPermissionFinder(new Vodovoz.Domain.Permissions.EntitiesWithPermissionFinder());

			//пространство имен специально прописано чтобы при изменениях не было случайного совпадения с валидатором из QS
			var entityPermissionValidator = new Vodovoz.Domain.Permissions.EntityPermissionValidator();
			var presetPermissionValidator = new QS.DomainModel.Entity.PresetPermissions.PresetPermissionValidator();
			PermissionsSettings.PermissionService = new PermissionService(entityPermissionValidator, presetPermissionValidator);
		}
	}
}
