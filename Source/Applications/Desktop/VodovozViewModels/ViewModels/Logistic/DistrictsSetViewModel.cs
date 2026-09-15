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
using QS.Utilities.Text;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Sale;
using Vodovoz.Presentation.ViewModels.Logistic;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Sale;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.ViewModels.Logistic
{
	public sealed class DistrictsSetViewModel : EntityTabViewModelBase<DistrictsSet>
	{
		private readonly IEntityDeleteWorker _entityDeleteWorker;
		private readonly IDeliveryScheduleJournalFactory _deliveryScheduleJournalFactory;
		private readonly GeometryFactory _geometryFactory;

		public readonly bool CanChangeDistrictWageTypePermissionResult;
		public readonly bool CanEditDistrict;
		public readonly bool CanEditDeliveryRules;

		public readonly bool CanDeleteDistrict;
		public readonly bool CanCreateDistrict;
		public readonly bool CanSave;
		public readonly bool CanEdit;

		private AcceptBeforeJournalViewModel _acceptBeforeJournalViewModel;

		private District _copiedDistrict;

		private GenericObservableList<PointLatLng> _selectedDistrictBorderVertices;
		private GenericObservableList<PointLatLng> _newBorderVertices;
		private District _selectedDistrict;
		private WeekDayName? _selectedWeekDayName;
		private CommonDistrictRuleItem _selectedCommonDistrictRuleItem;
		private WeekDayDistrictRuleItem _selectedWeekDayDistrictRuleItem;
		private DeliveryScheduleRestriction _selectedScheduleRestriction;
		private bool _isCreatingNewBorder;

		public DistrictsSetViewModel(IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IEntityDeleteWorker entityDeleteWorker,
			IEmployeeRepository employeeRepository,
			IDistrictRuleRepository districtRuleRepository,
			IDeliveryScheduleJournalFactory deliveryScheduleJournalFactory,
			INavigationManager navigation)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			_entityDeleteWorker = entityDeleteWorker ?? throw new ArgumentNullException(nameof(entityDeleteWorker));
			DistrictRuleRepository = districtRuleRepository ?? throw new ArgumentNullException(nameof(districtRuleRepository));
			_deliveryScheduleJournalFactory = deliveryScheduleJournalFactory ?? throw new ArgumentNullException(nameof(deliveryScheduleJournalFactory));

			TabName = "Районы с графиками доставки";

			CanChangeDistrictWageTypePermissionResult = commonServices.CurrentPermissionService.ValidatePresetPermission("can_change_district_wage_type");

			if(Entity.Id == 0)
			{
				Entity.Author = employeeRepository.GetEmployeeForCurrentUser(UoW);
				Entity.Status = DistrictsSetStatus.Draft;
			}

			var districtPermissionResult = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(District));
			CanEditDistrict = districtPermissionResult.CanUpdate && Entity.Status != DistrictsSetStatus.Active;
			CanEditDeliveryRules = (CanEditDistrict || commonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_delivery_rules")) && Entity.Status != DistrictsSetStatus.Active;
			CanDeleteDistrict = (districtPermissionResult.CanDelete || districtPermissionResult.CanCreate && Entity.Id == 0) && Entity.Status != DistrictsSetStatus.Active;
			CanCreateDistrict = districtPermissionResult.CanCreate && Entity.Status != DistrictsSetStatus.Active;

			var deliveryScheduleRestrictionPermissionResult =
				commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(DeliveryScheduleRestriction));
			CanEditDeliveryScheduleRestriction =
				(deliveryScheduleRestrictionPermissionResult.CanUpdate
					|| deliveryScheduleRestrictionPermissionResult.CanCreate && Entity.Id == 0)
				|| CanEditDistrict;

			var districtSetPermissionResult = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(DistrictsSet));
			CanSave = districtSetPermissionResult.CanUpdate || districtSetPermissionResult.CanCreate && Entity.Id == 0;
			CanEdit = CanSave && Entity.Status != DistrictsSetStatus.Active;

			SortDistricts();

			_geometryFactory = new GeometryFactory(new PrecisionModel(), 3857);
			SelectedDistrictBorderVertices = new GenericObservableList<PointLatLng>();
			NewBorderVertices = new GenericObservableList<PointLatLng>();

			#region Command Initialization

			AddDistrictCommand = CreateAddDistrictCommand();
			RemoveDistrictCommand = CreateRemoveDistrictCommand();
			CreateBorderCommand = CreateCreateBorderCommand();
			ConfirmNewBorderCommand = CreateConfirmNewBorderCommand();
			CancelNewBorderCommand = CreateCancelNewBorderCommand();
			RemoveBorderCommand = CreateRemoveBorderCommand();
			AddNewVertexCommand = CreateAddNewVertexCommand();
			RemoveNewBorderVertexCommand = CreateRemoveNewBorderVertexCommand();
			AddScheduleRestrictionCommand = CreateAddScheduleRestrictionCommand();
			RemoveScheduleRestrictionCommand = CreateRemoveScheduleRestrictionCommand();
			RemoveWeekDayDistrictRuleItemCommand = CreateRemoveWeekDayDistrictRuleItemCommand();
			RemoveCommonDistrictRuleItemCommand = CreateRemoveCommonDistrictRuleItemCommand();
			AddWeekDayDeliveryPriceRuleCommand = CreateAddWeekDayDeliveryPriceRuleCommand();
			AddCommonDeliveryPriceRuleCommand = CreateAddCommonDeliveryPriceRuleCommand();

			AddAcceptBeforeCommand = new DelegateCommand(AddAcceptBefore, () => CanEditSchedule);
			AddAcceptBeforeCommand.CanExecuteChangedWith(this, x => x.CanEditSchedule);

			RemoveAcceptBeforeCommand = CreateRemoveAcceptBeforeCommand();
			CopyDistrictSchedulesCommand = new DelegateCommand(CopyDistrictSchedules);
			PasteSchedulesToDistrictCommand = new DelegateCommand(PasteSchedulesToSelectedDistrict);
			PasteSchedulesToZoneCommand = new DelegateCommand(PasteSchedulesToZone);

			#endregion Command Initialization
		}

		public IUnitOfWorkFactory UoWFactory => UnitOfWorkFactory;

		public bool CanEditSchedule => SelectedScheduleRestriction != null
			&& CanEditDeliveryScheduleRestriction
			&& SelectedDistrict != null
			&& SelectedScheduleRestriction != null;

		public bool CanCopyDeliveryScheduleRestrictions =>
			CanEditDeliveryScheduleRestriction
			&& SelectedDistrict != null;

		public bool CanPasteDeliveryScheduleRestrictions =>
			CanEditDeliveryScheduleRestriction
			&& SelectedDistrict != null
			&& _copiedDistrict != null;

		public string CopyDistrictScheduleMenuItemLabel =>
			$"Копировать график доставки {SelectedDistrict?.Id} {SelectedDistrict?.DistrictName}";

		public string PasteScheduleToDistrictMenuItemLabel =>
			$"Вставить график доставки {_copiedDistrict?.Id} {_copiedDistrict?.DistrictName} В {SelectedDistrict?.Id} {SelectedDistrict?.DistrictName}";

		public string PasteScheduleToTafiffZoneMenuItemLabel =>
			$"Вставить график доставки {_copiedDistrict?.Id} {_copiedDistrict?.DistrictName} для {SelectedDistrict?.TariffZone.Name}";

		public IDistrictRuleRepository DistrictRuleRepository { get; }
		public GenericObservableList<DeliveryScheduleRestriction> ScheduleRestrictions => SelectedWeekDayName.HasValue && SelectedDistrict != null
			? SelectedDistrict.GetScheduleRestrictionCollectionByWeekDayName(SelectedWeekDayName.Value)
			: null;

		public GenericObservableList<CommonDistrictRuleItem> CommonDistrictRuleItems => SelectedDistrict.CommonDistrictRuleItems;

		public GenericObservableList<WeekDayDistrictRuleItem> WeekDayDistrictRuleItems => SelectedWeekDayName.HasValue && SelectedDistrict != null
			? SelectedDistrict.GetWeekDayRuleItemCollectionByWeekDayName(SelectedWeekDayName.Value)
			: null;

		public GenericObservableList<PointLatLng> SelectedDistrictBorderVertices
		{
			get => _selectedDistrictBorderVertices;
			set => SetField(ref _selectedDistrictBorderVertices, value);
		}

		public GenericObservableList<PointLatLng> NewBorderVertices
		{
			get => _newBorderVertices;
			set => SetField(ref _newBorderVertices, value);
		}

		[PropertyChangedAlso(nameof(CanEditSchedule))]
		public District SelectedDistrict
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
						SelectedWeekDayName = WeekDayName.Today;
					}
					else
					{
						OnPropertyChanged(nameof(WeekDayDistrictRuleItems));
						OnPropertyChanged(nameof(ScheduleRestrictions));
					}
					if(SelectedDistrict.DistrictBorder?.Coordinates != null)
					{
						foreach(Coordinate coord in SelectedDistrict.DistrictBorder.Coordinates)
						{
							SelectedDistrictBorderVertices.Add(new PointLatLng
							{
								Lat = coord.X,
								Lng = coord.Y
							});
						}
					}

					OnPropertyChanged(nameof(CommonDistrictRuleItems));
					OnPropertyChanged(nameof(SelectedGeoGroup));
					OnPropertyChanged(nameof(SelectedWageDistrict));
				}
				OnPropertyChanged(nameof(SelectedDistrictBorderVertices));
			}
		}

		public GeoGroup SelectedGeoGroup
		{
			get => SelectedDistrict?.GeographicGroup;
			set
			{
				if(SelectedDistrict.GeographicGroup != value)
				{
					SelectedDistrict.GeographicGroup = value;
					OnPropertyChanged(nameof(SelectedGeoGroup));
				}
			}
		}

		public WageDistrict SelectedWageDistrict
		{
			get => SelectedDistrict?.WageDistrict;
			set
			{
				if(SelectedDistrict.WageDistrict != value)
				{
					SelectedDistrict.WageDistrict = value;
					OnPropertyChanged(nameof(SelectedWageDistrict));
				}
			}
		}

		public WeekDayName? SelectedWeekDayName
		{
			get => _selectedWeekDayName;
			set
			{
				if(SetField(ref _selectedWeekDayName, value))
				{
					if(SelectedWeekDayName != null)
					{
						OnPropertyChanged(nameof(ScheduleRestrictions));
						OnPropertyChanged(nameof(WeekDayDistrictRuleItems));
					}
				}
			}
		}

		public CommonDistrictRuleItem SelectedCommonDistrictRuleItem
		{
			get => _selectedCommonDistrictRuleItem;
			set => SetField(ref _selectedCommonDistrictRuleItem, value);
		}

		public WeekDayDistrictRuleItem SelectedWeekDayDistrictRuleItem
		{
			get => _selectedWeekDayDistrictRuleItem;
			set => SetField(ref _selectedWeekDayDistrictRuleItem, value);
		}

		[PropertyChangedAlso(nameof(CanEditSchedule))]
		public DeliveryScheduleRestriction SelectedScheduleRestriction
		{
			get => _selectedScheduleRestriction;
			set => SetField(ref _selectedScheduleRestriction, value);
		}

		[PropertyChangedAlso(nameof(CanEditSchedule))]
		public bool CanEditDeliveryScheduleRestriction { get; }

		public bool IsCreatingNewBorder
		{
			get => _isCreatingNewBorder;
			private set
			{
				if(value && SelectedDistrict == null)
				{
					throw new ArgumentNullException(nameof(SelectedDistrict));
				}

				SetField(ref _isCreatingNewBorder, value);
			}
		}

		#region Commands

		public DelegateCommand AddAcceptBeforeCommand { get; }

		public DelegateCommand AddDistrictCommand { get; }

		private DelegateCommand CreateAddDistrictCommand() =>
			new DelegateCommand(
				() =>
				{
					var newDistrict = new District { PriceType = DistrictWaterPrice.Standart, DistrictName = "Новый район", DistrictsSet = Entity };
					Entity.ObservableDistricts.Add(newDistrict);
					SelectedDistrict = newDistrict;
				},
				() => true);

		public DelegateCommand RemoveDistrictCommand { get; }

		private DelegateCommand CreateRemoveDistrictCommand() =>
			new DelegateCommand(
				() =>
				{
					var distrToDel = _selectedDistrict;
					Entity.ObservableDistricts.Remove(SelectedDistrict);
					if(distrToDel.Id == 0)
					{
						return;
					}

					if(_entityDeleteWorker.DeleteObject<District>(distrToDel.Id, UoW))
					{
						SelectedDistrict = null;
					}
					else
					{
						Entity.ObservableDistricts.Add(distrToDel);
						SelectedDistrict = distrToDel;
					}
				},
				() => SelectedDistrict != null);

		public DelegateCommand CreateBorderCommand { get; }

		private DelegateCommand CreateCreateBorderCommand() =>
			new DelegateCommand(
				() =>
				{
					IsCreatingNewBorder = true;
					NewBorderVertices.Clear();
				},
				() => !IsCreatingNewBorder);

		public DelegateCommand ConfirmNewBorderCommand { get; }

		private DelegateCommand CreateConfirmNewBorderCommand() =>
			 new DelegateCommand(
				() =>
				{
					if(NewBorderVertices.Count < 3)
					{
						return;
					}

					var closingPoint = NewBorderVertices[0];
					NewBorderVertices.Add(closingPoint);
					SelectedDistrictBorderVertices = new GenericObservableList<PointLatLng>(NewBorderVertices.ToList());
					NewBorderVertices.Clear();
					SelectedDistrict.DistrictBorder = _geometryFactory.CreatePolygon(SelectedDistrictBorderVertices.Select(p => new Coordinate(p.Lat, p.Lng)).ToArray());
					IsCreatingNewBorder = false;
				},
				() => IsCreatingNewBorder);

		public DelegateCommand CancelNewBorderCommand { get; }

		public DelegateCommand CreateCancelNewBorderCommand() =>
			new DelegateCommand(
				() =>
				{
					NewBorderVertices.Clear();
					IsCreatingNewBorder = false;
					OnPropertyChanged(nameof(NewBorderVertices));
				},
				() => IsCreatingNewBorder);

		public DelegateCommand RemoveBorderCommand { get; }

		private DelegateCommand CreateRemoveBorderCommand() =>
			new DelegateCommand(
				() =>
				{
					SelectedDistrict.DistrictBorder = null;
					SelectedDistrictBorderVertices.Clear();
					OnPropertyChanged(nameof(SelectedDistrictBorderVertices));
					OnPropertyChanged(nameof(SelectedDistrict));
				},
				() => !IsCreatingNewBorder);

		public DelegateCommand<PointLatLng> AddNewVertexCommand { get; }

		private DelegateCommand<PointLatLng> CreateAddNewVertexCommand() =>
			new DelegateCommand<PointLatLng>(
				point =>
				{
					NewBorderVertices.Add(point);
					OnPropertyChanged(nameof(NewBorderVertices));
				},
				point => IsCreatingNewBorder);

		public DelegateCommand<PointLatLng> RemoveNewBorderVertexCommand { get; }

		private DelegateCommand<PointLatLng> CreateRemoveNewBorderVertexCommand() =>
			new DelegateCommand<PointLatLng>(
				point =>
				{
					NewBorderVertices.Remove(point);
					OnPropertyChanged(nameof(NewBorderVertices));
				},
				point => IsCreatingNewBorder && !point.IsEmpty);

		public DelegateCommand AddScheduleRestrictionCommand { get; }

		private DelegateCommand CreateAddScheduleRestrictionCommand() => new DelegateCommand(AddScheduleRestriction);

		public DelegateCommand RemoveScheduleRestrictionCommand { get; }

		private DelegateCommand CreateRemoveScheduleRestrictionCommand() =>
			new DelegateCommand(
				() =>
				{
					ScheduleRestrictions.Remove(SelectedScheduleRestriction);
				},
				() => SelectedScheduleRestriction != null);

		public DelegateCommand RemoveWeekDayDistrictRuleItemCommand { get; }

		private DelegateCommand CreateRemoveWeekDayDistrictRuleItemCommand() =>
			new DelegateCommand(
				() =>
				{
					WeekDayDistrictRuleItems.Remove(SelectedWeekDayDistrictRuleItem);
				},
				() => SelectedWeekDayDistrictRuleItem != null);

		public DelegateCommand RemoveCommonDistrictRuleItemCommand { get; }

		private DelegateCommand CreateRemoveCommonDistrictRuleItemCommand() =>
			new DelegateCommand(
				() =>
				{
					CommonDistrictRuleItems.Remove(SelectedCommonDistrictRuleItem);
				},
				() => SelectedCommonDistrictRuleItem != null);

		#region Команда добавления правила цен доставки дня недели

		public DelegateCommand AddWeekDayDeliveryPriceRuleCommand { get; }

		private DelegateCommand CreateAddWeekDayDeliveryPriceRuleCommand()
		{
			var command = new DelegateCommand(AddWeekDayDeliveryPriceRule, () => CanAddWeekDayDeliveryPriceRuleCommand);
			command.CanExecuteChangedWith(this, x => x.CanAddWeekDayDeliveryPriceRuleCommand);

			return command;
		}

		private bool CanAddWeekDayDeliveryPriceRuleCommand => CanEditDeliveryRules;

		private void AddWeekDayDeliveryPriceRule()
		{
			NavigationManager.OpenViewModel<DeliveryPriceRuleJournalViewModel>(
				this,
				OpenPageOptions.AsSlave,
				viewModel =>
				{
					viewModel.SelectionMode = JournalSelectionMode.Single;
					viewModel.OnSelectResult += JournalOnWeekDayEntitySelectedResult;
				});
		}

		private void JournalOnWeekDayEntitySelectedResult(object sender, JournalSelectedEventArgs e)
		{
			var node = e.SelectedObjects.Cast<DeliveryPriceRuleJournalNode>().FirstOrDefault();

			if(node == null)
			{
				return;
			}

			if(SelectedWeekDayName.HasValue && WeekDayDistrictRuleItems.All(i => i.DeliveryPriceRule.Id != node.Id))
			{
				WeekDayDistrictRuleItems.Add(new WeekDayDistrictRuleItem
				{
					District = SelectedDistrict,
					WeekDay = SelectedWeekDayName.Value,
					Price = 0,
					DeliveryPriceRule = UoW.Session.Query<DeliveryPriceRule>()
						.Where(d => d.Id == node.Id)
						.First()
				});
			}
		}

		#endregion Команда добавления правила цен доставки дня недели

		#region Команда добавления общего правила цен доставки

		public DelegateCommand AddCommonDeliveryPriceRuleCommand { get; }

		private DelegateCommand CreateAddCommonDeliveryPriceRuleCommand()
		{
			var command = new DelegateCommand(AddCommonDeliveryPriceRule, () => CanAddCommonDeliveryPriceRuleCommand);
			command.CanExecuteChangedWith(this, x => x.CanAddCommonDeliveryPriceRuleCommand);

			return command;
		}

		private bool CanAddCommonDeliveryPriceRuleCommand => CanEditDeliveryRules;

		private void AddCommonDeliveryPriceRule()
		{
			NavigationManager.OpenViewModel<DeliveryPriceRuleJournalViewModel>(
				this,
				OpenPageOptions.AsSlave,
				viewModel =>
				{
					viewModel.SelectionMode = JournalSelectionMode.Single;
					viewModel.OnSelectResult += JournalOnCommonEntitySelectedResult;
				});
		}

		private void JournalOnCommonEntitySelectedResult(object sender, JournalSelectedEventArgs e)
		{
			var node = e.SelectedObjects.Cast<DeliveryPriceRuleJournalNode>().FirstOrDefault();

			if(node == null)
			{
				return;
			}

			if(CommonDistrictRuleItems.All(i => i.DeliveryPriceRule.Id != node.Id))
			{
				CommonDistrictRuleItems.Add(new CommonDistrictRuleItem
				{
					District = SelectedDistrict,
					Price = 0,
					DeliveryPriceRule = UoW.Session.Query<DeliveryPriceRule>()
						.Where(d => d.Id == node.Id)
						.First()
				});
			}
		}

		#endregion Команда добавления общего правила цен доставки


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

		public DelegateCommand RemoveAcceptBeforeCommand { get; }

		private DelegateCommand CreateRemoveAcceptBeforeCommand() =>
			new DelegateCommand(
				() =>
				{
					SelectedScheduleRestriction.AcceptBefore = null;
				},
				() => SelectedScheduleRestriction != null);

		public DelegateCommand CopyDistrictSchedulesCommand { get; }
		public DelegateCommand PasteSchedulesToDistrictCommand { get; }
		public DelegateCommand PasteSchedulesToZoneCommand { get; }
		#endregion

		#region Copy Paste District Schedule
		private void CopyDistrictSchedules()
		{
			if(SelectedDistrict == null)
			{
				CommonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Error,
					"Для копирования графиков доставки необходимо сначала выбрать район, из которого данные будут скопированы");

				return;
			}

			_copiedDistrict = SelectedDistrict;
		}

		private void PasteSchedulesToSelectedDistrict()
		{
			if(SelectedDistrict == null)
			{
				CommonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Error,
					"Для вставки графиков доставки необходимо сначала выбрать район, в который данные будут скопированы");

				return;
			}

			PasteSchedulesToDistrict(SelectedDistrict);

			OnPropertyChanged(nameof(ScheduleRestrictions));
		}

		private void PasteSchedulesToDistrict(District district)
		{
			if(_copiedDistrict == null)
			{
				CommonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Error,
					"Для вставки графиков доставки необходимо сначала скопировать район");

				return;
			}

			var schedulesToSet = new List<DeliveryScheduleRestriction>();

			foreach(var schedule in _copiedDistrict.GetAllDeliveryScheduleRestrictions())
			{
				var newSchedule = schedule.Clone() as DeliveryScheduleRestriction;

				newSchedule.District = district;

				schedulesToSet.Add(newSchedule);
			}

			district.ReplaceDistrictDeliveryScheduleRestrictions(schedulesToSet);
		}

		private void PasteSchedulesToZone()
		{
			if(SelectedDistrict == null)
			{
				CommonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Error,
					"Для вставки графиков доставки необходимо сначала выбрать район для определения тарифной зоны");

				return;
			}

			var districts = GetDistrictsByTariffZone(SelectedDistrict.TariffZone);

			districts.ForEach(d => PasteSchedulesToDistrict(d));

			OnPropertyChanged(nameof(ScheduleRestrictions));
		}

		private IEnumerable<District> GetDistrictsByTariffZone(TariffZone tariffZone)
		{
			var districts = Entity.ObservableDistricts
				.Where(d => d.TariffZone.Id == tariffZone.Id)
				.Select(d => d);

			return districts;
		}
		#endregion Copy Paste District Schedule

		private void AddAcceptBefore()
		{
			var acceptBeforePage = NavigationManager.OpenViewModel<AcceptBeforeJournalViewModel>(null);
			_acceptBeforeJournalViewModel = acceptBeforePage.ViewModel;
			_acceptBeforeJournalViewModel.VisibleEditAction = false;
			_acceptBeforeJournalViewModel.VisibleDeleteAction = false;
			_acceptBeforeJournalViewModel.SelectionMode = JournalSelectionMode.Single;
			_acceptBeforeJournalViewModel.OnSelectResult += OnAcceptBeforeJournalViewModelSelectResult;
		}

		private void SortDistricts()
		{
			for(int i = 0; i < Entity.Districts.Count - 1; i++)
			{
				for(int j = 0; j < Entity.Districts.Count - 2 - i; j++)
				{
					switch(NaturalStringComparer.CompareStrings(Entity.Districts[j].TariffZone?.Name ?? "", Entity.Districts[j + 1].TariffZone?.Name ?? ""))
					{
						case -1:
							break;
						case 1:
							(Entity.Districts[j], Entity.Districts[j + 1]) = (Entity.Districts[j + 1], Entity.Districts[j]);
							break;
						case 0:
							if(string.Compare(Entity.Districts[j].DistrictName, Entity.Districts[j + 1].DistrictName, StringComparison.InvariantCulture) > 0)
							{
								(Entity.Districts[j], Entity.Districts[j + 1]) = (Entity.Districts[j + 1], Entity.Districts[j]);
							}

							break;
					}
				}
			}
		}

		private void AddScheduleRestriction()
		{
			var journal = _deliveryScheduleJournalFactory.CreateJournal(JournalSelectionMode.Multiple);
			journal.OnEntitySelectedResult += Journal_OnEntitySelectedResult;
			TabParent.AddSlaveTab(this, journal);
		}

		private void Journal_OnEntitySelectedResult(object sender, JournalSelectedNodesEventArgs e)
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
				ScheduleRestrictions.Add(new DeliveryScheduleRestriction
				{
					District = SelectedDistrict,
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
				if(!CommonServices.InteractiveService.Question("Продолжить редактирование районов?", "Успешно сохранено"))
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
			get => base.HasChanges && (CanEditDistrict || CanSave);
			set => base.HasChanges = value;
		}

		public override void Dispose()
		{
			if(_acceptBeforeJournalViewModel != null)
			{
				_acceptBeforeJournalViewModel.OnSelectResult -= OnAcceptBeforeJournalViewModelSelectResult;
			}

			UoW?.Dispose();
			base.Dispose();
		}
	}
}
