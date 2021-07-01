using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using Vodovoz.Core;
using Vodovoz.Domain.Permissions.Warehouse;
using Vodovoz.Domain.Store;

namespace Vodovoz.Additions.Store
{
	public class StoreDocumentHelper
	{
		private CurrentPermissions permissions;
		public CurrentPermissions Permissions
		{
			get => permissions;
		}
		public StoreDocumentHelper()
		{
			permissions = new CurrentPermissions();
		}
		public Warehouse GetDefaultWarehouse(IUnitOfWork uow, WarehousePermissions edit)
		{
			if(CurrentUserSettings.Settings.DefaultWarehouse != null) {
				var warehouse = uow.GetById<Warehouse>(CurrentUserSettings.Settings.DefaultWarehouse.Id);
				var warehouses = Permissions.Warehouse.Where(x => x.Warehouse.Id == warehouse.Id);
				var permission = warehouses
					.SingleOrDefault(x => x.WarehousePermissionType == WarehousePermissions.WarehouseView)
					?.ValuePermission;
				var permissionEdit =
					warehouses.SingleOrDefault(x => x.WarehousePermissionType == edit)?.ValuePermission;
				if((permission.HasValue && permission.Value) && (permissionEdit.HasValue && permissionEdit.Value))
					return warehouse;
			}

			if(Permissions.Warehouse.Count(x => x.WarehousePermissionType == edit) == 1)
				return Permissions.Warehouse.First(x => x.WarehousePermissionType == edit).Warehouse;

			return null;
		}

		/// <summary>
		/// Проверка прав на просмотр документа
		/// </summary>
		/// <returns>Если <c>true</c> нет прав на просмотр.</returns>
		public bool CheckViewWarehouse(WarehousePermissions edit, params Warehouse[] warehouses)
		{
			//Внимание!!! Склад пустой обычно у новых документов. Возможность создания должна проверятся другими условиями. Тут пропускаем.
			warehouses = warehouses.Where(x => x != null).ToArray();
			if(warehouses.Length == 0)
				return false;
			var permission =
				Permissions.Warehouse.Where(x =>
					x.WarehousePermissionType == WarehousePermissions.WarehouseView);
			var permissionEdit = Permissions.Warehouse.Where(x => x.WarehousePermissionType == edit);
			if(warehouses.Any(x => permission.SingleOrDefault(y=>y.Warehouse.Id == x.Id).ValuePermission.Value 
			                       || permissionEdit.SingleOrDefault(y=>y.Warehouse.Id == x.Id).ValuePermission.Value))
				return false;

			MessageDialogHelper.RunErrorDialog($"У вас нет прав на просмотр документов склада '{string.Join(";", warehouses.Distinct().Select(x => x.Name))}'.");
			return true;
		}

		/// <summary>
		/// Проверка прав на создание документа
		/// </summary>
		/// <returns>Если <c>true</c> нет прав на создание.</returns>
		public bool CheckCreateDocument(WarehousePermissions edit, params Warehouse[] warehouses)
		{
			warehouses = warehouses.Where(x => x != null).ToArray();
			if(warehouses.Any()) {
				if(warehouses.Any(x => Permissions.Warehouse.SingleOrDefault(y=>y.WarehousePermissionType == edit && y.Warehouse.Id == x.Id).ValuePermission.Value))
					return false;

				MessageDialogHelper.RunErrorDialog(
					string.Format(
						"У вас нет прав на создание этого документа для склада '{0}'.",
						string.Join(";", warehouses.Distinct().Select(x => x.Name))
					)
				);
			} else {
				if(Permissions.Warehouse.Any(x => x.WarehousePermissionType == edit))
					return false;

				MessageDialogHelper.RunErrorDialog("У вас нет прав на создание этого документа.");
			}
			return true;
		}

		/// <summary>
		/// Проверка всех прав диалога. 
		/// </summary>
		/// <returns>Если <c>true</c> нет прав на просмотр.</returns>
		public bool CheckAllPermissions(bool isNew, WarehousePermissions edit, params Warehouse[] warehouses)
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
		public bool CanEditDocument(WarehousePermissions edit, params Warehouse[] warehouses)
		{
			warehouses = warehouses.Where(x => x != null).ToArray();
			if(warehouses.Any())
				return warehouses.Any(x => Permissions.Warehouse.SingleOrDefault(y=>y.WarehousePermissionType == edit && y.Warehouse.Id == x.Id).ValuePermission.Value);
			return Permissions.Warehouse.Any(x => x.WarehousePermissionType == edit);
		}

		public QueryOver<Warehouse> GetWarehouseQuery() => QueryOver.Of<Warehouse>().AndNot(w => w.IsArchive);

		public QueryOver<Warehouse> GetRestrictedWarehouseQuery(params WarehousePermissions[] permissions)
		{
			var query = QueryOver.Of<Warehouse>().WhereNot(w => w.IsArchive);
			var disjunction = new Disjunction();

			foreach(var p in permissions) {
				disjunction.Add<Warehouse>(w => w.Id.IsIn(Permissions.Warehouse.Where(x=>x.WarehousePermissionType == p).Select(x => x.Id).ToArray()));
			}

			return query.Where(disjunction);
		}

		public IList<Warehouse> GetRestrictedWarehousesList(IUnitOfWork uow, params WarehousePermissions[] permissions)
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
		public QueryOver<Warehouse> GetRestrictedWarehouseQuery()
		{
			return QueryOver.Of<Warehouse>()
							.Where(w => w.Id.IsIn(Permissions.Warehouse.Where(x=>x.ValuePermission == true).Select(x => x.Id).ToArray()))
							.AndNot(w => w.IsArchive);
		}
	}
}