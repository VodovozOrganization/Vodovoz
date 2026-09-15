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
using System.Text;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Presentation.ViewModels.Logistic;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
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
		private AcceptBeforeJournalViewModel _acceptBeforeJournalViewModel;
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

			var districtPermissionResult = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(ServiceDistrict));
			CanEditServiceDistrict = districtPermissionResult.CanUpdate && Entity.Status != ServiceDistrictsSetStatus.Active;

			CanEditServiceDeliveryRules = (commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.ServiceDistrictsSetPermissions.CanEditServiceDeliveryRules))
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

			AddDistrictCommand = new DelegateCommand(AddDistrict, () => CanCreateDistrict);
			AddDistrictCommand.CanExecuteChangedWith(this, x => x.CanCreateDistrict);

			RemoveDistrictCommand = new DelegateCommand(RemoveDistrict, () => CanRemoveDistrict);
			RemoveDistrictCommand.CanExecuteChangedWith(this, x => x.CanRemoveDistrict);

			CreateBorderCommand = new DelegateCommand(CreateBorder, () => CanCreateBorder);
			CreateBorderCommand.CanExecuteChangedWith(this, x => x.CanCreateBorder);

			RemoveBorderCommand = new DelegateCommand(RemoveBorder, () => CanRemoveBorder);
			RemoveBorderCommand.CanExecuteChangedWith(this, x => x.CanRemoveBorder);

			ConfirmNewBorderCommand = new DelegateCommand(ConfirmNewBorder, () => CanConfirmNewBorder);
			ConfirmNewBorderCommand.CanExecuteChangedWith(this, x => x.CanConfirmNewBorder);

			CancelNewBorderCommand = new DelegateCommand(CancelNewBorder, () => CanConfirmNewBorder);
			CancelNewBorderCommand.CanExecuteChangedWith(this, x => x.CanConfirmNewBorder);

			AddNewVertexCommand = new DelegateCommand<PointLatLng>(AddNewVertex, point => IsCreatingNewBorder);
			AddNewVertexCommand.CanExecuteChangedWith(this, x => x.IsCreatingNewBorder);

			RemoveNewBorderVertexCommand = new DelegateCommand<PointLatLng>(RemoveNewBorderVertex, point => IsCreatingNewBorder && !point.IsEmpty);
			RemoveNewBorderVertexCommand.CanExecuteChangedWith(this, x => x.IsCreatingNewBorder);

			AddScheduleRestrictionCommand = new DelegateCommand(AddScheduleRestriction, () => CanAddScheduleRestriction);
			AddScheduleRestrictionCommand.CanExecuteChangedWith(this, x => x.CanAddScheduleRestriction);

			RemoveScheduleRestrictionCommand = new DelegateCommand(RemoveScheduleRestriction, () => CanEditSchedule);
			RemoveScheduleRestrictionCommand.CanExecuteChangedWith(this, x => x.CanEditSchedule);

			AddAcceptBeforeCommand = new DelegateCommand(AddAcceptBefore, () => CanEditSchedule);
			AddAcceptBeforeCommand.CanExecuteChangedWith(this, x => x.CanEditSchedule);

			RemoveAcceptBeforeCommand = new DelegateCommand(RemoveAcceptBefore, () => CanEditSchedule);
			RemoveAcceptBeforeCommand.CanExecuteChangedWith(this, x => x.CanEditSchedule);

			CopyDistrictSchedulesCommand = new DelegateCommand(CopyDistrictSchedules);
			PasteSchedulesToDistrictCommand = new DelegateCommand(PasteSchedulesToSelectedDistrict);

			SaveCommand = new DelegateCommand(Save, () => CanSave);
			SaveCommand.CanExecuteChangedWith(this, x => x.CanSave);

			CancelCommand = new DelegateCommand(Cancel);

			#endregion Command Initialization

			RefreshBordersAction?.Invoke();
		}

		[PropertyChangedAlso(nameof(CanCreateBorder), nameof(CanRemoveBorder))]
		public bool CanEditServiceDistrict { get; }
		public bool CanEditServiceDeliveryRules { get; }

		[PropertyChangedAlso(nameof(CanAddScheduleRestriction), nameof(CanEditSchedule))]
		public bool CanEditDeliveryScheduleRestriction { get; }

		[PropertyChangedAlso(nameof(CanRemoveDistrict))]
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

		public bool CanRemoveDistrict => SelectedServiceDistrict != null && CanDeleteDistrict;

		public bool CanCreateBorder =>
			CanEditServiceDistrict
			&& !IsCreatingNewBorder
			&& SelectedServiceDistrict != null
			&& SelectedServiceDistrict.ServiceDistrictBorder == null;

		public bool CanRemoveBorder =>
			CanEditServiceDistrict
			&& !IsCreatingNewBorder
			&& SelectedServiceDistrict != null
			&& SelectedServiceDistrict.ServiceDistrictBorder != null;

		public bool CanEditSchedule => SelectedScheduleRestriction != null
			&& CanEditDeliveryScheduleRestriction
			&& SelectedServiceDistrict != null
			&& SelectedScheduleRestriction != null;

		public bool CanAddScheduleRestriction =>
			CanEditDeliveryScheduleRestriction
			&& SelectedServiceDistrict != null
			&& SelectedWeekDayName.HasValue;

		public bool CanConfirmNewBorder => SelectedServiceDistrict != null && IsCreatingNewBorder;

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

		[PropertyChangedAlso(nameof(CanRemoveDistrict), nameof(CanAddScheduleRestriction), nameof(CanEditSchedule), nameof(CanCreateBorder),
			nameof(CanConfirmNewBorder), nameof(CanRemoveBorder))]
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

		[PropertyChangedAlso(nameof(ScheduleRestrictions), nameof(WeekDayServiceDistrictRules), nameof(CanAddScheduleRestriction))]
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

		[PropertyChangedAlso(nameof(CanEditSchedule))]
		public ServiceDeliveryScheduleRestriction SelectedScheduleRestriction
		{
			get => _selectedScheduleRestriction;
			set => SetField(ref _selectedScheduleRestriction, value);
		}

		[PropertyChangedAlso(nameof(CanCreateBorder), nameof(CanConfirmNewBorder), nameof(CanRemoveBorder))]
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

			//Пока не используем
			//CreateWeekDayRules();
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
			var districtToDelete = SelectedServiceDistrict;

			if(districtToDelete.Id == 0)
			{
				return;
			}

			UoW.Delete(districtToDelete);

			Entity.ServiceDistricts.Remove(SelectedServiceDistrict);

			SelectedServiceDistrict = null;
			RefreshBordersAction?.Invoke();
			SelectedDistrictBorderVerticesChangedAction?.Invoke();
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
			var acceptBeforePage = NavigationManager.OpenViewModel<AcceptBeforeJournalViewModel>(null);
			_acceptBeforeJournalViewModel = acceptBeforePage.ViewModel;
			_acceptBeforeJournalViewModel.VisibleEditAction = false;
			_acceptBeforeJournalViewModel.VisibleDeleteAction = false;
			_acceptBeforeJournalViewModel.SelectionMode = JournalSelectionMode.Single;
			_acceptBeforeJournalViewModel.OnSelectResult += OnAcceptBeforeJournalViewModelSelectResult;
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
			var zeroPriceDistrictsStringBuilder = new StringBuilder();

			foreach(var serviceDistrict in Entity.ServiceDistricts)
			{
				if(serviceDistrict.AllServiceDistrictRules.Any(
					x => x is CommonServiceDistrictRule
					&& x.Price == 0))
				{
					zeroPriceDistrictsStringBuilder.AppendLine($"- {serviceDistrict.ServiceDistrictName}");
				}
			}

			if(zeroPriceDistrictsStringBuilder.Length > 0
				&& !_interactiveService.Question($"Для следующих райнов указаны не все цены, продолжить сохранение?\n{zeroPriceDistrictsStringBuilder}"))
			{
				return;
			}

			Save(false);
		}

		private void OnAcceptBeforeJournalViewModelSelectResult(object sender, JournalSelectedEventArgs e)
		{
			var node = e.GetSelectedObjects<AcceptBefore>().FirstOrDefault();

			if(node != null)
			{
				var acceptBefore = UoW.GetById<AcceptBefore>(node.Id);

				if(acceptBefore != null && SelectedScheduleRestriction != null)
				{

					SelectedScheduleRestriction.AcceptBefore = acceptBefore;
				}
			}
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

		public override void Dispose()
		{
			if(_deliveryScheduleJournal != null)
			{
				_deliveryScheduleJournal.OnEntitySelectedResult -= OnDeliveryScheduleJournalEntitySelectedResult;
			}

			if(_acceptBeforeJournalViewModel != null)
			{
				_acceptBeforeJournalViewModel.OnSelectResult -= OnAcceptBeforeJournalViewModelSelectResult;
			}

			UoW?.Dispose();
			base.Dispose();
		}
	}
}
