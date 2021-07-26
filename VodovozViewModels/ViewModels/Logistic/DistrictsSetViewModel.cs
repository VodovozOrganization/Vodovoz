// using System;
// using System.Collections.Generic;
// using System.Data.Bindings.Collections.Generic;
// using System.Linq;
// using GMap.NET;
// using NetTopologySuite.Geometries;
// using QS.Commands;
// using QS.DomainModel.UoW;
// using QS.Navigation;
// using QS.Project.Domain;
// using QS.Services;
// using QS.Utilities.Text;
// using QS.ViewModels;
// using Vodovoz.Domain.Logistic;
// using Vodovoz.Domain.Sale;
// using Vodovoz.Domain.Sectors;
// using Vodovoz.Domain.WageCalculation;
// using Vodovoz.EntityRepositories.Employees;
// using Vodovoz.TempAdapters;
//
// namespace Vodovoz.ViewModels.Logistic
// {
//     public sealed class DistrictsSetViewModel : EntityTabViewModelBase<DistrictsSet>
//     {
//         public DistrictsSetViewModel(IEntityUoWBuilder uowBuilder,
//             IUnitOfWorkFactory unitOfWorkFactory,
//             ICommonServices commonServices,
//             IEntityDeleteWorker entityDeleteWorker,
//             IEmployeeRepository employeeRepository,
//             INavigationManager navigation = null) 
//             : base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
//         {
//             this.entityDeleteWorker = entityDeleteWorker ?? throw new ArgumentNullException(nameof(entityDeleteWorker));
//             this.commonServices = commonServices;
//             TabName = "Районы с графиками доставки";
//             
//             CanChangeDistrictWageTypePermissionResult = commonServices.CurrentPermissionService.ValidatePresetPermission("can_change_district_wage_type");
//
//             if(Entity.Id == 0) {
//                 Entity.Author = employeeRepository.GetEmployeeForCurrentUser(UoW);
//                 Entity.Status = SectorsSetStatus.Draft;
//             }
//
//             var permissionResult = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Sector));
//             CanEditDistrict = permissionResult.CanUpdate && Entity.Status != SectorsSetStatus.Active;
//             CanDeleteDistrict = permissionResult.CanDelete && Entity.Status != SectorsSetStatus.Active;
//             CanCreateDistrict = permissionResult.CanCreate && Entity.Status != SectorsSetStatus.Active;
//             
//             var permissionRes = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(DistrictsSet));
//             CanEdit = permissionRes.CanUpdate && Entity.Status != SectorsSetStatus.Active;
//
//             SortDistricts();
//
//             geometryFactory = new GeometryFactory(new PrecisionModel(), 3857);
//             SelectedDistrictBorderVertices = new GenericObservableList<PointLatLng>();
//             NewBorderVertices = new GenericObservableList<PointLatLng>();
//         }
//
//         private readonly ICommonServices commonServices;
//         private readonly IEntityDeleteWorker entityDeleteWorker;
//         private readonly GeometryFactory geometryFactory;
//
//         public readonly bool CanChangeDistrictWageTypePermissionResult;
//         public readonly bool CanEditDistrict;
//         public readonly bool CanDeleteDistrict;
//         public readonly bool CanCreateDistrict;
//         public readonly bool CanEdit;
//
//         public GenericObservableList<DeliveryScheduleRestriction> ScheduleRestrictions => SelectedWeekDayName.HasValue && SelectedSector != null
//             ? SelectedSector.GetScheduleRestrictionCollectionByWeekDayName(SelectedWeekDayName.Value)
//             : null;
//
//         public GenericObservableList<CommonDistrictRuleItem> CommonDistrictRuleItems => SelectedSector.ObservableCommonDistrictRuleItems;
//         
//         public GenericObservableList<WeekDayDistrictRuleItem> WeekDayDistrictRuleItems => SelectedWeekDayName.HasValue && SelectedSector != null
//             ? SelectedSector.GetWeekDayRuleItemCollectionByWeekDayName(SelectedWeekDayName.Value)
//             : null;
//         
//         private GenericObservableList<PointLatLng> selectedDistrictBorderVertices;
//         public GenericObservableList<PointLatLng> SelectedDistrictBorderVertices {
//             get => selectedDistrictBorderVertices;
//             set => SetField(ref selectedDistrictBorderVertices, value, () => SelectedDistrictBorderVertices);
//         }
//         
//         private GenericObservableList<PointLatLng> newBorderVertices;
//         public GenericObservableList<PointLatLng> NewBorderVertices {
//             get => newBorderVertices;
//             set => SetField(ref newBorderVertices, value, () => NewBorderVertices);
//         }
//
//         private Sector _selectedSector;
//         public Sector SelectedSector {
//             get => _selectedSector;
//             set {
//                 if(!SetField(ref _selectedSector, value, () => SelectedSector)) 
//                     return;
//                 
//                 SelectedDistrictBorderVertices.Clear();
//
//                 if(_selectedSector != null) {
//                     if(!SelectedWeekDayName.HasValue) {
//                         SelectedWeekDayName = WeekDayName.Today;
//                     }
//                     else {
//                         OnPropertyChanged(nameof(WeekDayDistrictRuleItems));
//                         OnPropertyChanged(nameof(ScheduleRestrictions));
//                     }
//                     if(SelectedSector.SectorBorder?.Coordinates != null) {
//                         foreach(Coordinate coord in SelectedSector.SectorBorder.Coordinates) {
//                             SelectedDistrictBorderVertices.Add(new PointLatLng {
//                                 Lat = coord.X,
//                                 Lng = coord.Y
//                             });
//                         }
//                     }
//
//                     OnPropertyChanged(nameof(CommonDistrictRuleItems));
//                     OnPropertyChanged(nameof(SelectedGeoGroup));
//                     OnPropertyChanged(nameof(SelectedWageSector));
//                 }
//                 OnPropertyChanged(nameof(SelectedDistrictBorderVertices));
//             }
//         }
//         
//         public GeographicGroup SelectedGeoGroup {
//             get => SelectedSector?.GeographicGroup;
//             set {
//                 if(SelectedSector.GeographicGroup != value) {
//                     SelectedSector.GeographicGroup = value;
//                     OnPropertyChanged(nameof(SelectedGeoGroup));
//                 }
//             }
//         }
//         
//         public WageSector SelectedWageSector {
//             get => SelectedSector?.WageSector;
//             set {
//                 if(SelectedSector.WageSector != value) {
//                     SelectedSector.WageSector = value;
//                     OnPropertyChanged(nameof(SelectedWageSector));
//                 }
//             }
//         }
//
//         private WeekDayName? selectedWeekDayName;
//         public WeekDayName? SelectedWeekDayName {
//             get => selectedWeekDayName;
//             set {
//                 if(SetField(ref selectedWeekDayName, value, () => SelectedWeekDayName)) {
//                     if(SelectedWeekDayName != null) {
//                         OnPropertyChanged(nameof(ScheduleRestrictions));
//                         OnPropertyChanged(nameof(WeekDayDistrictRuleItems));
//                     }
//                 }
//             }
//         }
//         
//         private CommonDistrictRuleItem selectedCommonDistrictRuleItem;
//         public CommonDistrictRuleItem SelectedCommonDistrictRuleItem {
//             get => selectedCommonDistrictRuleItem;
//             set => SetField(ref selectedCommonDistrictRuleItem, value, () => SelectedCommonDistrictRuleItem);
//         }
//         
//         private WeekDayDistrictRuleItem selectedWeekDayDistrictRuleItem;
//         public WeekDayDistrictRuleItem SelectedWeekDayDistrictRuleItem {
//             get => selectedWeekDayDistrictRuleItem;
//             set => SetField(ref selectedWeekDayDistrictRuleItem, value, () => SelectedWeekDayDistrictRuleItem);
//         }
//         
//         private DeliveryScheduleRestriction selectedScheduleRestriction;
//         public DeliveryScheduleRestriction SelectedScheduleRestriction {
//             get => selectedScheduleRestriction;
//             set => SetField(ref selectedScheduleRestriction, value, () => SelectedScheduleRestriction);
//         }
//         
//         private bool isCreatingNewBorder;
//         public bool IsCreatingNewBorder {
//             get => isCreatingNewBorder;
//             private set {
//                 if(value && SelectedSector == null)
//                     throw new ArgumentNullException(nameof(SelectedSector));
//                 SetField(ref isCreatingNewBorder, value, () => IsCreatingNewBorder);
//             }
//         }
//
//         #region Commands
//
//         private DelegateCommand addDistrictCommand;
//         public DelegateCommand AddDistrictCommand => addDistrictCommand ?? (addDistrictCommand = new DelegateCommand(
//             () => {
//                 var newDistrict = new Sector { PriceType = SectorWaterPrice.Standart, SectorName = "Новый район" };
//                 Entity.ObservableDistricts.Add(newDistrict);
//                 SelectedSector = newDistrict;
//             }, () => true
//         ));
//
//         private DelegateCommand removeDistrictCommand;
//         public DelegateCommand RemoveDistrictCommand => removeDistrictCommand ?? (removeDistrictCommand = new DelegateCommand(
//             () => {
//                 var distrToDel = _selectedSector;
//                 Entity.ObservableDistricts.Remove(SelectedSector);
//                 if(entityDeleteWorker.DeleteObject<Sector>(distrToDel.Id, UoW)) {
//                     SelectedSector = null;
//                 }
//                 else {
//                     Entity.ObservableDistricts.Add(distrToDel);
//                     SelectedSector = distrToDel;
//                 }
//             },
//             () => SelectedSector != null
//         ));
//         
//         private DelegateCommand createBorderCommand;
//         public DelegateCommand CreateBorderCommand => createBorderCommand ?? (createBorderCommand = new DelegateCommand(
//             () => {
//                 IsCreatingNewBorder = true;
//                 NewBorderVertices.Clear();
//             },
//             () => !IsCreatingNewBorder
//         ));
//         
//         private DelegateCommand confirmNewBorderCommand;
//         public DelegateCommand ConfirmNewBorderCommand => confirmNewBorderCommand ?? (confirmNewBorderCommand = new DelegateCommand(
//             () => {
//                 if(NewBorderVertices.Count < 3)
//                     return;
//                 var closingPoint = NewBorderVertices[0];
//                 NewBorderVertices.Add(closingPoint);
//                 SelectedDistrictBorderVertices = new GenericObservableList<PointLatLng>(NewBorderVertices.ToList());
//                 NewBorderVertices.Clear();
//                 SelectedSector.SectorBorder = geometryFactory.CreatePolygon(SelectedDistrictBorderVertices.Select(p => new Coordinate(p.Lat,p.Lng)).ToArray());
//                 IsCreatingNewBorder = false;
//             },
//             () => IsCreatingNewBorder
//         ));
//         
//         private DelegateCommand cancelNewBorderCommand;
//         public DelegateCommand CancelNewBorderCommand => cancelNewBorderCommand ?? (cancelNewBorderCommand = new DelegateCommand(
//             () => {
//                 NewBorderVertices.Clear();
//                 IsCreatingNewBorder = false;
//                 OnPropertyChanged(nameof(NewBorderVertices));
//             },
//             () => IsCreatingNewBorder
//         ));
//         
//         private DelegateCommand removeBorderCommand;
//         public DelegateCommand RemoveBorderCommand => removeBorderCommand ?? (removeBorderCommand = new DelegateCommand(
//             () => {
//                 SelectedSector.SectorBorder = null;
//                 SelectedDistrictBorderVertices.Clear();
//                 OnPropertyChanged(nameof(SelectedDistrictBorderVertices));
//                 OnPropertyChanged(nameof(SelectedSector));
//             },
//             () => !IsCreatingNewBorder
//         ));
//         
//         private DelegateCommand<PointLatLng> addNewVertexCommand;
//         public DelegateCommand<PointLatLng> AddNewVertexCommand => addNewVertexCommand ?? (addNewVertexCommand = new DelegateCommand<PointLatLng>(
//             point => {
//                 NewBorderVertices.Add(point);
//                 OnPropertyChanged(nameof(NewBorderVertices));
//             },
//             point => IsCreatingNewBorder
//         ));
//         
//         private DelegateCommand<PointLatLng> removeNewBorderVerteCommand;
//         public DelegateCommand<PointLatLng> RemoveNewBorderVertexCommand => removeNewBorderVerteCommand ?? (removeNewBorderVerteCommand = new DelegateCommand<PointLatLng>(
//             point => {
//                 NewBorderVertices.Remove(point);
//                 OnPropertyChanged(nameof(NewBorderVertices));
//             },
//             point => IsCreatingNewBorder && !point.IsEmpty
//         ));
//         
//         private DelegateCommand<IEnumerable<DeliverySchedule>> addScheduleRestrictionCommand;
//         public DelegateCommand<IEnumerable<DeliverySchedule>> AddScheduleRestrictionCommand => addScheduleRestrictionCommand ?? (addScheduleRestrictionCommand = new DelegateCommand<IEnumerable<DeliverySchedule>>(
//             schedules => {
//                 foreach (var schedule in schedules) {
//                     if(SelectedWeekDayName.HasValue && ScheduleRestrictions.All(x => x.DeliverySchedule.Id != schedule.Id))
//                         ScheduleRestrictions.Add(new DeliveryScheduleRestriction {
//                             Sector = SelectedSector, WeekDay = SelectedWeekDayName.Value, DeliverySchedule = schedule
//                     });
//                 }
//             },
//             schedules => schedules.Any()
//         ));
//         
//         private DelegateCommand removeScheduleRestrictionCommand;
//         public DelegateCommand RemoveScheduleRestrictionCommand => removeScheduleRestrictionCommand ?? (removeScheduleRestrictionCommand = new DelegateCommand(
//             () => {
//                 ScheduleRestrictions.Remove(SelectedScheduleRestriction);
//             },
//             () => SelectedScheduleRestriction != null
//         ));
//         
//         private DelegateCommand<IEnumerable<DeliveryPriceRule>> addWeekDayDistrictRuleItemCommand;
//         public DelegateCommand<IEnumerable<DeliveryPriceRule>> AddWeekDayDistrictRuleItemCommand => addWeekDayDistrictRuleItemCommand ?? (addWeekDayDistrictRuleItemCommand = new DelegateCommand<IEnumerable<DeliveryPriceRule>>(
//             ruleItems => {
//                 foreach (var ruleItem in ruleItems) {
//                     if(SelectedWeekDayName.HasValue && WeekDayDistrictRuleItems.All(i => i.DeliveryPriceRule.Id != ruleItem.Id)) {
//                         WeekDayDistrictRuleItems.Add(new WeekDayDistrictRuleItem {
//                             Sector = SelectedSector, WeekDay = SelectedWeekDayName.Value, Price = 0,
//                             DeliveryPriceRule = ruleItem
//                         });
//                     }
//                 }
//             },
//             ruleItems => ruleItems.Any()
//         ));
//         
//         private DelegateCommand removeWeekDayDistrictRuleItemCommand;
//         public DelegateCommand RemoveWeekDayDistrictRuleItemCommand => removeWeekDayDistrictRuleItemCommand ?? (removeWeekDayDistrictRuleItemCommand = new DelegateCommand(
//             () => {
//                 WeekDayDistrictRuleItems.Remove(SelectedWeekDayDistrictRuleItem);
//             },
//             () => SelectedWeekDayDistrictRuleItem != null
//         ));
//         
//         private DelegateCommand<IEnumerable<DeliveryPriceRule>> addCommonDistrictRuleItemCommand;
//         public DelegateCommand<IEnumerable<DeliveryPriceRule>> AddCommonDistrictRuleItemCommand => addCommonDistrictRuleItemCommand ?? (addCommonDistrictRuleItemCommand = new DelegateCommand<IEnumerable<DeliveryPriceRule>>(
//             ruleItems => {
//                 foreach (var ruleItem in ruleItems) {
//                     if(CommonDistrictRuleItems.All(i => i.DeliveryPriceRule.Id != ruleItem.Id)) {
//                         CommonDistrictRuleItems.Add(new CommonDistrictRuleItem {
//                             Sector = SelectedSector, Price = 0, DeliveryPriceRule = ruleItem
//                         });
//                     }
//                 }
//             },
//             ruleItems => ruleItems.Any()
//         ));
//         
//         private DelegateCommand removeCommonDistrictRuleItemCommand;
//         public DelegateCommand RemoveCommonDistrictRuleItemCommand => removeCommonDistrictRuleItemCommand ?? (removeCommonDistrictRuleItemCommand = new DelegateCommand(
//             () => {
//                 CommonDistrictRuleItems.Remove(SelectedCommonDistrictRuleItem);
//             },
//             () => SelectedCommonDistrictRuleItem != null
//         ));
//         
//         private DelegateCommand<AcceptBefore> addAcceptBeforeCommand;
//         public DelegateCommand<AcceptBefore> AddAcceptBeforeCommand => addAcceptBeforeCommand ?? (addAcceptBeforeCommand = new DelegateCommand<AcceptBefore>(
//             acceptBefore => {
//                 SelectedScheduleRestriction.AcceptBefore = acceptBefore;
//             },
//             acceptBefore => acceptBefore != null && SelectedScheduleRestriction != null
//         ));
//         
//         private DelegateCommand removeAcceptBeforeCommand;
//         public DelegateCommand RemoveAcceptBeforeCommand => removeAcceptBeforeCommand ?? (removeAcceptBeforeCommand = new DelegateCommand(
//             () => {
//                 SelectedScheduleRestriction.AcceptBefore = null;
//             },
//             () => SelectedScheduleRestriction != null
//         ));
//
//         #endregion
//
//         private void SortDistricts()
//         {
//             for(int i = 0; i < Entity.Districts.Count - 1; i++)
//             {
//                 for(int j = 0; j < Entity.Districts.Count - 2 - i; j++)
//                 {
//                     switch (NaturalStringComparer.CompareStrings(Entity.Districts[j].TariffZone.Name, Entity.Districts[j + 1].TariffZone.Name)) {
//                         case -1:
//                             break;
//                         case 1: (Entity.Districts[j], Entity.Districts[j + 1]) = (Entity.Districts[j + 1], Entity.Districts[j]);
//                             break;
//                         case 0: 
//                             if(String.Compare(Entity.Districts[j].SectorName, Entity.Districts[j + 1].SectorName, StringComparison.InvariantCulture) > 0) 
//                                 (Entity.Districts[j], Entity.Districts[j + 1]) = (Entity.Districts[j + 1], Entity.Districts[j]); 
//                             break;
//                     }
//                 }
//             }
//         }
//
//         public override bool Save(bool close)
//         {
//             if(Entity.Id == 0)
//                 Entity.DateCreated = DateTime.Now;
//             if(base.Save(close)) {
//                 if(!commonServices.InteractiveService.Question("Продолжить редактирование районов?", "Успешно сохранено"))
//                     Close(false, CloseSource.Save);
//                 return true;
//             }
//             return false;
//         }
//
//         public override void Close(bool askSave, CloseSource source)
//         {
//             if(askSave)
//                 TabParent?.AskToCloseTab(this, source);
//             else
//                 TabParent?.ForceCloseTab(this, source);
//         }
//
//         public override bool HasChanges {
//             get => base.HasChanges && (CanEditDistrict || CanEdit);
//             set => base.HasChanges = value;
//         }
//
//         public override void Dispose()
//         {
//             UoW?.Dispose();
//             base.Dispose();
//         }
//     }
// }
