using System.Drawing;
using System.Linq;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using Gdk;
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
using Vodovoz.Domain.WageCalculation;
using Vodovoz.ViewModels.Journals.JournalViewModels.Sale;
using Vodovoz.ViewModels.Logistic;

namespace Vodovoz.Views.Logistic
{
	public partial class DistrictsSetView : TabViewBase<DistrictsSetViewModel>
	{
        private readonly GMapOverlay bordersOverlay = new GMapOverlay("district_borders");
        private readonly GMapOverlay newBordersPreviewOverlay = new GMapOverlay("district_preview_borders");
        private readonly GMapOverlay verticeOverlay = new GMapOverlay("district_vertice");
        private readonly Pen selectedDistrictBorderPen = new Pen(System.Drawing.Color.Red, 2);

        private const string acceptBeforeColumnTag = "Прием до";

        public DistrictsSetView(DistrictsSetViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}	
		
		private void Configure()
		{
			#region TreeViews

			var colorRed = new Gdk.Color(255, 0, 0);
			var colorWhite = new Gdk.Color(255, 255, 255);

			ytreeDistricts.ColumnsConfig = ColumnsConfigFactory.Create<District>()
				.AddColumn("Код")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.Id.ToString())
				.AddColumn("Название")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.DistrictName)
					.Editable(ViewModel.CanEditDistrict)
				.AddColumn("Тарифная зона")
					.HeaderAlignment(0.5f)
				.AddComboRenderer(x => x.TariffZone)
					.SetDisplayFunc(x => x.Name)
					.FillItems(ViewModel.UoW.GetAll<TariffZone>().ToList(), "Нет")
					.Editing(ViewModel.CanEditDistrict)
				.AddColumn("Мин. бутылей")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x =>
						x.ObservableCommonDistrictRuleItems.Any()
							? x.ObservableCommonDistrictRuleItems.Min(c => c.DeliveryPriceRule.Water19LCount).ToString()
							: "-"
					)
					.XAlign(0.5f)
				.AddColumn("")
				.Finish();
			
			ytreeDistricts.Binding.AddBinding(ViewModel.Entity, e => e.ObservableDistricts, w => w.ItemsDataSource).InitializeFromSource();
			ytreeDistricts.Selection.Changed += (sender, args) => {
				if(ViewModel.IsCreatingNewBorder) {
					ViewModel.CancelNewBorderCommand.Execute();
					toggleNewBorderPreview.Active = false;
				}
				ViewModel.SelectedDistrict = ytreeDistricts.GetSelectedObject() as District;
			};

			ytreeScheduleRestrictions.ColumnsConfig = ColumnsConfigFactory.Create<DeliveryScheduleRestriction>()
				.AddColumn("График")
					.MinWidth(100)
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.DeliverySchedule.Name)
				.AddColumn(acceptBeforeColumnTag)
					.SetTag(acceptBeforeColumnTag)
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.AcceptBeforeTitle)
					.AddSetter((c, r) =>
						c.BackgroundGdk = r.WeekDay == WeekDayName.Today && r.AcceptBefore == null ? colorRed : colorWhite)
				.Finish();
			ytreeScheduleRestrictions.Binding.AddBinding(ViewModel, vm => vm.ScheduleRestrictions, w => w.ItemsDataSource);
			ytreeScheduleRestrictions.Selection.Changed += (sender, args) =>
				ViewModel.SelectedScheduleRestriction = ytreeScheduleRestrictions.GetSelectedObject() as DeliveryScheduleRestriction;

			ytreeCommonRulePrices.ColumnsConfig = ColumnsConfigFactory.Create<CommonDistrictRuleItem>()
				.AddColumn("Цена")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(p => p.Price)
					.Digits(2)
					.WidthChars(10)
					.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0))
					.Editing(ViewModel.CanEditDeliveryRules)
					.AddSetter((c, r) => c.BackgroundGdk = r.Price <= 0 ? colorRed : colorWhite)
					.AddTextRenderer(node => CurrencyWorks.CurrencyShortName, false)
				.AddColumn("Правило")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(p => p.DeliveryPriceRule.Title)
					.WrapMode(Pango.WrapMode.WordChar)
					.WrapWidth(500)
				.Finish();
			ytreeCommonRulePrices.Binding.AddBinding(ViewModel, vm => vm.CommonDistrictRuleItems, w => w.ItemsDataSource);
			ytreeCommonRulePrices.Selection.Changed += (sender, args) =>
				ViewModel.SelectedCommonDistrictRuleItem = ytreeCommonRulePrices.GetSelectedObject() as CommonDistrictRuleItem;
			
			ytreeWeekDayRulePrices.ColumnsConfig = ColumnsConfigFactory.Create<WeekDayDistrictRuleItem>()
				.AddColumn("Цена")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(p => p.Price)
					.Digits(2)
					.WidthChars(10)
					.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0))
					.Editing(ViewModel.CanEditDeliveryRules)
					.AddSetter((c, r) => c.BackgroundGdk = r.Price <= 0 ? colorRed : colorWhite)
					.AddTextRenderer(node => CurrencyWorks.CurrencyShortName, false)
				.AddColumn("Правило")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(p => p.DeliveryPriceRule.ToString())
					.WrapMode(Pango.WrapMode.WordChar)
					.WrapWidth(390)
				.Finish();
			ytreeWeekDayRulePrices.Binding.AddBinding(ViewModel, vm => vm.WeekDayDistrictRuleItems, w => w.ItemsDataSource);
			ytreeWeekDayRulePrices.Selection.Changed += (sender, args) =>
				ViewModel.SelectedWeekDayDistrictRuleItem = ytreeWeekDayRulePrices.GetSelectedObject() as WeekDayDistrictRuleItem;

			#endregion

			btnSave.Clicked += (sender, args) => ViewModel.Save();
			btnSave.Binding.AddFuncBinding(ViewModel, vm => vm.CanSave, w => w.Sensitive).InitializeFromSource();
			
			btnCancel.Clicked += (sender, args) => ViewModel.Close(true, CloseSource.Cancel);
			
			ylabelStatusString.Text = ViewModel.Entity.Status.GetEnumTitle();
			
			entryName.Binding.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text).InitializeFromSource();
			entryName.Binding.AddFuncBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			btnAddDistrict.Binding.AddFuncBinding(ViewModel, vm => vm.CanCreateDistrict, w => w.Sensitive).InitializeFromSource();
			btnAddDistrict.Clicked += (sender, args) => {
				ViewModel.AddDistrictCommand.Execute();
				ScrollToSelectedDistrict();
			};

			btnRemoveDistrict.Clicked += (sender, args) => {
				ViewModel.RemoveDistrictCommand.Execute();
				if(ViewModel.SelectedDistrict == null)
					RefreshBorders();
				else 
					ScrollToSelectedDistrict();
			};
			btnRemoveDistrict.Binding.AddFuncBinding(ViewModel, vm => vm.SelectedDistrict != null && vm.CanDeleteDistrict, w => w.Sensitive).InitializeFromSource();
			
			ytextComment.Binding.AddBinding(ViewModel.Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();
			ytextComment.Binding.AddFuncBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
			
			btnAddCommonRule.Binding.AddFuncBinding(ViewModel, vm => vm.SelectedDistrict != null && vm.CanEditDeliveryRules, w => w.Sensitive).InitializeFromSource();
			btnAddCommonRule.Clicked += (sender, args) => ViewModel.AddCommonDeliveryPriceRuleCommand.Execute();

			btnRemoveCommonRule.Binding.AddFuncBinding(ViewModel, vm => vm.CanEditDeliveryRules && vm.SelectedDistrict != null && vm.SelectedCommonDistrictRuleItem != null, w => w.Sensitive).InitializeFromSource();
			btnRemoveCommonRule.Clicked += (sender, args) => ViewModel.RemoveCommonDistrictRuleItemCommand.Execute();

			btnToday.TooltipText = "День в день.\nГрафик доставки при создании заказа сегодня и на сегодняшнюю дату доставки.";
			btnToday.Clicked += (sender, args) => ViewModel.SelectedWeekDayName = WeekDayName.Today;
			btnMonday.Clicked += (sender, args) => ViewModel.SelectedWeekDayName = WeekDayName.Monday;
			btnTuesday.Clicked += (sender, args) => ViewModel.SelectedWeekDayName = WeekDayName.Tuesday;
			btnWednesday.Clicked += (sender, args) => ViewModel.SelectedWeekDayName = WeekDayName.Wednesday;
			btnThursday.Clicked += (sender, args) => ViewModel.SelectedWeekDayName = WeekDayName.Thursday;
			btnFriday.Clicked += (sender, args) => ViewModel.SelectedWeekDayName = WeekDayName.Friday;
			btnSaturday.Clicked += (sender, args) => ViewModel.SelectedWeekDayName = WeekDayName.Saturday;
			btnSunday.Clicked += (sender, args) => ViewModel.SelectedWeekDayName = WeekDayName.Sunday;

			btnAddSchedule.Binding.AddFuncBinding(ViewModel, vm => vm.CanEditDeliveryScheduleRestriction && vm.SelectedDistrict != null && vm.SelectedWeekDayName.HasValue, w => w.Sensitive).InitializeFromSource();
			btnAddSchedule.Clicked += (sender, args) => ViewModel.AddScheduleRestrictionCommand.Execute();
			ViewModel.AddScheduleRestrictionCommand.CanExecuteChanged += (sender, args) => btnAddSchedule.Sensitive = ViewModel.AddScheduleRestrictionCommand.CanExecute();

			btnRemoveSchedule.Binding.AddFuncBinding(ViewModel, vm => vm.CanEditDeliveryScheduleRestriction && vm.SelectedDistrict != null && vm.SelectedScheduleRestriction != null, w => w.Sensitive).InitializeFromSource();
			btnRemoveSchedule.Clicked += (sender, args) => ViewModel.RemoveScheduleRestrictionCommand.Execute();

			btnAddAcceptBefore.Binding.AddFuncBinding(ViewModel, vm => vm.CanEditDeliveryScheduleRestriction && vm.SelectedDistrict != null && vm.SelectedScheduleRestriction != null, w => w.Sensitive).InitializeFromSource();
			btnAddAcceptBefore.Clicked += (sender, args) => {
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
				acceptBeforeTimeViewModel.SetActionsVisible(deleteActionEnabled: false, editActionEnabled:false);
				acceptBeforeTimeViewModel.OnEntitySelectedResult += (o, eventArgs) => {
					var node = eventArgs.SelectedNodes.FirstOrDefault();
					if(node != null) {
						ViewModel.AddAcceptBeforeCommand.Execute(ViewModel.UoW.GetById<AcceptBefore>(node.Id));
					}
				};
				Tab.TabParent.AddSlaveTab(Tab, acceptBeforeTimeViewModel);
			};

			btnRemoveAcceptBefore.Binding.AddFuncBinding(ViewModel,
					vm => vm.CanEditDeliveryScheduleRestriction && vm.SelectedDistrict != null && vm.SelectedScheduleRestriction != null && vm.SelectedScheduleRestriction.AcceptBefore != null,
					w => w.Sensitive)
				.InitializeFromSource();
			btnRemoveAcceptBefore.Clicked += (sender, args) => ViewModel.RemoveAcceptBeforeCommand.Execute();
			
			btnAddWeekDayRule.Binding.AddFuncBinding(ViewModel, vm => vm.CanEditDeliveryRules && vm.SelectedDistrict != null && vm.SelectedWeekDayName.HasValue, w => w.Sensitive).InitializeFromSource();
			btnAddWeekDayRule.Clicked += (sender, args) => ViewModel.AddWeekDayDeliveryPriceRuleCommand.Execute();

			btnRemoveWeekDayRule.Binding.AddFuncBinding(ViewModel, vm => vm.CanEditDeliveryRules && vm.SelectedDistrict != null && vm.SelectedWeekDayDistrictRuleItem != null, w => w.Sensitive).InitializeFromSource();
			btnRemoveWeekDayRule.Clicked += (sender, args) => ViewModel.RemoveWeekDayDistrictRuleItemCommand.Execute();

			cmbGeoGroup.ItemsList = ViewModel.UoW.GetAll<GeoGroup>().ToList();
			cmbGeoGroup.Binding.AddBinding(ViewModel, vm => vm.SelectedGeoGroup, w => w.SelectedItem).InitializeFromSource();
			cmbGeoGroup.Binding.AddFuncBinding(ViewModel, vm => vm.CanEditDistrict && vm.SelectedDistrict != null, w => w.Sensitive).InitializeFromSource();

			cmbWageDistrict.ItemsList = ViewModel.UoW.Session.QueryOver<WageDistrict>().Where(d => !d.IsArchive).List();
			cmbWageDistrict.Binding.AddBinding(ViewModel, vm => vm.SelectedWageDistrict, w => w.SelectedItem).InitializeFromSource();
			cmbWageDistrict.Binding.AddFuncBinding(ViewModel, vm => vm.CanEditDistrict && vm.SelectedDistrict != null && vm.CanChangeDistrictWageTypePermissionResult, w => w.Sensitive).InitializeFromSource();
			cmbWageDistrict.SetRenderTextFunc<WageDistrict>(x => x.Name);

			#region GMap

			btnAddBorder.Binding.AddFuncBinding(ViewModel, vm => !vm.IsCreatingNewBorder, w => w.Visible).InitializeFromSource();
			btnAddBorder.Binding.AddFuncBinding(ViewModel, vm => vm.CanEditDistrict && !vm.IsCreatingNewBorder && vm.SelectedDistrict != null && vm.SelectedDistrict.DistrictBorder == null, w => w.Sensitive).InitializeFromSource();
			btnAddBorder.Clicked += (sender, args) => ViewModel.CreateBorderCommand.Execute();

			btnRemoveBorder.Binding.AddFuncBinding(ViewModel, vm => !vm.IsCreatingNewBorder, w => w.Visible).InitializeFromSource();
			btnRemoveBorder.Binding.AddFuncBinding(ViewModel, vm => vm.CanEditDistrict && !vm.IsCreatingNewBorder && vm.SelectedDistrict != null && vm.SelectedDistrict.DistrictBorder != null, w => w.Sensitive).InitializeFromSource();
			btnRemoveBorder.Clicked += (sender, args) => {
				if(MessageDialogHelper.RunQuestionDialog($"Удалить границу района {ViewModel.SelectedDistrict.DistrictName}?")) {
					ViewModel.RemoveBorderCommand.Execute();
					RefreshBorders();
				}
			};

			btnConfirmNewBorder.Binding.AddFuncBinding(ViewModel, vm => vm.IsCreatingNewBorder, w => w.Visible).InitializeFromSource();
			btnConfirmNewBorder.Binding.AddFuncBinding(ViewModel, vm => vm.SelectedDistrict != null && vm.IsCreatingNewBorder, w => w.Sensitive).InitializeFromSource();
			btnConfirmNewBorder.Clicked += (sender, args) => {
				if(MessageDialogHelper.RunQuestionDialog("Завершить создание границы района?")) {
					if(ViewModel.NewBorderVertices.Count < 3) {
						MessageDialogHelper.RunInfoDialog("Нельзя создать границу района меньше чем за 3 точки");
						return;
					}
					toggleNewBorderPreview.Active = false;
					ViewModel.ConfirmNewBorderCommand.Execute();
					RefreshBorders();
				}
			};

			btnCancelNewBorder.Binding.AddFuncBinding(ViewModel, vm => vm.IsCreatingNewBorder, w => w.Visible).InitializeFromSource();
			btnCancelNewBorder.Binding.AddFuncBinding(ViewModel, vm => vm.SelectedDistrict != null && vm.IsCreatingNewBorder, w => w.Sensitive).InitializeFromSource();
			btnCancelNewBorder.Clicked += (sender, args) => {
				if(MessageDialogHelper.RunQuestionDialog("Отменить создание границы района?")) {
					ViewModel.CancelNewBorderCommand.Execute();
					toggleNewBorderPreview.Active = false;
				}
			};

			toggleNewBorderPreview.Binding.AddFuncBinding(ViewModel, vm => vm.IsCreatingNewBorder, w => w.Visible).InitializeFromSource();
			toggleNewBorderPreview.Binding.AddFuncBinding(ViewModel, vm => vm.SelectedDistrict != null && vm.IsCreatingNewBorder, w => w.Sensitive).InitializeFromSource();
			toggleNewBorderPreview.Toggled += (sender, args) => {
				if(toggleNewBorderPreview.Active && ViewModel.NewBorderVertices.Any()) {
					var previewBorder = new GMapPolygon(ViewModel.NewBorderVertices.ToList(), "Предпросмотр новых границ");
					newBordersPreviewOverlay.Polygons.Add(previewBorder);
				}
				else {
					newBordersPreviewOverlay.Clear();
				}
			};

			cmbMapType.ItemsEnum = typeof(MapProviders);
			cmbMapType.TooltipText = "Если карта отображается некорректно или не отображается вовсе - смените тип карты";
			cmbMapType.EnumItemSelected += (sender, args) =>
				gmapWidget.MapProvider = MapProvidersHelper.GetPovider((MapProviders)args.SelectedItem);
			cmbMapType.SelectedItem = MapProviders.GoogleMap;

			gmapWidget.Position = new PointLatLng(59.93900, 30.31646);
			gmapWidget.HeightRequest = 150;
			gmapWidget.HasFrame = true;
			gmapWidget.Overlays.Add(bordersOverlay);
			gmapWidget.Overlays.Add(newBordersPreviewOverlay);
			gmapWidget.Overlays.Add(verticeOverlay);
			RefreshBorders();

			gmapWidget.ButtonPressEvent += (o, args) => {
				if(args.Event.Button == 1 && ViewModel.IsCreatingNewBorder) {
					ViewModel.AddNewVertexCommand.Execute(gmapWidget.FromLocalToLatLng((int) args.Event.X, (int) args.Event.Y));
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
				foreach (District district in ViewModel.Entity.ObservableDistricts) {
					if(district.DistrictBorder != null) {
						bordersOverlay.Polygons.Add(new GMapPolygon(
							district.DistrictBorder.Coordinates.Select(p => new PointLatLng(p.X, p.Y)).ToList(), district.DistrictName)
						);
					}
				}
			}

			#endregion

			ViewModel.PropertyChanged += (sender, args) => {
				Application.Invoke((o, eventArgs) => {
					switch (args.PropertyName) {
						case nameof(ViewModel.SelectedWeekDayName):
							var column = ytreeScheduleRestrictions.ColumnsConfig.GetColumnsByTag(acceptBeforeColumnTag).First();
							column.Title = ViewModel.SelectedWeekDayName == WeekDayName.Today
								? acceptBeforeColumnTag
								: $"{acceptBeforeColumnTag} прошлого дня";
							break;
						case nameof(ViewModel.SelectedDistrict):
							if(ViewModel.SelectedDistrict != null)
								ytreeDistricts.SelectObject(ViewModel.SelectedDistrict);
							break;
						case nameof(ViewModel.SelectedScheduleRestriction):
							if(ViewModel.SelectedScheduleRestriction != null)
								ytreeScheduleRestrictions.SelectObject(ViewModel.SelectedScheduleRestriction);
							break;
						case nameof(ViewModel.SelectedCommonDistrictRuleItem):
							if(ViewModel.SelectedCommonDistrictRuleItem != null)
								ytreeCommonRulePrices.SelectObject(ViewModel.SelectedCommonDistrictRuleItem);
							break;
						case nameof(ViewModel.SelectedWeekDayDistrictRuleItem):
							if(ViewModel.SelectedWeekDayDistrictRuleItem != null)
								ytreeWeekDayRulePrices.SelectObject(ViewModel.SelectedWeekDayDistrictRuleItem);
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
								if(toggleNewBorderPreview.Active) {
									toggleNewBorderPreview.Active = false;
									toggleNewBorderPreview.Active = true;
								}
							}
							break;
						case nameof(ViewModel.ScheduleRestrictions):
							ScrollToSelectedScheduleRestriction();
							break;
					}
				});
			};
		}

		private void ScrollToSelectedScheduleRestriction()
		{
			if(ViewModel.SelectedScheduleRestriction != null)
			{
				var iter = ytreeScheduleRestrictions.YTreeModel.IterFromNode(ViewModel.SelectedScheduleRestriction);
				var path = ytreeScheduleRestrictions.YTreeModel.GetPath(iter);
				ytreeScheduleRestrictions.ScrollToCell(path, ytreeScheduleRestrictions.Columns.FirstOrDefault(), false, 0, 0);
			}
		}

		private void ScrollToSelectedDistrict()
		{
			if(ViewModel.SelectedDistrict != null) {
				var iter = ytreeDistricts.YTreeModel.IterFromNode(ViewModel.SelectedDistrict);
				var path = ytreeDistricts.YTreeModel.GetPath(iter);
				ytreeDistricts.ScrollToCell(path, ytreeDistricts.Columns.FirstOrDefault(), false, 0, 0);
			}
		}
		
	}
}
