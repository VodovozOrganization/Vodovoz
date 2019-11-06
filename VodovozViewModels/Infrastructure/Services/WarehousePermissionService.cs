using System;
using Vodovoz.Infrastructure.Permissions;
using Vodovoz.Infrastructure.Services;
namespace Vodovoz.Services.Permissions
{
	public class WarehousePermissionService : IWarehousePermissionService
	{
		private static IWarehousePermissionValidatorFactory warehousePermissionValidatorFactory;

		//FIXME Хранение в статическом свойстве (как и сама фабрика) необходимо временно на момент переноса базового функционала в проект без зависимостей от проектов с зависимостями на Gtk
		public static IWarehousePermissionValidatorFactory WarehousePermissionValidatorFactory {
			get {
				if(warehousePermissionValidatorFactory == null) {
					throw new InvalidProgramException($"Не настроена фабрика {nameof(IWarehousePermissionValidatorFactory)} для валидатора прав на складские документы");
				}
				return warehousePermissionValidatorFactory;
			}

			set => warehousePermissionValidatorFactory = value;
		}

		public IWarehousePermissionValidator GetValidator(int userId)
		{
			return WarehousePermissionValidatorFactory.CreateValidator(userId);
		}
	}
}
