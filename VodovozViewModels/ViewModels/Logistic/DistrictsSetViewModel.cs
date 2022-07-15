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
using QS.Utilities.Text;
using QS.ViewModels;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Sale;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;

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
            INavigationManager navigation = null)
            : base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
        {
            _entityDeleteWorker = entityDeleteWorker ?? throw new ArgumentNullException(nameof(entityDeleteWorker));
            DistrictRuleRepository = districtRuleRepository ?? throw new ArgumentNullException(nameof(districtRuleRepository));

            TabName = "Районы с графиками доставки";

            CanChangeDistrictWageTypePermissionResult = commonServices.CurrentPermissionService.ValidatePresetPermission("can_change_district_wage_type");

            if(Entity.Id == 0) {
                Entity.Author = employeeRepository.GetEmployeeForCurrentUser(UoW);
                Entity.Status = DistrictsSetStatus.Draft;
            }

            var districtPermissionResult = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(District));
            CanEditDistrict = districtPermissionResult.CanUpdate && Entity.Status != DistrictsSetStatus.Active;
            CanEditDeliveryRules = CanEditDistrict || commonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_delivery_rules");
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
        }

        private readonly IEntityDeleteWorker _entityDeleteWorker;
        private readonly GeometryFactory _geometryFactory;

        public readonly bool CanChangeDistrictWageTypePermissionResult;
        public readonly bool CanEditDistrict;
        public readonly bool CanEditDeliveryRules;
		public readonly bool CanEditDeliveryScheduleRestriction;
        public readonly bool CanDeleteDistrict;
        public readonly bool CanCreateDistrict;
        public readonly bool CanSave;
        public readonly bool CanEdit;

        public IDistrictRuleRepository DistrictRuleRepository { get; }
        public GenericObservableList<DeliveryScheduleRestriction> ScheduleRestrictions => SelectedWeekDayName.HasValue && SelectedDistrict != null
            ? SelectedDistrict.GetScheduleRestrictionCollectionByWeekDayName(SelectedWeekDayName.Value)
            : null;

        public GenericObservableList<CommonDistrictRuleItem> CommonDistrictRuleItems => SelectedDistrict.ObservableCommonDistrictRuleItems;

        public GenericObservableList<WeekDayDistrictRuleItem> WeekDayDistrictRuleItems => SelectedWeekDayName.HasValue && SelectedDistrict != null
            ? SelectedDistrict.GetWeekDayRuleItemCollectionByWeekDayName(SelectedWeekDayName.Value)
            : null;

        private GenericObservableList<PointLatLng> selectedDistrictBorderVertices;
        public GenericObservableList<PointLatLng> SelectedDistrictBorderVertices {
            get => selectedDistrictBorderVertices;
            set => SetField(ref selectedDistrictBorderVertices, value, () => SelectedDistrictBorderVertices);
        }

        private GenericObservableList<PointLatLng> newBorderVertices;
        public GenericObservableList<PointLatLng> NewBorderVertices {
            get => newBorderVertices;
            set => SetField(ref newBorderVertices, value, () => NewBorderVertices);
        }

        private District selectedDistrict;
        public District SelectedDistrict {
            get => selectedDistrict;
            set {
                if(!SetField(ref selectedDistrict, value, () => SelectedDistrict))
                    return;

                SelectedDistrictBorderVertices.Clear();

                if(selectedDistrict != null) {
                    if(!SelectedWeekDayName.HasValue) {
                        SelectedWeekDayName = WeekDayName.Today;
                    }
                    else {
                        OnPropertyChanged(nameof(WeekDayDistrictRuleItems));
                        OnPropertyChanged(nameof(ScheduleRestrictions));
                    }
                    if(SelectedDistrict.DistrictBorder?.Coordinates != null) {
                        foreach(Coordinate coord in SelectedDistrict.DistrictBorder.Coordinates) {
                            SelectedDistrictBorderVertices.Add(new PointLatLng {
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

        public GeographicGroup SelectedGeoGroup {
            get => SelectedDistrict?.GeographicGroup;
            set {
                if(SelectedDistrict.GeographicGroup != value) {
                    SelectedDistrict.GeographicGroup = value;
                    OnPropertyChanged(nameof(SelectedGeoGroup));
                }
            }
        }

        public WageDistrict SelectedWageDistrict {
            get => SelectedDistrict?.WageDistrict;
            set {
                if(SelectedDistrict.WageDistrict != value) {
                    SelectedDistrict.WageDistrict = value;
                    OnPropertyChanged(nameof(SelectedWageDistrict));
                }
            }
        }

        private WeekDayName? selectedWeekDayName;
        public WeekDayName? SelectedWeekDayName {
            get => selectedWeekDayName;
            set {
                if(SetField(ref selectedWeekDayName, value, () => SelectedWeekDayName)) {
                    if(SelectedWeekDayName != null) {
                        OnPropertyChanged(nameof(ScheduleRestrictions));
                        OnPropertyChanged(nameof(WeekDayDistrictRuleItems));
                    }
                }
            }
        }

        private CommonDistrictRuleItem selectedCommonDistrictRuleItem;
        public CommonDistrictRuleItem SelectedCommonDistrictRuleItem {
            get => selectedCommonDistrictRuleItem;
            set => SetField(ref selectedCommonDistrictRuleItem, value, () => SelectedCommonDistrictRuleItem);
        }

        private WeekDayDistrictRuleItem selectedWeekDayDistrictRuleItem;
        public WeekDayDistrictRuleItem SelectedWeekDayDistrictRuleItem {
            get => selectedWeekDayDistrictRuleItem;
            set => SetField(ref selectedWeekDayDistrictRuleItem, value, () => SelectedWeekDayDistrictRuleItem);
        }

        private DeliveryScheduleRestriction selectedScheduleRestriction;
        public DeliveryScheduleRestriction SelectedScheduleRestriction {
            get => selectedScheduleRestriction;
            set => SetField(ref selectedScheduleRestriction, value, () => SelectedScheduleRestriction);
        }

        private bool isCreatingNewBorder;
        public bool IsCreatingNewBorder {
            get => isCreatingNewBorder;
            private set {
                if(value && SelectedDistrict == null)
                    throw new ArgumentNullException(nameof(SelectedDistrict));
                SetField(ref isCreatingNewBorder, value, () => IsCreatingNewBorder);
            }
        }

        #region Commands

        private DelegateCommand addDistrictCommand;
        public DelegateCommand AddDistrictCommand => addDistrictCommand ?? (addDistrictCommand = new DelegateCommand(
            () => {
                var newDistrict = new District { PriceType = DistrictWaterPrice.Standart, DistrictName = "Новый район", DistrictsSet = Entity };
                Entity.ObservableDistricts.Add(newDistrict);
                SelectedDistrict = newDistrict;
            }, () => true
        ));

        private DelegateCommand removeDistrictCommand;
        public DelegateCommand RemoveDistrictCommand => removeDistrictCommand ?? (removeDistrictCommand = new DelegateCommand(
            () => {
                var distrToDel = selectedDistrict;
                Entity.ObservableDistricts.Remove(SelectedDistrict);
                if(distrToDel.Id == 0)
                {
	                return;
                }

                if(_entityDeleteWorker.DeleteObject<District>(distrToDel.Id, UoW)) {
                    SelectedDistrict = null;
                }
                else {
                    Entity.ObservableDistricts.Add(distrToDel);
                    SelectedDistrict = distrToDel;
                }
            },
            () => SelectedDistrict != null
        ));

        private DelegateCommand createBorderCommand;
        public DelegateCommand CreateBorderCommand => createBorderCommand ?? (createBorderCommand = new DelegateCommand(
            () => {
                IsCreatingNewBorder = true;
                NewBorderVertices.Clear();
            },
            () => !IsCreatingNewBorder
        ));

        private DelegateCommand confirmNewBorderCommand;
        public DelegateCommand ConfirmNewBorderCommand => confirmNewBorderCommand ?? (confirmNewBorderCommand = new DelegateCommand(
            () => {
                if(NewBorderVertices.Count < 3)
                    return;
                var closingPoint = NewBorderVertices[0];
                NewBorderVertices.Add(closingPoint);
                SelectedDistrictBorderVertices = new GenericObservableList<PointLatLng>(NewBorderVertices.ToList());
                NewBorderVertices.Clear();
                SelectedDistrict.DistrictBorder = _geometryFactory.CreatePolygon(SelectedDistrictBorderVertices.Select(p => new Coordinate(p.Lat,p.Lng)).ToArray());
                IsCreatingNewBorder = false;
            },
            () => IsCreatingNewBorder
        ));

        private DelegateCommand cancelNewBorderCommand;
        public DelegateCommand CancelNewBorderCommand => cancelNewBorderCommand ?? (cancelNewBorderCommand = new DelegateCommand(
            () => {
                NewBorderVertices.Clear();
                IsCreatingNewBorder = false;
                OnPropertyChanged(nameof(NewBorderVertices));
            },
            () => IsCreatingNewBorder
        ));

        private DelegateCommand removeBorderCommand;
        public DelegateCommand RemoveBorderCommand => removeBorderCommand ?? (removeBorderCommand = new DelegateCommand(
            () => {
                SelectedDistrict.DistrictBorder = null;
                SelectedDistrictBorderVertices.Clear();
                OnPropertyChanged(nameof(SelectedDistrictBorderVertices));
                OnPropertyChanged(nameof(SelectedDistrict));
            },
            () => !IsCreatingNewBorder
        ));

        private DelegateCommand<PointLatLng> addNewVertexCommand;
        public DelegateCommand<PointLatLng> AddNewVertexCommand => addNewVertexCommand ?? (addNewVertexCommand = new DelegateCommand<PointLatLng>(
            point => {
                NewBorderVertices.Add(point);
                OnPropertyChanged(nameof(NewBorderVertices));
            },
            point => IsCreatingNewBorder
        ));

        private DelegateCommand<PointLatLng> removeNewBorderVerteCommand;
        public DelegateCommand<PointLatLng> RemoveNewBorderVertexCommand => removeNewBorderVerteCommand ?? (removeNewBorderVerteCommand = new DelegateCommand<PointLatLng>(
            point => {
                NewBorderVertices.Remove(point);
                OnPropertyChanged(nameof(NewBorderVertices));
            },
            point => IsCreatingNewBorder && !point.IsEmpty
        ));

        private DelegateCommand<IEnumerable<DeliveryScheduleJournalNode>> addScheduleRestrictionCommand;
        public DelegateCommand<IEnumerable<DeliveryScheduleJournalNode>> AddScheduleRestrictionCommand => addScheduleRestrictionCommand ?? (addScheduleRestrictionCommand = new DelegateCommand<IEnumerable<DeliveryScheduleJournalNode>>(
            scheduleNodes => {
                foreach (var scheduleNode in scheduleNodes) {
					if(SelectedWeekDayName.HasValue && ScheduleRestrictions.All(x => x.DeliverySchedule.Id != scheduleNode.Id))
					{
						var deliverySchedule = UoW.GetById<DeliverySchedule>(scheduleNode.Id);
						ScheduleRestrictions.Add(new DeliveryScheduleRestriction
						{
							District = SelectedDistrict,
							WeekDay = SelectedWeekDayName.Value,
							DeliverySchedule = deliverySchedule
						});
					}
                }
            },
            schedules => schedules.Any()
        ));

        private DelegateCommand removeScheduleRestrictionCommand;
        public DelegateCommand RemoveScheduleRestrictionCommand => removeScheduleRestrictionCommand ?? (removeScheduleRestrictionCommand = new DelegateCommand(
            () => {
                ScheduleRestrictions.Remove(SelectedScheduleRestriction);
            },
            () => SelectedScheduleRestriction != null
        ));

        private DelegateCommand<IEnumerable<DeliveryPriceRule>> addWeekDayDistrictRuleItemCommand;
        public DelegateCommand<IEnumerable<DeliveryPriceRule>> AddWeekDayDistrictRuleItemCommand => addWeekDayDistrictRuleItemCommand ?? (addWeekDayDistrictRuleItemCommand = new DelegateCommand<IEnumerable<DeliveryPriceRule>>(
            ruleItems => {
                foreach (var ruleItem in ruleItems) {
                    if(SelectedWeekDayName.HasValue && WeekDayDistrictRuleItems.All(i => i.DeliveryPriceRule.Id != ruleItem.Id)) {
                        WeekDayDistrictRuleItems.Add(new WeekDayDistrictRuleItem {
                            District = SelectedDistrict, WeekDay = SelectedWeekDayName.Value, Price = 0,
                            DeliveryPriceRule = ruleItem
                        });
                    }
                }
            },
            ruleItems => ruleItems.Any()
        ));

        private DelegateCommand removeWeekDayDistrictRuleItemCommand;
        public DelegateCommand RemoveWeekDayDistrictRuleItemCommand => removeWeekDayDistrictRuleItemCommand ?? (removeWeekDayDistrictRuleItemCommand = new DelegateCommand(
            () => {
                WeekDayDistrictRuleItems.Remove(SelectedWeekDayDistrictRuleItem);
            },
            () => SelectedWeekDayDistrictRuleItem != null
        ));

        private DelegateCommand<IEnumerable<DeliveryPriceRule>> addCommonDistrictRuleItemCommand;
        public DelegateCommand<IEnumerable<DeliveryPriceRule>> AddCommonDistrictRuleItemCommand => addCommonDistrictRuleItemCommand ?? (addCommonDistrictRuleItemCommand = new DelegateCommand<IEnumerable<DeliveryPriceRule>>(
            ruleItems => {
                foreach (var ruleItem in ruleItems) {
                    if(CommonDistrictRuleItems.All(i => i.DeliveryPriceRule.Id != ruleItem.Id)) {
                        CommonDistrictRuleItems.Add(new CommonDistrictRuleItem {
                            District = SelectedDistrict, Price = 0, DeliveryPriceRule = ruleItem
                        });
                    }
                }
            },
            ruleItems => ruleItems.Any()
        ));

        private DelegateCommand removeCommonDistrictRuleItemCommand;
        public DelegateCommand RemoveCommonDistrictRuleItemCommand => removeCommonDistrictRuleItemCommand ?? (removeCommonDistrictRuleItemCommand = new DelegateCommand(
            () => {
                CommonDistrictRuleItems.Remove(SelectedCommonDistrictRuleItem);
            },
            () => SelectedCommonDistrictRuleItem != null
        ));

        private DelegateCommand<AcceptBefore> addAcceptBeforeCommand;
        public DelegateCommand<AcceptBefore> AddAcceptBeforeCommand => addAcceptBeforeCommand ?? (addAcceptBeforeCommand = new DelegateCommand<AcceptBefore>(
            acceptBefore => {
                SelectedScheduleRestriction.AcceptBefore = acceptBefore;
            },
            acceptBefore => acceptBefore != null && SelectedScheduleRestriction != null
        ));

        private DelegateCommand removeAcceptBeforeCommand;
        public DelegateCommand RemoveAcceptBeforeCommand => removeAcceptBeforeCommand ?? (removeAcceptBeforeCommand = new DelegateCommand(
            () => {
                SelectedScheduleRestriction.AcceptBefore = null;
            },
            () => SelectedScheduleRestriction != null
        ));

        #endregion

        private void SortDistricts()
        {
            for(int i = 0; i < Entity.Districts.Count - 1; i++)
            {
                for(int j = 0; j < Entity.Districts.Count - 2 - i; j++)
                {
                    switch (NaturalStringComparer.CompareStrings(Entity.Districts[j].TariffZone.Name, Entity.Districts[j + 1].TariffZone.Name)) {
                        case -1:
                            break;
                        case 1: (Entity.Districts[j], Entity.Districts[j + 1]) = (Entity.Districts[j + 1], Entity.Districts[j]);
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

        public override bool HasChanges {
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
