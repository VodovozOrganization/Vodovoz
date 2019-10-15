using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Services;
using QS.ViewModels;
using Vodovoz.PermissionExtensions;
using Vodovoz.Repositories.Permissions;

namespace Vodovoz.ViewModels
{
	public class PermissionListViewModel: WidgetViewModelBase
	{
		public PermissionListViewModel(IInteractiveService interactiveService, PermissionExtensionSingletonStore permissionExtensionStore) : base(interactiveService)
		{
			PermissionExtensionStore = permissionExtensionStore ?? throw new NullReferenceException(nameof(permissionExtensionStore));
		}

		public PermissionExtensionSingletonStore PermissionExtensionStore { get; set; }

		private bool readOnly = false;
		public virtual bool ReadOnly {
			get => readOnly;
			set => SetField(ref readOnly, value, () => ReadOnly);
		}

		private GenericObservableList<PermissionNode> permissionsList;
		public virtual  GenericObservableList<PermissionNode> PermissionsList {
			get => permissionsList;
			set { SetField(ref permissionsList, value, () => PermissionsList);}
		}

		public void SaveExtendedPermissions(IUnitOfWork uow)
		{
			foreach(var item in PermissionsList.SelectMany(x => x.EntityPermissionExtended).Where(x => x.IsPermissionAvailable != null)) {
				uow.Save(item);
			}
		}

		#region Commands

		public DelegateCommand AddItemCommand { get; private set; }
		public DelegateCommand<PermissionNode> DeleteItemCommand { get; private set; }

		private void CreateCommands()
		{
			AddItemCommand = new DelegateCommand(
				() => {
					var phone = new PermissionNode();
					if(PermissionsList == null)
						PermissionsList = new GenericObservableList<PermissionNode>();
					PermissionsList.Add(phone);
				},
				() => { return !ReadOnly; }
			);

			DeleteItemCommand = new DelegateCommand<PermissionNode>(
				(phone) => {
					PermissionsList.Remove(phone);
				},
				(phone) => { return !ReadOnly; }
			);
		}

		#endregion Commands
	}
}
