using System;
using QS.DomainModel.UoW;
using Vodovoz.Core;
using Vodovoz.Domain.Permissions.Warehouse;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Infrastructure.Services;

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
			var repository = EmployeeSingletonRepository.GetInstance();
			var employee = repository.GetEmployeeForCurrentUser(uow);
			return WarehousePermissionValidatorFactory.CreateValidator(employee.Subdivision);
		}
	}
}
