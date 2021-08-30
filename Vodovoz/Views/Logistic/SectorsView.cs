using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using GMap.NET;
using GMap.NET.GtkSharp;
using GMap.NET.GtkSharp.Markers;
using Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Navigation;
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
				.AddColumn("Окончание")
					.AddTextRenderer(x => x.EndDate.HasValue ? x.EndDate.Value.ToShortDateString() : "")
				.AddColumn("Статус")
					.AddComboRenderer(x => x.Status)
					.SetDisplayFunc(x => x.GetEnumTitle())
					.FillItems(((SectorsSetStatus[]) Enum.GetValues(typeof(SectorsSetStatus))).ToList())
					.AddSetter((c, n) =>
					{
						if(ViewModel.ObservableSectorVersions.Count(x => x.Status == SectorsSetStatus.OnActivation) == 1)
						{
							c.Editable = true;
							c.Items = new List<SectorsSetStatus> {SectorsSetStatus.Draft};
							c.UpdateComboList(default);
						}
					
						if(n.Status == SectorsSetStatus.Active || n.Status == SectorsSetStatus.Closed)
							c.Editable = false;
						else if(n.Status == SectorsSetStatus.Draft)
						{
							c.Editable = true;
							c.Items = new List<SectorsSetStatus> {SectorsSetStatus.Draft, SectorsSetStatus.OnActivation};
							c.UpdateComboList(default);
						}
					})
					.Editing()
				.AddColumn("Название")
					.AddTextRenderer(x => x.SectorName)
					.AddSetter((c, n) => c.Editable = n.Status != SectorsSetStatus.Active)
				.AddColumn("Тарифная зона")
					.AddComboRenderer(x => x.TariffZone)
					.SetDisplayFunc(x => x.Name)
					.FillItems(ViewModel.UoW.GetAll<TariffZone>().ToList(), "Нет")
					.AddSetter((c, n) => c.Editable = n.Status != SectorsSetStatus.Active)
				.AddColumn("Мин. бутылей")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x =>
						x.Sector.GetActiveDeliveryRuleVersion(DateTime.Now) != null ? x.Sector.GetActiveDeliveryRuleVersion(DateTime.Now).ObservableCommonDistrictRuleItems.Any()
							? x.Sector.GetActiveDeliveryRuleVersion(DateTime.Now).ObservableCommonDistrictRuleItems.Min(c => c.DeliveryPriceRule.Water19LCount).ToString()
							: "-" : "-"
					)
					.XAlign(0.5f)
				.AddColumn("Часть города")
					.AddComboRenderer(x => x.GeographicGroup)
					.SetDisplayFunc(x => x.Name)
					.FillItems(ViewModel.UoW.GetAll<GeographicGroup>().ToList(), "Нет")
					.AddSetter((c, n) => c.Editable = n.Status != SectorsSetStatus.Active)
				.AddColumn("Зарплатный тип")
					.AddEnumRenderer(x => x.PriceType)
				.AddColumn("Город/пригород")
					.AddComboRenderer(x => x.WageSector)
					.SetDisplayFunc(x => x.Name)
					.FillItems(ViewModel.UoW.GetAll<WageSector>().ToList(), "Нет")
					.AddSetter((combo, version) => combo.Editable =
						ViewModel.CanEditSector && ViewModel.CanChangeSectorWageTypePermissionResult &&
						version.Status != SectorsSetStatus.Active)
				.Finish();
			treeViewMainProperty.Binding.AddBinding(ViewModel, s => s.ObservableSectorVersions, t => t.ItemsDataSource);
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
				.AddColumn("Окончание")
					.AddTextRenderer(x => x.EndDate.HasValue ? x.EndDate.Value.ToShortDateString() : "")
				.AddColumn("Статус")
					.AddComboRenderer(x => x.Status)
					.SetDisplayFunc(x => x.GetEnumTitle())
					.FillItems(((SectorsSetStatus[]) Enum.GetValues(typeof(SectorsSetStatus))).ToList())
					.AddSetter((c, n) =>
					{
						if(ViewModel.ObservableSectorDeliveryRuleVersions.Count(x => x.Status == SectorsSetStatus.OnActivation) == 1)
						{
							c.Editable = true;
							c.Items = new List<SectorsSetStatus> {SectorsSetStatus.Draft};
							c.UpdateComboList(default);
						}
						if(n.Status == SectorsSetStatus.Active || n.Status == SectorsSetStatus.Closed)
							c.Editable = false;
						else if(n.Status == SectorsSetStatus.Draft)
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
				ViewModel.SelectedSectorDeliveryRuleVersion = treeViewRulesDelivery.GetSelectedObject<SectorDeliveryRuleVersion>();

			treeViewRules.ColumnsConfig = FluentColumnsConfig<CommonSectorsRuleItem>
				.Create()
				.AddColumn("Цена")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(p => p.Price)
				.Digits(2)
					.WidthChars(10)
					.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0))
					.AddSetter((c, r) =>
				{
					c.Editable = ViewModel.SelectedSectorDeliveryRuleVersion?.Status != SectorsSetStatus.Active;
					c.BackgroundGdk = r.Price <= 0 ? colorRed : colorWhite;
				})
				.AddTextRenderer(node => CurrencyWorks.CurrencyShortName, false)
				.AddColumn("Правило")
					.HeaderAlignment(0.5f)
					.AddComboRenderer(p => p.DeliveryPriceRule)
					.SetDisplayFunc(x=>x.Title)
					.FillItems(ViewModel.ObservablePriceRule, "Нет")
					.AddSetter((c, r) => c.Editable = ViewModel.SelectedSectorDeliveryRuleVersion?.Status != SectorsSetStatus.Active)
				.Finish();
			treeViewRules.Binding.AddBinding(ViewModel, s => s.ObservableCommonDistrictRuleItems, t => t.ItemsDataSource);
			treeViewRules.Selection.Changed += (sender, args) =>
				ViewModel.SelectedCommonSectorsRuleItem = treeViewRules.GetSelectedObject<CommonSectorsRuleItem>();

			
			treeViewScheduleVersions.ColumnsConfig = FluentColumnsConfig<SectorWeekDayScheduleVersion>
				.Create()
				.AddColumn("Начало")
					.AddTextRenderer(x => x.StartDate.HasValue ? x.StartDate.Value.ToShortDateString() : "")
				.AddColumn("Окончание")
					.AddTextRenderer(x => x.EndDate.HasValue ? x.EndDate.Value.ToShortDateString() : "")
				.AddColumn("Статус")
					.AddComboRenderer(x => x.Status)
					.SetDisplayFunc(x => x.GetEnumTitle())
					.FillItems(((SectorsSetStatus[]) Enum.GetValues(typeof(SectorsSetStatus))).ToList())
					.AddSetter((c, n) =>
					{
						if(ViewModel.ObservableSectorWeekDayScheduleVersions.Count(x => x.Status == SectorsSetStatus.OnActivation) == 1)
						{
							c.Editable = true;
							c.Items = new List<SectorsSetStatus> {SectorsSetStatus.Draft};
							c.UpdateComboList(default);
						}
						if(n.Status == SectorsSetStatus.Active || n.Status == SectorsSetStatus.Closed)
							c.Editable = false;
						else if(n.Status == SectorsSetStatus.Draft)
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
				ViewModel.SelectedSectorWeekDayScheduleVersion = treeViewScheduleVersions.GetSelectedObject<SectorWeekDayScheduleVersion>();
			
			treeViewGraphic.ColumnsConfig = ColumnsConfigFactory.Create<DeliveryScheduleRestriction>()
				.AddColumn("График")
					.AddTextRenderer(x=>x.DeliverySchedule.Name)
				.AddColumn("Прием до")
					.SetTag(acceptBeforeColumnTag)
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.AcceptBeforeTitle)
					.AddSetter((c, r) => c.BackgroundGdk = r.AcceptBefore == null ? colorRed : colorWhite)
				.Finish();
			treeViewGraphic.Binding.AddBinding(ViewModel, s => s.ObservableDeliveryScheduleRestriction, t => t.ItemsDataSource);
			treeViewGraphic.Selection.Changed += (sender, args) =>
				ViewModel.SelectedDeliveryScheduleRestriction = treeViewGraphic.GetSelectedObject<DeliveryScheduleRestriction>(); 
			
			treeViewDeliveryRules.ColumnsConfig = FluentColumnsConfig<SectorWeekDayDeliveryRuleVersion>
				.Create()
				.AddColumn("Начало")
					.AddTextRenderer(x => x.StartDate.HasValue ? x.StartDate.Value.ToShortDateString() : "")
				.AddColumn("Окончание")
					.AddTextRenderer(x => x.EndDate.HasValue ? x.EndDate.Value.ToShortDateString() : "")
				.AddColumn("Статус")
					.AddComboRenderer(x => x.Status)
					.SetDisplayFunc(x => x.GetEnumTitle())
					.FillItems(((SectorsSetStatus[]) Enum.GetValues(typeof(SectorsSetStatus))).ToList())
					.AddSetter((c, n) =>
					{
						if(ViewModel.ObservableSectorWeekDayDeliveryRuleVersions.Count(x => x.Status == SectorsSetStatus.OnActivation) == 1)
						{
							c.Editable = true;
							c.Items = new List<SectorsSetStatus> {SectorsSetStatus.Draft};
							c.UpdateComboList(default);
						}
						if(n.Status == SectorsSetStatus.Active || n.Status == SectorsSetStatus.Closed)
							c.Editable = false;
						else if(n.Status == SectorsSetStatus.Draft)
						{
							c.Editable = true;
							c.Items = new List<SectorsSetStatus> {SectorsSetStatus.Draft, SectorsSetStatus.OnActivation};
							c.UpdateComboList(default);
						}
					})
					.Editing()
				.Finish();
			treeViewDeliveryRules.Binding.AddBinding(ViewModel, s => s.ObservableSectorWeekDayDeliveryRuleVersions, t => t.ItemsDataSource);
			treeViewDeliveryRules.Selection.Changed += (sender, args) =>
				ViewModel.SelectedSectorWeekDayDeliveryRuleVersion = treeViewDeliveryRules.GetSelectedObject<SectorWeekDayDeliveryRuleVersion>();

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
					.FillItems(ViewModel.ObservablePriceRule, "Нет")
					.Editing()
				.Finish();
			treeViewSpecialRules.Binding.AddBinding(ViewModel, s => s.ObservableWeekDayDistrictRuleItems, t => t.ItemsDataSource);
			treeViewSpecialRules.Selection.Changed += (sender, args) =>
				ViewModel.SelectedWeekDayDistrictRuleItem = treeViewSpecialRules.GetSelectedObject<WeekDayDistrictRuleItem>();


			#endregion

			#region Binding of the datepickers

			dateForFilter.Binding.AddBinding(ViewModel, vm => vm.StartDateSector, t => t.StartDateOrNull).InitializeFromSource();
			dateForFilter.Binding.AddBinding(ViewModel, vm => vm.EndDateSector, t => t.EndDateOrNull).InitializeFromSource();
			sectorVersionStartDate.Binding.AddBinding(ViewModel, vm => vm.StartDateSectorVersion, t => t.DateOrNull)
				.InitializeFromSource();
			deliveryRuleStartDate.Binding.AddBinding(ViewModel, vm => vm.StartDateSectorDeliveryRuleVersion, t => t.DateOrNull)
				.InitializeFromSource();
			weekDayScheduleStartDate.Binding.AddBinding(ViewModel, vm => vm.StartDateSectorDaySchedule, t => t.DateOrNull)
				.InitializeFromSource();
			weekDayDeliveryStartDate.Binding.AddBinding(ViewModel, vm => vm.StartDateSectorWeekDayDeliveryRuleVersion, t => t.DateOrNull)
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
			btnAddRulesDelivery.Clicked += (sender, args) => ViewModel.AddSectorDeliveryRuleVersion.Execute();

			btnRemoveRulesDelivery.Binding
				.AddFuncBinding(ViewModel, vm => vm.SelectedSectorDeliveryRuleVersion != null && vm.CanEditSector && vm.SelectedSectorDeliveryRuleVersion.Status != SectorsSetStatus.Active, t => t.Sensitive)
				.InitializeFromSource();
			btnRemoveRulesDelivery.Clicked += (sender, args) => ViewModel.RemoveSectorDeliveryRuleVersion.Execute();

			btnCopyRulesDelivery.Binding
				.AddFuncBinding(ViewModel, vm => vm.SelectedSectorDeliveryRuleVersion != null && vm.CanEditSector, t => t.Sensitive)
				.InitializeFromSource();
			btnCopyRulesDelivery.Clicked += (sender, args) => ViewModel.CopySectorDeliveryRuleVersion.Execute();
			
			btnAddRules.Binding
				.AddFuncBinding(ViewModel, vm => vm.SelectedSectorDeliveryRuleVersion != null && vm.CanEditSector && vm.SelectedSectorDeliveryRuleVersion.Status != SectorsSetStatus.Active, t => t.Sensitive)
				.InitializeFromSource();
			btnAddRules.Clicked += (sender, args) => ViewModel.AddCommonDistrictRule.Execute();

			btnRemoveRules.Binding
				.AddFuncBinding(ViewModel, vm => vm.SelectedCommonSectorsRuleItem != null && vm.CanEditSector && vm.SelectedSectorDeliveryRuleVersion != null && vm.SelectedSectorDeliveryRuleVersion.Status != SectorsSetStatus.Active, t => t.Sensitive)
				.InitializeFromSource();
			btnRemoveRules.Clicked += (sender, args) => ViewModel.RemoveCommonDistrictRule.Execute();
			
			#endregion

			btnOnActive.Clicked += (sender, args) => ViewModel.Activate.Execute();

			btnSummary.Clicked += (sender, args) =>
			{
				ViewModel.SummaryActive.Execute();
				var textView = new yTextView {Buffer = {Text = ViewModel.SummaryText}, WrapMode = WrapMode.WordChar, WidthRequest = 1000};

				var scroll = new ScrolledWindow { textView};
				var vbox = new VBox { scroll };
				var messageWindow = new Window(WindowType.Toplevel) {
					HeightRequest = 400,
					WidthRequest = 1000,
					Resizable = false,
					Title = "Информация", 
					WindowPosition = WindowPosition.Center,
					Modal = true
				};
				messageWindow.Add(vbox);
				messageWindow.ShowAll();
			};

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
				.AddFuncBinding(ViewModel, vm => vm.SelectedSectorWeekDayScheduleVersion != null && vm.CanEditSector && vm.SelectedSectorWeekDayScheduleVersion.Status != SectorsSetStatus.Active, t => t.Sensitive)
				.InitializeFromSource();
			btnRemoveDaySchedule.Clicked += (sender, args) => ViewModel.RemoveSectorWeekDayScheduleVersion.Execute(); 

			btnCopyDaySchedule.Binding
				.AddFuncBinding(ViewModel, vm => vm.SelectedSectorWeekDayScheduleVersion != null && vm.CanEditSector, t => t.Sensitive)
				.InitializeFromSource();
			btnCopyDaySchedule.Clicked += (sender, args) => ViewModel.CopySectorWeekDayScheduleVersion.Execute(); 
			
			btnAddGraphic.Binding.AddFuncBinding(ViewModel,
					vm => vm.SelectedWeekDayName != null && vm.SelectedSectorWeekDayScheduleVersion != null && vm.CanEditSector && vm.SelectedSectorWeekDayScheduleVersion.Status != SectorsSetStatus.Active, t => t.Sensitive)
				.InitializeFromSource();
			btnAddGraphic.Clicked += (sender, args) =>
			{
				var selectSchedules = new OrmReference(typeof(DeliverySchedule), ViewModel.UoW) {
					Mode = OrmReferenceMode.MultiSelect
				};
				selectSchedules.ObjectSelected += (o, eventArgs) => {
					ViewModel.AddDeliveryScheduleRestriction.Execute(eventArgs.Subjects.Cast<DeliverySchedule>());

					if(ViewModel.SelectedDeliveryScheduleRestriction != null) {
						var iter = treeViewGraphic.YTreeModel.IterFromNode(ViewModel.SelectedDeliveryScheduleRestriction);
						var path = treeViewGraphic.YTreeModel.GetPath(iter);
						treeViewGraphic.ScrollToCell(path, treeViewGraphic.Columns.FirstOrDefault(), false, 0, 0);
					}
				};
				Tab.TabParent.AddSlaveTab(this.Tab, selectSchedules);
			};
			
			btnRemoveGraphic.Binding.AddFuncBinding(ViewModel, vm => vm.SelectedSectorWeekDayScheduleVersion != null && vm.CanEditSector && vm.SelectedSectorWeekDayScheduleVersion.Status != SectorsSetStatus.Active, t => t.Sensitive).InitializeFromSource();
			btnRemoveGraphic.Clicked += (sender, args) => ViewModel.RemoveDeliveryScheduleRestriction.Execute();

			btnAddRestriction.Binding.AddFuncBinding(ViewModel,
					vm => vm.CanEditSector && vm.SelectedDeliveryScheduleRestriction != null && vm.SelectedSectorWeekDayScheduleVersion.Status != SectorsSetStatus.Active, w => w.Sensitive)
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
				.AddFuncBinding(ViewModel, vm => vm.CanEditSector && vm.SelectedSectorWeekDayScheduleVersion != null && vm.SelectedSectorWeekDayScheduleVersion.Status != SectorsSetStatus.Active, w => w.Sensitive)
				.InitializeFromSource();
			btnRemoveRestriction.Clicked += (sender, args) => ViewModel.RemoveAcceptBeforeCommand.Execute();

			#endregion

			#region Binding of the button for delivery rule on days

			btnAddDayDeliveryRule.Binding
				.AddFuncBinding(ViewModel, vm => vm.SelectedSectorNodeViewModel != null && vm.CanEditSector, t => t.Sensitive)
				.InitializeFromSource();
			btnAddDayDeliveryRule.Clicked += (sender, args) => ViewModel.AddSectorWeekDayDeliveryRuleVersion.Execute();

			btnRemoveDayDeliveryRule.Binding
				.AddFuncBinding(ViewModel, vm => vm.SelectedSectorWeekDayDeliveryRuleVersion != null && vm.CanEditSector && vm.SelectedSectorWeekDayDeliveryRuleVersion.Status != SectorsSetStatus.Active, t => t.Sensitive)
				.InitializeFromSource();
			btnRemoveDayDeliveryRule.Clicked += (sender, args) => ViewModel.RemoveSectorWeekDayDeliveryRuleVersion.Execute();

			btnCopyDayDeliveryRule.Binding
				.AddFuncBinding(ViewModel, vm => vm.SelectedSectorWeekDayDeliveryRuleVersion != null && vm.CanEditSector, t => t.Sensitive)
				.InitializeFromSource();
			btnCopyDayDeliveryRule.Clicked += (sender, args) => ViewModel.CopySectorWeekDayDeliveryRuleVersion.Execute();
			
			btnAddSpecialRules.Binding.AddFuncBinding(ViewModel, vm => vm.SelectedSectorWeekDayDeliveryRuleVersion != null && vm.SelectedSectorWeekDayDeliveryRuleVersion.Status != SectorsSetStatus.Active && vm.CanEditSector && vm.SelectedWeekDayName != null, t => t.Sensitive).InitializeFromSource();
			btnAddSpecialRules.Clicked += (sender, args) => ViewModel.AddWeekDayDistrictRule.Execute();
			
			btnRemoveSpecial.Binding.AddFuncBinding(ViewModel, vm => vm.SelectedSectorWeekDayDeliveryRuleVersion != null && vm.SelectedSectorWeekDayDeliveryRuleVersion.Status != SectorsSetStatus.Active && vm.CanEditSector, t => t.Sensitive).InitializeFromSource();
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
			
			#endregion

			
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
						case nameof(ViewModel.SelectedSectorVersion):
							if(ViewModel.SelectedSectorVersion != null)
								treeViewMainProperty.SelectObject(ViewModel.SelectedSectorVersion);
							break;
						case nameof(ViewModel.SelectedSectorDeliveryRuleVersion):
							if(ViewModel.SelectedSectorDeliveryRuleVersion != null)
								treeViewRulesDelivery.SelectObject(ViewModel.SelectedSectorDeliveryRuleVersion);
							break;
						case nameof(ViewModel.SelectedCommonSectorsRuleItem):
							if(ViewModel.SelectedCommonSectorsRuleItem != null)
								treeViewRules.SelectObject(ViewModel.SelectedCommonSectorsRuleItem);
							break;
						case nameof(ViewModel.SelectedSectorWeekDayScheduleVersion):
							if(ViewModel.SelectedSectorWeekDayScheduleVersion != null)
								treeViewScheduleVersions.SelectObject(ViewModel.SelectedSectorWeekDayScheduleVersion);
							break;
						case nameof(ViewModel.SelectedDeliveryScheduleRestriction):
							if(ViewModel.SelectedDeliveryScheduleRestriction != null)
								treeViewGraphic.SelectObject(ViewModel.SelectedDeliveryScheduleRestriction);
							break;
						case nameof(ViewModel.SelectedSectorWeekDayDeliveryRuleVersion):
							if(ViewModel.SelectedSectorWeekDayDeliveryRuleVersion != null)
								treeViewDeliveryRules.SelectObject(ViewModel.SelectedSectorWeekDayDeliveryRuleVersion);
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
			
			#region SaveAndCancelButton

			btnSave.Clicked += (sender, args) => ViewModel.Save();
			btnSave.Binding.AddFuncBinding(ViewModel, vm => vm.CanEditSector || vm.CanEdit, w => w.Sensitive).InitializeFromSource();
			
			btnCancel.Clicked += (sender, args) => ViewModel.Close(true, CloseSource.Cancel);

			#endregion
		}
	}
}
