using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using GMap.NET;
using NetTopologySuite.Geometries;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Sectors;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Sectors;
using Vodovoz.TempAdapters;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class SectorsViewModel : EntityTabViewModelBase<SectorVersion>
	{
		private readonly ICommonServices _commonServices;
		private readonly IEntityDeleteWorker _entityDeleteWorker;
		private readonly GeometryFactory _geometryFactory;
		private readonly ISectorsRepository _sectorRepository;
		
		private int _personellId;
		private Employee _employee;
		
		private bool _isCreatingNewBorder;
		
		private GenericObservableList<Sector> _sectors;
		private GenericObservableList<SectorVersion> _sectorVersions;
		private List<SectorDeliveryRuleVersion> _sectorDeliveryRuleVersions = new List<SectorDeliveryRuleVersion>();
		private List<SectorWeekDayRulesVersion> _sectorWeekDeliveryRuleVersions = new List<SectorWeekDayRulesVersion>();
		private List<DeliveryPointSectorVersion> _deliveryPointSectorVersions = new List<DeliveryPointSectorVersion>();
		
		private GenericObservableList<Sector> _observableSectorsInSession;
		private GenericObservableList<SectorVersion> _observableSectorVersionsInSession;
		private GenericObservableList<SectorDeliveryRuleVersion> _observableSectorDeliveryRuleVersionsInSession;
		private GenericObservableList<SectorWeekDayRulesVersion> _observableSectorWeekDeliveryRuleVersionsInSession;
		private GenericObservableList<PointLatLng> selectedDistrictBorderVertices;
		private GenericObservableList<PointLatLng> newBorderVertices;
		
		private SectorVersion _selectedSectorVersion;
		private Sector _selectedSector;
		private SectorDeliveryRuleVersion _selectedDeliveryRuleVersion;
		private SectorWeekDayRulesVersion _selectedWeekDayRulesVersion;
		
		public readonly bool CanChangeDistrictWageTypePermissionResult;
		public readonly bool CanEditSector;
		public readonly bool CanDeleteDistrict;
		public readonly bool CanCreateDistrict;
		public readonly bool CanEdit;
		
		public SectorsViewModel(IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory, 
			ICommonServices commonServices,
			IEntityDeleteWorker entityDeleteWorker,
			IEmployeeRepository employeeRepository,
			ISectorsRepository sectorRepository,
			INavigationManager navigation = null) : base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			_entityDeleteWorker = entityDeleteWorker ?? throw new ArgumentNullException(nameof(entityDeleteWorker));
			_sectorRepository = sectorRepository ?? throw new ArgumentNullException(nameof(sectorRepository));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			
			_employee = employeeRepository.GetEmployeeForCurrentUser(UoW);
			_personellId = _employee.Id;
			TabName = "Районы с графиками доставки";
			Entity.LastEditor = _employee;
			
			if(Entity.Id == 0) {
				Entity.Author = _employee;
				Entity.Status = SectorsSetStatus.Draft;
				Entity.LastEditor = _employee;
			}
			
			var permissionResult = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Sector));
			
			CanEditSector = permissionResult.CanUpdate && Entity.Status != SectorsSetStatus.Active;
			CanDeleteDistrict = permissionResult.CanDelete && Entity.Status != SectorsSetStatus.Active;
			CanCreateDistrict = permissionResult.CanCreate && Entity.Status != SectorsSetStatus.Active;
			
			var permissionRes = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(SectorVersion));
			
			CanEdit = permissionRes.CanUpdate && Entity.Status != SectorsSetStatus.Active;
			_geometryFactory = new GeometryFactory(new PrecisionModel(), 3857);

			Sectors = new GenericObservableList<Sector>(UoW.GetAll<Sector>().ToList());
			
			SelectedDistrictBorderVertices = new GenericObservableList<PointLatLng>();
			NewBorderVertices = new GenericObservableList<PointLatLng>();
		}

		#region Свойства
		
		public GenericObservableList<PointLatLng> SelectedDistrictBorderVertices {
			get => selectedDistrictBorderVertices;
			set => SetField(ref selectedDistrictBorderVertices, value);
		}
		
		public GenericObservableList<PointLatLng> NewBorderVertices {
			get => newBorderVertices;
			set => SetField(ref newBorderVertices, value);
		}

		public GenericObservableList<Sector> Sectors
		{
			get => _sectors;
			set => _sectors = value;
		}

		public GenericObservableList<Sector> ObservableSectorsInSession =>
			_observableSectorsInSession ?? (_observableSectorsInSession = new GenericObservableList<Sector>());

		public GenericObservableList<SectorVersion> SectorVersions => SelectedSector.ObservableSectorVersions;
		

		public GenericObservableList<SectorVersion> ObservableSectorVersionsInSession =>
			_observableSectorVersionsInSession ?? (_observableSectorVersionsInSession = new GenericObservableList<SectorVersion>());

		public GenericObservableList<SectorDeliveryRuleVersion> SectorDeliveryRuleVersions => SelectedSector.ObservableSectorDeliveryRuleVersions;

		public GenericObservableList<SectorDeliveryRuleVersion> ObservableSectorDeliveryRuleVersionsInSession =>
			_observableSectorDeliveryRuleVersionsInSession ?? (_observableSectorDeliveryRuleVersionsInSession =
				new GenericObservableList<SectorDeliveryRuleVersion>());

		public GenericObservableList<SectorWeekDayRulesVersion> SectorWeekDeliveryRuleVersions => SelectedSector.ObservableSectorWeekDayRulesVersions;

		public GenericObservableList<SectorWeekDayRulesVersion> ObservableSectorWeekDeliveryRuleVersionsInSession =>
			_observableSectorWeekDeliveryRuleVersionsInSession ?? (_observableSectorWeekDeliveryRuleVersionsInSession =
				new GenericObservableList<SectorWeekDayRulesVersion>());

		public List<DeliveryPointSectorVersion> DeliveryPointSectorVersions => SelectedSector.DeliveryPointSectorVersions;
		
		public bool IsCreatingNewBorder {
			get => _isCreatingNewBorder;
			private set {
				if(value && SelectedSectorVersion == null)
					throw new ArgumentNullException(nameof(SelectedSectorVersion));
				SetField(ref _isCreatingNewBorder, value);
			}
		}

		public SectorVersion SelectedSectorVersion
		{
			get => _selectedSectorVersion;
			set
			{
				if(!SetField(ref _selectedSectorVersion, value))
					return;
				
			}
		}

		public Sector SelectedSector
		{
			get => _selectedSector;
			set
			{
				if(!SetField(ref _selectedSector,value))
					return;
				if(_selectedSector != null)
				{
					OnPropertyChanged();
				}
			}
		}

		public SectorDeliveryRuleVersion SelectedDeliveryRuleVersion
		{
			get => _selectedDeliveryRuleVersion;
			set
			{
				if(!SetField(ref _selectedDeliveryRuleVersion, value))
					return;
				
			}
		}

		public SectorWeekDayRulesVersion SelectedWeekDayRulesVersion
		{
			get => _selectedWeekDayRulesVersion;
			set
			{
				if(!SetField(ref _selectedWeekDayRulesVersion, value))
					return;
			}
		}
		#endregion

		#region Операции над секторами

		private DelegateCommand _addSector;

		public DelegateCommand AddSector => _addSector ?? (_addSector = new DelegateCommand(
			() =>
			{
				var sector = new Sector{DateCreated = DateTime.Today};
				ObservableSectorsInSession.Add(sector);
				Sectors.Add(sector);
				SelectedSector = sector;
			}));

		private DelegateCommand _removeSector;

		public DelegateCommand RemoveSector => _removeSector ?? (_removeSector = new DelegateCommand(() =>
		{
			if(CheckRemoveSectorInSession(SelectedSector))
			{
				ObservableSectorsInSession.Remove(SelectedSector);
				Sectors.Remove(SelectedSector);
			}
		}));
		
		private bool CheckRemoveSectorInSession(Sector sector) => ObservableSectorsInSession.Contains(sector);

		#endregion

		#region Операции над версиями секторов(основные характеристики)

		private DelegateCommand _addSectorVersion;

		public DelegateCommand AddSectorVersion => _addSectorVersion ?? (_addSectorVersion = new DelegateCommand(() =>
		{
			var sectorVersion = SelectedSector.ActiveSectorVersion;
			SectorVersion newVersion;
			if(sectorVersion != null)
			{
				newVersion = sectorVersion.Clone() as SectorVersion;
			}
			else
			{
				newVersion = new SectorVersion {Sector = SelectedSector};
			}
			
			ObservableSectorVersionsInSession.Add(newVersion);
			SectorVersions.Add(newVersion);
		}));

		private bool CheckRemoveSectorVersion(SectorVersion sectorVersion) => ObservableSectorVersionsInSession.Contains(sectorVersion);

		private DelegateCommand _removeSectorVersion;

		public DelegateCommand RemoveSectorVersion => _removeSectorVersion ?? (_removeSectorVersion = new DelegateCommand(
			(() =>
			{
				if(CheckRemoveSectorVersion(SelectedSectorVersion))
				{
					ObservableSectorVersionsInSession.Remove(SelectedSectorVersion);
					SectorVersions.Remove(SelectedSectorVersion);
				}
			})));

		#endregion

		#region Операции над дополнительными характеристиками версии сектора

		private DelegateCommand _addRulesDelivery;

		public DelegateCommand AddRulesDelivery => _addRulesDelivery ?? (_addRulesDelivery = new DelegateCommand(() =>
		{
			var deliveryRuleVersion = new SectorDeliveryRuleVersion();
			ObservableSectorDeliveryRuleVersionsInSession.Add(deliveryRuleVersion);
			SectorDeliveryRuleVersions.Add(deliveryRuleVersion);
		}));

		private DelegateCommand _copyRulesDelivery;

		public DelegateCommand CopyRulesDelivery => _copyRulesDelivery ?? (_copyRulesDelivery = new DelegateCommand(() =>
		{
			var deliveryRuleVersionClone = SelectedDeliveryRuleVersion.Clone() as SectorDeliveryRuleVersion;
			ObservableSectorDeliveryRuleVersionsInSession.Add(deliveryRuleVersionClone);
			SectorDeliveryRuleVersions.Add(deliveryRuleVersionClone);
		}));

		private DelegateCommand _removeRulesDelivery;

		public DelegateCommand RemoveRulesDelivery => _removeRulesDelivery ?? (_removeRulesDelivery = new DelegateCommand(() =>
		{
			if(CheckRemoveRulesDelivery(SelectedDeliveryRuleVersion))
			{
				ObservableSectorDeliveryRuleVersionsInSession.Remove(SelectedDeliveryRuleVersion);
				SectorDeliveryRuleVersions.Remove(SelectedDeliveryRuleVersion);
			}
		}));

		private bool CheckRemoveRulesDelivery(SectorDeliveryRuleVersion sectorDeliveryRuleVersion) =>
			ObservableSectorDeliveryRuleVersionsInSession.Contains(sectorDeliveryRuleVersion);

		private DelegateCommand _addWeekRuleDelivery;

		public DelegateCommand AddWeekRuleDelivery => _addWeekRuleDelivery ?? (_addWeekRuleDelivery = new DelegateCommand(() =>
			{
				var weekRuleDelivery = new SectorWeekDayRulesVersion();
				ObservableSectorWeekDeliveryRuleVersionsInSession.Add(weekRuleDelivery);
				SectorWeekDeliveryRuleVersions.Add(weekRuleDelivery);
			}));

		private DelegateCommand _copyWeekRuleDelivery;

		public DelegateCommand CopyWeekRuleDelivery => _copyWeekRuleDelivery ?? (_copyWeekRuleDelivery = new DelegateCommand(() =>
		{
			var weekRuleDelivery = SelectedWeekDayRulesVersion.Clone() as SectorWeekDayRulesVersion;
			ObservableSectorWeekDeliveryRuleVersionsInSession.Add(weekRuleDelivery);
			SectorWeekDeliveryRuleVersions.Add(weekRuleDelivery);
		}));

		private DelegateCommand _removeWeekRuleDelivery;

		public DelegateCommand RemoveWeekRuleDelivery => _removeWeekRuleDelivery ?? (_removeWeekRuleDelivery = new DelegateCommand(() =>
		{
			if(CheckRemoveWeekRuleDelivery(SelectedWeekDayRulesVersion))
			{
				ObservableSectorWeekDeliveryRuleVersionsInSession.Remove(SelectedWeekDayRulesVersion);
				SectorWeekDeliveryRuleVersions.Remove(SelectedWeekDayRulesVersion);
			}
		}));

		private bool CheckRemoveWeekRuleDelivery(SectorWeekDayRulesVersion sectorWeekDayRulesVersion) =>
			ObservableSectorWeekDeliveryRuleVersionsInSession.Contains(sectorWeekDayRulesVersion);
		
		#endregion

		#region Операции с картой

		private DelegateCommand _createBorderCommand;
		public DelegateCommand CreateBorderCommand => _createBorderCommand ?? (_createBorderCommand = new DelegateCommand(
			() => {
				IsCreatingNewBorder = true;
				NewBorderVertices.Clear();
			},
			() => !IsCreatingNewBorder
		));
		
		private DelegateCommand _removeBorderCommand;
		public DelegateCommand RemoveBorderCommand => _removeBorderCommand ?? (_removeBorderCommand = new DelegateCommand(
			() => {
				SelectedSectorVersion.Polygon = null;
				SelectedDistrictBorderVertices.Clear();
				OnPropertyChanged(nameof(SelectedDistrictBorderVertices));
				OnPropertyChanged(nameof(SelectedSectorVersion));
			},
			() => !IsCreatingNewBorder
		));
		
		private DelegateCommand _confirmNewBorderCommand;
		public DelegateCommand ConfirmNewBorderCommand => _confirmNewBorderCommand ?? (_confirmNewBorderCommand = new DelegateCommand(
			() => {
				if(NewBorderVertices.Count < 3)
					return;
				var closingPoint = NewBorderVertices[0];
				NewBorderVertices.Add(closingPoint);
				SelectedDistrictBorderVertices = new GenericObservableList<PointLatLng>(NewBorderVertices.ToList());
				NewBorderVertices.Clear();
				SelectedSectorVersion.Polygon = _geometryFactory.CreatePolygon(SelectedDistrictBorderVertices.Select(p => new Coordinate(p.Lat,p.Lng)).ToArray());
				IsCreatingNewBorder = false;
			},
			() => IsCreatingNewBorder
		));
		
		private DelegateCommand _cancelNewBorderCommand;
		public DelegateCommand CancelNewBorderCommand => _cancelNewBorderCommand ?? (_cancelNewBorderCommand = new DelegateCommand(
			() => {
				NewBorderVertices.Clear();
				IsCreatingNewBorder = false;
				OnPropertyChanged(nameof(NewBorderVertices));
			},
			() => IsCreatingNewBorder
		));

		private DelegateCommand<PointLatLng> _addNewVertexCommand;
		public DelegateCommand<PointLatLng> AddNewVertexCommand => _addNewVertexCommand ?? (_addNewVertexCommand = new DelegateCommand<PointLatLng>(
			point => {
				NewBorderVertices.Add(point);
				OnPropertyChanged(nameof(NewBorderVertices));
			},
			point => IsCreatingNewBorder
		));
		
		private DelegateCommand<PointLatLng> _removeNewBorderVerteCommand;
		public DelegateCommand<PointLatLng> RemoveNewBorderVertexCommand => _removeNewBorderVerteCommand ?? (_removeNewBorderVerteCommand = new DelegateCommand<PointLatLng>(
			point => {
				NewBorderVertices.Remove(point);
				OnPropertyChanged(nameof(NewBorderVertices));
			},
			point => IsCreatingNewBorder && !point.IsEmpty
		));
		
		#endregion

		#region Сводка

		

		#endregion

		#region На активацию

		public void OnActivation(SectorVersion sectorVersion)
		{
			sectorVersion.Status = SectorsSetStatus.OnActivation;
			var draftSectors = SectorVersions.Concat(ObservableSectorVersionsInSession).Where(x => x.Status == SectorsSetStatus.OnActivation
			                                                                             && x.LastEditor.Id == _personellId && x.Sector.Id == sectorVersion.Sector.Id).ToList();
			for(int i = 0; i < draftSectors.Count; i++)
			{
				draftSectors[i].Status = SectorsSetStatus.Draft;
			}
			
			SaveUow();
		}

		#endregion

		#region Вернуть в черновик

		public void DraftSectors(SectorVersion sectorVersion)
		{
			sectorVersion.Status = SectorsSetStatus.Draft;
			
			SaveUow();
		}

		#endregion

		#region Активировать

		public void ActivateSectors(SectorVersion sectorVersion)
		{
			var activeSectorVersion = SectorVersions.Concat(ObservableSectorVersionsInSession)
				.SingleOrDefault(x => x.Status == SectorsSetStatus.Active && x.Sector.Id == sectorVersion.Sector.Id);
			if(activeSectorVersion != null)
			{
				activeSectorVersion.Status = SectorsSetStatus.Closed;
				if(!sectorVersion.Polygon.EqualsExact(activeSectorVersion.Polygon))
				{
					DeliveryPointSectorVersions.ForEach(x => { x.FindAndAssociateDistrict(UoW, _sectorRepository); });
				}
			}
			sectorVersion.Status = SectorsSetStatus.Active;
			
			SaveUow();
		}

		#endregion
		
		#region Сохранение

		private void SaveUow()
		{
			UoW.Save();
			ObservableSectorVersionsInSession.Clear();
			ObservableSectorWeekDeliveryRuleVersionsInSession.Clear();
			ObservableSectorDeliveryRuleVersionsInSession.Clear();
		}

		#endregion
	}
}