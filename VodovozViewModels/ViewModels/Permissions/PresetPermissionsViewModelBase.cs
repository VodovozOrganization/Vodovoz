using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Permissions;
using QS.ViewModels;
using Vodovoz.Domain.Permissions;
using Vodovoz.EntityRepositories.Permissions;

namespace Vodovoz.ViewModels.Permissions
{
	public abstract class PresetPermissionsViewModelBase : UoWWidgetViewModelBase
	{
		protected IPermissionRepository permissionRepository;

		protected IList<HierarchicalPresetPermissionBase> deletePermissionList = new List<HierarchicalPresetPermissionBase>();

		protected IList<HierarchicalPresetPermissionBase> permissionList;
		public GenericObservableList<HierarchicalPresetPermissionBase> ObservablePermissionsList { get; protected set; }

		protected IList<PresetUserPermissionSource> originalPermissionsSourceList;
		public GenericObservableList<PresetUserPermissionSource> ObservablePermissionsSourceList { get; protected set; }

		protected PresetPermissionsViewModelBase(IUnitOfWork unitOfWork, IPermissionRepository permissionRepository)
		{
			UoW = unitOfWork ?? throw new NullReferenceException(nameof(unitOfWork));
			this.permissionRepository = permissionRepository ?? throw new NullReferenceException(nameof(permissionRepository));
		}

		protected DelegateCommand<PresetUserPermissionSource> addPermissionCommand;
		public virtual DelegateCommand<PresetUserPermissionSource> AddPermissionCommand { get; }

		protected DelegateCommand<HierarchicalPresetPermissionBase> removePermissionCommand;
		public virtual DelegateCommand<HierarchicalPresetPermissionBase> RemovePermissionCommand { get; }

		protected DelegateCommand saveCommand;
		public virtual DelegateCommand SaveCommand { get; }
	}
}
