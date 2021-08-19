using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using GMap.NET;
using MoreLinq;
using NetTopologySuite.Geometries;
using QS.Commands;
using QS.Dialog;
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
		private GenericObservableList<SectorNodeViewModel> _sectorNodeViewModels;
		
		private GenericObservableList<Sector> _observableSectorsInSession;
		private GenericObservableList<SectorVersion> _observableSectorVersionsInSession;
		private GenericObservableList<SectorDeliveryRuleVersion> _observableSectorDeliveryRuleVersionsInSession;
		private GenericObservableList<SectorWeekDayScheduleVersion> _observableSectorWeekDayScheduleVersionsInSession;
		private GenericObservableList<DeliveryScheduleRestriction> _observableSectorDayDeliveryRestrictions;
		private GenericObservableList<SectorWeekDayDeliveryRuleVersion> _observableSectorWeekDayDeliveryRuleVersionsInSession;
		private GenericObservableList<PointLatLng> _selectedDistrictBorderVertices;
		private GenericObservableList<PointLatLng> _newBorderVertices;
		
		private SectorVersion _selectedSectorVersion;
		private SectorNodeViewModel _selectedSectorNodeViewModel;
		private SectorDeliveryRuleVersion _selectedDeliveryRuleVersion;
		private SectorWeekDayScheduleVersion _selectedWeekDayScheduleVersion;
		private SectorWeekDayDeliveryRuleVersion _selectedWeekDayDeliveryRuleVersion;
		private CommonDistrictRuleItem _selectedCommonDistrictRuleItem;
		private WeekDayDistrictRuleItem _selectedWeekDayDistrictRuleItem;
		private DeliveryScheduleRestriction _selectedScheduleRestriction;
		
		public readonly bool CanChangeSectorWageTypePermissionResult;
		public readonly bool CanEditSector;
		public readonly bool CanDeleteSector;
		public readonly bool CanCreateSector;
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
			
			CanChangeSectorWageTypePermissionResult = commonServices.CurrentPermissionService.ValidatePresetPermission("can_change_district_wage_type");

			_employee = employeeRepository.GetEmployeeForCurrentUser(UoW);
			_personellId = _employee.Id;
			TabName = "Районы с графиками доставки";
			
			if(Entity.Id == 0)
				Entity.Status = SectorsSetStatus.Draft;

			var permissionResult = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Sector));
			
			CanEditSector = permissionResult.CanUpdate && Entity.Status != SectorsSetStatus.Active;
			CanDeleteSector = permissionResult.CanDelete && Entity.Status != SectorsSetStatus.Active;
			CanCreateSector = permissionResult.CanCreate && Entity.Status != SectorsSetStatus.Active;
			
			var permissionRes = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(SectorVersion));
			
			CanEdit = permissionRes.CanUpdate && Entity.Status != SectorsSetStatus.Active;
			_geometryFactory = new GeometryFactory(new PrecisionModel(), 3857);

			Sectors = new GenericObservableList<Sector>(UoW.GetAll<Sector>().ToList());
			
			Sectors.ForEach(x =>
			{
				var sectorVersionForNode = x.GetActiveSectorVersion() ??
				                   x.SectorVersions.SingleOrDefault(y => y.Status == SectorsSetStatus.OnActivation) ??
				                   x.SectorVersions.LastOrDefault(z => z.Status == SectorsSetStatus.Draft);
				ObservableSectorNodeViewModels.Add(new SectorNodeViewModel(x.Id, x.DateCreated, sectorVersionForNode != null ? sectorVersionForNode.SectorName : ""));
			});
			
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
			set
			{
				_startDateSectorVersion = value;
				if(SelectedSectorVersion != null)
					SelectedSectorVersion.StartDate = _startDateSectorVersion;
			}
		}

		public DateTime? StartDateSectorDeliveryRule
		{
			get => _startDateSectorDeliveryRule;
			set
			{
				_startDateSectorDeliveryRule = value;
				if(SelectedDeliveryRuleVersion != null)
					SelectedDeliveryRuleVersion.StartDate = _startDateSectorDeliveryRule;
			}
		}

		public DateTime? StartDateSectorDayDeliveryRule
		{
			get => _startDateSectorDayDeliveryRule;
			set
			{
				_startDateSectorDayDeliveryRule = value;
				if(SelectedWeekDayDeliveryRuleVersion != null)
					SelectedWeekDayDeliveryRuleVersion.StartDate = _startDateSectorDayDeliveryRule;
			}
		}

		public DateTime? StartDateSectorDaySchedule
		{
			get => _startDateSectorDaySchedule;
			set
			{
				_startDateSectorDaySchedule = value;
				if(SelectedWeekDayScheduleVersion != null)
					SelectedWeekDayScheduleVersion.StartDate = _startDateSectorDaySchedule;
			}
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

		public GenericObservableList<SectorNodeViewModel> ObservableSectorNodeViewModels => _sectorNodeViewModels ?? (_sectorNodeViewModels = new GenericObservableList<SectorNodeViewModel>());

		public GenericObservableList<Sector> ObservableSectorsInSession =>
			_observableSectorsInSession ?? (_observableSectorsInSession = new GenericObservableList<Sector>());
		
		public SectorNodeViewModel SelectedSectorNodeViewModel
		{
			get => _selectedSectorNodeViewModel;
			set
			{
				if(!SetField(ref _selectedSectorNodeViewModel,value))
					return;
				if(_selectedSectorNodeViewModel != null)
				{
					OnPropertyChanged(nameof(SectorVersions));
					OnPropertyChanged(nameof(ObservableSectorDeliveryRuleVersions));
					OnPropertyChanged(nameof(ObservableSectorWeekDayScheduleVersions));
					OnPropertyChanged(nameof(ObservableSectorWeekDeliveryRuleVersions));
					OnPropertyChanged(nameof(ObservableCommonDistrictRuleItems));
					OnPropertyChanged(nameof(ObservableSectorDayDeliveryRestrictions));
					OnPropertyChanged(nameof(ObservableSectorDeliveryRules));
				}
			}
		}

		public Sector SelectedSector
		{
			get
			{
				if(Sectors.Any())
					return Sectors.SingleOrDefault(x => x.Id == SelectedSectorNodeViewModel.Id);
				return null;
			}
		}


		private DelegateCommand _addSector;

		public DelegateCommand AddSector => _addSector ?? (_addSector = new DelegateCommand(
			() =>
			{
				var sector = new Sector{DateCreated = DateTime.Today};
				var sectorNodeViewModel = new SectorNodeViewModel(sector);
				ObservableSectorNodeViewModels.Add(sectorNodeViewModel);
				ObservableSectorsInSession.Add(sector);
				Sectors.Add(sector);
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
			set
			{
				if(!SetField(ref _selectedSectorVersion, value))
					return;

				SelectedDistrictBorderVertices.Clear();
				NewBorderVertices.Clear();
				
				if(SelectedSectorVersion?.Polygon != null)
				{
					SelectedDistrictBorderVertices = new GenericObservableList<PointLatLng>(SelectedSectorVersion.Polygon.Coordinates.Select(x => new PointLatLng(x.X, x.Y)).ToList());
					IsCreatingNewBorder = false;
				}
			}
		}

		private DelegateCommand _addSectorVersion;

		public DelegateCommand AddSectorVersion => _addSectorVersion ?? (_addSectorVersion = new DelegateCommand(() =>
		{
			var sectorVersion = SelectedSector.GetActiveSectorVersion();
			SectorVersion newVersion;
			if(sectorVersion != null)
				newVersion = sectorVersion.Clone() as SectorVersion;
			else
				newVersion = new SectorVersion {Sector = SelectedSector};

			if(StartDateSectorVersion.HasValue)
				newVersion.StartDate = StartDateSectorVersion.Value;
			
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

		public GenericObservableList<CommonDistrictRuleItem> ObservableCommonDistrictRuleItems
		{
			get
			{
				if(SelectedDeliveryRuleVersion != null)
					return SelectedDeliveryRuleVersion.ObservableCommonDistrictRuleItems;
				return new GenericObservableList<CommonDistrictRuleItem>();
			}
		}

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
			if(StartDateSectorDeliveryRule.HasValue)
				deliveryRuleVersion.StartDate = StartDateSectorDeliveryRule.Value;
			
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
				var commonDistrictRule = new CommonDistrictRuleItem{ SectorDeliveryRuleVersion = SelectedDeliveryRuleVersion};
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
				if(SelectedWeekDayScheduleVersion != null && SelectedWeekDayScheduleVersion.ObservableSectorSchedules.Where(x => x.WeekDay == SelectedWeekDayName.Value).Any())
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
				if(StartDateSectorDaySchedule.HasValue)
					dayScheduleVersion.StartDate = StartDateSectorDaySchedule.Value;
				
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
		
		private DelegateCommand<IEnumerable<DeliverySchedule>> _addScheduleRestrictionCommand;
		public DelegateCommand<IEnumerable<DeliverySchedule>> AddScheduleRestrictionCommand => _addScheduleRestrictionCommand ?? (_addScheduleRestrictionCommand = new DelegateCommand<IEnumerable<DeliverySchedule>>(
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
		
		private DelegateCommand _removeScheduleRestrictionCommand;
		public DelegateCommand RemoveScheduleRestrictionCommand => _removeScheduleRestrictionCommand ?? (_removeScheduleRestrictionCommand = new DelegateCommand(
			() => {
				ObservableSectorDayDeliveryRestrictions.Remove(SelectedScheduleRestriction);
				OnPropertyChanged(nameof(ObservableSectorDayDeliveryRestrictions));
			},
			() => SelectedScheduleRestriction != null
		));
		
		private DelegateCommand _removeAcceptBeforeCommand;
		public DelegateCommand RemoveAcceptBeforeCommand => _removeAcceptBeforeCommand ?? (_removeAcceptBeforeCommand = new DelegateCommand(
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
				if(SelectedWeekDayDeliveryRuleVersion != null && SelectedWeekDayDeliveryRuleVersion.ObservableWeekDayDistrictRules.Where(x=>x.WeekDay == SelectedWeekDayName.Value).Any())
					return new GenericObservableList<WeekDayDistrictRuleItem>(SelectedWeekDayDeliveryRuleVersion.ObservableWeekDayDistrictRules.Where(x=>x.WeekDay == SelectedWeekDayName.Value).ToList());
				return new GenericObservableList<WeekDayDistrictRuleItem>();
			}
		}
		
		public WeekDayDistrictRuleItem SelectedWeekDayDistrictRuleItem
		{
			get => _selectedWeekDayDistrictRuleItem;
			set => SetField(ref _selectedWeekDayDistrictRuleItem, value);
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
				var weekDayDistrictRuleItem = new WeekDayDistrictRuleItem{ SectorWeekDayDeliveryRuleVersion = SelectedWeekDayDeliveryRuleVersion, WeekDay = SelectedWeekDayName.Value};
				SelectedWeekDayDeliveryRuleVersion.ObservableWeekDayDistrictRules.Add(weekDayDistrictRuleItem);
				//ObservableSectorDeliveryRulesInSession.Add(weekDayDistrictRuleItem);
				SelectedWeekDayDistrictRuleItem = weekDayDistrictRuleItem;
				OnPropertyChanged(nameof(ObservableSectorDeliveryRules));
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
		
		private DelegateCommand _copyDayDeliveryRule;

		public DelegateCommand CopyDayDeliveryRule => _copyDayDeliveryRule ?? (_copyDayDeliveryRule = new DelegateCommand(
			() =>
			{
				var sectorWeekDayDeliveryRuleVersionClone = SelectedWeekDayScheduleVersion.Clone() as SectorWeekDayDeliveryRuleVersion;
				SelectedWeekDayDeliveryRuleVersion = sectorWeekDayDeliveryRuleVersionClone;
				ObservableSectorWeekDeliveryRuleVersions.Add(sectorWeekDayDeliveryRuleVersionClone);
				ObservableSectorWeekDeliveryRuleVersionsInSession.Add(sectorWeekDayDeliveryRuleVersionClone);
			}));

		private DelegateCommand<AcceptBefore> _addAcceptBeforeCommand;
		public DelegateCommand<AcceptBefore> AddAcceptBeforeCommand => _addAcceptBeforeCommand ?? (_addAcceptBeforeCommand = new DelegateCommand<AcceptBefore>(
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

		public IList<DeliveryPointSectorVersion> DeliveryPointSectorVersions => SelectedSector.DeliveryPointSectorVersions;
		
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

		#region Фильтрация

		private DelegateCommand _filterableSectors;

		public DelegateCommand FilterableSectors => _filterableSectors ?? (_filterableSectors = new DelegateCommand(() =>
		{
			var sectors = UoW.GetAll<Sector>().ToList();
			sectors.ForEach(s =>
			{
				if(Status.HasValue)
				{
					sectors = s.SectorVersions.Where(x => x.Status == Status).Select(x=>x.Sector).ToList();
					sectors = s.SectorDeliveryRuleVersions.Where(x => x.Status == Status).Select(x=>x.Sector).ToList();
					sectors = s.SectorWeekDaySchedulesVersions.Where(x => x.Status == Status).Select(x=>x.Sector).ToList();
					sectors = s.SectorWeekDayDeliveryRuleVersions.Where(x => x.Status == Status).Select(x=>x.Sector).ToList();
				}
				if(StartDateSector.HasValue)
				{
					sectors = s.SectorVersions.Where(x => x.StartDate >= StartDateSector).Select(x=>x.Sector).ToList();
					sectors = s.SectorDeliveryRuleVersions.Where(x => x.StartDate >= StartDateSector.Value).Select(x=>x.Sector).ToList();
					sectors = s.SectorWeekDaySchedulesVersions.Where(x => x.StartDate >= StartDateSector.Value).Select(x=>x.Sector).ToList();
					sectors = s.SectorWeekDayDeliveryRuleVersions.Where(x => x.StartDate >= StartDateSector.Value).Select(x=>x.Sector).ToList();
				}
			});
			Sectors = new GenericObservableList<Sector>(sectors);
			OnPropertyChanged(nameof(Sectors));
		}));

		private SectorsSetStatus? _status;

		public SectorsSetStatus? Status
		{
			get => _status;
			set => _status = value;
		}

		#endregion

		#region Сводка

		
		
		#endregion

		#region Активировать

		private DelegateCommand _activate;

		public DelegateCommand Activate => _activate ?? (_activate = new DelegateCommand(() =>
		{
			Sectors.ForEach(sector => 
			{
				var onActiveSectorVersion = sector.SectorVersions.SingleOrDefault(x => x.Status == SectorsSetStatus.OnActivation);
				if(onActiveSectorVersion != null)
				{
					var activeSector = sector.GetActiveSectorVersion();
					ValidationContext validationContext = new ValidationContext(Entity);
					validationContext.ServiceContainer.AddService(typeof(ISectorsRepository), _sectorRepository);
					if(_commonServices.ValidationService.Validate(Entity, validationContext))
					{
						if(activeSector != null)
						{
							activeSector.EndDate = onActiveSectorVersion?.StartDate?.Date.AddMilliseconds(-1);
							activeSector.Status = SectorsSetStatus.Closed;
							if(!onActiveSectorVersion.Polygon.EqualsExact(activeSector.Polygon))
								DeliveryPointSectorVersions.ForEach(x => x.FindAndAssociateDistrict(UoW, _sectorRepository));
						}

						DeliveryPointSectorVersions.ForEach(x => x.FindAndAssociateDistrict(UoW, _sectorRepository));

						onActiveSectorVersion.Status = SectorsSetStatus.Active;
					}
				}

				var onActiveDeliveryRule = sector.SectorDeliveryRuleVersions.SingleOrDefault(x => x.Status == SectorsSetStatus.OnActivation);
				if(onActiveDeliveryRule != null)
				{
					var activeDeliveryRule = sector.GetActiveDeliveryRuleVersion();
					ValidationContext validationContext = new ValidationContext(onActiveDeliveryRule);
					validationContext.ServiceContainer.AddService(typeof(ISectorsRepository), _sectorRepository);
					if(_commonServices.ValidationService.Validate(onActiveDeliveryRule, validationContext))
					{
						if(activeDeliveryRule != null)
						{
							activeDeliveryRule.EndDate = onActiveDeliveryRule?.StartDate?.Date.AddMilliseconds(-1);
							activeDeliveryRule.Status = SectorsSetStatus.Closed;
						}

						onActiveDeliveryRule.Status = SectorsSetStatus.Active;
					}
				}

				var onActiveWeekDayScheduleVersion =
					sector.SectorWeekDaySchedulesVersions.SingleOrDefault(x => x.Status == SectorsSetStatus.OnActivation);
				if(onActiveWeekDayScheduleVersion != null)
				{
					var activeWeekDaySchedule = sector.GetActiveWeekDayScheduleVersion();
					ValidationContext validationContext = new ValidationContext(onActiveWeekDayScheduleVersion);
					validationContext.ServiceContainer.AddService(typeof(ISectorsRepository), _sectorRepository);
					if(!_commonServices.ValidationService.Validate(onActiveWeekDayScheduleVersion, validationContext))
					{

						if(activeWeekDaySchedule != null)
						{
							activeWeekDaySchedule.EndDate = onActiveWeekDayScheduleVersion?.StartDate?.Date.AddMilliseconds(-1);
							activeWeekDaySchedule.Status = SectorsSetStatus.Closed;
						}

						onActiveWeekDayScheduleVersion.Status = SectorsSetStatus.Active;
					}
				}

				var onActiveWeekDayDeliveryRuleVersion =
					sector.SectorWeekDayDeliveryRuleVersions.SingleOrDefault(x => x.Status == SectorsSetStatus.OnActivation);
				if(onActiveWeekDayDeliveryRuleVersion != null)
				{
					var activeWeekDayDeliveryRule = sector.GetActiveWeekDayDeliveryRuleVersion();
					ValidationContext validationContext = new ValidationContext(onActiveWeekDayDeliveryRuleVersion);
					validationContext.ServiceContainer.AddService(typeof(ISectorsRepository), _sectorRepository);
					if(!_commonServices.ValidationService.Validate(onActiveWeekDayDeliveryRuleVersion, validationContext))
					{
						if(activeWeekDayDeliveryRule != null)
						{
							activeWeekDayDeliveryRule.EndDate = onActiveWeekDayDeliveryRuleVersion?.StartDate?.Date.AddMilliseconds(-1);
							activeWeekDayDeliveryRule.Status = SectorsSetStatus.Closed;
						}

						onActiveWeekDayDeliveryRuleVersion.Status = SectorsSetStatus.Active;
					}
				}
			});
		}));

		#endregion
		
		#region Сохранение

		public override bool Save(bool close)
		{
			// if(base.Save(close)) {
			// 	if(!_commonServices.InteractiveService.Question("Продолжить редактирование районов?", "Успешно сохранено"))
			// 		Close(false, CloseSource.Save);
			// 	return true;
			// }
			Sectors.ForEach(x => UoW.Save(x));
			ObservableSectorDeliveryRules.ForEach(x => UoW.Save());
			return false;
		}
		
		public override void Close(bool askSave, CloseSource source)
		{
			if(askSave)
				TabParent?.AskToCloseTab(this, source);
			else
				TabParent?.ForceCloseTab(this, source);
		}        
		public override bool HasChanges {
			get => base.HasChanges && (CanEditSector || CanEdit);
			set => base.HasChanges = value;
		}

		public override void Dispose()
		{
			UoW?.Dispose();
			base.Dispose();
		}
		#endregion
	}
}