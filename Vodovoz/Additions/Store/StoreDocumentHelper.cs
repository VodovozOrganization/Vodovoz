using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using Vodovoz.Core;
using Vodovoz.Core.Permissions;
using Vodovoz.Domain.Store;

namespace Vodovoz.Additions.Store
{
	public static class StoreDocumentHelper
	{
		public static Warehouse GetDefaultWarehouse(IUnitOfWork uow, WarehousePermissions edit){

			if(CurrentUserSettings.Settings.DefaultWarehouse != null) {
				var warehouse = uow.GetById<Warehouse>(CurrentUserSettings.Settings.DefaultWarehouse.Id);
				if(CurrentPermissions.Warehouse[WarehousePermissions.WarehouseView, warehouse] && CurrentPermissions.Warehouse[edit, warehouse])
					return warehouse;
			}

			if(CurrentPermissions.Warehouse.Allowed(edit).Count() == 1)
				return CurrentPermissions.Warehouse.Allowed(edit).First();

			return null;
		}

		/// <summary>
		/// Проверка прав на просмотр документа
		/// </summary>
		/// <returns>Если <c>true</c> нет прав на просмотр.</returns>
		public static bool CheckViewWarehouse(WarehousePermissions edit, params Warehouse[] warehouses)
		{
			//Внимание!!! Склад пустой обычно у новых документов. Возможность создания должна проверятся другими условиями. Тут пропускаем.
			warehouses = warehouses.Where(x => x != null).ToArray();
			if(warehouses.Length == 0)
				return false;

			if(warehouses.Any(x => CurrentPermissions.Warehouse[WarehousePermissions.WarehouseView, x] || CurrentPermissions.Warehouse[edit, x]))
				return false;

			MessageDialogHelper.RunErrorDialog("У вас нет прав на просмотр документов склада '{0}'.", String.Join(";", warehouses.Distinct().Select(x => x.Name)));
			return true;
		}

		/// <summary>
		/// Проверка прав на создание документа
		/// </summary>
		/// <returns>Если <c>true</c> нет прав на создание.</returns>
		public static bool CheckCreateDocument(WarehousePermissions edit, params Warehouse[] warehouses)
		{
			warehouses = warehouses.Where(x => x != null).ToArray();
			if(warehouses.Any())
			{
				if(warehouses.Any(x => CurrentPermissions.Warehouse[edit, x])) 
					return false;

				MessageDialogHelper.RunErrorDialog("У вас нет прав на создание этого документа для склада '{0}'.", String.Join(";", warehouses.Distinct().Select(x => x.Name)));
			}
			else
			{
				if(CurrentPermissions.Warehouse.Allowed(edit).Any())
					return false;

				MessageDialogHelper.RunErrorDialog("У вас нет прав на создание этого документа.");
			}
			return true;
		}

		/// <summary>
		/// Проверка всех прав диалога. 
		/// </summary>
		/// <returns>Если <c>true</c> нет прав на просмотр.</returns>
		public static bool CheckAllPermissions(bool isNew, WarehousePermissions edit, params Warehouse[] warehouses)
		{
			if(isNew && CheckCreateDocument(edit, warehouses)) 
				return true;

			if(CheckViewWarehouse(edit, warehouses))
				return true;

			return false;
		}

		/// <summary>
		/// Проверка прав на изменение документа
		/// </summary>
		/// <returns>Если <c>false</c> нет прав на создание.</returns>
		public static bool CanEditDocument(WarehousePermissions edit, params Warehouse[] warehouses)
		{
			warehouses = warehouses.Where(x => x != null).ToArray();
			if(warehouses.Any())
				return warehouses.Any(x => CurrentPermissions.Warehouse[edit, x]);
			else
				return CurrentPermissions.Warehouse.Allowed(edit).Any();
		}

		public static QueryOver<Warehouse> GetWarehouseQuery()
		{
			return QueryOver.Of<Warehouse>()
							.AndNot(w => w.IsArchive);
		}

		public static QueryOver<Warehouse> GetRestrictedWarehouseQuery(params WarehousePermissions[] permissions)
		{
			var query = QueryOver.Of<Warehouse>().WhereNot(w => w.IsArchive);
			var disjunction = new Disjunction();

			foreach(var p in permissions) {
				disjunction.Add<Warehouse>(w => w.Id.IsIn(CurrentPermissions.Warehouse.Allowed(p).Select(x => x.Id).ToArray()));
			}

			return query.Where(disjunction);
		}

		public static IList<Warehouse> GetRestrictedWarehousesList(IUnitOfWork uow, params WarehousePermissions[] permissions)
		{
			var result = GetRestrictedWarehouseQuery(permissions)
				.DetachedCriteria
				.GetExecutableCriteria(uow.Session)
				.List<Warehouse>()
				.Where(w => w.OwningSubdivision != null)
				.ToList();
			return result;
		}

		/// <summary>
		/// Запрос на возврат всех скадов для которых есть хотя бы одно из разрешений.
		/// </summary>
		public static QueryOver<Warehouse> GetRestrictedWarehouseQuery()
		{
			return QueryOver.Of<Warehouse>()
				            .Where(w => w.Id.IsIn(CurrentPermissions.Warehouse.AnyPermissions()
				                                  .Select(x => x.Id).ToArray()))
				            .AndNot(w => w.IsArchive);
		}

	}
}
