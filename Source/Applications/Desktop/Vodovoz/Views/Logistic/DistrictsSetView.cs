using Gamma.GtkWidgets;
using Gamma.Utilities;
using GMap.NET;
using GMap.NET.GtkSharp;
using GMap.NET.GtkSharp.Markers;
using Gtk;
using MoreLinq;
using QS.Dialog.GtkUI;
using QS.Journal.GtkUI;
using QS.Navigation;
using QS.Services;
using QS.Utilities;
using QS.Views.GtkUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using Vodovoz.Additions.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Logistic;
using Action = System.Action;
using GdkColor = Gdk.Color;

namespace Vodovoz.Views.Logistic
{
	public partial class DistrictsSetView : TabViewBase<DistrictsSetViewModel>
	{
		private readonly ICommonServices _commonServices;

		private const string _acceptBeforeColumnTag = "Прием до";
		private readonly PointLatLng _defaultWidgetMapPosition = new PointLatLng(59.93900, 30.31646);
		private readonly MapProviders _defaultMapProvicer = MapProviders.GoogleMap;

		private readonly GMapOverlay _bordersOverlay = new GMapOverlay("district_borders");
		private readonly GMapOverlay _newBordersPreviewOverlay = new GMapOverlay("district_preview_borders");
		private readonly GMapOverlay _verticeOverlay = new GMapOverlay("district_vertice");

		private readonly GdkColor _primaryBaseColor;
		private readonly GdkColor _dangerBaseColor;

		private readonly Pen _selectedDistrictBorderPen = new Pen(Color.Red, 2);

		private Menu _popupDistrictScheduleMenu;
		private List<Action> _popupDistiictScheduleStateActions;

		public DistrictsSetView(
			DistrictsSetViewModel viewModel,
			ICommonServices commonServices)
			: base(viewModel)
		{
			_primaryBaseColor = GdkColors.PrimaryBase;
			_dangerBaseColor = GdkColors.DangerBase;
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));

			Build();
			Configure();
		}

		private void Configure()
		{
			AddCopyPasteDistrictPopupActions();

			#region TreeViews

			PrepareDistrictsTreeView();
			PrepareDeliveryScheduleRestrictionsTreeView();
			PrepareCommonDistrictRuleItemsTreeView();
			PrepareWeekDayDistrictRuleItemsTreeView();

			#endregion

			btnSave.Clicked += (sender, args) => ViewModel.Save();
			btnSave.Binding
				.AddFuncBinding(ViewModel, vm => vm.CanSave, w => w.Sensitive)
				.InitializeFromSource();

			btnCancel.Clicked += OnButtonCancelClicked;

			ylabelStatusString.Text = ViewModel.Entity.Status.GetEnumTitle();

			entryName.Binding
				.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text)
				.InitializeFromSource();

			entryName.Binding
				.AddFuncBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			btnAddDistrict.Binding
				.AddFuncBinding(ViewModel, vm => vm.CanCreateDistrict, w => w.Sensitive)
				.InitializeFromSource();

			btnAddDistrict.Clicked += OnButtonAddDistrictClicked;

			btnRemoveDistrict.Clicked += OnButtonRemoveDistrictClicked;

			btnRemoveDistrict.Binding
				.AddFuncBinding(ViewModel, vm => vm.SelectedDistrict != null && vm.CanDeleteDistrict, w => w.Sensitive)
				.InitializeFromSource();

			ytextComment.Binding
				.AddBinding(ViewModel.Entity, e => e.Comment, w => w.Buffer.Text)
				.InitializeFromSource();

			ytextComment.Binding
				.AddFuncBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			btnAddCommonRule.Binding
				.AddFuncBinding(ViewModel, vm => vm.SelectedDistrict != null && vm.CanEditDeliveryRules, w => w.Sensitive)
				.InitializeFromSource();

			btnAddCommonRule.Clicked += (sender, args) => ViewModel.AddCommonDeliveryPriceRuleCommand.Execute();

			btnRemoveCommonRule.Binding
				.AddFuncBinding(
					ViewModel,
					vm => vm.CanEditDeliveryRules
						&& vm.SelectedDistrict != null
						&& vm.SelectedCommonDistrictRuleItem != null,
					w => w.Sensitive)
				.InitializeFromSource();

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

			btnAddSchedule.Binding
				.AddFuncBinding(
					ViewModel,
					vm => vm.CanEditDeliveryScheduleRestriction
						&& vm.SelectedDistrict != null
						&& vm.SelectedWeekDayName.HasValue,
					w => w.Sensitive)
				.InitializeFromSource();

			btnAddSchedule.Clicked += (sender, args) => ViewModel.AddScheduleRestrictionCommand.Execute();

			ViewModel.AddScheduleRestrictionCommand.CanExecuteChanged += (sender, args) =>
				btnAddSchedule.Sensitive = ViewModel.AddScheduleRestrictionCommand.CanExecute();

			btnRemoveSchedule.Binding
				.AddFuncBinding(
					ViewModel,
					vm => vm.CanEditDeliveryScheduleRestriction
						&& vm.SelectedDistrict != null
						&& vm.SelectedScheduleRestriction != null,
					w => w.Sensitive)
				.InitializeFromSource();

			btnRemoveSchedule.Clicked += (sender, args) => ViewModel.RemoveScheduleRestrictionCommand.Execute();

			btnAddAcceptBefore.BindCommand(ViewModel.AddAcceptBeforeCommand);

			btnRemoveAcceptBefore.Binding
				.AddFuncBinding(
					ViewModel,
					vm => vm.CanEditDeliveryScheduleRestriction
						&& vm.SelectedDistrict != null
						&& vm.SelectedScheduleRestriction != null
						&& vm.SelectedScheduleRestriction.AcceptBefore != null,
					w => w.Sensitive)
				.InitializeFromSource();

			btnRemoveAcceptBefore.Clicked += (sender, args) => ViewModel.RemoveAcceptBeforeCommand.Execute();

			btnAddWeekDayRule.Binding
				.AddFuncBinding(
					ViewModel,
					vm => vm.CanEditDeliveryRules
						&& vm.SelectedDistrict != null
						&& vm.SelectedWeekDayName.HasValue,
					w => w.Sensitive)
				.InitializeFromSource();

			btnAddWeekDayRule.Clicked += (sender, args) => ViewModel.AddWeekDayDeliveryPriceRuleCommand.Execute();

			btnRemoveWeekDayRule.Binding
				.AddFuncBinding(
					ViewModel,
					vm => vm.CanEditDeliveryRules
						&& vm.SelectedDistrict != null
						&& vm.SelectedWeekDayDistrictRuleItem != null,
					w => w.Sensitive)
				.InitializeFromSource();

			btnRemoveWeekDayRule.Clicked += (sender, args) => ViewModel.RemoveWeekDayDistrictRuleItemCommand.Execute();

			cmbGeoGroup.ItemsList = ViewModel.UoW.GetAll<GeoGroup>().ToList();
			cmbGeoGroup.Binding
				.AddBinding(ViewModel, vm => vm.SelectedGeoGroup, w => w.SelectedItem)
				.InitializeFromSource();

			cmbGeoGroup.Binding
				.AddFuncBinding(
					ViewModel,
					vm => vm.CanEditDistrict && vm.SelectedDistrict != null,
					w => w.Sensitive)
				.InitializeFromSource();

			cmbWageDistrict.ItemsList = ViewModel.UoW.Session.QueryOver<WageDistrict>().Where(d => !d.IsArchive).List();

			cmbWageDistrict.Binding
				.AddBinding(ViewModel, vm => vm.SelectedWageDistrict, w => w.SelectedItem)
				.InitializeFromSource();

			cmbWageDistrict.Binding
				.AddFuncBinding(
					ViewModel,
					vm => vm.CanEditDistrict
						&& vm.SelectedDistrict != null
						&& vm.CanChangeDistrictWageTypePermissionResult,
					w => w.Sensitive)
				.InitializeFromSource();

			cmbWageDistrict.SetRenderTextFunc<WageDistrict>(x => x.Name);

			#region GMap

			btnAddBorder.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsCreatingNewBorder, w => w.Visible)
				.InitializeFromSource();

			btnAddBorder.Binding
				.AddFuncBinding(
					ViewModel,
					vm => vm.CanEditDistrict
						&& !vm.IsCreatingNewBorder
						&& vm.SelectedDistrict != null
						&& vm.SelectedDistrict.DistrictBorder == null,
					w => w.Sensitive)
				.InitializeFromSource();

			btnAddBorder.Clicked += (sender, args) => ViewModel.CreateBorderCommand.Execute();

			btnRemoveBorder.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsCreatingNewBorder, w => w.Visible)
				.InitializeFromSource();

			btnRemoveBorder.Binding
				.AddFuncBinding(
					ViewModel,
					vm => vm.CanEditDistrict
						&& !vm.IsCreatingNewBorder
						&& vm.SelectedDistrict != null
						&& vm.SelectedDistrict.DistrictBorder != null,
					w => w.Sensitive)
				.InitializeFromSource();

			btnRemoveBorder.Clicked += (sender, args) =>
			{
				if(MessageDialogHelper.RunQuestionDialog($"Удалить границу района {ViewModel.SelectedDistrict.DistrictName}?"))
				{
					ViewModel.RemoveBorderCommand.Execute();
					RefreshBorders();
				}
			};

			btnConfirmNewBorder.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsCreatingNewBorder, w => w.Visible)
				.InitializeFromSource();

			btnConfirmNewBorder.Binding
				.AddFuncBinding(ViewModel, vm => vm.SelectedDistrict != null && vm.IsCreatingNewBorder, w => w.Sensitive)
				.InitializeFromSource();

			btnConfirmNewBorder.Clicked += (sender, args) =>
			{
				if(MessageDialogHelper.RunQuestionDialog("Завершить создание границы района?"))
				{
					if(ViewModel.NewBorderVertices.Count < 3)
					{
						MessageDialogHelper.RunInfoDialog("Нельзя создать границу района меньше чем за 3 точки");
						return;
					}
					toggleNewBorderPreview.Active = false;
					ViewModel.ConfirmNewBorderCommand.Execute();
					RefreshBorders();
				}
			};

			btnCancelNewBorder.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsCreatingNewBorder, w => w.Visible)
				.InitializeFromSource();

			btnCancelNewBorder.Binding
				.AddFuncBinding(ViewModel, vm => vm.SelectedDistrict != null && vm.IsCreatingNewBorder, w => w.Sensitive)
				.InitializeFromSource();

			btnCancelNewBorder.Clicked += (sender, args) =>
			{
				if(MessageDialogHelper.RunQuestionDialog("Отменить создание границы района?"))
				{
					ViewModel.CancelNewBorderCommand.Execute();
					toggleNewBorderPreview.Active = false;
				}
			};

			toggleNewBorderPreview.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsCreatingNewBorder, w => w.Visible)
				.InitializeFromSource();

			toggleNewBorderPreview.Binding
				.AddFuncBinding(ViewModel, vm => vm.SelectedDistrict != null && vm.IsCreatingNewBorder, w => w.Sensitive)
				.InitializeFromSource();

			toggleNewBorderPreview.Toggled += (sender, args) =>
			{
				if(toggleNewBorderPreview.Active && ViewModel.NewBorderVertices.Any())
				{
					var previewBorder = new GMapPolygon(ViewModel.NewBorderVertices.ToList(), "Предпросмотр новых границ");
					_newBordersPreviewOverlay.Polygons.Add(previewBorder);
				}
				else
				{
					_newBordersPreviewOverlay.Clear();
				}
			};

			cmbMapType.ItemsEnum = typeof(MapProviders);
			cmbMapType.TooltipText = "Если карта отображается некорректно или не отображается вовсе - смените тип карты";
			cmbMapType.EnumItemSelected += (sender, args) =>
				gmapWidget.MapProvider = MapProvidersHelper.GetPovider((MapProviders)args.SelectedItem);
			cmbMapType.SelectedItem = _defaultMapProvicer;

			gmapWidget.Position = _defaultWidgetMapPosition;
			gmapWidget.HeightRequest = 150;
			gmapWidget.HasFrame = true;
			gmapWidget.Overlays.Add(_bordersOverlay);
			gmapWidget.Overlays.Add(_newBordersPreviewOverlay);
			gmapWidget.Overlays.Add(_verticeOverlay);

			RefreshBorders();

			gmapWidget.ButtonPressEvent += OnGmapWidgetButtonPressed;

			#endregion

			ViewModel.PropertyChanged += (sender, args) =>
			{
				Gtk.Application.Invoke((s, e) =>
				{
					ViewModelPropertyChangedHandler(sender, args);
				});
			};
		}

		private void OnButtonCancelClicked(object sender, EventArgs e)
		{
			ViewModel.Close(true, CloseSource.Cancel);
		}

		private void OnButtonAddDistrictClicked(object sender, EventArgs e)
		{
			ViewModel.AddDistrictCommand.Execute();
			ScrollToSelectedDistrict();
		}

		private void PrepareWeekDayDistrictRuleItemsTreeView()
		{
			ytreeWeekDayRulePrices.ColumnsConfig = ColumnsConfigFactory.Create<WeekDayDistrictRuleItem>()
				.AddColumn("Цена")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(p => p.Price)
					.Digits(2)
					.WidthChars(10)
					.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0))
					.Editing(ViewModel.CanEditDeliveryRules)
					.AddSetter((c, r) => c.BackgroundGdk = r.Price <= 0 ? _dangerBaseColor : _primaryBaseColor)
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
		}

		private void PrepareCommonDistrictRuleItemsTreeView()
		{
			ytreeCommonRulePrices.ColumnsConfig = ColumnsConfigFactory.Create<CommonDistrictRuleItem>()
				.AddColumn("Цена")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(p => p.Price)
					.Digits(2)
					.WidthChars(10)
					.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0))
					.Editing(ViewModel.CanEditDeliveryRules)
					.AddSetter((c, r) => c.BackgroundGdk = r.Price <= 0 ? _dangerBaseColor : _primaryBaseColor)
					.AddTextRenderer(node => CurrencyWorks.CurrencyShortName, false)
				.AddColumn("Правило")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(p => p.DeliveryPriceRule.Title)
					.WrapMode(Pango.WrapMode.WordChar)
					.WrapWidth(500)
				.Finish();

			ytreeCommonRulePrices.Binding
				.AddBinding(ViewModel, vm => vm.CommonDistrictRuleItems, w => w.ItemsDataSource);

			ytreeCommonRulePrices.Selection.Changed += (sender, args) =>
				ViewModel.SelectedCommonDistrictRuleItem = ytreeCommonRulePrices.GetSelectedObject() as CommonDistrictRuleItem;
		}

		private void PrepareDeliveryScheduleRestrictionsTreeView()
		{
			ytreeScheduleRestrictions.ColumnsConfig = ColumnsConfigFactory.Create<DeliveryScheduleRestriction>()
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

			ytreeScheduleRestrictions.Binding
				.AddBinding(ViewModel, vm => vm.ScheduleRestrictions, w => w.ItemsDataSource);

			ytreeScheduleRestrictions.Selection.Changed += (sender, args) =>
				ViewModel.SelectedScheduleRestriction = ytreeScheduleRestrictions.GetSelectedObject() as DeliveryScheduleRestriction;
		}

		private void PrepareDistrictsTreeView()
		{
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
				.AddColumn("Мин. бутылей для бесплатной доставки")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x =>
						x.CommonDistrictRuleItems.Any()
							? x.CommonDistrictRuleItems.Max(c => c.DeliveryPriceRule.Water19LCount).ToString()
							: "-")
					.XAlign(0.5f)
				.AddColumn("")
				.Finish();

			ytreeDistricts.Binding
				.AddBinding(ViewModel.Entity, e => e.ObservableDistricts, w => w.ItemsDataSource)
				.InitializeFromSource();

			ytreeDistricts.Selection.Changed += OnTreeDistrictsTreeViewSelectionChanged;

			ytreeDistricts.ButtonReleaseEvent += OnButtonDistrictsRelease;
		}

		private void OnTreeDistrictsTreeViewSelectionChanged(object sender, EventArgs e)
		{
			if(ViewModel.IsCreatingNewBorder)
			{
				ViewModel.CancelNewBorderCommand.Execute();
				toggleNewBorderPreview.Active = false;
			}

			ViewModel.SelectedDistrict = ytreeDistricts.GetSelectedObject() as District;
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
					item.Activated += (sender, e) => ViewModel.RemoveNewBorderVertexCommand.Execute(pointMarker);
					popupMenu.Add(item);
					popupMenu.ShowAll();
					popupMenu.Popup();
				}
			}
		}

		private void RefreshBorders()
		{
			_bordersOverlay.Clear();

			foreach(District district in ViewModel.Entity.ObservableDistricts)
			{
				if(district.DistrictBorder != null)
				{
					_bordersOverlay.Polygons.Add(new GMapPolygon(
						district.DistrictBorder.Coordinates
							.Select(p => new PointLatLng(p.X, p.Y)).ToList(), district.DistrictName));
				}
			}
		}

		private void OnButtonRemoveDistrictClicked(object sender, EventArgs e)
		{
			ViewModel.RemoveDistrictCommand.Execute();

			if(ViewModel.SelectedDistrict == null)
			{
				RefreshBorders();
			}
			else
			{
				ScrollToSelectedDistrict();
			}
		}

		public void ViewModelPropertyChangedHandler(object sender, EventArgs e)
		{
			if(!(e is PropertyChangedEventArgs args))
			{
				return;
			}

			switch(args.PropertyName)
			{
				case nameof(ViewModel.SelectedWeekDayName):
					var column = ytreeScheduleRestrictions.ColumnsConfig
						.GetColumnsByTag(_acceptBeforeColumnTag)
						.First();

					column.Title =
						ViewModel.SelectedWeekDayName == WeekDayName.Today
							? _acceptBeforeColumnTag
							: $"{_acceptBeforeColumnTag} прошлого дня";

					break;
				case nameof(ViewModel.SelectedDistrict):
					if(ViewModel.SelectedDistrict != null)
					{
						ytreeDistricts.SelectObject(ViewModel.SelectedDistrict);
					}

					break;
				case nameof(ViewModel.SelectedScheduleRestriction):
					if(ViewModel.SelectedScheduleRestriction != null)
					{
						ytreeScheduleRestrictions.SelectObject(ViewModel.SelectedScheduleRestriction);
					}

					break;
				case nameof(ViewModel.SelectedCommonDistrictRuleItem):
					if(ViewModel.SelectedCommonDistrictRuleItem != null)
					{
						ytreeCommonRulePrices.SelectObject(ViewModel.SelectedCommonDistrictRuleItem);
					}

					break;
				case nameof(ViewModel.SelectedWeekDayDistrictRuleItem):
					if(ViewModel.SelectedWeekDayDistrictRuleItem != null)
					{
						ytreeWeekDayRulePrices.SelectObject(ViewModel.SelectedWeekDayDistrictRuleItem);
					}

					break;
				case nameof(ViewModel.SelectedDistrictBorderVertices):
					_verticeOverlay.Clear();

					if(ViewModel.SelectedDistrictBorderVertices != null)
					{
						var polygon = new GMapPolygon(ViewModel.SelectedDistrictBorderVertices.ToList(), "polygon")
						{
							Stroke = _selectedDistrictBorderPen
						};

						_verticeOverlay.Polygons.Add(polygon);
					}

					break;
				case nameof(ViewModel.NewBorderVertices):
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

					break;
				case nameof(ViewModel.ScheduleRestrictions):
					ytreeScheduleRestrictions.ItemsDataSource = ViewModel.ScheduleRestrictions;
					ScrollToSelectedScheduleRestriction();
					break;
			}
		}

		private void ScrollToSelectedScheduleRestriction()
		{
			if(ViewModel.SelectedScheduleRestriction == null)
			{
				return;
			}

			var iter = ytreeScheduleRestrictions.YTreeModel.IterFromNode(ViewModel.SelectedScheduleRestriction);
			var path = ytreeScheduleRestrictions.YTreeModel.GetPath(iter);

			ytreeScheduleRestrictions.ScrollToCell(
				path,
				ytreeScheduleRestrictions.Columns.FirstOrDefault(),
				false,
				0,
				0);
		}

		private void ScrollToSelectedDistrict()
		{
			if(ViewModel.SelectedDistrict == null)
			{
				return;
			}

			var iter = ytreeDistricts.YTreeModel.IterFromNode(ViewModel.SelectedDistrict);
			var path = ytreeDistricts.YTreeModel.GetPath(iter);

			ytreeDistricts.ScrollToCell(path, ytreeDistricts.Columns.FirstOrDefault(), false, 0, 0);
		}

		#region Copy Paste District Schedule

		private void AddCopyPasteDistrictPopupActions()
		{
			_popupDistrictScheduleMenu = new Menu();
			_popupDistiictScheduleStateActions = new List<Action>();

			var copyDistrictScheduleMenuEntry = new MenuItem("Копировать график доставки");
			copyDistrictScheduleMenuEntry.ButtonPressEvent += (s, e) => ViewModel.CopyDistrictSchedulesCommand.Execute();
			copyDistrictScheduleMenuEntry.Visible = true;

			_popupDistrictScheduleMenu.Add(copyDistrictScheduleMenuEntry);

			var pasteScheduleToDistrictMenuEntry = new MenuItem("Вставить график доставки в район");
			pasteScheduleToDistrictMenuEntry.ButtonPressEvent += (s, e) => ViewModel.PasteSchedulesToDistrictCommand.Execute();
			pasteScheduleToDistrictMenuEntry.Visible = true;

			_popupDistrictScheduleMenu.Add(pasteScheduleToDistrictMenuEntry);

			var pasteScheduleToZoneMenuEntry = new MenuItem("Вставить график доставки в тарифную зону");
			pasteScheduleToZoneMenuEntry.ButtonPressEvent += (s, e) => ViewModel.PasteSchedulesToZoneCommand.Execute();
			pasteScheduleToZoneMenuEntry.Visible = true;

			_popupDistrictScheduleMenu.Add(pasteScheduleToZoneMenuEntry);

			_popupDistiictScheduleStateActions
				.Add(CopyDistrictScheduleStateActions(copyDistrictScheduleMenuEntry));

			_popupDistiictScheduleStateActions
				.Add(PasteScheduleToDistrictStateActions(pasteScheduleToDistrictMenuEntry));

			_popupDistiictScheduleStateActions
				.Add(PasteScheduleToZoneStateActions(pasteScheduleToZoneMenuEntry));
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

			_popupDistrictScheduleMenu.Popup();
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

		private Action PasteScheduleToZoneStateActions(MenuItem item) => () =>
		{
			if(item.Child is AccelLabel label)
			{
				label.LabelProp = ViewModel.PasteScheduleToTafiffZoneMenuItemLabel;
			}

			item.Sensitive = ViewModel.CanPasteDeliveryScheduleRestrictions;
		};

		#endregion Copy Paste District Schedule
	}
}
