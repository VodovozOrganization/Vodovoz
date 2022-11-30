using System;
using QS.DomainModel.UoW;
using Vodovoz.Core;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Infrastructure.Services;
using Vodovoz.ViewModels.Infrastructure.Services;

namespace Vodovoz.Services.Permissions
{
	public class WarehousePermissionService : IWarehousePermissionService
	{
		private IWarehousePermissionValidatorFactory warehousePermissionValidatorFactory;

		//FIXME Хранение в статическом свойстве (как и сама фабрика) необходимо временно на момент переноса базового функционала в проект без зависимостей от проектов с зависимостями на Gtk
		public IWarehousePermissionValidatorFactory WarehousePermissionValidatorFactory {
			get {
				if(warehousePermissionValidatorFactory == null) {
					throw new InvalidProgramException($"Не настроена фабрика {nameof(IWarehousePermissionValidatorFactory)} для валидатора прав на складские документы");
				}
				return warehousePermissionValidatorFactory;
			}

			set => warehousePermissionValidatorFactory = value;
		}

		public WarehousePermissionService()
		{
			WarehousePermissionValidatorFactory = new WarehousePermissionValidatorFactory();
		}

		public IWarehousePermissionValidator GetValidator(IUnitOfWork uow, int userId)
		{
			var repository = new EmployeeRepository();
			var employee = repository.GetEmployeeForCurrentUser(uow);
			return GetValidator(employee.Subdivision);
		}

		public IWarehousePermissionValidator GetValidator(Subdivision subdivision) =>
			WarehousePermissionValidatorFactory.CreateValidator(subdivision);
	}
}
