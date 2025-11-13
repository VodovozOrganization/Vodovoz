using Gamma.GtkWidgets;
using Gamma.Utilities;
using GMap.NET;
using GMap.NET.GtkSharp;
using GMap.NET.GtkSharp.Markers;
using Gtk;
using QS.Dialog;
using QS.Journal.GtkUI;
using QS.Services;
using QS.Utilities;
using QS.Views.GtkUI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Vodovoz.Additions.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.ViewModels.Orders;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Service;
using Action = System.Action;
using GdkColor = Gdk.Color;

namespace Vodovoz.Views.Orders
{
	public partial class ServiceDistrictsSetView : TabViewBase<ServiceDistrictsSetViewModel>
	{
		private const string _acceptBeforeColumnTag = "Прием до";
		private readonly PointLatLng _defaultWidgetMapPosition = new PointLatLng(59.93900, 30.31646);
		private readonly MapProviders _defaultMapProvicer = MapProviders.GoogleMap;

		private readonly GMapOverlay _bordersOverlay = new GMapOverlay("district_borders");
		private readonly GMapOverlay _newBordersPreviewOverlay = new GMapOverlay("district_preview_borders");
		private readonly GMapOverlay _verticeOverlay = new GMapOverlay("district_vertice");

		private readonly GdkColor _primaryBaseColor = GdkColors.PrimaryBase;
		private readonly GdkColor _dangerBaseColor = GdkColors.DangerBase;

		private readonly Pen _selectedDistrictBorderPen = new Pen(Color.Red, 2);

		private Menu _popupDistrictScheduleMenu;
		private List<Action> _popupDistiictScheduleStateActions;
		private MenuItem _copyDistrictScheduleMenuEntry;
		private MenuItem _pasteScheduleToDistrictMenuEntry;
		private IInteractiveService _interactiveService;

		public ServiceDistrictsSetView(
			ServiceDistrictsSetViewModel viewModel,
			ICommonServices commonServices)
			: base(viewModel)
		{
			if(commonServices is null)
			{
				throw new ArgumentNullException(nameof(commonServices));
			}

			_interactiveService = commonServices.InteractiveService;

			Build();
			Configure();
		}

		private void Configure()
		{
			AddCopyPasteDistrictPopupActions();

			#region TreeViews

			PrepareDistrictsTreeView();
			PrepareServiceDeliveryScheduleRestrictionsTreeView();
			PrepareCommonDistrictRuleItemsTreeView();
			// Пока не используем
			//PrepareWeekDayServiceDistrictPriceRulesTreeView();

			#endregion

			btnSave.BindCommand(ViewModel.SaveCommand);

			btnCancel.BindCommand(ViewModel.CancelCommand);

			ylabelStatusString.Text = ViewModel.Entity.Status.GetEnumTitle();

			entryName.Binding
				.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text)
				.InitializeFromSource();

			entryName.Binding
				.AddFuncBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			btnAddDistrict.BindCommand(ViewModel.AddDistrictCommand);

			btnRemoveDistrict.BindCommand(ViewModel.RemoveDistrictCommand);

			ytextComment.Binding
				.AddBinding(ViewModel.Entity, e => e.Comment, w => w.Buffer.Text)
				.InitializeFromSource();

			ytextComment.Binding
				.AddFuncBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			#region Days schedule restrictinos

			btnToday.TooltipText = "День в день.\nГрафик доставки при создании заказа сегодня и на сегодняшнюю дату доставки.";

			btnToday.Binding.AddBinding(ViewModel, vm => vm.IsToDaySelected, w => w.Active).InitializeFromSource();
			btnMonday.Binding.AddBinding(ViewModel, vm => vm.IsMondaySelected, w => w.Active).InitializeFromSource();
			btnTuesday.Binding.AddBinding(ViewModel, vm => vm.IsTuesdaySelected, w => w.Active).InitializeFromSource();
			btnWednesday.Binding.AddBinding(ViewModel, vm => vm.IsToWednesdaySelected, w => w.Active).InitializeFromSource();
			btnThursday.Binding.AddBinding(ViewModel, vm => vm.IsThursdaySelected, w => w.Active).InitializeFromSource();
			btnFriday.Binding.AddBinding(ViewModel, vm => vm.IsFridaySelected, w => w.Active).InitializeFromSource();
			btnSaturday.Binding.AddBinding(ViewModel, vm => vm.IsSaturdaySelected, w => w.Active).InitializeFromSource();
			btnSunday.Binding.AddBinding(ViewModel, vm => vm.IsSundaySelected, w => w.Active).InitializeFromSource();			

			btnAddSchedule.BindCommand(ViewModel.AddScheduleRestrictionCommand);

			btnRemoveSchedule.BindCommand(ViewModel.RemoveScheduleRestrictionCommand);

			btnAddAcceptBefore.BindCommand(ViewModel.AddAcceptBeforeCommand);

			btnRemoveAcceptBefore.BindCommand(ViewModel.RemoveAcceptBeforeCommand);

			#endregion Days schedule restrictinos			

			cmbGeoGroup.ItemsList = ViewModel.UoW.GetAll<GeoGroup>().ToList();
			cmbGeoGroup.Binding
				.AddBinding(ViewModel, vm => vm.SelectedGeoGroup, w => w.SelectedItem)
				.InitializeFromSource();

			cmbGeoGroup.Binding
				.AddFuncBinding(
					ViewModel,
					vm => vm.CanEditServiceDistrict && vm.SelectedServiceDistrict != null,
					w => w.Sensitive)
				.InitializeFromSource();

			#region GMap

			btnAddBorder.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsCreatingNewBorder, w => w.Visible)
				.InitializeFromSource();

			btnAddBorder.BindCommand(ViewModel.CreateBorderCommand);

			btnRemoveBorder.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsCreatingNewBorder, w => w.Visible)
				.InitializeFromSource();

			btnRemoveBorder.BindCommand(ViewModel.RemoveBorderCommand);

			btnConfirmNewBorder.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsCreatingNewBorder, w => w.Visible)
				.InitializeFromSource();

			btnConfirmNewBorder.BindCommand(ViewModel.ConfirmNewBorderCommand);

			btnCancelNewBorder.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsCreatingNewBorder, w => w.Visible)
				.InitializeFromSource();

			btnCancelNewBorder.BindCommand(ViewModel.CancelNewBorderCommand);

			toggleNewBorderPreview.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsCreatingNewBorder, w => w.Visible)
				.InitializeFromSource();

			toggleNewBorderPreview.Binding
				.AddFuncBinding(ViewModel, vm => vm.SelectedServiceDistrict != null && vm.IsCreatingNewBorder, w => w.Sensitive)
				.InitializeFromSource();

			toggleNewBorderPreview.Binding
				.AddBinding(ViewModel, vm => vm.IsNewBorderPreviewActive, w => w.Active)
				.InitializeFromSource();

			toggleNewBorderPreview.Toggled += OnToggleNewBorderPreviewToggled;

			cmbMapType.ItemsEnum = typeof(MapProviders);
			cmbMapType.TooltipText = "Если карта отображается некорректно или не отображается вовсе - смените тип карты";
			cmbMapType.EnumItemSelected += (s, e) => gmapWidget.MapProvider = MapProvidersHelper.GetPovider((MapProviders)e.SelectedItem);

			cmbMapType.SelectedItem = _defaultMapProvicer;

			gmapWidget.Position = _defaultWidgetMapPosition;
			gmapWidget.HeightRequest = 150;
			gmapWidget.HasFrame = true;
			gmapWidget.Overlays.Add(_bordersOverlay);
			gmapWidget.Overlays.Add(_newBordersPreviewOverlay);
			gmapWidget.Overlays.Add(_verticeOverlay);

			gmapWidget.ButtonPressEvent += OnGmapWidgetButtonPressed;

			#endregion Gmap

			ViewModel.RefreshBordersAction = RefreshBorders;
			ViewModel.SelectedWeekDayChangedAction = OnSelectedWeekDayChanged;
			ViewModel.SelectedDistrictBorderVerticesChangedAction = OnSelectedDistrictBorderVerticesChanged;
			ViewModel.NewBorderVerticiesAction = OnNewBordersVertices;

			RefreshBorders();
		}

		private void OnToggleNewBorderPreviewToggled(object sender, EventArgs e)
		{
			if(toggleNewBorderPreview.Active && ViewModel.NewBorderVertices.Any())
			{
				var previewBorder = new GMapPolygon(ViewModel.NewBorderVertices, "Предпросмотр новых границ");
				_newBordersPreviewOverlay.Polygons.Add(previewBorder);
			}
			else
			{
				_newBordersPreviewOverlay.Clear();
			}
		}

		private void PrepareWeekDayServiceDistrictPriceRulesTreeView()
		{
			ytreeWeekDayRulePrices.ColumnsConfig = ColumnsConfigFactory.Create<WeekDayServiceDistrictRule>()
				.AddColumn("Цена")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(p => p.Price)
					.Digits(2)
					.WidthChars(10)
					.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0))
					.Editing(ViewModel.CanEditServiceDeliveryRules)
					.AddSetter((c, r) => c.BackgroundGdk = r.Price <= 0 ? _dangerBaseColor : _primaryBaseColor)
					.AddTextRenderer(node => CurrencyWorks.CurrencyShortName, false)
				.AddColumn("Тип услуги")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(p => p.ServiceType.GetEnumTitle())
					.WrapMode(Pango.WrapMode.WordChar)
					.WrapWidth(500)
				.Finish();

			ytreeWeekDayRulePrices.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.WeekDayServiceDistrictRules, w => w.ItemsDataSource)
				.InitializeFromSource();
		}

		private void PrepareCommonDistrictRuleItemsTreeView()
		{
			ytreeCommonRulePrices.ColumnsConfig = ColumnsConfigFactory.Create<CommonServiceDistrictRule>()
				.AddColumn("Цена")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(p => p.Price)
					.Digits(2)
					.WidthChars(10)
					.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0))
					.Editing(ViewModel.CanEditServiceDeliveryRules)
					.AddSetter((c, r) => c.BackgroundGdk = r.Price <= 0 ? _dangerBaseColor : _primaryBaseColor)
					.AddTextRenderer(node => CurrencyWorks.CurrencyShortName, false)
				.AddColumn("Тип услуги")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(p => p.ServiceType.GetEnumTitle())
					.WrapMode(Pango.WrapMode.WordChar)
					.WrapWidth(500)
				.Finish();

			ytreeCommonRulePrices.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CommonServiceDistrictRules, w => w.ItemsDataSource)
				.InitializeFromSource();
		}

		private void PrepareServiceDeliveryScheduleRestrictionsTreeView()
		{
			ytreeScheduleRestrictions.ColumnsConfig = ColumnsConfigFactory.Create<ServiceDeliveryScheduleRestriction>()
				.AddColumn("График")
					.MinWidth(100)
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.DeliverySchedule.Name)
				.AddColumn(_acceptBeforeColumnTag)
					.SetTag(_acceptBeforeColumnTag)
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.AcceptBeforeTitle)
					.AddSetter((c, r) =>
						c.BackgroundGdk = r.WeekDay == WeekDayName.Today && r.AcceptBefore == null ? _dangerBaseColor : _primaryBaseColor)
				.Finish();

			ytreeScheduleRestrictions.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.ScheduleRestrictions, w => w.ItemsDataSource)
				.AddBinding(vm => vm.SelectedScheduleRestriction, w => w.SelectedRow)
				.InitializeFromSource();
		}

		private void PrepareDistrictsTreeView()
		{
			ytreeDistricts.ColumnsConfig = ColumnsConfigFactory.Create<ServiceDistrict>()
				.AddColumn("Код")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.Id.ToString())
				.AddColumn("Название")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.ServiceDistrictName)
					.Editable(ViewModel.CanEditServiceDistrict)
				.AddColumn("")
				.Finish();

			ytreeDistricts.Binding
				.AddBinding(ViewModel.Entity, e => e.ServiceDistricts, w => w.ItemsDataSource)
				.AddBinding(ViewModel, vm => vm.SelectedServiceDistrict, w => w.SelectedRow)
				.InitializeFromSource();

			ytreeDistricts.ButtonReleaseEvent += OnButtonDistrictsRelease;
		}


		private void OnGmapWidgetButtonPressed(object o, ButtonPressEventArgs args)
		{
			if(args.Event.Button == 1 && ViewModel.IsCreatingNewBorder)
			{
				ViewModel.AddNewVertexCommand.Execute(gmapWidget.FromLocalToLatLng((int)args.Event.X, (int)args.Event.Y));
			}

			if(args.Event.Button == 3 && ViewModel.IsCreatingNewBorder)
			{
				var marker = _verticeOverlay.Markers.FirstOrDefault(m => m.IsMouseOver);

				if(marker == null)
				{
					return;
				}

				var pointMarker = new PointLatLng(marker.Position.Lat, marker.Position.Lng);

				if(ViewModel.NewBorderVertices.Contains(pointMarker))
				{
					Menu popupMenu = new Menu();
					var item = new MenuItem("Удалить");
					item.Activated += (sender, ea) => ViewModel.RemoveNewBorderVertexCommand.Execute(pointMarker);
					popupMenu.Add(item);
					popupMenu.ShowAll();
					popupMenu.Popup();
				}
			}
		}

		private void RefreshBorders()
		{
			_bordersOverlay.Clear();

			foreach(ServiceDistrict district in ViewModel.Entity.ServiceDistricts)
			{
				if(district.ServiceDistrictBorder != null)
				{
					_bordersOverlay.Polygons.Add(new GMapPolygon(
						district.ServiceDistrictBorder.Coordinates
							.Select(p => new PointLatLng(p.X, p.Y)).ToList(), district.ServiceDistrictName));
				}
			}
		}

		private void OnNewBordersVertices()
		{
			_verticeOverlay.Clear();

			if(ViewModel.NewBorderVertices != null && ViewModel.NewBorderVertices.Any())
			{
				for(int i = 0; i < ViewModel.NewBorderVertices.Count; i++)
				{
					var color = GMarkerGoogleType.red;

					if(i == 0)
					{
						color = GMarkerGoogleType.yellow;
					}
					else if(i == ViewModel.NewBorderVertices.Count - 1)
					{
						color = GMarkerGoogleType.green;
					}

					GMapMarker point = new GMarkerGoogle(ViewModel.NewBorderVertices[i], color);
					_verticeOverlay.Markers.Add(point);
				}

				if(toggleNewBorderPreview.Active)
				{
					toggleNewBorderPreview.Active = false;
					toggleNewBorderPreview.Active = true;
				}
			}
		}

		private void OnSelectedDistrictBorderVerticesChanged()
		{

			_verticeOverlay.Clear();

			var polygon = new GMapPolygon(ViewModel.SelectedDistrictBorderVertices.ToList(), "polygon")
			{
				Stroke = _selectedDistrictBorderPen
			};

			_verticeOverlay.Polygons.Add(polygon);
		}

		private void OnSelectedWeekDayChanged()
		{
			var column = ytreeScheduleRestrictions.ColumnsConfig
				.GetColumnsByTag(_acceptBeforeColumnTag)
				.First();

			column.Title =
				ViewModel.SelectedWeekDayName == WeekDayName.Today
					? _acceptBeforeColumnTag
					: $"{_acceptBeforeColumnTag} прошлого дня";
		}


		#region Copy Paste District Schedule

		private void AddCopyPasteDistrictPopupActions()
		{
			_popupDistrictScheduleMenu = new Menu();
			_popupDistiictScheduleStateActions = new List<Action>();

			_copyDistrictScheduleMenuEntry = new MenuItem("Копировать график доставки");
			_copyDistrictScheduleMenuEntry.ButtonPressEvent += OnCopyDistrictScheduleButtonPress;
			_copyDistrictScheduleMenuEntry.Visible = true;

			_popupDistrictScheduleMenu.Add(_copyDistrictScheduleMenuEntry);

			_pasteScheduleToDistrictMenuEntry = new MenuItem("Вставить график доставки в район");
			_pasteScheduleToDistrictMenuEntry.ButtonPressEvent += OnPasteScheduleToDistrictButtonPress;
			_pasteScheduleToDistrictMenuEntry.Visible = true;

			_popupDistrictScheduleMenu.Add(_pasteScheduleToDistrictMenuEntry);

			_popupDistiictScheduleStateActions
				.Add(CopyDistrictScheduleStateActions(_copyDistrictScheduleMenuEntry));

			_popupDistiictScheduleStateActions
				.Add(PasteScheduleToDistrictStateActions(_pasteScheduleToDistrictMenuEntry));
		}

		private void OnPasteScheduleToDistrictButtonPress(object o, ButtonPressEventArgs args)
		{
			ViewModel.PasteSchedulesToDistrictCommand.Execute();
		}

		private void OnCopyDistrictScheduleButtonPress(object o, ButtonPressEventArgs args)
		{
			ViewModel.CopyDistrictSchedulesCommand.Execute();
		}

		private void OnButtonDistrictsRelease(object o, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button != (uint)GtkMouseButton.Right)
			{
				return;
			}

			UpdateDistrictSchedulePopupMenuItemsStates();

			_popupDistrictScheduleMenu.Show();

			if(_popupDistrictScheduleMenu.Children.Length == 0)
			{
				return;
			}

			_popupDistrictScheduleMenu.Popup(); ;
		}

		private void UpdateDistrictSchedulePopupMenuItemsStates()
		{
			_popupDistiictScheduleStateActions.ForEach(x => x.Invoke());
		}

		private Action CopyDistrictScheduleStateActions(MenuItem item) => () =>
		{
			if(item.Child is AccelLabel label)
			{
				label.LabelProp = ViewModel.CopyDistrictScheduleMenuItemLabel;
			}

			item.Sensitive = ViewModel.CanCopyDeliveryScheduleRestrictions;
		};

		private Action PasteScheduleToDistrictStateActions(MenuItem item) => () =>
		{
			if(item.Child is AccelLabel label)
			{
				label.LabelProp = ViewModel.PasteScheduleToDistrictMenuItemLabel;
			}

			item.Sensitive = ViewModel.CanPasteDeliveryScheduleRestrictions;
		};


		#endregion Copy Paste District Schedule

		public override void Destroy()
		{
			toggleNewBorderPreview.Toggled -= OnToggleNewBorderPreviewToggled;
			gmapWidget.ButtonPressEvent -= OnGmapWidgetButtonPressed;
			ytreeDistricts.ButtonReleaseEvent -= OnButtonDistrictsRelease;
			_copyDistrictScheduleMenuEntry.ButtonPressEvent -= OnCopyDistrictScheduleButtonPress;
			_pasteScheduleToDistrictMenuEntry.ButtonPressEvent -= OnPasteScheduleToDistrictButtonPress;

			base.Destroy();
		}
	}
}
