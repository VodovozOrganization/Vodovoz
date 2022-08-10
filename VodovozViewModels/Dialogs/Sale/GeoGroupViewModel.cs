using NHibernate.Util;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using System;
using System.Linq;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.Store;
using Vodovoz.Infrastructure.Services;
using VodovozInfrastructure.Versions;
using Gamma.Utilities;

namespace Vodovoz.ViewModels.Dialogs.Sales
{
	public class GeoGroupViewModel : EntityTabViewModelBase<GeoGroup>
	{
		private readonly GeoGroupVersionsModel _geoGroupVersionsModel;

		private bool _canEdit;
		private IPermissionResult _versionsPermissionResult;
		private DelegateCommand _addVersionCommand;
		private DelegateCommand _copyVersionCommand;
		private DelegateCommand _activateVersionCommand;
		private DelegateCommand _closeVersionCommand;
		private DelegateCommand _removeVersionCommand;

		public GeoGroupViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, GeoGroupVersionsModel geoGroupVersionsModel, ICommonServices commonServices) : base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			_geoGroupVersionsModel = geoGroupVersionsModel ?? throw new ArgumentNullException(nameof(geoGroupVersionsModel));

			CheckPermissions();
		}

		private void CheckPermissions()
		{
			_canEdit = PermissionResult.CanUpdate || (PermissionResult.CanCreate && UoW.IsNew);
			_versionsPermissionResult = CommonServices.CurrentPermissionService.ValidateEntityPermission(typeof(GeoGroupVersion));
		}

		public bool CanEdit => _canEdit;
		public bool CanReadVersions => _versionsPermissionResult.CanRead;


		#region Add version

		public DelegateCommand AddVersionCommand
		{
			get
			{
				if(_addVersionCommand == null)
				{
					_addVersionCommand = new DelegateCommand(AddVersion, () => CanAddVersion);
					_addVersionCommand.CanExecuteChangedWith(this, x => x.CanAddVersion);
				}
				return _addVersionCommand;
			}
		}

		public bool CanAddVersion => CanReadVersions && CanEdit && _versionsPermissionResult.CanCreate;

		private void AddVersion()
		{
			_geoGroupVersionsModel.AddVersion(UoW, Entity);
		}

		#endregion Add version

		#region Copy version

		public DelegateCommand CopyVersionCommand
		{
			get
			{
				if(_copyVersionCommand == null)
				{
					_copyVersionCommand = new DelegateCommand(CopyVersion, () => CanCopyVersion);
					_copyVersionCommand.CanExecuteChangedWith(this, x => x.CanCopyVersion, x => x.SelectedVersion);
				}
				return _copyVersionCommand;
			}
		}

		public bool CanCopyVersion => CanReadVersions
			&& CanEdit 
			&& _versionsPermissionResult.CanCreate
			&& SelectedVersion != null;

		private void CopyVersion()
		{
			_geoGroupVersionsModel.CopyVersion(UoW, Entity, SelectedVersion);
		}

		#endregion Copy version

		#region Close version

		public DelegateCommand CloseVersionCommand
		{
			get
			{
				if(_closeVersionCommand == null)
				{
					_closeVersionCommand = new DelegateCommand(CloseVersion, () => CanCloseVersion);
					_closeVersionCommand.CanExecuteChangedWith(this, x => x.CanCloseVersion, x => x.SelectedVersion);
				}
				return _closeVersionCommand;
			}
		}

		public bool CanCloseVersion => CanReadVersions
			&& CanEdit
			&& _versionsPermissionResult.CanUpdate
			&& SelectedVersion != null
			&& SelectedVersion.Status == VersionStatus.Active;

		private void CloseVersion()
		{
			_geoGroupVersionsModel.CloseVersion(Entity, SelectedVersion);
		}

		#endregion Close version

		#region Remove version

		public DelegateCommand RemoveVersionCommand
		{
			get
			{
				if(_removeVersionCommand == null)
				{
					_removeVersionCommand = new DelegateCommand(RemoveVersion, () => CanRemoveVersion);
					_removeVersionCommand.CanExecuteChangedWith(this, x => x.CanRemoveVersion, x => x.SelectedVersion);
				}
				return _removeVersionCommand;
			}
		}

		public bool CanRemoveVersion => CanReadVersions 
			&& CanEdit 
			&& _versionsPermissionResult.CanDelete
			&& SelectedVersion != null
			&& SelectedVersion.Status == VersionStatus.Draft;

		private void RemoveVersion()
		{
			_geoGroupVersionsModel.RemoveVersion(Entity, SelectedVersion);
		}

		#endregion Remove version

		#region Activate version

		public DelegateCommand ActivateVersionCommand
		{
			get
			{
				if(_activateVersionCommand == null)
				{
					_activateVersionCommand = new DelegateCommand(ActivateVersion, () => CanActivateVersion);
					_activateVersionCommand.CanExecuteChangedWith(this, x => x.CanActivateVersion, x => x.SelectedVersion);
				}
				return _activateVersionCommand;
			}
		}

		public bool CanActivateVersion => CanReadVersions
			&& CanEdit
			&& _versionsPermissionResult.CanUpdate
			&& SelectedVersion != null
			&& SelectedVersion.Status == VersionStatus.Draft;

		private void ActivateVersion()
		{
			_geoGroupVersionsModel.ActivateVersion(Entity, SelectedVersion);
		}

		#endregion Activate version

		private GeoGroupVersion _selectedVersion;
		public virtual GeoGroupVersion SelectedVersion
		{
			get => _selectedVersion;
			set => SetField(ref _selectedVersion, value);
		}
	}

	public class GeoGroupVersionsModel
	{
		private readonly IUserService _userService;
		private readonly IEmployeeService _employeeService;

		public GeoGroupVersionsModel(IUserService userService, IEmployeeService employeeService)
		{
			_userService = userService ?? throw new System.ArgumentNullException(nameof(userService));
			_employeeService = employeeService ?? throw new System.ArgumentNullException(nameof(employeeService));
		}

		public void AddVersion(IUnitOfWork uow, GeoGroup geoGroup)
		{
			var currentEmployee = _employeeService.GetEmployeeForUser(uow, _userService.CurrentUserId);

			var newVersion = new GeoGroupVersion();
			newVersion.GeoGroup = geoGroup;
			newVersion.Author = currentEmployee;
			newVersion.BaseLatitude = null;
			newVersion.BaseLongitude = null;
			newVersion.CashSubdivision = null;
			newVersion.Warehouse = null;
			newVersion.DateCreated = DateTime.Now;
			newVersion.Status = VersionStatus.Draft;

			geoGroup.ObservableVersions.Add(newVersion);
		}

		public void CopyVersion(IUnitOfWork uow, GeoGroup geoGroup, GeoGroupVersion copyFrom)
		{
			var currentEmployee = _employeeService.GetEmployeeForUser(uow, _userService.CurrentUserId);
			var cash = copyFrom.CashSubdivision != null ? uow.GetById<Subdivision>(copyFrom.CashSubdivision.Id) : null;
			var warehouse = copyFrom.Warehouse != null ? uow.GetById<Warehouse>(copyFrom.Warehouse.Id) : null;

			var newVersion = new GeoGroupVersion();
			newVersion.GeoGroup = geoGroup;
			newVersion.Author = currentEmployee;
			newVersion.BaseLatitude = copyFrom.BaseLatitude;
			newVersion.BaseLongitude = copyFrom.BaseLongitude;
			newVersion.CashSubdivision = cash;
			newVersion.Warehouse = warehouse;
			newVersion.DateCreated = DateTime.Now;
			newVersion.Status = VersionStatus.Draft;

			geoGroup.ObservableVersions.Add(newVersion);
		}

		public void ActivateVersion(GeoGroup geoGroup, GeoGroupVersion activatingVersion)
		{
			if(!geoGroup.Versions.Contains(activatingVersion))
			{
				throw new InvalidOperationException($"Активируемая версия данных части города должна находиться в редактируемой части города ({geoGroup.Name})");
			}

			var activeVersion = geoGroup.Versions.FirstOrDefault(v => v.Status == VersionStatus.Active);
			if (activeVersion != null)
			{
				CloseVersion(geoGroup, activeVersion);
			}

			activatingVersion.DateActivated = DateTime.Now;
			activatingVersion.Status = VersionStatus.Active;
		}

		public void CloseVersion(GeoGroup geoGroup, GeoGroupVersion closingVersion)
		{
			if(!geoGroup.Versions.Contains(closingVersion))
			{
				throw new InvalidOperationException($"Закрываемая версия данных части города должна находиться в редактируемой части города ({geoGroup.Name})");
			}

			if(closingVersion.Status != VersionStatus.Active)
			{
				throw new InvalidOperationException($"Можно закрывать только данных части города в статусе {VersionStatus.Active.GetEnumTitle()}");
			}

			closingVersion.DateClosed = DateTime.Now;
			closingVersion.Status = VersionStatus.Closed;
		}

		public void RemoveVersion(GeoGroup geoGroup, GeoGroupVersion deletingVersion)
		{
			if(!geoGroup.Versions.Contains(deletingVersion))
			{
				throw new InvalidOperationException($"Удаляемая версия данных части города должна находиться в редактируемой части города ({geoGroup.Name})");
			}

			if(deletingVersion.Status != VersionStatus.Draft)
			{
				throw new InvalidOperationException($"Можно удалять только данных части города в статусе {VersionStatus.Draft.GetEnumTitle()}");
			}

			geoGroup.ObservableVersions.Remove(deletingVersion);
		}
	}
}
