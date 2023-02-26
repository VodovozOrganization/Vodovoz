﻿using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.Domain.Store;

namespace Vodovoz.Additions.Store
{
	public class StoreDocumentHelper
	{
		private CurrentWarehousePermissions WarehousePermissions { get; }

		public StoreDocumentHelper()
		{
			WarehousePermissions = new CurrentWarehousePermissions();
		}
		public Warehouse GetDefaultWarehouse(IUnitOfWork uow, WarehousePermissionsType edit)
		{
			if(CurrentUserSettings.Settings.DefaultWarehouse != null) {
				var warehouse = uow.GetById<Warehouse>(CurrentUserSettings.Settings.DefaultWarehouse.Id);
				var warehouses = WarehousePermissions.WarehousePermissions.Where(x => x.Warehouse.Id == warehouse.Id);
				var permission = warehouses
					.SingleOrDefault(x => x.WarehousePermissionType == WarehousePermissionsType.WarehouseView)
					?.PermissionValue;
				var permissionEdit =
					warehouses.SingleOrDefault(x => x.WarehousePermissionType == edit)?.PermissionValue;
				if((permission.HasValue && permission.Value) && (permissionEdit.HasValue && permissionEdit.Value))
				{
					return warehouse;
				}
			}

			if(WarehousePermissions.WarehousePermissions.Count(x => x.WarehousePermissionType == edit) == 1)
			{
				var warehouse =
					WarehousePermissions.WarehousePermissions.First(x => x.WarehousePermissionType == edit).Warehouse;
				
				return uow.GetById<Warehouse>(warehouse.Id);
			}

			return null;
		}

		/// <summary>
		/// Проверка прав на просмотр документа
		/// </summary>
		/// <returns>Если <c>true</c> нет прав на просмотр.</returns>
		public bool CheckViewWarehouse(WarehousePermissionsType edit, params Warehouse[] warehouses)
		{
			//Внимание!!! Склад пустой обычно у новых документов. Возможность создания должна проверятся другими условиями. Тут пропускаем.
			warehouses = warehouses.Where(x => x != null).ToArray();
			if(warehouses.Length == 0)
			{
				return false;
			}

			var permission =
				WarehousePermissions.WarehousePermissions.Where(x =>
					x.WarehousePermissionType == WarehousePermissionsType.WarehouseView).ToArray();
			var permissionEdit =
				WarehousePermissions.WarehousePermissions.Where(x => x.WarehousePermissionType == edit).ToArray();

			foreach(var warehouse in warehouses)
			{
				var matchPermission = permission.FirstOrDefault(y => y.Warehouse.Id == warehouse.Id);
				
				if(matchPermission?.PermissionValue != null && matchPermission.PermissionValue.Value)
				{
					return false;
				}

				var matchPermissionEdit = permissionEdit.FirstOrDefault(y => y.Warehouse.Id == warehouse.Id);
				
				if(matchPermissionEdit?.PermissionValue != null && matchPermissionEdit.PermissionValue.Value)
				{
					return false;
				}
			}

			MessageDialogHelper.RunErrorDialog($"У вас нет прав на просмотр документов склада '{string.Join(";", warehouses.Distinct().Select(x => x.Name))}'.");
			return true;
		}

		/// <summary>
		/// Проверка прав на создание документа
		/// </summary>
		/// <returns>Если <c>true</c> нет прав на создание.</returns>
		public bool CheckCreateDocument(WarehousePermissionsType edit, params Warehouse[] warehouses)
		{
			warehouses = warehouses.Where(x => x != null).ToArray();
			if(warehouses.Any())
			{
				foreach(var warehouse in warehouses)
				{
					var warehousePermission = WarehousePermissions.WarehousePermissions.FirstOrDefault(
						x => x.WarehousePermissionType == edit && x.Warehouse.Id == warehouse.Id);

					if(warehousePermission?.PermissionValue != null && warehousePermission.PermissionValue.Value)
					{
						return false;
					}
				}

				MessageDialogHelper.RunErrorDialog(
					string.Format(
						"У вас нет прав на создание этого документа для склада '{0}'.",
						string.Join(";", warehouses.Distinct().Select(x => x.Name))
					)
				);
			}
			else
			{
				if(WarehousePermissions.WarehousePermissions.Any(x => x.WarehousePermissionType == edit))
				{
					return false;
				}

				MessageDialogHelper.RunErrorDialog("У вас нет прав на создание этого документа.");
			}
			return true;
		}

		/// <summary>
		/// Проверка всех прав диалога.
		/// </summary>
		/// <returns>Если <c>true</c> нет прав на просмотр.</returns>
		public bool CheckAllPermissions(bool isNew, WarehousePermissionsType edit, params Warehouse[] warehouses)
		{
			if(isNew && CheckCreateDocument(edit, warehouses))
			{
				return true;
			}

			if(CheckViewWarehouse(edit, warehouses))
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Проверка прав на изменение документа
		/// </summary>
		/// <returns>Если <c>false</c> нет прав на создание.</returns>
		public bool CanEditDocument(WarehousePermissionsType edit, params Warehouse[] warehouses)
		{
			warehouses = warehouses.Where(x => x != null).ToArray();
			if(warehouses.Any())
			{
				foreach(var warehouse in warehouses)
				{
					var warehousePermission = WarehousePermissions.WarehousePermissions.FirstOrDefault(
						x => x.WarehousePermissionType == edit && x.Warehouse.Id == warehouse.Id);

					if(warehousePermission?.PermissionValue != null && warehousePermission.PermissionValue.Value)
					{
						return warehousePermission.PermissionValue.Value;
					}
				}
				
				return false;
			}

			return WarehousePermissions.WarehousePermissions.Any(x => x.WarehousePermissionType == edit);
		}

		public static QueryOver<Warehouse> GetNotArchiveWarehousesQuery() => QueryOver.Of<Warehouse>().AndNot(w => w.IsArchive);

		public QueryOver<Warehouse> GetRestrictedWarehouseQuery(params WarehousePermissionsType[] permissions)
		{
			var query = QueryOver.Of<Warehouse>().WhereNot(w => w.IsArchive);
			var disjunction = new Disjunction();

			foreach(var p in permissions) {
				disjunction.Add<Warehouse>(
					w => w.Id.IsIn(
						WarehousePermissions.WarehousePermissions
							.Where(x=>x.WarehousePermissionType == p && x.PermissionValue == true)
							.Select(x => x.Warehouse.Id)
							.ToArray()));
			}

			return query.Where(disjunction);
		}

		public IList<Warehouse> GetRestrictedWarehousesList(IUnitOfWork uow, params WarehousePermissionsType[] permissions)
		{
			var result = GetRestrictedWarehouses(permissions)
				.GetExecutableCriteria(uow.Session)
				.List<Warehouse>();
				
			return result;
		}
		
		public IEnumerable<int> GetRestrictedWarehousesIds(IUnitOfWork uow, params WarehousePermissionsType[] permissions)
		{
			var result = GetRestrictedWarehouses(permissions)
				.GetExecutableCriteria(uow.Session)
				.SetProjection(Projections.Property(nameof(Warehouse.Id)))
				.List<int>();
				
			return result;
		}

		/// <summary>
		/// Запрос на возврат всех скадов для которых есть хотя бы одно из разрешений.
		/// </summary>
		public QueryOver<Warehouse> GetRestrictedWarehouseQuery()
		{
			return QueryOver.Of<Warehouse>()
							.Where(w => w.Id.IsIn(
								WarehousePermissions.WarehousePermissions
									.Where(x=>x.PermissionValue == true)
									.Select(x => x.Warehouse.Id).ToArray()))
							.AndNot(w => w.IsArchive);
		}

		private DetachedCriteria GetRestrictedWarehouses(params WarehousePermissionsType[] permissions)
		{
			var result = GetRestrictedWarehouseQuery(permissions)
				.DetachedCriteria
				.Add(Restrictions.IsNotNull(nameof(Warehouse.OwningSubdivision)));

			return result;
		}
	}
}
