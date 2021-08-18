using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using Gamma.GtkWidgets.Cells;
using Gamma.Utilities;
using GMap.NET;
using GMap.NET.GtkSharp;
using GMap.NET.GtkSharp.Markers;
using Gtk;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Utilities;
using QS.Views.GtkUI;
using QSOrmProject;
using Vodovoz.Additions.Logistic;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.Sectors;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.ViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.Views.Logistic
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SectorsView : TabViewBase<SectorsViewModel>
	{
		private readonly GMapOverlay bordersOverlay = new GMapOverlay("district_borders");
		private readonly GMapOverlay newBordersPreviewOverlay = new GMapOverlay("district_preview_borders");
		private readonly GMapOverlay verticeOverlay = new GMapOverlay("district_vertice");
		private readonly Pen selectedDistrictBorderPen = new Pen(System.Drawing.Color.Red, 2);
		
		private const string acceptBeforeColumnTag = "Прием до";
		
		public SectorsView(SectorsViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureView();
		}

		private void ConfigureView()
		{
			#region TreeViews

			var colorRed = new Gdk.Color(255, 0, 0);
			var colorWhite = new Gdk.Color(255, 255, 255);

			treeViewMainDistricts.ColumnsConfig = FluentColumnsConfig<SectorNodeViewModel>
				.Create()
				.AddColumn("Код").AddTextRenderer(x=>x.Id.ToString())
				.AddColumn("Название").AddTextRenderer(x=>x.Name).Editable()
				.AddColumn("Дата создания").AddTextRenderer(x=>x.CreateDate.ToShortDateString())
				.Finish();
			treeViewMainDistricts.Binding.AddBinding(ViewModel, s => s.ObservableSectorNodeViewModels, t => t.ItemsDataSource).InitializeFromSource();
			treeViewMainDistricts.Selection.Changed += (sender, args) =>
				ViewModel.SelectedSectorNodeViewModel = treeViewMainDistricts.GetSelectedObject<SectorNodeViewModel>();

			treeViewMainProperty.ColumnsConfig = FluentColumnsConfig<SectorVersion>
				.Create()
				.AddColumn("Начало")
					.AddTextRenderer(x => x.StartDate.HasValue ? x.StartDate.Value.ToShortDateString() : "")
					.Editable()
				.AddColumn("Окончание")
					.AddTextRenderer(x => x.EndDate.HasValue ? x.EndDate.Value.ToShortDateString() : "")
					.Editable()
				.AddColumn("Статус")
					.AddComboRenderer(x => x.Status)
					.SetDisplayFunc(x => x.GetEnumTitle())
					.FillItems(((SectorsSetStatus[])Enum.GetValues(typeof(SectorsSetStatus))).ToList())
					.AddSetter((c, n) =>
				{
					if(n.Status == SectorsSetStatus.Active || n.Status == SectorsSetStatus.Closed)
						c.Editable = false;
					else if(n.Status == SectorsSetStatus.Draft)
					{
						c.Editable = true;
						c.Items = new List<SectorsSetStatus> {SectorsSetStatus.Draft, SectorsSetStatus.OnActivation};
						c.UpdateComboList(default);
					}
					if(ViewModel.SectorVersions.Count(x => x.Status == SectorsSetStatus.OnActivation) == 1)
					{
						c.Editable = true;
						c.Items = new List<SectorsSetStatus> {SectorsSetStatus.Draft};
						c.UpdateComboList(default);
					}
				})
					.Editing()
				.AddColumn("Название")
					.AddTextRenderer(x => x.SectorName)
					.Editable()
				.AddColumn("Тарифная зона")
					.AddComboRenderer(x => x.TariffZone)
					.SetDisplayFunc(x => x.Name)
					.FillItems(ViewModel.UoW.GetAll<TariffZone>().ToList(), "Нет")
					.Editing()
				.AddColumn("Часть города")
					.AddComboRenderer(x => x.GeographicGroup)
					.SetDisplayFunc(x => x.Name)
					.FillItems(ViewModel.UoW.GetAll<GeographicGroup>().ToList(), "Нет")
					.Editing()
				.AddColumn("Зарплатный тип")
					.AddEnumRenderer(x=>x.PriceType)
				.AddColumn("Город/пригород")
					.AddComboRenderer(x => x.WageSector)
					.SetDisplayFunc(x => x.Name)
					.FillItems(ViewModel.UoW.GetAll<WageSector>().ToList(), "Нет")
					.AddSetter((combo, version) =>
					{
						combo.Editable = ViewModel.CanEditSector && ViewModel.CanChangeSectorWageTypePermissionResult;
					})
					.Editing()
				.Finish();
			treeViewMainProperty.Binding.AddBinding(ViewModel, s => s.SectorVersions, t => t.ItemsDataSource);
			treeViewMainProperty.Selection.Changed += (sender, args) =>
				{
					if(ViewModel.IsCreatingNewBorder)
					{
						ViewModel.CancelNewBorderCommand.Execute();
						previewMapBtn.Active = false;
					}

					ViewModel.SelectedSectorVersion = treeViewMainProperty.GetSelectedObject<SectorVersion>();
					RefreshVerticeBorders();
					RefreshBorders();
				};
			
			treeViewRulesDelivery.ColumnsConfig = FluentColumnsConfig<SectorDeliveryRuleVersion>
				.Create()
				.AddColumn("Начало")
					.AddTextRenderer(x => x.StartDate.HasValue ? x.StartDate.Value.ToShortDateString() : "")
				.Editable()
				.AddColumn("Окончание")
					.AddTextRenderer(x => x.EndDate.HasValue ? x.EndDate.Value.ToShortDateString() : "")
					.Editable()
				.AddColumn("Статус")
					.AddComboRenderer(x => x.Status)
					.SetDisplayFunc(x => x.GetEnumTitle())
					.FillItems(((SectorsSetStatus[]) Enum.GetValues(typeof(SectorsSetStatus))).ToList())
					.AddSetter((c, n) =>
					{
						if(n.Status == SectorsSetStatus.Active || n.Status == SectorsSetStatus.Closed)
							c.Editable = false;
						if(ViewModel.ObservableSectorDeliveryRuleVersions.Count(x => x.Status == SectorsSetStatus.OnActivation) == 1)
						{
							c.Editable = true;
							c.Items = new List<SectorsSetStatus> {SectorsSetStatus.Draft};
							c.UpdateComboList(default);
						}
						else
						{
							c.Editable = true;
							c.Items = new List<SectorsSetStatus> {SectorsSetStatus.Draft, SectorsSetStatus.OnActivation};
							c.UpdateComboList(default);
						}
					})
					.Editing()
				.Finish();
			treeViewRulesDelivery.Binding.AddBinding(ViewModel, s => s.ObservableSectorDeliveryRuleVersions, t => t.ItemsDataSource);
			treeViewRulesDelivery.Selection.Changed += (sender, args) =>
				ViewModel.SelectedDeliveryRuleVersion = treeViewRulesDelivery.GetSelectedObject<SectorDeliveryRuleVersion>();

			treeViewRules.ColumnsConfig = FluentColumnsConfig<CommonDistrictRuleItem>
				.Create()
				.AddColumn("Цена")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(p => p.Price)
				.Digits(2)
					.WidthChars(10)
					.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0))
					.Editing()
					.AddSetter((c, r) => c.BackgroundGdk = r.Price <= 0 ? colorRed : colorWhite)
				.AddTextRenderer(node => CurrencyWorks.CurrencyShortName, false)
				.AddColumn("Правило")
					.HeaderAlignment(0.5f)
					.AddComboRenderer(p => p.DeliveryPriceRule)
					.SetDisplayFunc(x=>x.Title)
					.FillItems(ViewModel.UoW.GetAll<DeliveryPriceRule>().ToList(), "Нет")
					.Editing()
				.Finish();
			treeViewRules.Binding.AddBinding(ViewModel, s => s.ObservableCommonDistrictRuleItems, t => t.ItemsDataSource);
			treeViewRules.Selection.Changed += (sender, args) =>
				ViewModel.SelectedCommonDistrictRuleItem = treeViewRules.GetSelectedObject<CommonDistrictRuleItem>();

			
			treeViewScheduleVersions.ColumnsConfig = FluentColumnsConfig<SectorWeekDayScheduleVersion>
				.Create()
				.AddColumn("Начало")
					.AddTextRenderer(x => x.StartDate.HasValue ? x.StartDate.Value.ToShortDateString() : "")
					.Editable()
				.AddColumn("Окончание")
					.AddTextRenderer(x => x.EndDate.HasValue ? x.EndDate.Value.ToShortDateString() : "")
					.Editable()
				.AddColumn("Статус")
					.AddComboRenderer(x => x.Status)
					.SetDisplayFunc(x => x.GetEnumTitle())
					.FillItems(((SectorsSetStatus[]) Enum.GetValues(typeof(SectorsSetStatus))).ToList())
					.AddSetter((c, n) =>
					{
						if(n.Status == SectorsSetStatus.Active || n.Status == SectorsSetStatus.Closed)
							c.Editable = false;
						if(ViewModel.ObservableSectorWeekDayScheduleVersions.Count(x => x.Status == SectorsSetStatus.OnActivation) == 1)
						{
							c.Editable = true;
							c.Items = new List<SectorsSetStatus> {SectorsSetStatus.Draft};
							c.UpdateComboList(default);
						}
						else
						{
							c.Editable = true;
							c.Items = new List<SectorsSetStatus> {SectorsSetStatus.Draft, SectorsSetStatus.OnActivation};
							c.UpdateComboList(default);
						}
					})
					.Editing()
				.Finish();
			treeViewScheduleVersions.Binding.AddBinding(ViewModel, s => s.ObservableSectorWeekDayScheduleVersions, t => t.ItemsDataSource);
			treeViewScheduleVersions.Selection.Changed += (sender, args) =>
				ViewModel.SelectedWeekDayScheduleVersion = treeViewScheduleVersions.GetSelectedObject<SectorWeekDayScheduleVersion>();
			
			treeViewGraphic.ColumnsConfig = ColumnsConfigFactory.Create<DeliveryScheduleRestriction>()
				.AddColumn("График")
					.AddTextRenderer(x=>x.DeliverySchedule.Name)
				.AddColumn("Прием до")
					.SetTag(acceptBeforeColumnTag)
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.AcceptBeforeTitle)
					.AddSetter((c, r) => c.BackgroundGdk = r.AcceptBefore == null ? colorRed : colorWhite)
				.Finish();
			treeViewGraphic.Binding.AddBinding(ViewModel, s => s.ObservableSectorDayDeliveryRestrictions, t => t.ItemsDataSource);
			treeViewGraphic.Selection.Changed += (sender, args) =>
				ViewModel.SelectedScheduleRestriction = treeViewGraphic.GetSelectedObject<DeliveryScheduleRestriction>(); 
			
			treeViewDeliveryRules.ColumnsConfig = FluentColumnsConfig<SectorWeekDayDeliveryRuleVersion>
				.Create()
				.AddColumn("Начало")
					.AddTextRenderer(x => x.StartDate.HasValue ? x.StartDate.Value.ToShortDateString() : "")
					.Editable()
				.AddColumn("Окончание")
					.AddTextRenderer(x => x.EndDate.HasValue ? x.EndDate.Value.ToShortDateString() : "")
					.Editable()
				.AddColumn("Статус")
					.AddComboRenderer(x => x.Status)
					.SetDisplayFunc(x => x.GetEnumTitle())
					.FillItems(((SectorsSetStatus[]) Enum.GetValues(typeof(SectorsSetStatus))).ToList())
					.AddSetter((c, n) =>
					{
						if(n.Status == SectorsSetStatus.Active || n.Status == SectorsSetStatus.Closed)
							c.Editable = false;
						if(ViewModel.ObservableSectorWeekDeliveryRuleVersions.Count(x => x.Status == SectorsSetStatus.OnActivation) == 1)
						{
							c.Editable = true;
							c.Items = new List<SectorsSetStatus> {SectorsSetStatus.Draft};
							c.UpdateComboList(default);
						}
						else
						{
							c.Editable = true;
							c.Items = new List<SectorsSetStatus> {SectorsSetStatus.Draft, SectorsSetStatus.OnActivation};
							c.UpdateComboList(default);
						}
					})
					.Editing()
				.Finish();
			treeViewDeliveryRules.Binding.AddBinding(ViewModel, s => s.ObservableSectorWeekDeliveryRuleVersions, t => t.ItemsDataSource);
			treeViewDeliveryRules.Selection.Changed += (sender, args) =>
				ViewModel.SelectedWeekDayDeliveryRuleVersion = treeViewDeliveryRules.GetSelectedObject<SectorWeekDayDeliveryRuleVersion>();

			treeViewSpecialRules.ColumnsConfig = FluentColumnsConfig<WeekDayDistrictRuleItem>
				.Create()
				.AddColumn("Цена")
				.HeaderAlignment(0.5f)
				.AddNumericRenderer(p => p.Price)
				.Digits(2)
				.WidthChars(10)
				.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0))
				.Editing()
				.AddSetter((c, r) => c.BackgroundGdk = r.Price <= 0 ? colorRed : colorWhite)
				.AddTextRenderer(node => CurrencyWorks.CurrencyShortName, false)
				.AddColumn("Правило")
				.HeaderAlignment(0.5f)
				.AddComboRenderer(p => p.DeliveryPriceRule)
				.SetDisplayFunc(x=>x.Title)
				.FillItems(ViewModel.UoW.GetAll<DeliveryPriceRule>().ToList(), "Нет")
				.Editing()
				.Finish();
			treeViewSpecialRules.Binding.AddBinding(ViewModel, s => s.ObservableSectorDeliveryRules, t => t.ItemsDataSource);
			treeViewSpecialRules.Selection.Changed += (sender, args) =>
				ViewModel.SelectedWeekDayDistrictRuleItem = treeViewSpecialRules.GetSelectedObject<WeekDayDistrictRuleItem>();


			#endregion

			#region Binding of the datepickers

			dateForFilter.Binding.AddBinding(ViewModel, vm => vm.StartDateSector, t => t.StartDateOrNull).InitializeFromSource();
			dateForFilter.Binding.AddBinding(ViewModel, vm => vm.EndDateSector, t => t.EndDateOrNull).InitializeFromSource();
			sectorVersionStartDate.Binding.AddBinding(ViewModel, vm => vm.StartDateSectorVersion, t => t.DateOrNull)
				.InitializeFromSource();
			deliveryRuleStartDate.Binding.AddBinding(ViewModel, vm => vm.StartDateSectorDeliveryRule, t => t.DateOrNull)
				.InitializeFromSource();
			weekDayScheduleStartDate.Binding.AddBinding(ViewModel, vm => vm.StartDateSectorDaySchedule, t => t.DateOrNull)
				.InitializeFromSource();
			weekDayDeliveryStartDate.Binding.AddBinding(ViewModel, vm => vm.StartDateSectorDayDeliveryRule, t => t.DateOrNull)
				.InitializeFromSource();
			#endregion

			#region Binding of the button for sector and sectorVersion

			btnAddDistrict.Binding.AddFuncBinding(ViewModel, vm => vm.CanCreateSector, t => t.Sensitive).InitializeFromSource();
			btnAddDistrict.Clicked += (sender, args) => ViewModel.AddSector.Execute();
			
			btnRemoveDistrict.Binding.AddFuncBinding(ViewModel, vm => vm.CanDeleteSector, t => t.Sensitive).InitializeFromSource();
			btnRemoveDistrict.Clicked += (sender, args) => ViewModel.RemoveSector.Execute();

			btnAddMainProperty.Binding.AddFuncBinding(ViewModel, vm => vm.SelectedSectorNodeViewModel != null && vm.CanEditSector, t => t.Sensitive)
				.InitializeFromSource();
			btnAddMainProperty.Clicked += (sender, args) => ViewModel.AddSectorVersion.Execute();

			btnRemoveMainProperty.Binding
				.AddFuncBinding(ViewModel, vm => vm.SelectedSectorVersion != null && vm.CanEditSector, t => t.Sensitive)
				.InitializeFromSource();
			btnRemoveMainProperty.Clicked += (sender, args) => ViewModel.RemoveSectorVersion.Execute();
			
			#endregion

			#region Binding of the button for common rules

			btnAddRulesDelivery.Binding.AddFuncBinding(ViewModel, vm => vm.SelectedSectorNodeViewModel != null && vm.CanEditSector, t => t.Sensitive)
				.InitializeFromSource();
			btnAddRulesDelivery.Clicked += (sender, args) => ViewModel.AddRulesDelivery.Execute();

			btnRemoveRulesDelivery.Binding
				.AddFuncBinding(ViewModel, vm => vm.SelectedDeliveryRuleVersion != null && vm.CanEditSector, t => t.Sensitive)
				.InitializeFromSource();
			btnRemoveRulesDelivery.Clicked += (sender, args) => ViewModel.RemoveRulesDelivery.Execute();

			btnCopyRulesDelivery.Binding
				.AddFuncBinding(ViewModel, vm => vm.SelectedDeliveryRuleVersion != null && vm.CanEditSector, t => t.Sensitive)
				.InitializeFromSource();
			btnCopyRulesDelivery.Clicked += (sender, args) => ViewModel.CopyRulesDelivery.Execute();
			
			btnAddRules.Binding
				.AddFuncBinding(ViewModel, vm => vm.SelectedDeliveryRuleVersion != null && vm.CanEditSector, t => t.Sensitive)
				.InitializeFromSource();
			btnAddRules.Clicked += (sender, args) => ViewModel.AddCommonDistrictRule.Execute();

			btnRemoveRules.Binding
				.AddFuncBinding(ViewModel, vm => vm.SelectedCommonDistrictRuleItem != null && vm.CanEditSector, t => t.Sensitive)
				.InitializeFromSource();
			btnRemoveRules.Clicked += (sender, args) => ViewModel.RemoveCommonDistrictRule.Execute();
			
			#endregion

			btnOnActive.Clicked += (sender, args) => ViewModel.Activate.Execute();

			#region Binding todayBtns

			btnToday.Clicked += (sender, args) => ViewModel.SelectedWeekDayName = WeekDayName.Today;
			btnMonday.Clicked += (sender, args) => ViewModel.SelectedWeekDayName = WeekDayName.Monday;
			btnTuesday.Clicked += (sender, args) => ViewModel.SelectedWeekDayName = WeekDayName.Tuesday;
			btnWednesday.Clicked += (sender, args) => ViewModel.SelectedWeekDayName = WeekDayName.Wednesday;
			btnThursday.Clicked += (sender, args) => ViewModel.SelectedWeekDayName = WeekDayName.Thursday;
			btnFriday.Clicked += (sender, args) => ViewModel.SelectedWeekDayName = WeekDayName.Friday;
			btnSaturday.Clicked += (sender, args) => ViewModel.SelectedWeekDayName = WeekDayName.Saturday;
			btnSunday.Clicked += (sender, args) => ViewModel.SelectedWeekDayName = WeekDayName.Sunday;
			
			#endregion

			#region Binding on the button for schedule of days

			btnAddDaySchedule.Binding
				.AddFuncBinding(ViewModel, vm => vm.SelectedSectorNodeViewModel != null && vm.CanEditSector, t => t.Sensitive)
				.InitializeFromSource();
			btnAddDaySchedule.Clicked += (sender, args) => ViewModel.AddSectorWeekDayScheduleVersion.Execute();

			btnRemoveDaySchedule.Binding
				.AddFuncBinding(ViewModel, vm => vm.SelectedWeekDayScheduleVersion != null && vm.CanEditSector, t => t.Sensitive)
				.InitializeFromSource();
			btnRemoveDaySchedule.Clicked += (sender, args) => ViewModel.RemoveSectorWeekDayScheduleVersion.Execute(); 

			btnCopyDaySchedule.Binding
				.AddFuncBinding(ViewModel, vm => vm.SelectedWeekDayScheduleVersion != null && vm.CanEditSector, t => t.Sensitive)
				.InitializeFromSource();
			btnCopyDaySchedule.Clicked += (sender, args) => ViewModel.CopySectorWeekDaySchedule.Execute(); 
			
			btnAddGraphic.Binding.AddFuncBinding(ViewModel,
					vm => vm.SelectedWeekDayName != null && vm.SelectedWeekDayScheduleVersion != null && vm.CanEditSector, t => t.Sensitive)
				.InitializeFromSource();
			btnAddGraphic.Clicked += (sender, args) =>
			{
				var selectSchedules = new OrmReference(typeof(DeliverySchedule), ViewModel.UoW) {
					Mode = OrmReferenceMode.MultiSelect
				};
				selectSchedules.ObjectSelected += (o, eventArgs) => {
					ViewModel.AddScheduleRestrictionCommand.Execute(eventArgs.Subjects.Cast<DeliverySchedule>());

					if(ViewModel.SelectedScheduleRestriction != null) {
						var iter = treeViewGraphic.YTreeModel.IterFromNode(ViewModel.SelectedScheduleRestriction);
						var path = treeViewGraphic.YTreeModel.GetPath(iter);
						treeViewGraphic.ScrollToCell(path, treeViewGraphic.Columns.FirstOrDefault(), false, 0, 0);
					}
				};
				Tab.TabParent.AddSlaveTab(this.Tab, selectSchedules);
			};
			
			btnRemoveGraphic.Binding.AddFuncBinding(ViewModel, vm => vm.SelectedWeekDayScheduleVersion != null && vm.CanEditSector, t => t.Sensitive).InitializeFromSource();
			btnRemoveGraphic.Clicked += (sender, args) => ViewModel.RemoveScheduleRestrictionCommand.Execute();

			btnAddRestriction.Binding.AddFuncBinding(ViewModel,
					vm => vm.CanEditSector && vm.SelectedScheduleRestriction != null, w => w.Sensitive)
				.InitializeFromSource();
			btnAddRestriction.Clicked += (sender, args) =>
			{
				var acceptBeforeTimeViewModel = new SimpleEntityJournalViewModel<AcceptBefore, AcceptBeforeViewModel>(
					x => x.Name,
					() => new AcceptBeforeViewModel(
						EntityUoWBuilder.ForCreate(),
						UnitOfWorkFactory.GetDefaultFactory,
						ServicesConfig.CommonServices
					),
					node => new AcceptBeforeViewModel(
						EntityUoWBuilder.ForOpen(node.Id),
						UnitOfWorkFactory.GetDefaultFactory,
						ServicesConfig.CommonServices
					),
					UnitOfWorkFactory.GetDefaultFactory,
					ServicesConfig.CommonServices
				);
				acceptBeforeTimeViewModel.SelectionMode = JournalSelectionMode.Single;
				acceptBeforeTimeViewModel.SetActionsVisible(deleteActionEnabled: false, editActionEnabled: false);
				acceptBeforeTimeViewModel.OnEntitySelectedResult += (o, eventArgs) =>
				{
					var node = eventArgs.SelectedNodes.FirstOrDefault();
					if(node != null)
					{
						ViewModel.AddAcceptBeforeCommand.Execute(ViewModel.UoW.GetById<AcceptBefore>(node.Id));
					}
				};
				Tab.TabParent.AddSlaveTab(Tab, acceptBeforeTimeViewModel);
			};

			btnRemoveRestriction.Binding
				.AddFuncBinding(ViewModel, vm => vm.CanEditSector && vm.SelectedWeekDayScheduleVersion != null, w => w.Sensitive)
				.InitializeFromSource();
			btnRemoveRestriction.Clicked += (sender, args) => ViewModel.RemoveAcceptBeforeCommand.Execute();

			#endregion

			#region Binding of the button for delivery rule on days

			btnAddDayDeliveryRule.Binding
				.AddFuncBinding(ViewModel, vm => vm.SelectedSectorNodeViewModel != null && vm.CanEditSector, t => t.Sensitive)
				.InitializeFromSource();
			btnAddDayDeliveryRule.Clicked += (sender, args) => ViewModel.AddWeekDayDeliveryRule.Execute();

			btnRemoveDayDeliveryRule.Binding
				.AddFuncBinding(ViewModel, vm => vm.SelectedWeekDayDeliveryRuleVersion != null && vm.CanEditSector, t => t.Sensitive)
				.InitializeFromSource();
			btnRemoveDayDeliveryRule.Clicked += (sender, args) => ViewModel.RemoveWeekDayDeliveryRule.Execute();

			btnCopyDayDeliveryRule.Binding
				.AddFuncBinding(ViewModel, vm => vm.SelectedWeekDayDeliveryRuleVersion != null && vm.CanEditSector, t => t.Sensitive)
				.InitializeFromSource();
			btnCopyDayDeliveryRule.Clicked += (sender, args) => ViewModel.CopyDayDeliveryRule.Execute();
			
			btnAddSpecialRules.Binding.AddFuncBinding(ViewModel, vm => vm.SelectedWeekDayDeliveryRuleVersion != null && vm.CanEditSector && vm.SelectedWeekDayName != null, t => t.Sensitive).InitializeFromSource();
			btnAddSpecialRules.Clicked += (sender, args) => ViewModel.AddWeekDayDistrictRule.Execute();
			
			btnRemoveSpecial.Binding.AddFuncBinding(ViewModel, vm => vm.SelectedWeekDayDeliveryRuleVersion != null && vm.CanEditSector, t => t.Sensitive).InitializeFromSource();
			btnRemoveSpecial.Clicked += (sender, args) => ViewModel.RemoveWeekDayDistrictRule.Execute();

			#endregion

			#region Фильтр

			filterBtn.Clicked += (sender, args) => ViewModel.FilterableSectors.Execute();
			enumStatusDistrict.Binding.AddBinding(ViewModel, vm => vm.Status, t => t.SelectedItemOrNull).InitializeFromSource();
			enumStatusDistrict.ItemsEnum = typeof(SectorsSetStatus);
			enumStatusDistrict.EnumItemSelected += (sender, args) => ViewModel.Status = ((SectorsSetStatus) args.SelectedItem);
			
			#endregion
			
			#region GMap

			AddBorderBtn.Binding.AddFuncBinding(ViewModel, vm => !vm.IsCreatingNewBorder, w => w.Visible).InitializeFromSource();
			AddBorderBtn.Binding.AddFuncBinding(ViewModel, vm => vm.CanEditSector && !vm.IsCreatingNewBorder && vm.SelectedSectorVersion != null && vm.SelectedSectorVersion.Polygon == null, w => w.Sensitive).InitializeFromSource();
			AddBorderBtn.Clicked += (sender, args) => ViewModel.CreateBorderCommand.Execute();

			RemoveBorderBtn.Binding.AddFuncBinding(ViewModel, vm => !vm.IsCreatingNewBorder, w => w.Visible).InitializeFromSource();
			RemoveBorderBtn.Binding.AddFuncBinding(ViewModel, vm => vm.CanEditSector && !vm.IsCreatingNewBorder && vm.SelectedSectorVersion != null && vm.SelectedSectorVersion.Polygon != null, w => w.Sensitive).InitializeFromSource();
			RemoveBorderBtn.Clicked += (sender, args) => {
				if(MessageDialogHelper.RunQuestionDialog($"Удалить границу района {ViewModel.SelectedSectorVersion.SectorName}?")) {
					ViewModel.RemoveBorderCommand.Execute();
					RefreshBorders();
				}
			};

			ApplyBorderBtn.Binding.AddFuncBinding(ViewModel, vm => vm.IsCreatingNewBorder, w => w.Visible).InitializeFromSource();
			ApplyBorderBtn.Binding.AddFuncBinding(ViewModel, vm => vm.SelectedSectorVersion != null && vm.IsCreatingNewBorder, w => w.Sensitive).InitializeFromSource();
			ApplyBorderBtn.Clicked += (sender, args) => {
				if(MessageDialogHelper.RunQuestionDialog("Завершить создание границы района?")) {
					if(ViewModel.NewBorderVertices.Count < 3) {
						MessageDialogHelper.RunInfoDialog("Нельзя создать границу района меньше чем за 3 точки");
						return;
					}
					previewMapBtn.Active = false;
					ViewModel.ConfirmNewBorderCommand.Execute();
					RefreshBorders();
				}
			};

			CancelBorderBtn.Binding.AddFuncBinding(ViewModel, vm => vm.IsCreatingNewBorder, w => w.Visible).InitializeFromSource();
			CancelBorderBtn.Binding.AddFuncBinding(ViewModel, vm => vm.SelectedSectorVersion != null && vm.IsCreatingNewBorder, w => w.Sensitive).InitializeFromSource();
			CancelBorderBtn.Clicked += (sender, args) => {
				if(MessageDialogHelper.RunQuestionDialog("Отменить создание границы района?")) {
					ViewModel.CancelNewBorderCommand.Execute();
					previewMapBtn.Active = false;
				}
			};

			previewMapBtn.Binding.AddFuncBinding(ViewModel, vm => vm.IsCreatingNewBorder, w => w.Visible).InitializeFromSource();
			previewMapBtn.Binding.AddFuncBinding(ViewModel, vm => vm.SelectedSectorVersion != null && vm.IsCreatingNewBorder, w => w.Sensitive).InitializeFromSource();
			previewMapBtn.Clicked += (sender, args) => {
				if(previewMapBtn.Active && ViewModel.NewBorderVertices.Any()) {
					var previewBorder = new GMapPolygon(ViewModel.NewBorderVertices.ToList(), "Предпросмотр новых границ");
					newBordersPreviewOverlay.Polygons.Add(previewBorder);
				}
				else {
					newBordersPreviewOverlay.Clear();
				}
			};

			enumProviderMap.ItemsEnum = typeof(MapProviders);
			enumProviderMap.EnumItemSelected += (sender, args) => gmapwidget.MapProvider = MapProvidersHelper.GetPovider((MapProviders) args.SelectedItem);
			enumProviderMap.SelectedItem = MapProviders.GoogleMap;

			gmapwidget.Position = new PointLatLng(59.93900, 30.31646);
			gmapwidget.HeightRequest = 150;
			gmapwidget.HasFrame = true;
			gmapwidget.Overlays.Add(bordersOverlay);
			gmapwidget.Overlays.Add(newBordersPreviewOverlay);
			gmapwidget.Overlays.Add(verticeOverlay);
			RefreshBorders();

			gmapwidget.ButtonPressEvent += (o, args) => {
				if(args.Event.Button == 1 && ViewModel.IsCreatingNewBorder) {
					ViewModel.AddNewVertexCommand.Execute(gmapwidget.FromLocalToLatLng((int) args.Event.X, (int) args.Event.Y));
				}
				if(args.Event.Button == 3 && ViewModel.IsCreatingNewBorder) {
					var marker = verticeOverlay.Markers.FirstOrDefault(m => m.IsMouseOver);
					if(marker == null)
						return;

					var pointMarker = new PointLatLng(marker.Position.Lat, marker.Position.Lng);
					if(ViewModel.NewBorderVertices.Contains(pointMarker)) {
						Menu popupMenu = new Menu();
						var item = new MenuItem("Удалить");
						item.Activated += (sender, e) => ViewModel.RemoveNewBorderVertexCommand.Execute(pointMarker);
						popupMenu.Add(item);
						popupMenu.ShowAll();
						popupMenu.Popup();
					}
				}
			};

			void RefreshBorders()
			{
				bordersOverlay.Clear();
				if(ViewModel.SelectedSectorVersion != null)
				{
					var sectorVersion = ViewModel.SelectedSectorVersion;
					if(sectorVersion.Polygon != null)
					{
						bordersOverlay.Polygons.Add(new GMapPolygon(
							sectorVersion.Polygon.Coordinates.Select(p => new PointLatLng(p.X, p.Y)).ToList(), sectorVersion.SectorName)
						);
					}
				}
			}

			void RefreshVerticeBorders()
			{
				verticeOverlay.Clear();
				if(ViewModel.SelectedSectorVersion != null)
				{
					var sectorVersion = ViewModel.SelectedSectorVersion;
					if(sectorVersion.Polygon != null)
					{
						verticeOverlay.Polygons.Add(new GMapPolygon(
							sectorVersion.Polygon.Coordinates.Select(p => new PointLatLng(p.X, p.Y)).ToList(), sectorVersion.SectorName)
						);
					}
				}
			}
			
			ViewModel.PropertyChanged += (sender, args) => {
				Application.Invoke((o, eventArgs) => {
					switch (args.PropertyName) {
						case nameof(ViewModel.SelectedWeekDayName):
							var column = treeViewGraphic.ColumnsConfig.GetColumnsByTag(acceptBeforeColumnTag).First();
							column.Visible = ViewModel.SelectedWeekDayName == WeekDayName.Today;
							break;
						case nameof(ViewModel.SelectedSectorNodeViewModel):
							if(ViewModel.SelectedSectorNodeViewModel != null)
								treeViewMainDistricts.SelectObject(ViewModel.SelectedSectorNodeViewModel);
							break;
						case nameof(ViewModel.SelectedScheduleRestriction):
							if(ViewModel.SelectedScheduleRestriction != null)
								treeViewGraphic.SelectObject(ViewModel.SelectedScheduleRestriction);
							break;
						case nameof(ViewModel.SelectedCommonDistrictRuleItem):
							if(ViewModel.SelectedCommonDistrictRuleItem != null)
								treeViewRules.SelectObject(ViewModel.SelectedCommonDistrictRuleItem);
							break;
						case nameof(ViewModel.SelectedWeekDayDistrictRuleItem):
							if(ViewModel.SelectedWeekDayDistrictRuleItem != null)
								treeViewSpecialRules.SelectObject(ViewModel.SelectedWeekDayDistrictRuleItem);
							break;
						case nameof(ViewModel.SelectedDistrictBorderVertices):
							verticeOverlay.Clear();
							if(ViewModel.SelectedDistrictBorderVertices != null){
                                GMapPolygon polygon = new GMapPolygon(ViewModel.SelectedDistrictBorderVertices.ToList(), "polygon");
                                polygon.Stroke = selectedDistrictBorderPen;
                                verticeOverlay.Polygons.Add(polygon);
                            }
							break;
						case nameof(ViewModel.NewBorderVertices):
							verticeOverlay.Clear();
							if(ViewModel.NewBorderVertices != null && ViewModel.NewBorderVertices.Any()) {
								for (int i = 0; i < ViewModel.NewBorderVertices.Count; i++) {
									var color = GMarkerGoogleType.red;
									if(i == 0)
										color = GMarkerGoogleType.yellow;
									else if(i == ViewModel.NewBorderVertices.Count - 1)
										color = GMarkerGoogleType.green;
									GMapMarker point = new GMarkerGoogle(ViewModel.NewBorderVertices[i], color);
									verticeOverlay.Markers.Add(point);
								}
								if(previewMapBtn.Active) {
									previewMapBtn.Active = false;
									previewMapBtn.Active = true;
								}
							}
							break;
					}
				});
			};
			#endregion

			#region SaveAndCancelButton

			btnSave.Clicked += (sender, args) => ViewModel.Save();

			#endregion
		}
	}
}
