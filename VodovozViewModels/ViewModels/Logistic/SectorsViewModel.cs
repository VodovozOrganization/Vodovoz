using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Text;
using Gamma.Utilities;
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
		private readonly GeometryFactory _geometryFactory;
		private readonly ISectorsRepository _sectorRepository;
		
		private bool _isCreatingNewBorder;

		private DateTime? _startDateSector;
		private DateTime? _endDateSector;
		private DateTime? _startDateSectorVersion;
		private DateTime? _startDateSectorDeliveryRuleVersion;
		private DateTime? _startDateSectorDaySchedule;
		private DateTime? _startDateSectorWeekDayDeliveryRuleVersion;
		
		private WeekDayName? _selectedWeekDayName;
		
		private GenericObservableList<Sector> _observableSectors;
		private GenericObservableList<SectorNodeViewModel> _observableSectorNodeViewModels;
		private GenericObservableList<DeliveryScheduleRestriction> _observableDeliveryScheduleRestrictionsInSession;
		
		private GenericObservableList<Sector> _observableSectorsInSession;
		private GenericObservableList<SectorVersion> _observableSectorVersionsInSession;
		private GenericObservableList<SectorDeliveryRuleVersion> _observableSectorDeliveryRuleVersionsInSession;
		private GenericObservableList<CommonSectorsRuleItem> _observableCommonDistrictRuleItemsInSession;
		private GenericObservableList<SectorWeekDayScheduleVersion> _observableSectorWeekDayScheduleVersionsInSession;
		private GenericObservableList<SectorWeekDayDeliveryRuleVersion> _observableSectorWeekDayDeliveryRuleVersionsInSession;
		private GenericObservableList<WeekDayDistrictRuleItem> _observableWeekDayDistrictRuleItemsInSession;
		private GenericObservableList<PointLatLng> _selectedDistrictBorderVertices;
		private GenericObservableList<PointLatLng> _newBorderVertices;
		
		private SectorVersion _selectedSectorVersion;
		private SectorNodeViewModel _selectedSectorNodeViewModel;
		private SectorDeliveryRuleVersion _selectedSectorDeliveryRuleVersion;
		private SectorWeekDayScheduleVersion _selectedSectorWeekDayScheduleVersion;
		private SectorWeekDayDeliveryRuleVersion _selectedSectorWeekDayDeliveryRuleVersion;
		private CommonSectorsRuleItem _selectedCommonSectorsRuleItem;
		private WeekDayDistrictRuleItem _selectedWeekDayDistrictRuleItem;
		private DeliveryScheduleRestriction _selectedDeliveryScheduleRestriction;
		
		public readonly bool CanChangeSectorWageTypePermissionResult;
		public readonly bool CanEditSector;
		public readonly bool CanDeleteSector;
		public readonly bool CanCreateSector;
		public readonly bool CanEdit;

		#endregion
		public SectorsViewModel(IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory, 
			ICommonServices commonServices,
			ISectorsRepository sectorRepository,
			INavigationManager navigation = null) : base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			_sectorRepository = sectorRepository ?? throw new ArgumentNullException(nameof(sectorRepository));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			
			CanChangeSectorWageTypePermissionResult = commonServices.CurrentPermissionService.ValidatePresetPermission("can_change_district_wage_type");

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

			ObservableSectors = new GenericObservableList<Sector>(UoW.GetAll<Sector>().ToList());

			var allSectors = UoW.GetAll<Sector>().Select(x => new {x.Id, x.DateCreated, SectorVersions = x.SectorVersions.Select(y => new {y.Status, y.SectorName})});
			
			allSectors.ForEach(x =>
			{
				var sectorVersionForNode = x.SectorVersions.SingleOrDefault(y => y.Status == SectorsSetStatus.Active) ??
				                           x.SectorVersions.SingleOrDefault(y => y.Status == SectorsSetStatus.OnActivation) ??
				                           x.SectorVersions.LastOrDefault(z => z.Status == SectorsSetStatus.Draft);
				ObservableSectorNodeViewModels.Add(new SectorNodeViewModel(x.Id, x.DateCreated, sectorVersionForNode != null ? sectorVersionForNode.SectorName : ""));
			
			});

			ObservablePriceRule = new GenericObservableList<DeliveryPriceRule>(UoW.GetAll<DeliveryPriceRule>().ToList());
			
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

		public DateTime? StartDateSectorDeliveryRuleVersion
		{
			get => _startDateSectorDeliveryRuleVersion;
			set
			{
				_startDateSectorDeliveryRuleVersion = value;
				if(SelectedSectorDeliveryRuleVersion != null)
					SelectedSectorDeliveryRuleVersion.StartDate = _startDateSectorDeliveryRuleVersion;
			}
		}

		public DateTime? StartDateSectorWeekDayDeliveryRuleVersion
		{
			get => _startDateSectorWeekDayDeliveryRuleVersion;
			set
			{
				_startDateSectorWeekDayDeliveryRuleVersion = value;
				if(SelectedSectorWeekDayDeliveryRuleVersion != null)
					SelectedSectorWeekDayDeliveryRuleVersion.StartDate = _startDateSectorWeekDayDeliveryRuleVersion;
			}
		}

		public DateTime? StartDateSectorDaySchedule
		{
			get => _startDateSectorDaySchedule;
			set
			{
				_startDateSectorDaySchedule = value;
				if(SelectedSectorWeekDayScheduleVersion != null)
					SelectedSectorWeekDayScheduleVersion.StartDate = _startDateSectorDaySchedule;
			}
		}

		#endregion
		
		public WeekDayName? SelectedWeekDayName {
			get => _selectedWeekDayName;
			set {
				if(SetField(ref _selectedWeekDayName, value)) {
					if(SelectedWeekDayName != null) {
						if(SelectedSectorWeekDayScheduleVersion != null)
							OnPropertyChanged(nameof(ObservableDeliveryScheduleRestriction));
						if(SelectedSectorWeekDayDeliveryRuleVersion != null)
							OnPropertyChanged(nameof(ObservableWeekDayDistrictRuleItems));
					}
				}
			}
		}
		
		public GenericObservableList<DeliveryPriceRule> ObservablePriceRule { get; set; }

		#region Операции над секторами
		public GenericObservableList<Sector> ObservableSectors
		{
			get => _observableSectors;
			set => _observableSectors = value;
		}

		public GenericObservableList<SectorNodeViewModel> ObservableSectorNodeViewModels => _observableSectorNodeViewModels ??
		                                                                                    (_observableSectorNodeViewModels =
			                                                                                    new GenericObservableList<
				                                                                                    SectorNodeViewModel>());

		private GenericObservableList<Sector> ObservableSectorsInSession =>
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
					if(!SelectedWeekDayName.HasValue)
						_selectedWeekDayName = WeekDayName.Today;
					
					OnPropertyChanged(nameof(ObservableSectorVersions));
					OnPropertyChanged(nameof(ObservableSectorDeliveryRuleVersions));
					SelectedSectorDeliveryRuleVersion = null;
					SelectedSectorWeekDayScheduleVersion = null;
					SelectedSectorWeekDayDeliveryRuleVersion = null;
					OnPropertyChanged(nameof(ObservableSectorWeekDayScheduleVersions));
					OnPropertyChanged(nameof(ObservableSectorWeekDayDeliveryRuleVersions));
				}
			}
		}

		private Sector SelectedSector =>
			ObservableSectors.Any() ? ObservableSectors.SingleOrDefault(x => x.Id == SelectedSectorNodeViewModel.Id) : null;

		private DelegateCommand _addSector;

		public DelegateCommand AddSector => _addSector ?? (_addSector = new DelegateCommand(
			() =>
			{
				var sector = new Sector {DateCreated = DateTime.Today};
				var sectorNodeViewModel = new SectorNodeViewModel(sector);
				ObservableSectorNodeViewModels.Add(sectorNodeViewModel);
				ObservableSectorsInSession.Add(sector);
				ObservableSectors.Add(sector);
			}));

		private DelegateCommand _removeSector;

		public DelegateCommand RemoveSector => _removeSector ?? (_removeSector = new DelegateCommand(() =>
		{
			if(CheckRemoveSectorInSession(SelectedSector))
			{
				ObservableSectorNodeViewModels.Remove(SelectedSectorNodeViewModel);
				ObservableSectorsInSession.Remove(SelectedSector);
				ObservableSectors.Remove(SelectedSector);
			}
			else _commonServices.InteractiveService.ShowMessage(ImportanceLevel.Error, "Удаление невозможно.");
		}));
		
		private bool CheckRemoveSectorInSession(Sector sector) => ObservableSectorsInSession.Contains(sector);

		#endregion

		#region Операции над версиями секторов(основные характеристики)
		
		public GenericObservableList<SectorVersion> ObservableSectorVersions => SelectedSector.ObservableSectorVersions;

		private GenericObservableList<SectorVersion> ObservableSectorVersionsInSession =>
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
			ObservableSectorVersions.Add(newVersion);
		}));

		private bool CheckRemoveSectorVersion(SectorVersion sectorVersion) => ObservableSectorVersionsInSession.Contains(sectorVersion);

		private DelegateCommand _removeSectorVersion;

		public DelegateCommand RemoveSectorVersion => _removeSectorVersion ?? (_removeSectorVersion = new DelegateCommand(
			(() =>
			{
				if(CheckRemoveSectorVersion(SelectedSectorVersion))
				{
					ObservableSectorVersionsInSession.Remove(SelectedSectorVersion);
					ObservableSectorVersions.Remove(SelectedSectorVersion);
					SelectedSectorVersion = null;
				}
				else _commonServices.InteractiveService.ShowMessage(ImportanceLevel.Error, "Удаление невозможно.");
			})));

		#endregion

		#region Операции над обычными графиками доставки районов

		public GenericObservableList<SectorDeliveryRuleVersion> ObservableSectorDeliveryRuleVersions =>
			SelectedSector.ObservableSectorDeliveryRuleVersions;

		private GenericObservableList<SectorDeliveryRuleVersion> ObservableSectorDeliveryRuleVersionsInSession =>
			_observableSectorDeliveryRuleVersionsInSession ?? (_observableSectorDeliveryRuleVersionsInSession =
				new GenericObservableList<SectorDeliveryRuleVersion>());
		
		public SectorDeliveryRuleVersion SelectedSectorDeliveryRuleVersion
		{
			get => _selectedSectorDeliveryRuleVersion;
			set
			{
				if(!SetField(ref _selectedSectorDeliveryRuleVersion, value))
					return;
				OnPropertyChanged(nameof(ObservableCommonDistrictRuleItems));
			}
		}

		public GenericObservableList<CommonSectorsRuleItem> ObservableCommonDistrictRuleItems
		{
			get
			{
				if(SelectedSectorDeliveryRuleVersion != null)
					return SelectedSectorDeliveryRuleVersion.ObservableCommonDistrictRuleItems;
				return new GenericObservableList<CommonSectorsRuleItem>();
			}
		}

		private GenericObservableList<CommonSectorsRuleItem> ObservableCommonDistrictRuleItemsInSession =>
			_observableCommonDistrictRuleItemsInSession ??
			(_observableCommonDistrictRuleItemsInSession = new GenericObservableList<CommonSectorsRuleItem>());

		public CommonSectorsRuleItem SelectedCommonSectorsRuleItem
		{
			get => _selectedCommonSectorsRuleItem;
			set
			{
				if(!SetField(ref _selectedCommonSectorsRuleItem, value))
					return;
				
				OnPropertyChanged(nameof(ObservableCommonDistrictRuleItems));
			}
		}

		private DelegateCommand _addSectorDeliveryRuleVersion;

		public DelegateCommand AddSectorDeliveryRuleVersion => _addSectorDeliveryRuleVersion ?? (_addSectorDeliveryRuleVersion =
			new DelegateCommand(() =>
			{
				var deliveryRuleVersion = new SectorDeliveryRuleVersion {Sector = SelectedSector};
				if(StartDateSectorDeliveryRuleVersion.HasValue)
					deliveryRuleVersion.StartDate = StartDateSectorDeliveryRuleVersion.Value;

				SelectedSectorDeliveryRuleVersion = deliveryRuleVersion;
				ObservableSectorDeliveryRuleVersionsInSession.Add(deliveryRuleVersion);
				ObservableSectorDeliveryRuleVersions.Add(deliveryRuleVersion);
			}));

		private DelegateCommand _removeSectorDeliveryRuleVersion;

		public DelegateCommand RemoveSectorDeliveryRuleVersion => _removeSectorDeliveryRuleVersion ?? (_removeSectorDeliveryRuleVersion =
			new DelegateCommand(() =>
			{
				if(CheckRemoveSectorDeliveryRuleVersion(SelectedSectorDeliveryRuleVersion))
				{
					ObservableSectorDeliveryRuleVersionsInSession.Remove(SelectedSectorDeliveryRuleVersion);
					ObservableSectorDeliveryRuleVersions.Remove(SelectedSectorDeliveryRuleVersion);
					SelectedSectorDeliveryRuleVersion = null;
				}
				else _commonServices.InteractiveService.ShowMessage(ImportanceLevel.Error, "Удаление невозможно.");
			}));
		
		private DelegateCommand _copySectorDeliveryRuleVersion;

		public DelegateCommand CopySectorDeliveryRuleVersion => _copySectorDeliveryRuleVersion ?? (_copySectorDeliveryRuleVersion =
			new DelegateCommand(() =>
			{
				var deliveryRuleVersionClone = SelectedSectorDeliveryRuleVersion.Clone() as SectorDeliveryRuleVersion;
				SelectedSectorDeliveryRuleVersion = deliveryRuleVersionClone;
				ObservableSectorDeliveryRuleVersionsInSession.Add(deliveryRuleVersionClone);
				ObservableSectorDeliveryRuleVersions.Add(deliveryRuleVersionClone);
			}));

		private bool CheckRemoveSectorDeliveryRuleVersion(SectorDeliveryRuleVersion sectorDeliveryRuleVersion) =>
			ObservableSectorDeliveryRuleVersionsInSession.Contains(sectorDeliveryRuleVersion);
		
		private DelegateCommand _addCommonDistrictRule;

		public DelegateCommand AddCommonDistrictRule => _addCommonDistrictRule ?? (_addCommonDistrictRule = new DelegateCommand(
			() =>
			{
				var commonDistrictRule = new CommonSectorsRuleItem {SectorDeliveryRuleVersion = SelectedSectorDeliveryRuleVersion};
				ObservableCommonDistrictRuleItems.Add(commonDistrictRule);
				ObservableCommonDistrictRuleItemsInSession.Add(commonDistrictRule);
				SelectedCommonSectorsRuleItem = commonDistrictRule;
			}));

		private DelegateCommand _removeCommonDistrictRule;

		public DelegateCommand RemoveCommonDistrictRule => _removeCommonDistrictRule ?? (_removeCommonDistrictRule = new DelegateCommand(
			() =>
			{
				if(CheckRemoveCommonDistrictRule(SelectedCommonSectorsRuleItem))
				{
					ObservableCommonDistrictRuleItemsInSession.Remove(SelectedCommonSectorsRuleItem);
					ObservableCommonDistrictRuleItems.Remove(SelectedCommonSectorsRuleItem);
					SelectedCommonSectorsRuleItem = null;
				}
			}));

		private bool CheckRemoveCommonDistrictRule(CommonSectorsRuleItem sectorsRuleItem) =>
			ObservableCommonDistrictRuleItemsInSession.Contains(sectorsRuleItem);

		#endregion

		#region Операции над графиками доставки района по дням

		public GenericObservableList<SectorWeekDayScheduleVersion> ObservableSectorWeekDayScheduleVersions =>
			SelectedSector.ObservableSectorWeekDayScheduleVersions;

		private GenericObservableList<SectorWeekDayScheduleVersion> ObservableSectorWeekDayScheduleVersionsInSession =>
			_observableSectorWeekDayScheduleVersionsInSession ?? (_observableSectorWeekDayScheduleVersionsInSession =
				new GenericObservableList<SectorWeekDayScheduleVersion>());
		
		public SectorWeekDayScheduleVersion SelectedSectorWeekDayScheduleVersion
		{
			get => _selectedSectorWeekDayScheduleVersion;
			set
			{
				if(SelectedWeekDayName != null)
				{
					if(SetField(ref _selectedSectorWeekDayScheduleVersion, value))
					{
						OnPropertyChanged(nameof(ObservableDeliveryScheduleRestriction));
					}
				}
				else
					_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Error, "Выберите сначала день");
			}
		}

		public GenericObservableList<DeliveryScheduleRestriction> ObservableDeliveryScheduleRestriction
		{
			get
			{
				if(SelectedSectorWeekDayScheduleVersion?.ObservableDeliveryScheduleRestriction != null &&
				   SelectedSectorWeekDayScheduleVersion.ObservableDeliveryScheduleRestriction
					   .Any(x => x.WeekDay == SelectedWeekDayName.Value))
					return new GenericObservableList<DeliveryScheduleRestriction>(SelectedSectorWeekDayScheduleVersion
						.ObservableDeliveryScheduleRestriction
						.Where(x => x.WeekDay == SelectedWeekDayName.Value).ToList());
				return new GenericObservableList<DeliveryScheduleRestriction>();
			}
		}

		private GenericObservableList<DeliveryScheduleRestriction> ObservableDeliveryScheduleRestrictionsInSession =>
			_observableDeliveryScheduleRestrictionsInSession ?? (_observableDeliveryScheduleRestrictionsInSession =
				new GenericObservableList<DeliveryScheduleRestriction>());
		
		public DeliveryScheduleRestriction SelectedDeliveryScheduleRestriction {
			get => _selectedDeliveryScheduleRestriction;
			set => SetField(ref _selectedDeliveryScheduleRestriction, value);
		}

		private DelegateCommand _addSectorWeekDayScheduleVersion;

		public DelegateCommand AddSectorWeekDayScheduleVersion => _addSectorWeekDayScheduleVersion ?? (_addSectorWeekDayScheduleVersion =
			new DelegateCommand(() =>
			{
				var dayScheduleVersion = new SectorWeekDayScheduleVersion {Sector = SelectedSector, Status = SectorsSetStatus.Draft};
				if(StartDateSectorDaySchedule.HasValue)
					dayScheduleVersion.StartDate = StartDateSectorDaySchedule.Value;
				
				SelectedSectorWeekDayScheduleVersion = dayScheduleVersion;
				ObservableSectorWeekDayScheduleVersionsInSession.Add(dayScheduleVersion);
				ObservableSectorWeekDayScheduleVersions.Add(dayScheduleVersion);
			}));
		
		private DelegateCommand _removeSectorWeekDayScheduleVersion;

		public DelegateCommand RemoveSectorWeekDayScheduleVersion => _removeSectorWeekDayScheduleVersion ??
		                                                             (_removeSectorWeekDayScheduleVersion = new DelegateCommand(() =>
		                                                             {
			                                                             if(CheckRemoveSectorWeekDayScheduleVersion(
				                                                             SelectedSectorWeekDayScheduleVersion))
			                                                             {
				                                                             ObservableSectorWeekDayScheduleVersionsInSession
					                                                             .Remove(
						                                                             SelectedSectorWeekDayScheduleVersion);
				                                                             ObservableSectorWeekDayScheduleVersions.Remove(
					                                                             SelectedSectorWeekDayScheduleVersion);
				                                                             SelectedSectorWeekDayScheduleVersion = null;
			                                                             }
		                                                             }));

		private bool CheckRemoveSectorWeekDayScheduleVersion(SectorWeekDayScheduleVersion sectorWeekDayScheduleVersion) =>
			ObservableSectorWeekDayScheduleVersionsInSession.Contains(sectorWeekDayScheduleVersion);

		private DelegateCommand _copySectorWeekDayScheduleVersion;

		public DelegateCommand CopySectorWeekDayScheduleVersion => _copySectorWeekDayScheduleVersion ?? (_copySectorWeekDayScheduleVersion =
			new DelegateCommand(
				() =>
				{
					var dayScheduleVersionClone = SelectedSectorWeekDayScheduleVersion.Clone() as SectorWeekDayScheduleVersion;
					SelectedSectorWeekDayScheduleVersion = dayScheduleVersionClone;
					ObservableSectorWeekDayScheduleVersionsInSession.Add(dayScheduleVersionClone);
					ObservableSectorWeekDayScheduleVersions.Add(dayScheduleVersionClone);
				}));
		
		private DelegateCommand<IEnumerable<DeliverySchedule>> _addDeliveryScheduleRestriction;

		public DelegateCommand<IEnumerable<DeliverySchedule>> AddDeliveryScheduleRestriction => _addDeliveryScheduleRestriction ??
			(_addDeliveryScheduleRestriction = new DelegateCommand<IEnumerable<DeliverySchedule>>(
				schedules =>
				{
					foreach(var schedule in schedules)
					{
						if(SelectedWeekDayName.HasValue &&
						   ObservableDeliveryScheduleRestriction.All(x => x.DeliverySchedule.Id != schedule.Id))
						{
							var restriction = new DeliveryScheduleRestriction
							{
								WeekDay = SelectedWeekDayName.Value, DeliverySchedule = schedule,
								SectorWeekDayScheduleVersion = SelectedSectorWeekDayScheduleVersion
							};
							SelectedSectorWeekDayScheduleVersion.ObservableDeliveryScheduleRestriction.Add(restriction);
							ObservableDeliveryScheduleRestrictionsInSession.Add(restriction);
							OnPropertyChanged(nameof(ObservableDeliveryScheduleRestriction));
						}
					}
				},
				schedules => schedules.Any()
			));
		
		private bool CheckRemoveDeliveryScheduleRestrictions(DeliveryScheduleRestriction deliveryScheduleRestriction) =>
			ObservableDeliveryScheduleRestrictionsInSession.Contains(deliveryScheduleRestriction);
		
		private DelegateCommand _removeDeliveryScheduleRestriction;

		public DelegateCommand RemoveDeliveryScheduleRestriction => _removeDeliveryScheduleRestriction ??
		                                                            (_removeDeliveryScheduleRestriction = new DelegateCommand(
			                                                            () =>
			                                                            {
				                                                            if(CheckRemoveDeliveryScheduleRestrictions(
					                                                            SelectedDeliveryScheduleRestriction))
				                                                            {
					                                                            ObservableDeliveryScheduleRestrictionsInSession.Remove(
						                                                            SelectedDeliveryScheduleRestriction);
					                                                            SelectedSectorWeekDayScheduleVersion
						                                                            .ObservableDeliveryScheduleRestriction
						                                                            .Remove(SelectedDeliveryScheduleRestriction);
					                                                            OnPropertyChanged(
						                                                            nameof(ObservableDeliveryScheduleRestriction));
				                                                            }
			                                                            },
			                                                            () => SelectedDeliveryScheduleRestriction != null
		                                                            ));
		
		private DelegateCommand<AcceptBefore> _addAcceptBeforeCommand;

		public DelegateCommand<AcceptBefore> AddAcceptBeforeCommand => _addAcceptBeforeCommand ?? (_addAcceptBeforeCommand =
			new DelegateCommand<AcceptBefore>(
				acceptBefore =>
				{
					SelectedDeliveryScheduleRestriction.AcceptBefore = acceptBefore;
				},
				acceptBefore => acceptBefore != null && SelectedSectorWeekDayScheduleVersion != null
			));

		private DelegateCommand _removeAcceptBeforeCommand;

		public DelegateCommand RemoveAcceptBeforeCommand => _removeAcceptBeforeCommand ?? (_removeAcceptBeforeCommand = new DelegateCommand(
			() =>
			{
				if(CheckRemoveDeliveryScheduleRestrictions(SelectedDeliveryScheduleRestriction))
				{
					SelectedDeliveryScheduleRestriction.AcceptBefore = null;
				}
			},
			() => SelectedDeliveryScheduleRestriction != null
		));
		
		#endregion

		#region WeekDayDistrictRuleItem 
		
		public GenericObservableList<WeekDayDistrictRuleItem> ObservableWeekDayDistrictRuleItems
		{
			get
			{
				if(SelectedSectorWeekDayDeliveryRuleVersion != null &&
				   SelectedSectorWeekDayDeliveryRuleVersion.ObservableWeekDayDistrictRules.Any(x => x.WeekDay == SelectedWeekDayName.Value))
					return new GenericObservableList<WeekDayDistrictRuleItem>(SelectedSectorWeekDayDeliveryRuleVersion
						.ObservableWeekDayDistrictRules.Where(x => x.WeekDay == SelectedWeekDayName.Value).ToList());
				return new GenericObservableList<WeekDayDistrictRuleItem>();
			}
		}

		private GenericObservableList<WeekDayDistrictRuleItem> ObservableWeekDayDistrictRuleItemsInSession =>
			_observableWeekDayDistrictRuleItemsInSession ?? (_observableWeekDayDistrictRuleItemsInSession =
				new GenericObservableList<WeekDayDistrictRuleItem>());
		
		public WeekDayDistrictRuleItem SelectedWeekDayDistrictRuleItem
		{
			get => _selectedWeekDayDistrictRuleItem;
			set => SetField(ref _selectedWeekDayDistrictRuleItem, value);
		}
		public GenericObservableList<SectorWeekDayDeliveryRuleVersion> ObservableSectorWeekDayDeliveryRuleVersions =>
			SelectedSector.ObservableSectorWeekDayDeliveryRuleVersions;

		private GenericObservableList<SectorWeekDayDeliveryRuleVersion> ObservableSectorWeekDayDeliveryRuleVersionsInSession =>
			_observableSectorWeekDayDeliveryRuleVersionsInSession ?? (_observableSectorWeekDayDeliveryRuleVersionsInSession =
				new GenericObservableList<SectorWeekDayDeliveryRuleVersion>());

		public SectorWeekDayDeliveryRuleVersion SelectedSectorWeekDayDeliveryRuleVersion
		{
			get => _selectedSectorWeekDayDeliveryRuleVersion;
			set
			{
				if(SelectedWeekDayName != null)
				{
					if(SetField(ref _selectedSectorWeekDayDeliveryRuleVersion, value))
					{
						OnPropertyChanged(nameof(ObservableWeekDayDistrictRuleItems));
					}
				}
				else 
					_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Error, "Выберите сначала день");
			}
		}

		private DelegateCommand _addSectorWeekDayDeliveryRuleVersion;

		public DelegateCommand AddSectorWeekDayDeliveryRuleVersion => _addSectorWeekDayDeliveryRuleVersion ??
		                                                              (_addSectorWeekDayDeliveryRuleVersion = new DelegateCommand(
			                                                              () =>
			                                                              {
				                                                              var sectorWeekDayDeliveryRuleVersion =
					                                                              new SectorWeekDayDeliveryRuleVersion
					                                                              {
						                                                              Sector = SelectedSector,
						                                                              Status = SectorsSetStatus.Draft
					                                                              };
				                                                              if(StartDateSectorWeekDayDeliveryRuleVersion.HasValue)
					                                                              sectorWeekDayDeliveryRuleVersion.StartDate =
						                                                              StartDateSectorWeekDayDeliveryRuleVersion.Value;

				                                                              ObservableSectorWeekDayDeliveryRuleVersionsInSession.Add(
					                                                              sectorWeekDayDeliveryRuleVersion);
				                                                              ObservableSectorWeekDayDeliveryRuleVersions.Add(
					                                                              sectorWeekDayDeliveryRuleVersion);
				                                                              SelectedSectorWeekDayDeliveryRuleVersion =
					                                                              sectorWeekDayDeliveryRuleVersion;
			                                                              }));
		
		private bool CheckRemoveSectorWeekDayDeliveryRuleVersion(SectorWeekDayDeliveryRuleVersion sectorWeekDayDeliveryRuleVersion) =>
			ObservableSectorWeekDayDeliveryRuleVersionsInSession.Contains(sectorWeekDayDeliveryRuleVersion);


		private DelegateCommand _removeSectorWeekDayDeliveryRuleVersion;

		public DelegateCommand RemoveSectorWeekDayDeliveryRuleVersion => _removeSectorWeekDayDeliveryRuleVersion ??
		                                                                 (_removeSectorWeekDayDeliveryRuleVersion = new DelegateCommand(
			                                                                 () =>
			                                                                 {
				                                                                 if(CheckRemoveSectorWeekDayDeliveryRuleVersion(
					                                                                 _selectedSectorWeekDayDeliveryRuleVersion))
				                                                                 {
					                                                                 ObservableSectorWeekDayDeliveryRuleVersionsInSession
						                                                                 .Remove(SelectedSectorWeekDayDeliveryRuleVersion);
					                                                                 ObservableSectorWeekDayDeliveryRuleVersions.Remove(
						                                                                 SelectedSectorWeekDayDeliveryRuleVersion);
					                                                                 SelectedSectorWeekDayDeliveryRuleVersion = null;
				                                                                 }
			                                                                 }));
		
		private DelegateCommand _copySectorWeekDayDeliveryRuleVersion;

		public DelegateCommand CopySectorWeekDayDeliveryRuleVersion => _copySectorWeekDayDeliveryRuleVersion ??
		                                                               (_copySectorWeekDayDeliveryRuleVersion = new DelegateCommand(
			                                                               () =>
			                                                               {
				                                                               var sectorWeekDayDeliveryRuleVersionClone =
					                                                               SelectedSectorWeekDayScheduleVersion.Clone() as
						                                                               SectorWeekDayDeliveryRuleVersion;
				                                                               SelectedSectorWeekDayDeliveryRuleVersion =
					                                                               sectorWeekDayDeliveryRuleVersionClone;
				                                                               ObservableSectorWeekDayDeliveryRuleVersionsInSession.Add(
					                                                               sectorWeekDayDeliveryRuleVersionClone);
				                                                               ObservableSectorWeekDayDeliveryRuleVersions.Add(
					                                                               sectorWeekDayDeliveryRuleVersionClone);
			                                                               }));

		private DelegateCommand _addWeekDayDistrictRule;

		public DelegateCommand AddWeekDayDistrictRule => _addWeekDayDistrictRule ?? (_addWeekDayDistrictRule = new DelegateCommand(
			() =>
			{
				var weekDayDistrictRuleItem = new WeekDayDistrictRuleItem
					{SectorWeekDayDeliveryRuleVersion = SelectedSectorWeekDayDeliveryRuleVersion, WeekDay = SelectedWeekDayName.Value};
				SelectedSectorWeekDayDeliveryRuleVersion.ObservableWeekDayDistrictRules.Add(weekDayDistrictRuleItem);
				SelectedWeekDayDistrictRuleItem = weekDayDistrictRuleItem;
				OnPropertyChanged(nameof(ObservableWeekDayDistrictRuleItems));
			}));

		private DelegateCommand _removeWeekDayDistrictRule;

		public DelegateCommand RemoveWeekDayDistrictRule => _removeWeekDayDistrictRule ?? (_removeWeekDayDistrictRule = new DelegateCommand(
			() =>
			{
				if(CheckRemoveWeekDayDeliveryRule(SelectedWeekDayDistrictRuleItem))
				{
					ObservableWeekDayDistrictRuleItemsInSession.Remove(SelectedWeekDayDistrictRuleItem);
					SelectedSectorWeekDayDeliveryRuleVersion.ObservableWeekDayDistrictRules.Remove(SelectedWeekDayDistrictRuleItem);
					OnPropertyChanged(nameof(ObservableWeekDayDistrictRuleItems));
					SelectedWeekDayDistrictRuleItem = null;
				}
			}));
		
		private bool CheckRemoveWeekDayDeliveryRule(WeekDayDistrictRuleItem weekDayDistrictRuleItem) =>
			ObservableWeekDayDistrictRuleItemsInSession.Contains(weekDayDistrictRuleItem);
		
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
			() =>
			{
				if(NewBorderVertices.Count < 3)
					return;
				var closingPoint = NewBorderVertices[0];
				NewBorderVertices.Add(closingPoint);
				SelectedDistrictBorderVertices = new GenericObservableList<PointLatLng>(NewBorderVertices.ToList());
				NewBorderVertices.Clear();
				SelectedSectorVersion.Polygon =
					_geometryFactory.CreatePolygon(SelectedDistrictBorderVertices.Select(p => new Coordinate(p.Lat, p.Lng)).ToArray());
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

		public DelegateCommand<PointLatLng> AddNewVertexCommand => _addNewVertexCommand ?? (_addNewVertexCommand =
			new DelegateCommand<PointLatLng>(
				point =>
				{
					NewBorderVertices.Add(point);
					OnPropertyChanged(nameof(NewBorderVertices));
				},
				point => IsCreatingNewBorder
			));
		
		private DelegateCommand<PointLatLng> _removeNewBorderVerteCommand;

		public DelegateCommand<PointLatLng> RemoveNewBorderVertexCommand => _removeNewBorderVerteCommand ?? (_removeNewBorderVerteCommand =
			new DelegateCommand<PointLatLng>(
				point =>
				{
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
					sectors = s.SectorVersions.Where(x => x.Status == Status).Select(x => x.Sector).ToList();
					sectors = s.SectorDeliveryRuleVersions.Where(x => x.Status == Status).Select(x => x.Sector).ToList();
					sectors = s.SectorWeekDaySchedulesVersions.Where(x => x.Status == Status).Select(x => x.Sector).ToList();
					sectors = s.SectorWeekDayDeliveryRuleVersions.Where(x => x.Status == Status).Select(x => x.Sector).ToList();
				}

				if(StartDateSector.HasValue)
				{
					sectors = s.SectorVersions.Where(x => x.StartDate >= StartDateSector).Select(x => x.Sector).ToList();
					sectors = s.SectorDeliveryRuleVersions.Where(x => x.StartDate >= StartDateSector.Value).Select(x => x.Sector).ToList();
					sectors = s.SectorWeekDaySchedulesVersions.Where(x => x.StartDate >= StartDateSector.Value).Select(x => x.Sector)
						.ToList();
					sectors = s.SectorWeekDayDeliveryRuleVersions.Where(x => x.StartDate >= StartDateSector.Value).Select(x => x.Sector)
						.ToList();
				}
			});
			ObservableSectors = new GenericObservableList<Sector>(sectors);
			OnPropertyChanged(nameof(ObservableSectors));
		}));

		private SectorsSetStatus? _status;

		public SectorsSetStatus? Status
		{
			get => _status;
			set => _status = value;
		}

		#endregion

		#region Сводка

		private DelegateCommand _summaryActive;

		public DelegateCommand SummaryActive => _summaryActive ?? (_summaryActive = new DelegateCommand(() =>
		{
			StringBuilder summaryText = new StringBuilder();
			ObservableSectors.ForEach(sector =>
			{
				summaryText.Append(
					$"Изменения по сектору {ObservableSectorNodeViewModels.SingleOrDefault(x => x.Id == sector.Id)?.Name}: ");

				var onActiveSectorVersion = sector.SectorVersions.SingleOrDefault(x => x.Status == SectorsSetStatus.OnActivation);
				if(onActiveSectorVersion != null)
				{
					var activeSectorVersion = sector.GetActiveSectorVersion();

					if(onActiveSectorVersion.TariffZone?.Name != activeSectorVersion?.TariffZone?.Name)
						summaryText.Append(
							$"Поле \"Тарифная зона\": {activeSectorVersion?.TariffZone?.Name} => {onActiveSectorVersion.TariffZone?.Name}; ");

					if(onActiveSectorVersion.Polygon?.Coordinates != activeSectorVersion?.Polygon?.Coordinates)
						summaryText.Append($"Поле \"Граница\" изменена; ");

					if(onActiveSectorVersion.WageSector?.Name != activeSectorVersion?.WageSector?.Name)
						summaryText.Append(
							$"Поле \"Группа района для расчёта ЗП\": {activeSectorVersion?.WageSector?.Name} => {onActiveSectorVersion.WageSector?.Name}");

					if(onActiveSectorVersion.GeographicGroup?.Name != activeSectorVersion?.GeographicGroup?.Name)
						summaryText.Append(
							$"Поле \"Часть города\": {activeSectorVersion?.GeographicGroup?.Name} => {onActiveSectorVersion.GeographicGroup?.Name}");

					if(onActiveSectorVersion.MinBottles != activeSectorVersion?.MinBottles)
						summaryText.Append(
							$"Поле \"Минимальное количество бутылей\": {activeSectorVersion?.MinBottles} => {onActiveSectorVersion.MinBottles}");

					if(onActiveSectorVersion.WaterPrice != activeSectorVersion?.WaterPrice)
						summaryText.Append(
							$"Поле \"Цена на воду\": {activeSectorVersion?.WaterPrice} => {onActiveSectorVersion.WaterPrice}");

					if(onActiveSectorVersion.PriceType != activeSectorVersion?.PriceType)
						summaryText.Append(
							$"Поле \"Вид цены\": {activeSectorVersion?.PriceType.GetEnumTitle()} => {onActiveSectorVersion.PriceType.GetEnumTitle()}");
				}

				var onActiveDeliveryRule =
					sector.SectorDeliveryRuleVersions.SingleOrDefault(x => x.Status == SectorsSetStatus.OnActivation);
				if(onActiveDeliveryRule != null)
				{
					var activeDeliveryRule = sector.GetActiveDeliveryRuleVersion();
					var onActiveCommonDistrict = onActiveDeliveryRule.ObservableCommonDistrictRuleItems;
					var activeCommonDistrict = activeDeliveryRule?.ObservableCommonDistrictRuleItems;

					var generalCommonDistrict = onActiveCommonDistrict?.Select((el, i) => (i, el.Title))
						.Except(activeCommonDistrict?.Select((el, i) => (i, el.Title))).Select(d => d.i);

					foreach(var index in generalCommonDistrict)
					{
						var activeCommon = activeCommonDistrict.Count > index ? activeCommonDistrict[index].Title : "";
						var onActiveCommon = onActiveCommonDistrict.Count > index ? onActiveCommonDistrict[index].Title : "";
						summaryText.Append($"Общие правила доставки: {activeCommon} => {onActiveCommon}");
					}
				}

				var onActiveWeekDayScheduleVersion =
					sector.SectorWeekDaySchedulesVersions.SingleOrDefault(x => x.Status == SectorsSetStatus.OnActivation);
				if(onActiveWeekDayScheduleVersion != null)
				{
					var activeWeekDaySchedule = sector.GetActiveWeekDayScheduleVersion();
					var onActiveSectorSchedules = onActiveWeekDayScheduleVersion.ObservableDeliveryScheduleRestriction;
					var activeSectorSchedules = activeWeekDaySchedule?.ObservableDeliveryScheduleRestriction;

					var generalCommonDistrict = onActiveSectorSchedules?.Select((el, i) => (i, el))
						.Except(activeSectorSchedules?.Select((el, i) => (i, el))).Select(d => d.i);

					foreach(var index in generalCommonDistrict)
					{
						var activeCommon = activeSectorSchedules.Count > index ? activeSectorSchedules[index].ToString() : "";
						var onActiveCommon = onActiveSectorSchedules.Count > index ? onActiveSectorSchedules[index].ToString() : "";
						summaryText.Append($"Графики доставки: {activeCommon} => {onActiveCommon}");
					}
				}

				var onActiveWeekDayDeliveryRuleVersion =
					sector.SectorWeekDayDeliveryRuleVersions.SingleOrDefault(x => x.Status == SectorsSetStatus.OnActivation);
				if(onActiveWeekDayDeliveryRuleVersion != null)
				{
					var activeWeekDayDeliveryRuleVersion = sector.GetActiveWeekDayDeliveryRuleVersion();
					var onActiveWeekDayDistrictRules = onActiveWeekDayDeliveryRuleVersion.ObservableWeekDayDistrictRules;
					var activeWeekDayDistrictRules = activeWeekDayDeliveryRuleVersion?.ObservableWeekDayDistrictRules;

					if(activeWeekDayDistrictRules != null)
					{
						var generalCommonDistrict = onActiveWeekDayDistrictRules?.Select((el, i) => (i, el.Title))
							.Except(activeWeekDayDistrictRules?.Select((el, i) => (i, el.Title))).Select(d => d.i);

						foreach(var index in generalCommonDistrict)
						{
							var activeCommon = activeWeekDayDistrictRules.Count > index ? activeWeekDayDistrictRules[index].ToString() : "";
							var onActiveCommon = onActiveWeekDayDistrictRules.Count > index
								? onActiveWeekDayDistrictRules[index].ToString()
								: "";
							summaryText.Append($"Особые правила доставки:{activeCommon} => {onActiveCommon}");
						}
					}
					else onActiveWeekDayDistrictRules.ForEach(x => summaryText.Append($"Особые правила доставки: добавлено {x.Title}"));
				}

				summaryText.AppendLine();
			});
			SummaryText = summaryText.ToString();
		}));

		private string _summaryText;

		public string SummaryText
		{
			get => _summaryText;
			set => _summaryText = value;
		}

		#endregion

		#region Активировать

		private DelegateCommand _activate;

		public DelegateCommand Activate => _activate ?? (_activate = new DelegateCommand(() =>
		{
			ObservableSectors.ForEach(sector =>
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
							{

								DeliveryPointSectorVersions.ForEach(x =>
								{
									var foundSectorVersion = _sectorRepository
										.GetSectorVersionInCoordinates(UoW, x.Latitude.Value, x.Longitude.Value).FirstOrDefault();
									if(foundSectorVersion?.Sector != x.Sector)
									{
										var newDeliveryPointSectorVersion = x.Clone() as DeliveryPointSectorVersion;
										x.EndDate = DateTime.Today.Date.AddDays(1).AddMilliseconds(-1);
										newDeliveryPointSectorVersion?.FindAndAssociateDistrict(UoW, _sectorRepository);
										DeliveryPointSectorVersions.Add(newDeliveryPointSectorVersion);
									}
								});
							}
						}
						else
						{
							DeliveryPointSectorVersions.ForEach(x =>
							{
								var foundSectorVersion = _sectorRepository
									.GetSectorVersionInCoordinates(UoW, x.Latitude.Value, x.Longitude.Value).FirstOrDefault();
								if(foundSectorVersion?.Sector != x.Sector)
								{
									var newDeliveryPointSectorVersion = x.Clone() as DeliveryPointSectorVersion;
									x.EndDate = DateTime.Today.Date.AddDays(1).AddMilliseconds(-1);
									newDeliveryPointSectorVersion?.FindAndAssociateDistrict(UoW, _sectorRepository);
									DeliveryPointSectorVersions.Add(newDeliveryPointSectorVersion);
								}
							});
						}

						onActiveSectorVersion.Status = SectorsSetStatus.Active;
					}
				}

				var onActiveDeliveryRule =
					sector.SectorDeliveryRuleVersions.SingleOrDefault(x => x.Status == SectorsSetStatus.OnActivation);
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
					if(_commonServices.ValidationService.Validate(onActiveWeekDayScheduleVersion, validationContext))
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
					if(_commonServices.ValidationService.Validate(onActiveWeekDayDeliveryRuleVersion, validationContext))
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
			ObservableSectors.ForEach(x => UoW.Save(x));
			UoW.Commit();
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