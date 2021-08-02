using System;
using System.Drawing;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using GMap.NET;
using GMap.NET.GtkSharp;
using Gtk;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Utilities;
using QS.Views.GtkUI;
using Vodovoz.Additions.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.Sectors;
using Vodovoz.Domain.WageCalculation;
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

			treeViewMainDistricts.ColumnsConfig = FluentColumnsConfig<Sector>
				.Create()
				.AddColumn("Код").AddTextRenderer(x=>x.Id.ToString())
				.AddColumn("Название").AddTextRenderer(x=>x.SectorName).Editable()
				.AddColumn("Дата создания").AddTextRenderer(x=>x.DateCreated.ToShortDateString())
				.Finish();
			treeViewMainDistricts.Binding.AddBinding(ViewModel, s => s.Sectors, t => t.ItemsDataSource).InitializeFromSource();
			treeViewMainDistricts.Selection.Changed += (sender, args) =>
				ViewModel.SelectedSector = treeViewMainDistricts.GetSelectedObject<Sector>();

			treeViewMainProperty.ColumnsConfig = FluentColumnsConfig<SectorVersion>
				.Create()
				.AddColumn("Начало")
					.AddTextRenderer(x => x.StartDate.ToShortDateString())
					.Editable()
				.AddColumn("Окончание")
					.AddTextRenderer(x => x.EndDate.ToShortDateString())
					.Editable()
				.AddColumn("Статус")
					.AddComboRenderer(x => x.Status)
					.SetDisplayFunc(x => x.GetEnumTitle())
					.FillItems(((SectorsSetStatus[])Enum.GetValues(typeof(SectorsSetStatus))).ToList())
					.Editing()
				.AddColumn("Название")
					.AddTextRenderer(x => x.SectorName)
					.Editable()
				.AddColumn("Архивный?")
					.AddToggleRenderer(x=>x.IsArchive)
				.AddColumn("Тарифная зона")
					.AddComboRenderer(x => x.TariffZone)
					.SetDisplayFunc(x => x.Name)
					.FillItems(ViewModel.UoW.GetAll<TariffZone>().ToList(), "Нет")
					.Editing()
				//.AddSetter((c, n) => c.Editable = ViewModel.CanEdit)
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
					.Editing()
				.Finish();
			treeViewMainProperty.Binding.AddBinding(ViewModel, s => s.SectorVersions, t => t.ItemsDataSource).InitializeFromSource();
			treeViewMainProperty.Selection.Changed += (sender, args) =>
				ViewModel.SelectedSectorVersion = treeViewMainProperty.GetSelectedObject<SectorVersion>();

			treeViewRulesDelivery.ColumnsConfig = FluentColumnsConfig<SectorDeliveryRuleVersion>
				.Create()
				.AddColumn("Начало").AddTextRenderer(x => x.StartDate.ToShortDateString())
				.AddColumn("Окончание").AddTextRenderer(x => x.EndDate.ToShortDateString())
				.AddColumn("Статус")
					.AddComboRenderer(x => x.Status)
					.SetDisplayFunc(x => x.GetEnumTitle())
					.FillItems(((SectorsSetStatus[]) Enum.GetValues(typeof(SectorsSetStatus))).ToList())
					.Editing()
				.Finish();
			treeViewRulesDelivery.Binding.AddBinding(ViewModel, s => s.SectorDeliveryRuleVersions, t => t.ItemsDataSource).InitializeFromSource();
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
			treeViewRules.Binding.AddBinding(ViewModel, s => s.SelectedDeliveryRuleVersion.ObservableCommonDistrictRuleItems, t => t.ItemsDataSource).InitializeFromSource();
			
			treeViewGraphicDelivery.ColumnsConfig = FluentColumnsConfig<SectorWeekDayRulesVersion>
				.Create()
				.AddColumn("Начало").AddTextRenderer(x => x.StartDate.ToShortDateString())
				.AddColumn("Окончание").AddTextRenderer(x => x.EndDate.ToShortDateString())
				.AddColumn("Статус")
					.AddComboRenderer(x => x.Status)
					.SetDisplayFunc(x => x.GetEnumTitle())
					.FillItems(((SectorsSetStatus[]) Enum.GetValues(typeof(SectorsSetStatus))).ToList())
					.Editing()
				.Finish();
			treeViewGraphicDelivery.Binding.AddBinding(ViewModel, s => s.SectorWeekDeliveryRuleVersions, t => t.ItemsDataSource).InitializeFromSource();

			treeViewGraphic.ColumnsConfig = FluentColumnsConfig<SectorWeekDaySchedule>
				.Create()
				.AddColumn("График")
					.AddTextRenderer(x=>x.DeliverySchedule.Name)
				.AddColumn("Прием до")
					.SetTag(acceptBeforeColumnTag)
					.AddTextRenderer(x => x.DeliveryScheduleRestriction.AcceptBeforeTitle)
					.AddSetter((c, r) => c.BackgroundGdk = r.DeliveryScheduleRestriction.AcceptBefore == null ? colorRed : colorWhite)
				.Finish();
			treeViewGraphic.Binding.AddBinding(ViewModel, s => s.SelectedWeekDayRulesVersion.SectorSchedules, t => t.ItemsDataSource).InitializeFromSource();

			treeViewSpecialRules.ColumnsConfig = FluentColumnsConfig<SectorWeekDayDeliveryRule>
				.Create()
				.AddColumn("Цена")
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
			treeViewSpecialRules.Binding.AddBinding(ViewModel, s => s.SelectedWeekDayRulesVersion.SectorDeliveryRules, t => t.ItemsDataSource).InitializeFromSource();

			#endregion

			#region Binding of the main buttons

			btnAddDistrict.Binding.AddFuncBinding(ViewModel, vm => vm.CanCreateDistrict, t => t.Sensitive).InitializeFromSource();
			btnAddDistrict.Clicked += (sender, args) =>
			{
				ViewModel.AddSector.Execute();
			};
			
			btnRemoveDistrict.Binding.AddFuncBinding(ViewModel, vm => vm.CanDeleteDistrict, t => t.Sensitive).InitializeFromSource();
			btnRemoveDistrict.Clicked += (sender, args) =>
			{
				ViewModel.RemoveSector.Execute();
			};

			btnAddMainProperty.Binding.AddFuncBinding(ViewModel, vm => vm.SelectedSector != null && vm.CanEditSector, t => t.Sensitive).InitializeFromSource();
			btnAddMainProperty.Clicked += (sender, args) =>
			{
				ViewModel.AddSectorVersion.Execute();
			};
			
			btnRemoveMainProperty.Binding.AddFuncBinding(ViewModel, vm => vm.SelectedSectorVersion != null && vm.CanEditSector, t => t.Sensitive).InitializeFromSource();
			btnRemoveMainProperty.Clicked += (sender, args) =>
			{
				ViewModel.RemoveSectorVersion.Execute();
			};
			
			btnAddRulesDelivery.Binding.AddFuncBinding(ViewModel, vm => vm.SelectedSector != null && vm.CanEditSector, t => t.Sensitive).InitializeFromSource();
			btnAddRulesDelivery.Clicked += (sender, args) =>
			{
				ViewModel.AddRulesDelivery.Execute();
			};
			
			btnRemoveRulesDelivery.Binding.AddFuncBinding(ViewModel, vm => vm.SelectedDeliveryRuleVersion != null && vm.CanEditSector, t => t.Sensitive).InitializeFromSource();
			btnRemoveRulesDelivery.Clicked += (sender, args) =>
			{
				ViewModel.RemoveRulesDelivery.Execute();
			};
			
			btnCopyRulesDelivery.Binding.AddFuncBinding(ViewModel, vm => vm.SelectedDeliveryRuleVersion != null && vm.CanEditSector, t => t.Sensitive).InitializeFromSource();
			btnCopyRulesDelivery.Clicked += (sender, args) =>
			{
				ViewModel.CopyRulesDelivery.Execute();
			};
			
			btnAddGraphicDelivery.Binding.AddFuncBinding(ViewModel, vm => vm.SelectedSector != null && vm.CanEditSector, t => t.Sensitive).InitializeFromSource();
			btnAddGraphicDelivery.Clicked += (sender, args) =>
			{
				ViewModel.AddWeekRuleDelivery.Execute();
			};
			
			btnRemoveGraphicDelivery.Binding.AddFuncBinding(ViewModel, vm => vm.SelectedWeekDayRulesVersion != null && vm.CanEditSector, t => t.Sensitive).InitializeFromSource();
			btnRemoveGraphicDelivery.Clicked += (sender, args) =>
			{
				ViewModel.RemoveWeekRuleDelivery.Execute();
			};
			
			btnCopyGraphicDelivery.Binding.AddFuncBinding(ViewModel, vm => vm.SelectedWeekDayRulesVersion != null && vm.CanEditSector, t => t.Sensitive).InitializeFromSource();
			btnCopyGraphicDelivery.Clicked += (sender, args) =>
			{
				ViewModel.CopyWeekRuleDelivery.Execute();
			};

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
					previewMapBtn.Sensitive = false;
					ViewModel.ConfirmNewBorderCommand.Execute();
					RefreshBorders();
				}
			};

			CancelBorderBtn.Binding.AddFuncBinding(ViewModel, vm => vm.IsCreatingNewBorder, w => w.Visible).InitializeFromSource();
			CancelBorderBtn.Binding.AddFuncBinding(ViewModel, vm => vm.SelectedSectorVersion != null && vm.IsCreatingNewBorder, w => w.Sensitive).InitializeFromSource();
			CancelBorderBtn.Clicked += (sender, args) => {
				if(MessageDialogHelper.RunQuestionDialog("Отменить создание границы района?")) {
					ViewModel.CancelNewBorderCommand.Execute();
					previewMapBtn.Sensitive = false;
				}
			};

			previewMapBtn.Binding.AddFuncBinding(ViewModel, vm => vm.IsCreatingNewBorder, w => w.Visible).InitializeFromSource();
			previewMapBtn.Binding.AddFuncBinding(ViewModel, vm => vm.SelectedSectorVersion != null && vm.IsCreatingNewBorder, w => w.Sensitive).InitializeFromSource();
			previewMapBtn.Clicked += (sender, args) => {
				if(previewMapBtn.Sensitive && ViewModel.NewBorderVertices.Any()) {
					var previewBorder = new GMapPolygon(ViewModel.NewBorderVertices.ToList(), "Предпросмотр новых границ");
					newBordersPreviewOverlay.Polygons.Add(previewBorder);
				}
				else {
					newBordersPreviewOverlay.Clear();
				}
			};

			enumProviderMap.ItemsEnum = typeof(MapProviders);
			enumProviderMap.EnumItemSelected += (sender, args) => gmapwidget.MapProvider = MapProvidersHelper.GetPovider((MapProviders) args.SelectedItem);
			enumProviderMap.SelectedItem = MapProviders.YandexMap;

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
				var sectorVersion = ViewModel.SelectedSectorVersion;
				if(sectorVersion.Polygon != null)
				{
					bordersOverlay.Polygons.Add(new GMapPolygon(
						sectorVersion.Polygon.Coordinates.Select(p => new PointLatLng(p.X, p.Y)).ToList(), sectorVersion.SectorName)
					);
				}
			}
			#endregion
		}
	}
}
