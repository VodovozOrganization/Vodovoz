using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.Entity.PresetPermissions;
using QS.Services;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.PermissionExtensions;
using Vodovoz.ViewModels.Infrastructure.Services;

namespace Vodovoz
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddPermissionValidation(this IServiceCollection services)
		{
			services.AddSingleton<IPermissionService, PermissionService>();
			services.AddSingleton<IEntityExtendedPermissionValidator, EntityExtendedPermissionValidator>();
			services.AddSingleton<IWarehousePermissionValidator, WarehousePermissionValidator>();
			services.AddSingleton<IPermissionExtensionStore>(sp => PermissionExtensionSingletonStore.GetInstance());

			services.AddSingleton<IEntityPermissionValidator, Vodovoz.Domain.Permissions.EntityPermissionValidator>();
			services.AddSingleton<IPresetPermissionValidator, Vodovoz.Domain.Permissions.HierarchicalPresetPermissionValidator>();

			return services;
		}
	}
}
