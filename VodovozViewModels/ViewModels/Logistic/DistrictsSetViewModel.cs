using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using GeoAPI.Geometries;
using GMap.NET;
using NetTopologySuite.Geometries;
using NLog;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.Utilities.Text;
using QS.ViewModels;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.WageCalculation;

namespace Vodovoz.ViewModels.Logistic
{
    public sealed class DistrictsSetViewModel : DialogTabViewModelBase
    {
        public DistrictsSetViewModel(IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices, INavigationManager navigation) : base(unitOfWorkFactory, commonServices.InteractiveService, navigation)
        {
            this.commonServices = commonServices;
            TabName = "Районы с графиками доставки";
            
            CanChangeDistrictWageTypePermissionResult = commonServices.CurrentPermissionService.ValidatePresetPermission("can_change_district_wage_type");
            
            var permissionResult = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(District));
            CanEdit = permissionResult.CanUpdate;
            CanDelete = permissionResult.CanDelete;
            CanCreate = permissionResult.CanCreate;
            
            Districts = new GenericObservableList<District>(UoW.Session.QueryOver<District>()
                .List<District>()
                .OrderBy(x => x.TariffZone.Name, new NaturalStringComparer())
                .ThenBy(x => x.DistrictName).ToList());
            
            geometryFactory = new GeometryFactory(new PrecisionModel(), 3857);
            SelectedDistrictBorderVertices = new GenericObservableList<PointLatLng>();
            NewBorderVertices = new GenericObservableList<PointLatLng>();
        }

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        
        private readonly ICommonServices commonServices;
        private readonly GeometryFactory geometryFactory;

        public readonly bool CanChangeDistrictWageTypePermissionResult;
        public readonly bool CanEdit;
        public readonly bool CanDelete;
        public readonly bool CanCreate;
        
        private GenericObservableList<District> districts;
        public GenericObservableList<District> Districts {
            get => districts;
            set => SetField(ref districts, value, () => Districts);
        }

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
                    OnPropertyChanged(nameof(SelectedDistrictBorderVertices));
                }
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
                var newDistrict = new District { PriceType = DistrictWaterPrice.Standart, DistrictName = "Новый район" };
                Districts.Add(newDistrict);
                SelectedDistrict = newDistrict;
            }, () => true
        ));

        private DelegateCommand removeDistrictCommand;
        public DelegateCommand RemoveDistrictCommand => removeDistrictCommand ?? (removeDistrictCommand = new DelegateCommand(
            () => {
                Districts.Remove(SelectedDistrict);
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
                SelectedDistrict.DistrictBorder = geometryFactory.CreatePolygon(SelectedDistrictBorderVertices.Select(p => new Coordinate(p.Lat,p.Lng)).ToArray());
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
        
        private DelegateCommand<IEnumerable<DeliverySchedule>> addScheduleRestrictionCommand;
        public DelegateCommand<IEnumerable<DeliverySchedule>> AddScheduleRestrictionCommand => addScheduleRestrictionCommand ?? (addScheduleRestrictionCommand = new DelegateCommand<IEnumerable<DeliverySchedule>>(
            schedules => {
                foreach (var schedule in schedules) {
                    if(SelectedWeekDayName.HasValue && ScheduleRestrictions.All(x => x.DeliverySchedule.Id != schedule.Id))
                        ScheduleRestrictions.Add(new DeliveryScheduleRestriction {
                            District = SelectedDistrict, WeekDay = SelectedWeekDayName.Value, DeliverySchedule = schedule
                    });
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
                    if(SelectedWeekDayName.HasValue && WeekDayDistrictRuleItems.All(i => i.Id != ruleItem.Id)) {
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
                    if(CommonDistrictRuleItems.All(i => i.Id != ruleItem.Id)) {
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

        public override bool HasChanges {
            get => base.HasChanges && CanEdit;
            set => base.HasChanges = value;
        }

        public override void Close(bool askSave, CloseSource source)
        {
            if(askSave)
                TabParent?.AskToCloseTab(this, source);
            else
                TabParent?.ForceCloseTab(this, source);
        }

        public override bool Save(bool needClose)
        {
            logger.Info("Сохранение...");
            foreach(District district in Districts) {
                if(!commonServices.ValidationService.Validate(district)) {
                    logger.Info("Сохранение отменено");
                    return false;
                }
            }
            foreach(District district in Districts) {
                UoW.Save(district);
            }
            UoW.Commit();
            if(needClose)
                Close(HasChanges, CloseSource.Save);
            logger.Info("Сохранено");
            return true;
        }

        public override void Dispose()
        {
            UoW?.Dispose();
            base.Dispose();
        }
    }
}