using System.Linq;
using QSOrmProject;
using QSProjectsLib;
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
		public static bool CheckViewWarehouse(Warehouse warehouse, WarehousePermissions edit)
		{
			//Внимание!!! Склад пустой обычно у новых документов. Возможность создания должна проверятся другими условиями. Тут пропускаем.
			if(warehouse == null)
				return false;

			if(!CurrentPermissions.Warehouse[WarehousePermissions.WarehouseView, warehouse] && !CurrentPermissions.Warehouse[edit, warehouse])
			{
				MessageDialogWorks.RunErrorDialog("У вас нет прав на просмотр документов склада '{0}'.", warehouse.Name);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Проверка прав на создание документа
		/// </summary>
		/// <returns>Если <c>true</c> нет прав на просмотр.</returns>
		public static bool CheckCreateDocument(Warehouse warehouse, WarehousePermissions edit)
		{
			if(warehouse != null)
			{
				if(!CurrentPermissions.Warehouse[edit, warehouse]) {
					MessageDialogWorks.RunErrorDialog("У вас нет прав на создание этого документа для склада '{0}'.", warehouse.Name);
					return true;
				}
			}
			else
			{
				if(!CurrentPermissions.Warehouse.Allowed(edit).Any()) {
					MessageDialogWorks.RunErrorDialog("У вас нет прав на создание этого документа.");
					return true;
				}
			}
			return false;
		}

		public static bool CanEditDocument(Warehouse warehouse, WarehousePermissions edit)
		{
			if(warehouse == null)
				return CurrentPermissions.Warehouse.Allowed(edit).Any();
			else
				return CurrentPermissions.Warehouse[edit, warehouse];
		}
	}
}
