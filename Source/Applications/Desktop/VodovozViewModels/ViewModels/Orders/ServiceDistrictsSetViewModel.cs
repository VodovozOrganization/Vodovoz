using GMap.NET;
using MoreLinq;
using NetTopologySuite.Geometries;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.Logistic;
using Vodovoz.ViewModels.TempAdapters;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Service;

namespace Vodovoz.ViewModels.ViewModels.Orders
{
	public sealed class ServiceDistrictsSetViewModel : EntityTabViewModelBase<ServiceDistrictsSet>
	{
		private readonly IInteractiveService _interactiveService;
		private readonly IEntityDeleteWorker _entityDeleteWorker;
		private readonly IDeliveryScheduleJournalFactory _deliveryScheduleJournalFactory;
		private readonly GeometryFactory _geometryFactory;

		private ServiceDistrict _copiedDistrict;

		private List<PointLatLng> _selectedDistrictBorderVertices = new List<PointLatLng>();
		private List<PointLatLng> _newBorderVertices = new List<PointLatLng>();
		private ServiceDistrict _selectedDistrict;
		private ServiceDeliveryScheduleRestriction _selectedScheduleRestriction;

		private bool _isCreatingNewBorder;
		private bool _isToDaySelected;
		private bool _isMondaySelected;
		private bool _isTuesdaySelected;
		private bool _isWednesdaySelected;
		private bool _isThursdaySelected;
		private bool _isFridaySelected;
		private bool _isSaturdaySelected;
		private bool _isSundaySelected;

		private WeekDayName? _selectedWeekDayName;
		private DeliveryScheduleJournalViewModel _deliveryScheduleJournal;
		private SimpleEntityJournalViewModel<AcceptBefore, AcceptBeforeViewModel> _acceptBeforeTimeViewModel;
		private bool _isNewBorderPreviewActive;

		public ServiceDistrictsSetViewModel(IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IEntityDeleteWorker entityDeleteWorker,
			IEmployeeRepository employeeRepository,
			IDeliveryScheduleJournalFactory deliveryScheduleJournalFactory,
			INavigationManager navigation)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			if(commonServices is null)
			{
				throw new ArgumentNullException(nameof(commonServices));
			}

			_interactiveService = commonServices.InteractiveService;

			_entityDeleteWorker = entityDeleteWorker ?? throw new ArgumentNullException(nameof(entityDeleteWorker));
			_deliveryScheduleJournalFactory = deliveryScheduleJournalFactory ?? throw new ArgumentNullException(nameof(deliveryScheduleJournalFactory));

			TabName = "Сервисные районы с графиками доставки";

			if(Entity.Id == 0)
			{
				Entity.Author = employeeRepository.GetEmployeeForCurrentUser(UoW);
				Entity.Status = ServiceDistrictsSetStatus.Draft;
			}

			var districtPermissionResult = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(District));
			CanEditServiceDistrict = districtPermissionResult.CanUpdate && Entity.Status != ServiceDistrictsSetStatus.Active;

			CanEditServiceDeliveryRules = (commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Permissions.ServiceDistrictsSet.CanEditServiceDeliveryRules))
				&& Entity.Status != ServiceDistrictsSetStatus.Active;
			CanDeleteDistrict = (districtPermissionResult.CanDelete || districtPermissionResult.CanCreate && Entity.Id == 0) && Entity.Status != ServiceDistrictsSetStatus.Active;
			CanCreateDistrict = districtPermissionResult.CanCreate && Entity.Status != ServiceDistrictsSetStatus.Active;

			var deliveryScheduleRestrictionPermissionResult =
				commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(DeliveryScheduleRestriction));
			CanEditDeliveryScheduleRestriction =
				deliveryScheduleRestrictionPermissionResult.CanUpdate
					|| deliveryScheduleRestrictionPermissionResult.CanCreate && Entity.Id == 0
				|| CanEditServiceDistrict;

			var districtSetPermissionResult = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(ServiceDistrictsSet));
			CanSave = districtSetPermissionResult.CanUpdate || districtSetPermissionResult.CanCreate && Entity.Id == 0;
			CanEdit = CanSave && Entity.Status != ServiceDistrictsSetStatus.Active;

			_geometryFactory = new GeometryFactory(new PrecisionModel(), 3857);

			#region Command Initialization

			AddDistrictCommand = new DelegateCommand(AddDistrict);
			RemoveDistrictCommand = new DelegateCommand(RemoveDistrict, () => SelectedServiceDistrict != null);
			CreateBorderCommand = new DelegateCommand(CreateBorder, () => !IsCreatingNewBorder);
			ConfirmNewBorderCommand = new DelegateCommand(ConfirmNewBorder, () => IsCreatingNewBorder);
			CancelNewBorderCommand = new DelegateCommand(CancelNewBorder, () => IsCreatingNewBorder);
			RemoveBorderCommand = new DelegateCommand(RemoveBorder, () => !IsCreatingNewBorder);
			AddNewVertexCommand = new DelegateCommand<PointLatLng>(AddNewVertex, point => IsCreatingNewBorder);
			RemoveNewBorderVertexCommand = new DelegateCommand<PointLatLng>(RemoveNewBorderVertex, point => IsCreatingNewBorder && !point.IsEmpty);
			AddScheduleRestrictionCommand = new DelegateCommand(AddScheduleRestriction);
			RemoveScheduleRestrictionCommand = new DelegateCommand(RemoveScheduleRestriction, () => SelectedScheduleRestriction != null);
			AddAcceptBeforeCommand = new DelegateCommand(AddAcceptBefore);
			RemoveAcceptBeforeCommand = new DelegateCommand(RemoveAcceptBefore, () => SelectedScheduleRestriction != null);
			CopyDistrictSchedulesCommand = new DelegateCommand(CopyDistrictSchedules);
			PasteSchedulesToDistrictCommand = new DelegateCommand(PasteSchedulesToSelectedDistrict);
			SaveCommand = new DelegateCommand(Save);
			CancelCommand = new DelegateCommand(Cancel);

			#endregion Command Initialization

			RefreshBordersAction?.Invoke();
		}

		private void Cancel()
		{
			Close(true, CloseSource.Cancel);
		}

		private void AddDistrict()
		{
			var newDistrict = new ServiceDistrict { ServiceDistrictName = "Новый район", ServiceDistrictsSet = Entity };
			Entity.ServiceDistricts.Add(newDistrict);
			SelectedServiceDistrict = newDistrict;

			CreateCommonRules();
			CreateWeekDayRules();
		}

		private void CreateCommonRules()
		{
			foreach(MasterServiceType type in Enum.GetValues(typeof(MasterServiceType)))
			{
				SelectedServiceDistrict.AllServiceDistrictRules.Add(new CommonServiceDistrictRule
				{
					ServiceType = type,
					ServiceDistrict = SelectedServiceDistrict
				});
			}

			OnPropertyChanged(nameof(CommonServiceDistrictRules));
		}

		private void CreateWeekDayRules()
		{
			if(SelectedWeekDayName is null)
			{
				return;
			}

			foreach(MasterServiceType type in Enum.GetValues(typeof(MasterServiceType)))
			{
				foreach(WeekDayName weekDay in Enum.GetValues(typeof(WeekDayName)))
				{
					var isRuleAlreadyExists = WeekDayServiceDistrictRules.Any(x => x.ServiceType == type && x.WeekDay == weekDay /*SelectedWeekDayName*/);

					if(isRuleAlreadyExists)
					{
						continue;
					}

					SelectedServiceDistrict.AllServiceDistrictRules.Add(
						new WeekDayServiceDistrictRule
						{
							ServiceType = type,
							ServiceDistrict = SelectedServiceDistrict,
							WeekDay = weekDay,
							Price = CommonServiceDistrictRules.FirstOrDefault(x => x.ServiceType == type)?.Price ?? 0
						});
				}
			}

			OnPropertyChanged(nameof(WeekDayServiceDistrictRules));
		}

		private bool GetActiveStatusByWeekDay(WeekDayName weekDayName) => SelectedWeekDayName == weekDayName;

		private void CreateBorder()
		{
			IsCreatingNewBorder = true;
			NewBorderVertices.Clear();
		}

		private void RemoveDistrict()
		{
			var distrToDel = SelectedServiceDistrict;

			Entity.ServiceDistricts.Remove(SelectedServiceDistrict);

			if(distrToDel.Id == 0)
			{
				return;
			}


			//Entity.ServiceDistricts.Remove(distrToDel);

			//UoW.Delete(distrToDel);


			if(_entityDeleteWorker.DeleteObject<ServiceDistrict>(distrToDel.Id, UoW))
			{
				SelectedServiceDistrict = null;
				RefreshBordersAction?.Invoke();
				SelectedDistrictBorderVerticesChangedAction?.Invoke();
			}
		}

		private void ConfirmNewBorder()
		{
			if(!_interactiveService.Question("Завершить создание границы района?"))
			{
				return;
			}

			if(NewBorderVertices.Count < 3)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Info, "Нельзя создать границу района меньше чем за 3 точки");
				return;
			}

			IsNewBorderPreviewActive = false;

			var closingPoint = NewBorderVertices[0];
			NewBorderVertices.Add(closingPoint);
			SelectedDistrictBorderVertices = new List<PointLatLng>(NewBorderVertices);
			NewBorderVertices.Clear();
			SelectedServiceDistrict.ServiceDistrictBorder = _geometryFactory.CreatePolygon(SelectedDistrictBorderVertices.Select(p => new Coordinate(p.Lat, p.Lng)).ToArray());
			IsCreatingNewBorder = false;

			RefreshBordersAction?.Invoke();
		}

		private void CancelNewBorder()
		{
			if(!_interactiveService.Question("Отменить создание границы района?"))
			{
				return;
			}

			NewBorderVertices.Clear();
			IsCreatingNewBorder = false;
			OnPropertyChanged(nameof(NewBorderVertices));
			IsNewBorderPreviewActive = false;

			NewBorderVerticiesAction?.Invoke();
		}

		private void RemoveBorder()
		{
			if(!_interactiveService.Question($"Удалить границу района {SelectedServiceDistrict.ServiceDistrictName}?"))
			{
				return;
			}

			SelectedServiceDistrict.ServiceDistrictBorder = null;
			SelectedDistrictBorderVertices.Clear();
			OnPropertyChanged(nameof(SelectedDistrictBorderVertices));
			OnPropertyChanged(nameof(SelectedServiceDistrict));

			RefreshBordersAction?.Invoke();
			SelectedDistrictBorderVerticesChangedAction?.Invoke();
		}

		private void AddNewVertex(PointLatLng point)
		{
			NewBorderVertices.Add(point);
			NewBorderVerticiesAction?.Invoke();
		}

		private void RemoveNewBorderVertex(PointLatLng point)
		{
			NewBorderVertices.Remove(point);
			NewBorderVerticiesAction?.Invoke();
		}

		private void RemoveScheduleRestriction()
		{
			SelectedServiceDistrict.AllServiceDeliveryScheduleRestrictions.Remove(SelectedScheduleRestriction);
			OnPropertyChanged(nameof(ScheduleRestrictions));
		}

		private void RemoveAcceptBefore()
		{
			SelectedScheduleRestriction.AcceptBefore = null;
		}

		private void AddScheduleRestriction()
		{
			_deliveryScheduleJournal = _deliveryScheduleJournalFactory.CreateJournal(JournalSelectionMode.Multiple);
			_deliveryScheduleJournal.OnEntitySelectedResult += OnDeliveryScheduleJournalEntitySelectedResult;
			TabParent.AddSlaveTab(this, _deliveryScheduleJournal);
		}

		private void AddAcceptBefore()
		{
			_acceptBeforeTimeViewModel = new SimpleEntityJournalViewModel<AcceptBefore, AcceptBeforeViewModel>(
			x => x.Name,
			() => new AcceptBeforeViewModel(
				EntityUoWBuilder.ForCreate(),
				UnitOfWorkFactory,
				CommonServices
			),
			node => new AcceptBeforeViewModel(
				EntityUoWBuilder.ForOpen(node.Id),
				UnitOfWorkFactory,
				CommonServices
			),
			UnitOfWorkFactory,
			CommonServices);

			_acceptBeforeTimeViewModel.SelectionMode = JournalSelectionMode.Single;
			_acceptBeforeTimeViewModel.SetActionsVisible(deleteActionEnabled: false, editActionEnabled: false);
			_acceptBeforeTimeViewModel.OnEntitySelectedResult += OnAcceptBeforeSelected;
			TabParent.AddSlaveTab(this, _acceptBeforeTimeViewModel);
		}

		#region Copy Paste District Schedule
		private void CopyDistrictSchedules()
		{
			if(SelectedServiceDistrict == null)
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Error,
					"Для копирования графиков доставки необходимо сначала выбрать район, из которого данные будут скопированы");

				return;
			}

			_copiedDistrict = SelectedServiceDistrict;
		}

		private void PasteSchedulesToSelectedDistrict()
		{
			if(SelectedServiceDistrict == null)
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Error,
					"Для вставки графиков доставки необходимо сначала выбрать район, в который данные будут скопированы");

				return;
			}

			PasteSchedulesToDistrict(SelectedServiceDistrict);

			OnPropertyChanged(nameof(ScheduleRestrictions));
		}

		private void PasteSchedulesToDistrict(ServiceDistrict district)
		{
			if(_copiedDistrict == null)
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Error,
					"Для вставки графиков доставки необходимо сначала скопировать район");

				return;
			}

			var schedulesToSet = new List<ServiceDeliveryScheduleRestriction>();

			foreach(var schedule in _copiedDistrict.GetAllDeliveryScheduleRestrictions())
			{
				var newSchedule = schedule.Clone() as ServiceDeliveryScheduleRestriction;

				newSchedule.ServiceDistrict = district;

				schedulesToSet.Add(newSchedule);
			}

			district.ReplaceServiceDistrictDeliveryScheduleRestrictions(schedulesToSet);
		}

		#endregion Copy Paste District Schedule

		private void Save()
		{
			Save(false);
		}

		private void OnDeliveryScheduleJournalEntitySelectedResult(object sender, JournalSelectedNodesEventArgs e)
		{
			var selectedNodes = e.SelectedNodes.OfType<DeliveryScheduleJournalNode>();

			if(!selectedNodes.Any())
			{
				return;
			}

			foreach(var selectedNode in selectedNodes)
			{
				if(!SelectedWeekDayName.HasValue || ScheduleRestrictions.Any(x => x.DeliverySchedule.Id == selectedNode.Id))
				{
					continue;
				}

				var deliverySchedule = UoW.GetById<DeliverySchedule>(selectedNode.Id);
				SelectedServiceDistrict?.AllServiceDeliveryScheduleRestrictions.Add(new ServiceDeliveryScheduleRestriction
				{
					ServiceDistrict = SelectedServiceDistrict,
					WeekDay = SelectedWeekDayName.Value,
					DeliverySchedule = deliverySchedule
				});

				OnPropertyChanged(nameof(ScheduleRestrictions));
			}
		}

		private void OnAcceptBeforeSelected(object sender, JournalSelectedNodesEventArgs eventArgs)
		{
			var node = eventArgs.SelectedNodes.FirstOrDefault();

			if(node != null)
			{
				var acceptBefore = UoW.GetById<AcceptBefore>(node.Id);

				if(acceptBefore != null && SelectedScheduleRestriction != null)
				{

					SelectedScheduleRestriction.AcceptBefore = acceptBefore;
				}
			}
		}

		public bool CanEditServiceDistrict { get; }
		public bool CanEditServiceDeliveryRules { get; }
		public bool CanEditDeliveryScheduleRestriction { get; }
		public bool CanDeleteDistrict { get; }
		public bool CanCreateDistrict { get; }
		public bool CanSave { get; }
		public bool CanEdit { get; }

		public bool CanCopyDeliveryScheduleRestrictions =>
			CanEditDeliveryScheduleRestriction
			&& SelectedServiceDistrict != null;

		public bool CanPasteDeliveryScheduleRestrictions =>
			CanEditDeliveryScheduleRestriction
			&& SelectedServiceDistrict != null
			&& _copiedDistrict != null;

		public string CopyDistrictScheduleMenuItemLabel =>
			$"Копировать график доставки {SelectedServiceDistrict?.Id} {SelectedServiceDistrict?.ServiceDistrictName}";

		public string PasteScheduleToDistrictMenuItemLabel =>
			$"Вставить график доставки {_copiedDistrict?.Id} {_copiedDistrict?.ServiceDistrictName} В {SelectedServiceDistrict?.Id} {SelectedServiceDistrict?.ServiceDistrictName}";

		public IList<ServiceDeliveryScheduleRestriction> ScheduleRestrictions => SelectedWeekDayName.HasValue && SelectedServiceDistrict != null
			? SelectedServiceDistrict.GetServiceScheduleRestrictionsByWeekDay(SelectedWeekDayName.Value)
			: null;

		public List<PointLatLng> SelectedDistrictBorderVertices
		{
			get => _selectedDistrictBorderVertices;
			set
			{
				if(SetField(ref _selectedDistrictBorderVertices, value) && value != null)
				{
					SelectedDistrictBorderVerticesChangedAction?.Invoke();
				}
			}
		}

		public List<PointLatLng> NewBorderVertices
		{
			get => _newBorderVertices;
			set
			{
				if(SetField(ref _newBorderVertices, value))
				{
					NewBorderVerticiesAction?.Invoke();
				}
			}
		}

		public ServiceDistrict SelectedServiceDistrict
		{
			get => _selectedDistrict;
			set
			{
				if(!SetField(ref _selectedDistrict, value))
				{
					return;
				}

				SelectedDistrictBorderVertices.Clear();

				if(_selectedDistrict != null)
				{
					if(!SelectedWeekDayName.HasValue)
					{
						IsToDaySelected = true;
					}
					else
					{
						OnPropertyChanged(nameof(WeekDayServiceDistrictRules));
						OnPropertyChanged(nameof(ScheduleRestrictions));
					}

					if(SelectedServiceDistrict.ServiceDistrictBorder?.Coordinates != null)
					{
						foreach(Coordinate coord in SelectedServiceDistrict.ServiceDistrictBorder.Coordinates)
						{
							SelectedDistrictBorderVertices.Add(new PointLatLng
							{
								Lat = coord.X,
								Lng = coord.Y
							});
						}
					}

					OnPropertyChanged(nameof(CommonServiceDistrictRules));
					OnPropertyChanged(nameof(SelectedGeoGroup));

					SelectedDistrictBorderVerticesChangedAction?.Invoke();
				}

				OnPropertyChanged(nameof(SelectedDistrictBorderVertices));
				OnPropertyChanged(nameof(WeekDayServiceDistrictRules));
				OnPropertyChanged(nameof(ScheduleRestrictions));
			}
		}

		public GeoGroup SelectedGeoGroup
		{
			get => SelectedServiceDistrict?.GeographicGroup;
			set
			{
				if(SelectedServiceDistrict.GeographicGroup != value)
				{
					SelectedServiceDistrict.GeographicGroup = value;
					OnPropertyChanged(nameof(SelectedGeoGroup));
				}
			}
		}

		[PropertyChangedAlso(nameof(ScheduleRestrictions), nameof(WeekDayServiceDistrictRules))]
		public WeekDayName? SelectedWeekDayName
		{
			get => _selectedWeekDayName;
			set
			{
				if(SetField(ref _selectedWeekDayName, value))
				{
					SelectedWeekDayChangedAction?.Invoke();
				}
			}
		}

		public ServiceDeliveryScheduleRestriction SelectedScheduleRestriction
		{
			get => _selectedScheduleRestriction;
			set => SetField(ref _selectedScheduleRestriction, value);
		}

		public bool IsCreatingNewBorder
		{
			get => _isCreatingNewBorder;
			private set
			{
				if(value && SelectedServiceDistrict == null)
				{
					throw new ArgumentNullException(nameof(SelectedServiceDistrict));
				}

				SetField(ref _isCreatingNewBorder, value);
			}
		}

		public IList<WeekDayServiceDistrictRule> WeekDayServiceDistrictRules =>
			SelectedServiceDistrict?.AllServiceDistrictRules
			.Where(x => x is WeekDayServiceDistrictRule && (x as WeekDayServiceDistrictRule).WeekDay == SelectedWeekDayName)
			.Cast<WeekDayServiceDistrictRule>().ToList();

		public IList<CommonServiceDistrictRule> CommonServiceDistrictRules =>
			SelectedServiceDistrict?.AllServiceDistrictRules.Where(x => x is CommonServiceDistrictRule)
			.Cast<CommonServiceDistrictRule>()
			.ToList();

		#region Commands

		public DelegateCommand CopyDistrictSchedulesCommand { get; }
		public DelegateCommand PasteSchedulesToDistrictCommand { get; }
		public DelegateCommand AddAcceptBeforeCommand { get; }
		public DelegateCommand RemoveAcceptBeforeCommand { get; }
		public DelegateCommand<PointLatLng> RemoveNewBorderVertexCommand { get; }
		public DelegateCommand<PointLatLng> AddNewVertexCommand { get; }
		public DelegateCommand AddScheduleRestrictionCommand { get; }
		public DelegateCommand RemoveScheduleRestrictionCommand { get; }
		public DelegateCommand SaveCommand { get; }
		public DelegateCommand CancelCommand { get; }
		public DelegateCommand AddDistrictCommand { get; }
		public DelegateCommand RemoveDistrictCommand { get; }
		public DelegateCommand CreateBorderCommand { get; }
		public DelegateCommand ConfirmNewBorderCommand { get; }
		public DelegateCommand CancelNewBorderCommand { get; }
		public DelegateCommand RemoveBorderCommand { get; }

		#endregion

		public Action RefreshBordersAction { get; set; }

		public override bool Save(bool close)
		{
			if(Entity.Id == 0)
			{
				Entity.DateCreated = DateTime.Now;
			}

			if(base.Save(close))
			{
				if(!_interactiveService.Question("Продолжить редактирование районов?", "Успешно сохранено"))
				{
					Close(false, CloseSource.Save);
				}

				return true;
			}
			return false;
		}

		public override void Close(bool askSave, CloseSource source)
		{
			if(askSave)
			{
				TabParent?.AskToCloseTab(this, source);
			}
			else
			{
				TabParent?.ForceCloseTab(this, source);
			}
		}

		public override bool HasChanges
		{
			get => base.HasChanges && (CanEditServiceDistrict || CanSave);
			set => base.HasChanges = value;
		}

		#region Days
		public bool IsToDaySelected
		{
			get => GetActiveStatusByWeekDay(WeekDayName.Today);
			set => SelectedWeekDayName = WeekDayName.Today;
		}

		public bool IsMondaySelected
		{
			get => GetActiveStatusByWeekDay(WeekDayName.Monday);
			set => SelectedWeekDayName = WeekDayName.Monday;
		}

		public bool IsTuesdaySelected
		{
			get => GetActiveStatusByWeekDay(WeekDayName.Tuesday);
			set => SelectedWeekDayName = WeekDayName.Tuesday;
		}

		public bool IsToWednesdaySelected
		{
			get => GetActiveStatusByWeekDay(WeekDayName.Wednesday);
			set => SelectedWeekDayName = WeekDayName.Wednesday;
		}

		public bool IsThursdaySelected
		{
			get => GetActiveStatusByWeekDay(WeekDayName.Thursday);
			set => SelectedWeekDayName = WeekDayName.Thursday;
		}

		public bool IsFridaySelected
		{
			get => GetActiveStatusByWeekDay(WeekDayName.Friday);
			set => SelectedWeekDayName = WeekDayName.Friday;
		}

		public bool IsSaturdaySelected
		{
			get => GetActiveStatusByWeekDay(WeekDayName.Saturday);
			set => SelectedWeekDayName = WeekDayName.Saturday;
		}

		public bool IsSundaySelected
		{
			get => GetActiveStatusByWeekDay(WeekDayName.Sunday);
			set => SelectedWeekDayName = WeekDayName.Sunday;
		}

		public bool IsNewBorderPreviewActive
		{
			get => _isNewBorderPreviewActive;
			set => SetField(ref _isNewBorderPreviewActive, value);
		}
		public Action SelectedWeekDayChangedAction { get; set; }
		public Action SelectedDistrictBorderVerticesChangedAction { get; set; }
		public Action NewBorderVerticiesAction { get; set; }

		#endregion Days

		public override void Dispose()
		{
			if(_deliveryScheduleJournal != null)
			{
				_deliveryScheduleJournal.OnEntitySelectedResult -= OnDeliveryScheduleJournalEntitySelectedResult;
			}

			if(_acceptBeforeTimeViewModel != null)
			{
				_acceptBeforeTimeViewModel.OnEntitySelectedResult -= OnAcceptBeforeSelected;
			}

			UoW?.Dispose();
			base.Dispose();
		}
	}
}
