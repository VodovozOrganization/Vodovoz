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
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.Sectors;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Sectors;
using Vodovoz.TempAdapters;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class SectorsViewModel : EntityTabViewModelBase<SectorVersion>
	{
		#region Поля
		
		private readonly ICommonServices _commonServices;
		private readonly IEntityDeleteWorker _entityDeleteWorker;
		private readonly GeometryFactory _geometryFactory;
		private readonly ISectorsRepository _sectorRepository;
		
		private int _personellId;
		private bool _isCreatingNewBorder;

		private DateTime? _startDateSector;
		private DateTime? _endDateSector;
		private DateTime? _startDateSectorVersion;
		private DateTime? _endDateSectorVersion;
		private DateTime? _startDateSectorDeliveryRule;
		private DateTime? _endDateSectorDeliveryRule;
		private DateTime? _startDateSectorDaySchedule;
		private DateTime? _endDateSectorDaySchedule;
		private DateTime? _startDateSectorDayDeliveryRule;
		private DateTime? _endDateSectorDayDeliveryRule;
		private Employee _employee;
		
		private GenericObservableList<Sector> _sectors;
		private GenericObservableList<SectorVersion> _sectorVersions;
		private List<SectorDeliveryRuleVersion> _sectorDeliveryRuleVersions = new List<SectorDeliveryRuleVersion>();
		private List<SectorWeekDayScheduleVersion> _sectorWeekDeliveryRuleVersions = new List<SectorWeekDayScheduleVersion>();
		private List<DeliveryPointSectorVersion> _deliveryPointSectorVersions = new List<DeliveryPointSectorVersion>();
		
		private GenericObservableList<Sector> _observableSectorsInSession;
		private GenericObservableList<SectorVersion> _observableSectorVersionsInSession;
		private GenericObservableList<SectorDeliveryRuleVersion> _observableSectorDeliveryRuleVersionsInSession;
		private GenericObservableList<SectorWeekDayScheduleVersion> _observableSectorWeekDayScheduleVersionsInSession;
		private GenericObservableList<DeliveryScheduleRestriction> _observableSectorDayDeliveryRestrictions;
		private GenericObservableList<SectorWeekDayDeliveryRuleVersion> _observableSectorWeekDayDeliveryRuleVersionsInSession;
		private GenericObservableList<PointLatLng> _selectedDistrictBorderVertices;
		private GenericObservableList<PointLatLng> _newBorderVertices;
		
		private SectorVersion _selectedSectorVersion;
		private Sector _selectedSector;
		private SectorDeliveryRuleVersion _selectedDeliveryRuleVersion;
		private SectorWeekDayDeliveryRule _selectedWeekDayDeliveryRule;
		private SectorWeekDayScheduleVersion _selectedWeekDayScheduleVersion;
		private SectorWeekDayDeliveryRuleVersion _selectedWeekDayDeliveryRuleVersion;
		private CommonDistrictRuleItem _selectedCommonDistrictRuleItem;
		private WeekDayDistrictRuleItem _selectedWeekDayDistrictRuleItem;
		private DeliveryScheduleRestriction _selectedScheduleRestriction;
		
		public readonly bool CanChangeDistrictWageTypePermissionResult;
		public readonly bool CanEditSector;
		public readonly bool CanDeleteDistrict;
		public readonly bool CanCreateDistrict;
		public readonly bool CanEdit;

		#endregion
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

		#region Даты

		public DateTime? StartDateSector
		{
			get => _startDateSector;
			set => _startDateSector = value;
		}

		public DateTime? EndDateSector
		{
			get => _endDateSector;
			set => _endDateSector = value;
		}

		public DateTime? StartDateSectorVersion
		{
			get => _startDateSectorVersion;
			set => _startDateSectorVersion = value;
		}

		public DateTime? EndDateSectorVersion
		{
			get => _endDateSectorVersion;
			set => _endDateSectorVersion = value;
		}

		public DateTime? StartDateSectorDeliveryRule
		{
			get => _startDateSectorDeliveryRule;
			set => _startDateSectorDeliveryRule = value;
		}

		public DateTime? EndDateSectorDeliveryRule
		{
			get => _endDateSectorDeliveryRule;
			set => _endDateSectorDeliveryRule = value;
		}

		public DateTime? StartDateSectorDayDeliveryRule
		{
			get => _startDateSectorDayDeliveryRule;
			set => _startDateSectorDayDeliveryRule = value;
		}

		public DateTime? EndDateSectorDayDeliveryRule
		{
			get => _endDateSectorDayDeliveryRule;
			set => _endDateSectorDayDeliveryRule = value;
		}

		public DateTime? StartDateSectorDaySchedule
		{
			get => _startDateSectorDaySchedule;
			set => _startDateSectorDaySchedule = value;
		}

		public DateTime? EndDateSectorDaySchedule
		{
			get => _endDateSectorDaySchedule;
			set => _endDateSectorDaySchedule = value;
		}
		
		#endregion

		private WeekDayName? selectedWeekDayName;
		public WeekDayName? SelectedWeekDayName {
			get => selectedWeekDayName;
			set {
				if(SetField(ref selectedWeekDayName, value)) {
					if(SelectedWeekDayName != null) {
						if(SelectedWeekDayScheduleVersion != null)
							OnPropertyChanged(nameof(ObservableSectorDayDeliveryRestrictions));
						if(SelectedWeekDayDeliveryRuleVersion != null)
							OnPropertyChanged(nameof(ObservableSectorDeliveryRules));
					}
				}
			}
		}

		#region Операции над секторами
		public GenericObservableList<Sector> Sectors
		{
			get => _sectors;
			set => _sectors = value;
		}

		public GenericObservableList<Sector> ObservableSectorsInSession =>
			_observableSectorsInSession ?? (_observableSectorsInSession = new GenericObservableList<Sector>());
		
		public Sector SelectedSector
		{
			get => _selectedSector;
			set
			{
				if(!SetField(ref _selectedSector,value))
					return;
				if(_selectedSector != null)
				{
					OnPropertyChanged(nameof(SectorVersions));
					OnPropertyChanged(nameof(ObservableSectorDeliveryRuleVersions));
					OnPropertyChanged(nameof(ObservableSectorWeekDayScheduleVersions));
					OnPropertyChanged(nameof(ObservableSectorWeekDeliveryRuleVersions));
				}
			}
		}

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
		
		public GenericObservableList<SectorVersion> SectorVersions => SelectedSector.ObservableSectorVersions ?? (_sectorVersions = new GenericObservableList<SectorVersion>());
		
		public GenericObservableList<SectorVersion> ObservableSectorVersionsInSession =>
			_observableSectorVersionsInSession ?? (_observableSectorVersionsInSession = new GenericObservableList<SectorVersion>());
		
		public SectorVersion SelectedSectorVersion
		{
			get => _selectedSectorVersion;
			set => SetField(ref _selectedSectorVersion, value);
		}

		private DelegateCommand _addSectorVersion;

		public DelegateCommand AddSectorVersion => _addSectorVersion ?? (_addSectorVersion = new DelegateCommand(() =>
		{
			var sectorVersion = SelectedSector.ActiveSectorVersion;
			SectorVersion newVersion;
			if(sectorVersion != null)
				newVersion = sectorVersion.Clone() as SectorVersion;
			else
				newVersion = new SectorVersion {Sector = SelectedSector};

			if(StartDateSectorVersion.HasValue)
				newVersion.StartDate = StartDateSectorVersion.Value;
			if(EndDateSectorVersion.HasValue)
				newVersion.EndDate = EndDateSectorVersion.Value;
			
			SelectedSectorVersion = newVersion;
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
					SelectedSectorVersion = null;
				}
			})));

		#endregion

		#region Операции над обычными графиками доставки районов

		public GenericObservableList<SectorDeliveryRuleVersion> ObservableSectorDeliveryRuleVersions => SelectedSector.ObservableSectorDeliveryRuleVersions;

		public GenericObservableList<SectorDeliveryRuleVersion> ObservableSectorDeliveryRuleVersionsInSession =>
			_observableSectorDeliveryRuleVersionsInSession ?? (_observableSectorDeliveryRuleVersionsInSession =
				new GenericObservableList<SectorDeliveryRuleVersion>());
		
		public SectorDeliveryRuleVersion SelectedDeliveryRuleVersion
		{
			get => _selectedDeliveryRuleVersion;
			set
			{
				if(!SetField(ref _selectedDeliveryRuleVersion, value))
					return;
				if(value != null)
					OnPropertyChanged(nameof(ObservableCommonDistrictRuleItems));
			}
		}

		public GenericObservableList<CommonDistrictRuleItem> ObservableCommonDistrictRuleItems => SelectedDeliveryRuleVersion.ObservableCommonDistrictRuleItems;

		public CommonDistrictRuleItem SelectedCommonDistrictRuleItem
		{
			get => _selectedCommonDistrictRuleItem;
			set
			{
				if(!SetField(ref _selectedCommonDistrictRuleItem, value))
					return;
				
				OnPropertyChanged(nameof(ObservableCommonDistrictRuleItems));
			}
		}

		private DelegateCommand _addRulesDelivery;

		public DelegateCommand AddRulesDelivery => _addRulesDelivery ?? (_addRulesDelivery = new DelegateCommand(() =>
		{
			var deliveryRuleVersion = new SectorDeliveryRuleVersion{Sector = SelectedSector};
			SelectedDeliveryRuleVersion = deliveryRuleVersion;
			ObservableSectorDeliveryRuleVersionsInSession.Add(deliveryRuleVersion);
			ObservableSectorDeliveryRuleVersions.Add(deliveryRuleVersion);
		}));

		private DelegateCommand _removeRulesDelivery;

		public DelegateCommand RemoveRulesDelivery => _removeRulesDelivery ?? (_removeRulesDelivery = new DelegateCommand(() =>
		{
			if(CheckRemoveRulesDelivery(SelectedDeliveryRuleVersion))
			{
				ObservableSectorDeliveryRuleVersionsInSession.Remove(SelectedDeliveryRuleVersion);
				ObservableSectorDeliveryRuleVersions.Remove(SelectedDeliveryRuleVersion);
				SelectedDeliveryRuleVersion = null;
			}
		}));
		
		private DelegateCommand _copyRulesDelivery;

		public DelegateCommand CopyRulesDelivery => _copyRulesDelivery ?? (_copyRulesDelivery = new DelegateCommand(() =>
		{
			var deliveryRuleVersionClone = SelectedDeliveryRuleVersion.Clone() as SectorDeliveryRuleVersion;
			SelectedDeliveryRuleVersion = deliveryRuleVersionClone;
			ObservableSectorDeliveryRuleVersionsInSession.Add(deliveryRuleVersionClone);
			ObservableSectorDeliveryRuleVersions.Add(deliveryRuleVersionClone);
		}));

		private bool CheckRemoveRulesDelivery(SectorDeliveryRuleVersion sectorDeliveryRuleVersion) =>
			ObservableSectorDeliveryRuleVersionsInSession.Contains(sectorDeliveryRuleVersion);
		
		private DelegateCommand _addCommonDistrictRule;

		public DelegateCommand AddCommonDistrictRule => _addCommonDistrictRule ?? (_addCommonDistrictRule = new DelegateCommand(
			() =>
			{
				var commonDistrictRule = new CommonDistrictRuleItem{ Sector = SelectedDeliveryRuleVersion.Sector};
				ObservableCommonDistrictRuleItems.Add(commonDistrictRule);
				SelectedCommonDistrictRuleItem = commonDistrictRule;
			}));

		private DelegateCommand _removeCommonDistrictRule;

		public DelegateCommand RemoveCommonDistrictRule => _removeCommonDistrictRule ?? (_removeCommonDistrictRule = new DelegateCommand(
			() =>
			{
				ObservableCommonDistrictRuleItems.Remove(SelectedCommonDistrictRuleItem);
				SelectedCommonDistrictRuleItem = null;
			}));
		
		#endregion

		#region Операции над графиками доставки района по дням

		public GenericObservableList<SectorWeekDayScheduleVersion> ObservableSectorWeekDayScheduleVersions =>
			SelectedSector.ObservableSectorWeekDaySchedulesVersions;

		public GenericObservableList<SectorWeekDayScheduleVersion> ObservableSectorWeekDayScheduleVersionsInSession =>
			_observableSectorWeekDayScheduleVersionsInSession ?? (_observableSectorWeekDayScheduleVersionsInSession =
				new GenericObservableList<SectorWeekDayScheduleVersion>());
		
		public SectorWeekDayScheduleVersion SelectedWeekDayScheduleVersion
		{
			get => _selectedWeekDayScheduleVersion;
			set
			{
				if(!SetField(ref _selectedWeekDayScheduleVersion, value))
					return;

				if(value != null)
					OnPropertyChanged(nameof(ObservableSectorDayDeliveryRestrictions));
			}
		}

		public GenericObservableList<DeliveryScheduleRestriction> ObservableSectorDayDeliveryRestrictions
		{
			get
			{
				if(SelectedWeekDayScheduleVersion.ObservableSectorSchedules.Where(x => x.WeekDay == SelectedWeekDayName.Value).Any())
					return new GenericObservableList<DeliveryScheduleRestriction>(SelectedWeekDayScheduleVersion.ObservableSectorSchedules
						.Where(x => x.WeekDay == SelectedWeekDayName.Value).ToList());
				return new GenericObservableList<DeliveryScheduleRestriction>();
			}
		}

		public GenericObservableList<DeliveryScheduleRestriction> ObservableSectorDayDeliveryRestrictionsInSession =>
			_observableSectorDayDeliveryRestrictions ?? ( _observableSectorDayDeliveryRestrictions = new GenericObservableList<DeliveryScheduleRestriction>());
		
		public DeliveryScheduleRestriction SelectedScheduleRestriction {
			get => _selectedScheduleRestriction;
			set => SetField(ref _selectedScheduleRestriction, value);
		}

		private DelegateCommand _addSectorWeekDayScheduleVersion;

		public DelegateCommand AddSectorWeekDayScheduleVersion => _addSectorWeekDayScheduleVersion ?? (_addSectorWeekDayScheduleVersion =
			new DelegateCommand(() =>
			{
				var dayScheduleVersion = new SectorWeekDayScheduleVersion {Sector = SelectedSector, Status = SectorsSetStatus.Draft};
				SelectedWeekDayScheduleVersion = dayScheduleVersion;
				ObservableSectorWeekDayScheduleVersionsInSession.Add(dayScheduleVersion);
				ObservableSectorWeekDayScheduleVersions.Add(dayScheduleVersion);
			}));
		
		private DelegateCommand _removeSectorWeekDaySchedule;

		public DelegateCommand RemoveSectorWeekDayScheduleVersion => _removeSectorWeekDaySchedule ?? (_removeSectorWeekDaySchedule = new DelegateCommand(
			() =>
			{
				if(CheckRemoveSectorWeekDaySchedule(SelectedWeekDayScheduleVersion))
				{
					ObservableSectorWeekDayScheduleVersionsInSession.Remove(SelectedWeekDayScheduleVersion);
					ObservableSectorWeekDayScheduleVersions.Remove(SelectedWeekDayScheduleVersion);
					SelectedWeekDayScheduleVersion = null;
				}
			}));

		private bool CheckRemoveSectorWeekDaySchedule(SectorWeekDayScheduleVersion sectorWeekDayScheduleVersion) =>
			ObservableSectorWeekDayScheduleVersionsInSession.Contains(sectorWeekDayScheduleVersion);

		private DelegateCommand _copySectorWeekDaySchedule;

		public DelegateCommand CopySectorWeekDaySchedule => _copySectorWeekDaySchedule ?? (_copySectorWeekDaySchedule = new DelegateCommand(
			() =>
			{
				var dayScheduleVersionClone = SelectedWeekDayScheduleVersion.Clone() as SectorWeekDayScheduleVersion;
				SelectedWeekDayScheduleVersion = dayScheduleVersionClone;
				ObservableSectorWeekDayScheduleVersionsInSession.Add(dayScheduleVersionClone);
				ObservableSectorWeekDayScheduleVersions.Add(dayScheduleVersionClone);
			}));
		
		private DelegateCommand<IEnumerable<DeliverySchedule>> addScheduleRestrictionCommand;
		public DelegateCommand<IEnumerable<DeliverySchedule>> AddScheduleRestrictionCommand => addScheduleRestrictionCommand ?? (addScheduleRestrictionCommand = new DelegateCommand<IEnumerable<DeliverySchedule>>(
			schedules => {
				foreach(var schedule in schedules)
				{
					if(SelectedWeekDayName.HasValue &&
					   ObservableSectorDayDeliveryRestrictions.All(x => x.DeliverySchedule.Id != schedule.Id))
					{
						var restriction = new DeliveryScheduleRestriction
							{WeekDay = SelectedWeekDayName.Value, DeliverySchedule = schedule};
						SelectedWeekDayScheduleVersion.ObservableSectorSchedules.Add(restriction);
						ObservableSectorDayDeliveryRestrictionsInSession.Add(restriction);
						OnPropertyChanged(nameof(ObservableSectorDayDeliveryRestrictions));
					}
				}
			},
			schedules => schedules.Any()
		));
		
		private DelegateCommand removeScheduleRestrictionCommand;
		public DelegateCommand RemoveScheduleRestrictionCommand => removeScheduleRestrictionCommand ?? (removeScheduleRestrictionCommand = new DelegateCommand(
			() => {
				ObservableSectorDayDeliveryRestrictions.Remove(SelectedScheduleRestriction);
				OnPropertyChanged(nameof(ObservableSectorDayDeliveryRestrictions));
			},
			() => SelectedScheduleRestriction != null
		));
		
		private DelegateCommand removeAcceptBeforeCommand;
		public DelegateCommand RemoveAcceptBeforeCommand => removeAcceptBeforeCommand ?? (removeAcceptBeforeCommand = new DelegateCommand(
			() => {
				SelectedScheduleRestriction.AcceptBefore = null;
			},
			() => SelectedScheduleRestriction != null
		));
		
		#endregion

		#region WeekDayDistrictRuleItem 
		
		public GenericObservableList<WeekDayDistrictRuleItem> ObservableSectorDeliveryRules
		{
			get
			{
				if(SelectedWeekDayDeliveryRuleVersion.ObservableWeekDayDistrictRules.Where(x=>x.WeekDay == SelectedWeekDayName.Value).Any())
					return new GenericObservableList<WeekDayDistrictRuleItem>(SelectedWeekDayDeliveryRuleVersion.ObservableWeekDayDistrictRules.Where(x=>x.WeekDay == SelectedWeekDayName.Value).ToList());
				return new GenericObservableList<WeekDayDistrictRuleItem>();
			}
		}
		
		public WeekDayDistrictRuleItem SelectedWeekDayDistrictRuleItem
		{
			get => _selectedWeekDayDistrictRuleItem;
			set
			{
				if(!SetField(ref _selectedWeekDayDistrictRuleItem, value))
					return;
				if(value != null)
					OnPropertyChanged(nameof(ObservableSectorDeliveryRules));
			}
		}
		public GenericObservableList<SectorWeekDayDeliveryRuleVersion> ObservableSectorWeekDeliveryRuleVersions =>
			SelectedSector.ObservableSectorWeekDayDeliveryRuleVersions;
		
		public GenericObservableList<SectorWeekDayDeliveryRuleVersion> ObservableSectorWeekDeliveryRuleVersionsInSession =>
			_observableSectorWeekDayDeliveryRuleVersionsInSession ?? (_observableSectorWeekDayDeliveryRuleVersionsInSession =
				new GenericObservableList<SectorWeekDayDeliveryRuleVersion>());

		public SectorWeekDayDeliveryRuleVersion SelectedWeekDayDeliveryRuleVersion
		{
			get => _selectedWeekDayDeliveryRuleVersion;
			set
			{
				if(!SetField(ref _selectedWeekDayDeliveryRuleVersion, value))
					return;
				if(value != null)
					OnPropertyChanged(nameof(ObservableSectorDeliveryRules));
			}
		}
		
		private DelegateCommand _addWeekDayDistrictRule;

		public DelegateCommand AddWeekDayDistrictRule => _addWeekDayDistrictRule ?? (_addWeekDayDistrictRule = new DelegateCommand(
			() =>
			{
				var weekDayDistrictRuleItem = new WeekDayDistrictRuleItem{ Sector = SelectedSector};
				SelectedWeekDayDeliveryRuleVersion.ObservableWeekDayDistrictRules.Add(weekDayDistrictRuleItem);
				//ObservableSectorDeliveryRulesInSession.Add(weekDayDistrictRuleItem);
				SelectedWeekDayDistrictRuleItem = weekDayDistrictRuleItem;
			}));

		private DelegateCommand _removeWeekDayDistrictRule;

		public DelegateCommand RemoveWeekDayDistrictRule => _removeWeekDayDistrictRule ?? (_removeWeekDayDistrictRule = new DelegateCommand(
			() =>
			{
				ObservableSectorDeliveryRules.Remove(SelectedWeekDayDistrictRuleItem);
				SelectedWeekDayDistrictRuleItem = null;
			}));

		private DelegateCommand _addWeekDayDeliveryRule;

		public DelegateCommand AddWeekDayDeliveryRule => _addWeekDayDeliveryRule ?? (_addWeekDayDeliveryRule = new DelegateCommand(
			() =>
			{
				var sectorWeekDayDeliveryRuleVersion = new SectorWeekDayDeliveryRuleVersion
					{ Sector = SelectedSector, Status = SectorsSetStatus.Draft};
				if(StartDateSectorDayDeliveryRule.HasValue)
					sectorWeekDayDeliveryRuleVersion.StartDate = StartDateSectorDayDeliveryRule.Value;
				if(EndDateSectorDayDeliveryRule.HasValue)
					sectorWeekDayDeliveryRuleVersion.EndDate = EndDateSectorDayDeliveryRule.Value;
				
				ObservableSectorWeekDeliveryRuleVersionsInSession.Add(sectorWeekDayDeliveryRuleVersion);
				ObservableSectorWeekDeliveryRuleVersions.Add(sectorWeekDayDeliveryRuleVersion);
				SelectedWeekDayDeliveryRuleVersion = sectorWeekDayDeliveryRuleVersion;
			}));

		private DelegateCommand _removeWeekDayDeliveryRule;

		public DelegateCommand RemoveWeekDayDeliveryRule => _removeWeekDayDeliveryRule ?? (_removeWeekDayDeliveryRule = new DelegateCommand(
			() =>
			{
				ObservableSectorWeekDeliveryRuleVersions.Remove(SelectedWeekDayDeliveryRuleVersion);
				SelectedWeekDayDeliveryRuleVersion = null;
			}));

		private DelegateCommand<AcceptBefore> addAcceptBeforeCommand;
		public DelegateCommand<AcceptBefore> AddAcceptBeforeCommand => addAcceptBeforeCommand ?? (addAcceptBeforeCommand = new DelegateCommand<AcceptBefore>(
			acceptBefore => {
				SelectedScheduleRestriction.AcceptBefore = acceptBefore;
			},
			acceptBefore => acceptBefore != null && SelectedWeekDayScheduleVersion != null
		));
		
		#endregion

		#region Операции с картой
		public GenericObservableList<PointLatLng> NewBorderVertices {
			get => _newBorderVertices;
			set => SetField(ref _newBorderVertices, value);
		}
		
		public GenericObservableList<PointLatLng> SelectedDistrictBorderVertices {
			get => _selectedDistrictBorderVertices;
			set => SetField(ref _selectedDistrictBorderVertices, value);
		}

		public List<DeliveryPointSectorVersion> DeliveryPointSectorVersions => SelectedSector.DeliveryPointSectorVersions;
		
		public bool IsCreatingNewBorder {
			get => _isCreatingNewBorder;
			private set {
				if(value && SelectedSectorVersion == null)
					throw new ArgumentNullException(nameof(SelectedSectorVersion));
				SetField(ref _isCreatingNewBorder, value);
			}
		}

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

		private DelegateCommand _activateSector;

		public DelegateCommand ActivateSector => _activateSector ?? (_activateSector = new DelegateCommand(() =>
		{
			var activeSectorVersion = SectorVersions.Concat(ObservableSectorVersionsInSession)
				.SingleOrDefault(x => x.Status == SectorsSetStatus.Active && x.Sector.Id == SelectedSectorVersion.Sector.Id);
			if(activeSectorVersion != null)
			{
				activeSectorVersion.Status = SectorsSetStatus.Closed;
				if(!SelectedSectorVersion.Polygon.EqualsExact(activeSectorVersion.Polygon))
				{
					DeliveryPointSectorVersions.ForEach(x => { x.FindAndAssociateDistrict(UoW, _sectorRepository); });
				}
			}

			var activeSectorDeliveryRule = ObservableSectorDeliveryRuleVersions.Concat(ObservableSectorDeliveryRuleVersionsInSession)
				.SingleOrDefault(x => x.Status == SectorsSetStatus.Active && x.Sector.Id == SelectedDeliveryRuleVersion.Sector.Id);
			if(activeSectorDeliveryRule != null)
			{
				activeSectorDeliveryRule.Status = SectorsSetStatus.Closed;
			}

			// var activeSectorWeekDayRule = SectorWeekDeliveryRuleVersions.Concat(ObservableSectorWeekDeliveryRuleVersionsInSession)
			// 	.SingleOrDefault(x => x.Status == SectorsSetStatus.Active && x.Sector.Id == SelectedWeekDayRulesVersion.Sector.Id);
			// if(activeSectorWeekDayRule != null)
			// {
			// 	activeSectorWeekDayRule.Status = SectorsSetStatus.Closed;
			// }
			//
			// SelectedWeekDayRulesVersion.Status = SectorsSetStatus.Active;
			// SelectedDeliveryRuleVersion.Status = SectorsSetStatus.Active;
			// SelectedSectorVersion.Status = SectorsSetStatus.Active;
		}));

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