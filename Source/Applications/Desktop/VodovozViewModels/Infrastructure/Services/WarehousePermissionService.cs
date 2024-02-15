﻿using System;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.EntityRepositories.Permissions;
using Vodovoz.Infrastructure.Services;
using Vodovoz.ViewModels.Infrastructure.Services;

namespace Vodovoz.Services.Permissions
{
	public class WarehousePermissionService : IWarehousePermissionService
	{
		private IWarehousePermissionValidatorFactory _warehousePermissionValidatorFactory;
		private readonly IPermissionRepository _permissionRepository;

		//FIXME Хранение в статическом свойстве (как и сама фабрика) необходимо временно на момент переноса базового функционала в проект без зависимостей от проектов с зависимостями на Gtk
		public IWarehousePermissionValidatorFactory WarehousePermissionValidatorFactory {
			get {
				if(_warehousePermissionValidatorFactory == null) {
					throw new InvalidProgramException($"Не настроена фабрика {nameof(IWarehousePermissionValidatorFactory)} для валидатора прав на складские документы");
				}
				return _warehousePermissionValidatorFactory;
			}

			set => _warehousePermissionValidatorFactory = value;
		}

		public WarehousePermissionService()
		{
			WarehousePermissionValidatorFactory = new WarehousePermissionValidatorFactory();
			_permissionRepository = new PermissionRepository();
		}

		public IWarehousePermissionValidator GetValidator()
			=> WarehousePermissionValidatorFactory.CreateValidator(UnitOfWorkFactory.GetDefaultFactory, _permissionRepository);
	}
}
