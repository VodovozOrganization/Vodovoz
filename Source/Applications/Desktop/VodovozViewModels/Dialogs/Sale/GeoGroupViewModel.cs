using Autofac;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using System;
using System.ComponentModel;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Sale;
using Vodovoz.Journals.JournalViewModels.Organizations;
using Vodovoz.Models;
using Vodovoz.ViewModels.Journals.JournalViewModels.Store;
using Vodovoz.ViewModels.ViewModels.Organizations;
using Vodovoz.ViewModels.Warehouses;
using VodovozInfrastructure.Versions;

namespace Vodovoz.ViewModels.Dialogs.Sales
{
	public class GeoGroupViewModel : EntityTabViewModelBase<GeoGroup>
	{
		private readonly GeoGroupVersionsModel _geoGroupVersionsModel;
		private ILifetimeScope _lifetimeScope;
		private bool _canEdit;
		private IPermissionResult _versionsPermissionResult;
		private IEntityAutocompleteSelectorFactory _cashSelectorFactory;
		private IEntityAutocompleteSelectorFactory _warehouseSelectorFactory;
		private DelegateCommand _createVersionCommand;
		private DelegateCommand _copyVersionCommand;
		private DelegateCommand _activateVersionCommand;
		private DelegateCommand _closeVersionCommand;
		private DelegateCommand _removeVersionCommand;
		private DelegateCommand _saveCommand;
		private DelegateCommand _cancelCommand;
		private GeoGroupVersionViewModel _selectedVersion;
		private GenericObservableList<GeoGroupVersionViewModel> _versions;

		public GeoGroupViewModel(
			IEntityUoWBuilder uowBuilder,
			INavigationManager navigationManager,
			IUnitOfWorkFactory unitOfWorkFactory,
			GeoGroupVersionsModel geoGroupVersionsModel,
			ViewModelEEVMBuilder<Subdivision> geoGroupVersionViewModelEEVMBuilder,
			ViewModelEEVMBuilder<Warehouse> warehouseVersionViewModelEEVMBuilder,
			ICommonServices commonServices,
			ILifetimeScope lifetimeScope) : base(uowBuilder, unitOfWorkFactory, commonServices, navigationManager)
		{
			if(geoGroupVersionViewModelEEVMBuilder is null)
			{
				throw new ArgumentNullException(nameof(geoGroupVersionViewModelEEVMBuilder));
			}

			if(warehouseVersionViewModelEEVMBuilder is null)
			{
				throw new ArgumentNullException(nameof(warehouseVersionViewModelEEVMBuilder));
			}

			_geoGroupVersionsModel = geoGroupVersionsModel ?? throw new ArgumentNullException(nameof(geoGroupVersionsModel));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			CheckPermissions();
			BindVersions();

			CashSubdivisionViewModel = geoGroupVersionViewModelEEVMBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(this, x => x.SelectedVersionCashSubdivision)
				.UseViewModelJournalAndAutocompleter<SubdivisionsJournalViewModel>()
				.UseViewModelDialog<SubdivisionViewModel>()
				.Finish();

			WarehouseViewModel = warehouseVersionViewModelEEVMBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(this, x => x.SelectedVersionWarehouse)
				.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
				.UseViewModelDialog<WarehouseViewModel>()
				.Finish();

			Entity.PropertyChanged += EntityPropertyChanged;
			Versions.ElementRemoved += VersionsElementRemoved;
		}

		public IEntityEntryViewModel CashSubdivisionViewModel { get; }
		public IEntityEntryViewModel WarehouseViewModel { get; }

		private void CheckPermissions()
		{
			_canEdit = PermissionResult.CanUpdate || (PermissionResult.CanCreate && UoW.IsNew);
			_versionsPermissionResult = CommonServices.CurrentPermissionService.ValidateEntityPermission(typeof(GeoGroupVersion));
		}

		private void EntityPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(Entity.ObservableVersions):
					BindVersions();
					break;
				default:
					break;
			}
		}

		private void VersionsElementRemoved(object aList, int[] aIdx, object aObject)
		{
			if(aObject == SelectedVersion)
			{
				SelectedVersion = null;
			}
		}

		public bool CanEdit => _canEdit;
		public bool CanReadVersions => _versionsPermissionResult.CanRead;

		public bool CanChangeCashSubdivision => _versionsPermissionResult.CanCreate && SelectedVersion != null && SelectedVersion.Status == VersionStatus.Draft;
		public bool CanChangeWarehouse => _versionsPermissionResult.CanCreate && SelectedVersion != null && SelectedVersion.Status == VersionStatus.Draft;

		#region Add version

		public DelegateCommand CreateVersionCommand
		{
			get
			{
				if(_createVersionCommand == null)
				{
					_createVersionCommand = new DelegateCommand(CreateVersion, () => CanCreateVersion);
					_createVersionCommand.CanExecuteChangedWith(this, x => x.CanCreateVersion);
				}
				return _createVersionCommand;
			}
		}

		public bool CanCreateVersion => CanReadVersions && CanEdit && _versionsPermissionResult.CanCreate;

		private void CreateVersion()
		{
			_geoGroupVersionsModel.CreateVersion(UoW, Entity);
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
			_geoGroupVersionsModel.CopyVersion(UoW, Entity, SelectedVersion.Entity);
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
			_geoGroupVersionsModel.CloseVersion(Entity, SelectedVersion.Entity);
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
			_geoGroupVersionsModel.RemoveVersion(Entity, SelectedVersion.Entity);
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
			_geoGroupVersionsModel.ActivateVersion(Entity, SelectedVersion.Entity);
		}

		#endregion Activate version

		#region Selected

		public virtual GeoGroupVersionViewModel SelectedVersion
		{
			get => _selectedVersion;
			set
			{
				UnsubscribeSelectedVersionPropertyChanged();
				SetField(ref _selectedVersion, value);
				SubscribeSelectedVersionPropertyChanged();
				OnPropertyChanged(nameof(CanChangeCashSubdivision));
				OnPropertyChanged(nameof(CanChangeWarehouse));
				OnPropertyChanged(nameof(SelectedVersionWarehouse));
				OnPropertyChanged(nameof(SelectedVersionCashSubdivision));
			}
		}

		public Warehouse SelectedVersionWarehouse
		{
			get => SelectedVersion?.Warehouse;
			set
			{
				if(SelectedVersion != null && CanChangeWarehouse)
				{
					SelectedVersion.Warehouse = value;
				}
			}
		}

		public Subdivision SelectedVersionCashSubdivision
		{
			get => SelectedVersion?.CashSubdivision;
			set
			{
				if(SelectedVersion != null && CanChangeCashSubdivision)
				{
					SelectedVersion.CashSubdivision = value;
				}
			}
		}

		public bool HasSelectedVersion => SelectedVersion != null;

		private void UnsubscribeSelectedVersionPropertyChanged()
		{
			if(_selectedVersion != null)
			{
				_selectedVersion.PropertyChanged -= SelectedVersion_PropertyChanged;
			}
		}

		private void SubscribeSelectedVersionPropertyChanged()
		{
			if(_selectedVersion != null)
			{
				_selectedVersion.PropertyChanged += SelectedVersion_PropertyChanged;
			}
		}

		private void SelectedVersion_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(_selectedVersion.Status):
					OnPropertyChanged(nameof(CanCloseVersion));
					OnPropertyChanged(nameof(CanRemoveVersion));
					OnPropertyChanged(nameof(CanActivateVersion));
					break;
				default:
					break;
			}
		}


		#endregion Selected

		#region Versions

		public virtual GenericObservableList<GeoGroupVersionViewModel> Versions
		{
			get => _versions;
			set => SetField(ref _versions, value);
		}

		private void BindVersions()
		{
			var versionViewModels = Entity.Versions.Select(x => new GeoGroupVersionViewModel(x)).ToList();
			Versions = new GenericObservableList<GeoGroupVersionViewModel>(versionViewModels);

			Entity.ObservableVersions.ElementAdded += ObservableVersionsElementAdded;
			Entity.ObservableVersions.ElementRemoved += ObservableVersionsElementRemoved;
			Entity.ObservableVersions.ElementChanged += ObservableVersionsElementChanged;

			SelectedVersion = Versions.LastOrDefault() ?? new GeoGroupVersionViewModel(new GeoGroupVersion());
		}

		private void ObservableVersionsElementAdded(object aList, int[] aIdx)
		{
			var source = aList as GenericObservableList<GeoGroupVersion>;
			if(source != Entity.ObservableVersions)
			{
				source.ElementAdded -= ObservableVersionsElementAdded;
			}

			foreach(var index in aIdx)
			{
				var viewModel = new GeoGroupVersionViewModel(source[index]);
				Versions.Insert(index, viewModel);
			}
		}

		private void ObservableVersionsElementRemoved(object aList, int[] aIdx, object aObject)
		{
			var source = aList as GenericObservableList<GeoGroupVersion>;
			if(source != Entity.ObservableVersions)
			{
				source.ElementRemoved -= ObservableVersionsElementRemoved;
			}

			foreach(var index in aIdx)
			{
				Versions.RemoveAt(index);
			}
		}

		private void ObservableVersionsElementChanged(object aList, int[] aIdx)
		{
			var source = aList as GenericObservableList<GeoGroupVersion>;
			if(source != Entity.ObservableVersions)
			{
				source.ElementChanged -= ObservableVersionsElementChanged;
			}

			foreach(var index in aIdx)
			{
				var viewModel = new GeoGroupVersionViewModel(source[index]);
				Versions[index] = viewModel;
			}
		}

		#endregion Versions

		#region Save

		public DelegateCommand SaveCommand
		{
			get
			{
				if(_saveCommand == null)
				{
					_saveCommand = new DelegateCommand(SaveAndClose, () => CanSave);
					_saveCommand.CanExecuteChangedWith(this, x => x.CanSave);
				}
				return _saveCommand;
			}
		}

		public bool CanSave => CanEdit;

		#endregion

		#region Cancel

		public DelegateCommand CancelCommand
		{
			get
			{
				if(_cancelCommand == null)
				{
					_cancelCommand = new DelegateCommand(CloseDialog);
				}
				return _cancelCommand;
			}
		}

		private void CloseDialog()
		{
			Close(true, CloseSource.Cancel);
		}

		#endregion

		public override void Dispose()
		{
			_lifetimeScope = null;
			base.Dispose();
		}
	}
}
