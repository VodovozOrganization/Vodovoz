using GMap.NET;
using MoreLinq;
using NetTopologySuite.Geometries;
using QS.Commands;
using QS.Dialog;
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
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Sale;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Sale;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.ViewModels.Logistic
{
	public sealed class DistrictsSetViewModel : EntityTabViewModelBase<DistrictsSet>
	{
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
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));

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

			CopyDistrictSchedulesCommand = new DelegateCommand(CopyDistrictSchedules);
			PasteSchedulesToDistrictCommand = new DelegateCommand(PasteSchedulesToDistrict);
			PasteScheduleToZoneCommand = new DelegateCommand(PasteScheduleToZone);
		}

		private readonly IEntityDeleteWorker _entityDeleteWorker;
		private readonly IDeliveryScheduleJournalFactory _deliveryScheduleJournalFactory;
		private readonly GeometryFactory _geometryFactory;
		private ICommonServices _commonServices;
		District _copiedDistrict;

		public readonly bool CanChangeDistrictWageTypePermissionResult;
		public readonly bool CanEditDistrict;
		public readonly bool CanEditDeliveryRules;
		public readonly bool CanEditDeliveryScheduleRestriction;
		public readonly bool CanDeleteDistrict;
		public readonly bool CanCreateDistrict;
		public readonly bool CanSave;
		public readonly bool CanEdit;

		public bool CanCopyDeliveryScheduleRestrictions =>
			CanEditDeliveryScheduleRestriction
			&& SelectedDistrict != null;

		public bool CanPasteDeliveryScheduleRestrictions =>
			CanEditDeliveryScheduleRestriction
			&& SelectedDistrict != null
			&& CopiedDistrict != null;

		public string CopyDistrictScheduleMenuItemLabel =>
			$"Копировать график доставки {SelectedDistrict?.Id} {SelectedDistrict?.DistrictName}";

		public string PasteScheduleToDistrictMenuItemLabel =>
			$"Вставить график доставки {CopiedDistrict?.Id} {CopiedDistrict?.DistrictName} В {SelectedDistrict?.Id} {SelectedDistrict?.DistrictName}";

		public string PasteScheduleToTafiffZoneMenuItemLabel =>
			$"Вставить график доставки {CopiedDistrict?.Id} {CopiedDistrict?.DistrictName} для {SelectedDistrict?.TariffZone.Name}";

		public IDistrictRuleRepository DistrictRuleRepository { get; }
		public GenericObservableList<DeliveryScheduleRestriction> ScheduleRestrictions => SelectedWeekDayName.HasValue && SelectedDistrict != null
			? SelectedDistrict.GetScheduleRestrictionCollectionByWeekDayName(SelectedWeekDayName.Value)
			: null;

		public GenericObservableList<CommonDistrictRuleItem> CommonDistrictRuleItems => SelectedDistrict.ObservableCommonDistrictRuleItems;

		public GenericObservableList<WeekDayDistrictRuleItem> WeekDayDistrictRuleItems => SelectedWeekDayName.HasValue && SelectedDistrict != null
			? SelectedDistrict.GetWeekDayRuleItemCollectionByWeekDayName(SelectedWeekDayName.Value)
			: null;

		private GenericObservableList<PointLatLng> selectedDistrictBorderVertices;
		public GenericObservableList<PointLatLng> SelectedDistrictBorderVertices
		{
			get => selectedDistrictBorderVertices;
			set => SetField(ref selectedDistrictBorderVertices, value, () => SelectedDistrictBorderVertices);
		}

		private GenericObservableList<PointLatLng> newBorderVertices;
		public GenericObservableList<PointLatLng> NewBorderVertices
		{
			get => newBorderVertices;
			set => SetField(ref newBorderVertices, value, () => NewBorderVertices);
		}

		private District selectedDistrict;
		public District SelectedDistrict
		{
			get => selectedDistrict;
			set
			{
				if(!SetField(ref selectedDistrict, value, () => SelectedDistrict))
					return;

				SelectedDistrictBorderVertices.Clear();

				if(selectedDistrict != null)
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

		private WeekDayName? selectedWeekDayName;
		public WeekDayName? SelectedWeekDayName
		{
			get => selectedWeekDayName;
			set
			{
				if(SetField(ref selectedWeekDayName, value, () => SelectedWeekDayName))
				{
					if(SelectedWeekDayName != null)
					{
						OnPropertyChanged(nameof(ScheduleRestrictions));
						OnPropertyChanged(nameof(WeekDayDistrictRuleItems));
					}
				}
			}
		}

		private CommonDistrictRuleItem selectedCommonDistrictRuleItem;
		public CommonDistrictRuleItem SelectedCommonDistrictRuleItem
		{
			get => selectedCommonDistrictRuleItem;
			set => SetField(ref selectedCommonDistrictRuleItem, value, () => SelectedCommonDistrictRuleItem);
		}

		private WeekDayDistrictRuleItem selectedWeekDayDistrictRuleItem;
		public WeekDayDistrictRuleItem SelectedWeekDayDistrictRuleItem
		{
			get => selectedWeekDayDistrictRuleItem;
			set => SetField(ref selectedWeekDayDistrictRuleItem, value, () => SelectedWeekDayDistrictRuleItem);
		}

		private DeliveryScheduleRestriction selectedScheduleRestriction;
		public DeliveryScheduleRestriction SelectedScheduleRestriction
		{
			get => selectedScheduleRestriction;
			set => SetField(ref selectedScheduleRestriction, value, () => SelectedScheduleRestriction);
		}

		private bool isCreatingNewBorder;
		public bool IsCreatingNewBorder
		{
			get => isCreatingNewBorder;
			private set
			{
				if(value && SelectedDistrict == null)
					throw new ArgumentNullException(nameof(SelectedDistrict));
				SetField(ref isCreatingNewBorder, value, () => IsCreatingNewBorder);
			}
		}

		public District CopiedDistrict
		{
			get => _copiedDistrict;
			private set => SetField(ref _copiedDistrict, value);
		}

		#region Commands

		private DelegateCommand addDistrictCommand;
		public DelegateCommand AddDistrictCommand => addDistrictCommand ?? (addDistrictCommand = new DelegateCommand(
			() =>
			{
				var newDistrict = new District { PriceType = DistrictWaterPrice.Standart, DistrictName = "Новый район", DistrictsSet = Entity };
				Entity.ObservableDistricts.Add(newDistrict);
				SelectedDistrict = newDistrict;
			}, () => true
		));

		private DelegateCommand removeDistrictCommand;
		public DelegateCommand RemoveDistrictCommand => removeDistrictCommand ?? (removeDistrictCommand = new DelegateCommand(
			() =>
			{
				var distrToDel = selectedDistrict;
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
			() => SelectedDistrict != null
		));

		private DelegateCommand createBorderCommand;
		public DelegateCommand CreateBorderCommand => createBorderCommand ?? (createBorderCommand = new DelegateCommand(
			() =>
			{
				IsCreatingNewBorder = true;
				NewBorderVertices.Clear();
			},
			() => !IsCreatingNewBorder
		));

		private DelegateCommand confirmNewBorderCommand;
		public DelegateCommand ConfirmNewBorderCommand => confirmNewBorderCommand ?? (confirmNewBorderCommand = new DelegateCommand(
			() =>
			{
				if(NewBorderVertices.Count < 3)
					return;
				var closingPoint = NewBorderVertices[0];
				NewBorderVertices.Add(closingPoint);
				SelectedDistrictBorderVertices = new GenericObservableList<PointLatLng>(NewBorderVertices.ToList());
				NewBorderVertices.Clear();
				SelectedDistrict.DistrictBorder = _geometryFactory.CreatePolygon(SelectedDistrictBorderVertices.Select(p => new Coordinate(p.Lat, p.Lng)).ToArray());
				IsCreatingNewBorder = false;
			},
			() => IsCreatingNewBorder
		));

		private DelegateCommand cancelNewBorderCommand;
		public DelegateCommand CancelNewBorderCommand => cancelNewBorderCommand ?? (cancelNewBorderCommand = new DelegateCommand(
			() =>
			{
				NewBorderVertices.Clear();
				IsCreatingNewBorder = false;
				OnPropertyChanged(nameof(NewBorderVertices));
			},
			() => IsCreatingNewBorder
		));

		private DelegateCommand removeBorderCommand;
		public DelegateCommand RemoveBorderCommand => removeBorderCommand ?? (removeBorderCommand = new DelegateCommand(
			() =>
			{
				SelectedDistrict.DistrictBorder = null;
				SelectedDistrictBorderVertices.Clear();
				OnPropertyChanged(nameof(SelectedDistrictBorderVertices));
				OnPropertyChanged(nameof(SelectedDistrict));
			},
			() => !IsCreatingNewBorder
		));

		private DelegateCommand<PointLatLng> addNewVertexCommand;
		public DelegateCommand<PointLatLng> AddNewVertexCommand => addNewVertexCommand ?? (addNewVertexCommand = new DelegateCommand<PointLatLng>(
			point =>
			{
				NewBorderVertices.Add(point);
				OnPropertyChanged(nameof(NewBorderVertices));
			},
			point => IsCreatingNewBorder
		));

		private DelegateCommand<PointLatLng> removeNewBorderVerteCommand;
		public DelegateCommand<PointLatLng> RemoveNewBorderVertexCommand => removeNewBorderVerteCommand ?? (removeNewBorderVerteCommand = new DelegateCommand<PointLatLng>(
			point =>
			{
				NewBorderVertices.Remove(point);
				OnPropertyChanged(nameof(NewBorderVertices));
			},
			point => IsCreatingNewBorder && !point.IsEmpty
		));

		private DelegateCommand _addScheduleRestrictionCommand;
		public DelegateCommand AddScheduleRestrictionCommand
		{
			get
			{
				if(_addScheduleRestrictionCommand == null)
				{
					_addScheduleRestrictionCommand = new DelegateCommand(AddScheduleRestriction);
				}
				return _addScheduleRestrictionCommand;
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

		private DelegateCommand removeScheduleRestrictionCommand;
		public DelegateCommand RemoveScheduleRestrictionCommand => removeScheduleRestrictionCommand ?? (removeScheduleRestrictionCommand = new DelegateCommand(
			() =>
			{
				ScheduleRestrictions.Remove(SelectedScheduleRestriction);
			},
			() => SelectedScheduleRestriction != null
		));

		private DelegateCommand removeWeekDayDistrictRuleItemCommand;
		public DelegateCommand RemoveWeekDayDistrictRuleItemCommand => removeWeekDayDistrictRuleItemCommand ?? (removeWeekDayDistrictRuleItemCommand = new DelegateCommand(
			() =>
			{
				WeekDayDistrictRuleItems.Remove(SelectedWeekDayDistrictRuleItem);
			},
			() => SelectedWeekDayDistrictRuleItem != null
		));

		private DelegateCommand removeCommonDistrictRuleItemCommand;
		public DelegateCommand RemoveCommonDistrictRuleItemCommand => removeCommonDistrictRuleItemCommand ?? (removeCommonDistrictRuleItemCommand = new DelegateCommand(
			() =>
			{
				CommonDistrictRuleItems.Remove(SelectedCommonDistrictRuleItem);
			},
			() => SelectedCommonDistrictRuleItem != null
		));

		#region Команда добавления правила цен доставки дня недели
		private DelegateCommand _addWeekDayDeliveryPriceRuleCommand;
		public DelegateCommand AddWeekDayDeliveryPriceRuleCommand
		{
			get
			{
				if(_addWeekDayDeliveryPriceRuleCommand == null)
				{
					_addWeekDayDeliveryPriceRuleCommand = new DelegateCommand(AddWeekDayDeliveryPriceRule, () => CanAddWeekDayDeliveryPriceRuleCommand);
					_addWeekDayDeliveryPriceRuleCommand.CanExecuteChangedWith(this, x => x.CanAddWeekDayDeliveryPriceRuleCommand);
				}
				return _addWeekDayDeliveryPriceRuleCommand;
			}
		}

		private bool CanAddWeekDayDeliveryPriceRuleCommand => CanEditDeliveryRules;

		private void AddWeekDayDeliveryPriceRule()
		{
			var journal = new DeliveryPriceRuleJournalViewModel(UnitOfWorkFactory, _commonServices, DistrictRuleRepository);
			journal.SelectionMode = JournalSelectionMode.Single;
			journal.OnEntitySelectedResult += JournalOnWeekDayEntitySelectedResult;
			TabParent.AddSlaveTab(this, journal);
		}

		private void JournalOnWeekDayEntitySelectedResult(object sender, JournalSelectedNodesEventArgs e)
		{
			var node = e.SelectedNodes.FirstOrDefault();

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
		#endregion

		#region Команда добавления общего правила цен доставки
		private DelegateCommand _addCommonDeliveryPriceRuleCommand;
		public DelegateCommand AddCommonDeliveryPriceRuleCommand
		{
			get
			{
				if(_addCommonDeliveryPriceRuleCommand == null)
				{
					_addCommonDeliveryPriceRuleCommand = new DelegateCommand(AddCommonDeliveryPriceRule, () => CanAddCommonDeliveryPriceRuleCommand);
					_addCommonDeliveryPriceRuleCommand.CanExecuteChangedWith(this, x => x.CanAddCommonDeliveryPriceRuleCommand);
				}
				return _addCommonDeliveryPriceRuleCommand;
			}
		}

		private bool CanAddCommonDeliveryPriceRuleCommand => CanEditDeliveryRules;

		private void AddCommonDeliveryPriceRule()
		{
			var journal = new DeliveryPriceRuleJournalViewModel(UnitOfWorkFactory, _commonServices, DistrictRuleRepository);
			journal.SelectionMode = JournalSelectionMode.Single;
			journal.OnEntitySelectedResult += JournalOnCommonEntitySelectedResult;
			TabParent.AddSlaveTab(this, journal);
		}

		private void JournalOnCommonEntitySelectedResult(object sender, JournalSelectedNodesEventArgs e)
		{
			var node = e.SelectedNodes.FirstOrDefault();

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
		#endregion

		private DelegateCommand<AcceptBefore> addAcceptBeforeCommand;
		public DelegateCommand<AcceptBefore> AddAcceptBeforeCommand => addAcceptBeforeCommand ?? (addAcceptBeforeCommand = new DelegateCommand<AcceptBefore>(
			acceptBefore =>
			{
				SelectedScheduleRestriction.AcceptBefore = acceptBefore;
			},
			acceptBefore => acceptBefore != null && SelectedScheduleRestriction != null
		));

		private DelegateCommand removeAcceptBeforeCommand;
		public DelegateCommand RemoveAcceptBeforeCommand => removeAcceptBeforeCommand ?? (removeAcceptBeforeCommand = new DelegateCommand(
			() =>
			{
				SelectedScheduleRestriction.AcceptBefore = null;
			},
			() => SelectedScheduleRestriction != null
		));

		public DelegateCommand CopyDistrictSchedulesCommand { get; }
		public DelegateCommand PasteSchedulesToDistrictCommand { get; }
		public DelegateCommand PasteScheduleToZoneCommand { get; }
		#endregion

		#region Copy Paste District Schedule
		private void CopyDistrictSchedules()
		{
			if(SelectedDistrict != null)
			{
				_copiedDistrict = SelectedDistrict;
			}
		}

		private void PasteSchedulesToDistrict()
		{
			if(SelectedDistrict == null)
			{
				CommonServices.InteractiveService.ShowMessage(
					   ImportanceLevel.Error,
					   "Для вставки графиков доставки необходимо сначала выбрать район, в который данные будут скопированы");

				return;
			}

			PasteSchedulesToDistrict(SelectedDistrict);
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

		private void PasteScheduleToZone()
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
		}

		private IEnumerable<District> GetDistrictsByTariffZone(TariffZone tariffZone)
		{
			var districts = Entity.ObservableDistricts
					.Where(d => d.TariffZone.Id == tariffZone.Id)
					.Select(d => d);

			return districts;
		}
		#endregion Copy Paste District Schedule

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
							if(String.Compare(Entity.Districts[j].DistrictName, Entity.Districts[j + 1].DistrictName, StringComparison.InvariantCulture) > 0)
								(Entity.Districts[j], Entity.Districts[j + 1]) = (Entity.Districts[j + 1], Entity.Districts[j]);
							break;
					}
				}
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
				TabParent?.AskToCloseTab(this, source);
			else
				TabParent?.ForceCloseTab(this, source);
		}

		public override bool HasChanges
		{
			get => base.HasChanges && (CanEditDistrict || CanSave);
			set => base.HasChanges = value;
		}

		public override void Dispose()
		{
			UoW?.Dispose();
			base.Dispose();
		}
	}
}
