using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using Gamma.Widgets.Additions;
using Gtk;
using QS.Dialog;
using QS.Views.GtkUI;
using System;
using System.Linq;
using System.Linq.Expressions;
using Vodovoz.Core.Domain.Logistics.Cars;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.ViewModels.ViewModels.Logistic.DriverSchedule;
using Vodovoz.ViewWidgets.Profitability;
using VodovozBusiness.Nodes;

namespace Vodovoz.Views.Logistic
{
	public partial class DriverScheduleView : TabViewBase<DriverScheduleViewModel>
	{
		private const int _carTypeOfUseColumnIndex = 0;
		private const int _carOwnTypeColumnIndex = 1;
		private const int _driverCarOwnTypeColumnIndex = 4;
		private const int _phoneColumnIndex = 5;
		private const int _districtColumnIndex = 6;
		private const int _arrivalTimeColumnIndex = 7;
		private const int _lastModifiedDateTimeColumnIndex = 12;
		private const int _totalTitleColumnIndex = 13;
		private const int _fixedColumnsCount = 14;

		private bool _isSynchronizingDriverScheduleVerticalScroll;
		private bool _isLockingFixedPartHorizontalScroll;
		private ScrolledWindow _filtersScrolledWindow;

		public DriverScheduleView(DriverScheduleViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();

			ViewModel.PropertyChanged += (sender, e) =>
			{
				if(e.PropertyName == nameof(DriverScheduleViewModel.DriverScheduleRows))
				{
					RefreshTreeViews();
					ConfigureDynamicTreeView();
				}

				if(IsColumnVisibilityProperty(e.PropertyName))
				{
					ApplyFixedColumnsVisibility();
				}
			};
		}

		private void Configure()
		{
			ConfigureFiltersScrolling();
			leftsidepanel5.Panel = _filtersScrolledWindow ?? (Widget)yvboxFilters;
			leftsidepanel5.Title = "Параметры";

			var weekPicker = new MonthPickerView(ViewModel.WeekPickerViewModel);
			weekPicker.Show();
			ViewModel.WeekPickerViewModel.DateEntryWidthRequest = -1;
			yhboxWeek.Add(weekPicker);

			buttonSave.Binding
				.AddBinding(ViewModel, vm => vm.CanSave, w => w.Sensitive)
				.InitializeFromSource();
			buttonSave.BindCommand(ViewModel.SaveCommand);

			buttonCancel.Clicked += (sender, e) => ViewModel.Close(ViewModel.AskSaveOnClose, QS.Navigation.CloseSource.Cancel);
			buttonCancel.BindCommand(ViewModel.CancelCommand);

			ybuttonExport.BindCommand(ViewModel.ExportCommand);
			ybuttonInfo.BindCommand(ViewModel.InfoCommand);

			enumcheckCarTypeOfUse.EnumType = typeof(CarTypeOfUse);
			enumcheckCarTypeOfUse.AddEnumToHideList(CarTypeOfUse.Loader);
			enumcheckCarTypeOfUse.AddEnumToHideList(CarTypeOfUse.Truck);
			enumcheckCarTypeOfUse.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.SelectedCarTypeOfUse, w => w.SelectedValuesList, new EnumsListConverter<CarTypeOfUse>())
				.InitializeFromSource();
			enumcheckCarTypeOfUse.SelectAll();

			enumcheckCarOwnType.EnumType = typeof(CarOwnType);
			enumcheckCarOwnType.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.SelectedCarOwnTypes, w => w.SelectedValuesList, new EnumsListConverter<CarOwnType>())
				.InitializeFromSource();
			enumcheckCarOwnType.SelectAll();

			ytreeviewSubdivison.ColumnsConfig = FluentColumnsConfig<SubdivisionNode>.Create()
				.AddColumn("Подразделение").AddTextRenderer(x => x.SubdivisionName)
				.AddColumn("").AddToggleRenderer((x => x.Selected))
				.Finish();

			ytreeviewSubdivison.Binding
				.AddBinding(ViewModel, x => x.Subdivisions, x => x.ItemsDataSource)
				.InitializeFromSource();

			ybuttonApplyFilters.BindCommand(ViewModel.ApplyFiltersCommand);
			ConfigureDriverSearchBinding();
			ConfigureColumnVisibilityBindings();

			ConfigureFixedTreeView();
			ConfigureDynamicTreeView();
			ConfigureDriverScheduleScrolling();
			ApplyFixedColumnsVisibility();
		}

		private void ConfigureFiltersScrolling()
		{
			if(_filtersScrolledWindow != null || yvboxFilters?.Parent != yhbox4)
			{
				return;
			}

			var filtersWidth = Math.Max(335, yvboxFilters.SizeRequest().Width + 22);
			yhbox4.Remove(yvboxFilters);

			var viewport = new Viewport
			{
				BorderWidth = 3,
				ShadowType = ShadowType.None
			};
			viewport.Add(yvboxFilters);

			_filtersScrolledWindow = new ScrolledWindow
			{
				Name = "scrolledwindowFilters",
				WidthRequest = filtersWidth,
				HscrollbarPolicy = PolicyType.Never,
				VscrollbarPolicy = PolicyType.Automatic,
				ShadowType = ShadowType.None
			};
			_filtersScrolledWindow.Add(viewport);

			yhbox4.PackStart(_filtersScrolledWindow, false, true, 0);
			yhbox4.ReorderChild(_filtersScrolledWindow, 0);
			_filtersScrolledWindow.ShowAll();
		}

		private void ConfigureDriverSearchBinding()
		{
			yentryDriverSearch.Binding
				.AddBinding(ViewModel, vm => vm.DriverSearchText, w => w.Text)
				.InitializeFromSource();
		}

		private void ConfigureColumnVisibilityBindings()
		{
			ycheckShowCarTypeOfUseColumn.Binding
				.AddBinding(ViewModel, vm => vm.ShowCarTypeOfUseColumn, w => w.Active)
				.InitializeFromSource();
			ycheckShowCarOwnTypeColumn.Binding
				.AddBinding(ViewModel, vm => vm.ShowCarOwnTypeColumn, w => w.Active)
				.InitializeFromSource();
			ycheckShowDriverCarOwnTypeColumn.Binding
				.AddBinding(ViewModel, vm => vm.ShowDriverCarOwnTypeColumn, w => w.Active)
				.InitializeFromSource();
			ycheckShowPhoneColumn.Binding
				.AddBinding(ViewModel, vm => vm.ShowPhoneColumn, w => w.Active)
				.InitializeFromSource();
			ycheckShowDistrictColumn.Binding
				.AddBinding(ViewModel, vm => vm.ShowDistrictColumn, w => w.Active)
				.InitializeFromSource();
			ycheckShowArrivalTimeColumn.Binding
				.AddBinding(ViewModel, vm => vm.ShowArrivalTimeColumn, w => w.Active)
				.InitializeFromSource();
			ycheckShowLastModifiedDateTimeColumn.Binding
				.AddBinding(ViewModel, vm => vm.ShowLastModifiedDateTimeColumn, w => w.Active)
				.InitializeFromSource();
		}

		private void ConfigureFixedTreeView()
		{
			var columnsConfig = FluentColumnsConfig<DriverScheduleRow>.Create()
				.AddColumn("Т")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.CarTypeOfUseString)
					.XAlign(0.5f)
				.AddColumn("П")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.CarOwnTypeString)
					.XAlign(0.5f)
				.AddColumn("Гос. номер")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.RegNumber ?? "")
					.XAlign(0.5f)
				.AddColumn("ФИО водителя")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.DriverFullName ?? "")
					.XAlign(0.5f)
				.AddColumn("Принадлежность")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.DriverCarOwnTypeString)
					.XAlign(0.5f)
				.AddColumn("Телефон")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.DriverPhone ?? "")
					.XAlign(0.5f)
				.AddColumn("Район проживания")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.DistrictString)
					.XAlign(0.5f)
				.AddColumn("Время приезда")
					.HeaderAlignment(0.5f)
					.AddTimeRenderer(node => node.ArrivalTime)
					.Editable()
					.XAlign(0.5f)
				.AddColumn(" Адр У ")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.MorningAddresses)
					.Adjustment(new Adjustment(0, 0, 1000, 1, 1, 1))
					.Editing()
					.XAlign(0.5f)
				.AddColumn(" Бут У" )
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.MorningBottles)
					.Adjustment(new Adjustment(0, 0, 1000, 1, 1, 1))
					.Editing()
					.XAlign(0.5f)
				.AddColumn(" Адр В ")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.EveningAddresses)
					.Adjustment(new Adjustment(0, 0, 1000, 1, 1, 1))
					.Editing()
					.XAlign(0.5f)
				.AddColumn(" Бут В ")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.EveningBottles)
					.Adjustment(new Adjustment(0, 0, 1000, 1, 1, 1))
					.Editing()
					.XAlign(0.5f)
				.AddColumn("Дата послед. изм.")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.LastModifiedDateTimeString)
					.Editable()
					.XAlign(0.5f)
				.AddColumn("")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => GetTotalTitle(node))
					.XAlign(0.5f)
				.Finish();

			ytreeviewFixedPart.ColumnsConfig = columnsConfig;
			ytreeviewFixedPart.SetItemsSource(ViewModel.DriverScheduleRows);

			ytreeviewFixedPart.BorderWidth = 0;
			ytreeviewFixedPart.RulesHint = true;
			ytreeviewFixedPart.EnableGridLines = TreeViewGridLines.Both;
			ytreeviewFixedPart.KeyPressEvent += OnFixedTreeViewKeyPress;
			ytreeviewFixedPart.Selection.Changed += (sender, e) => SynchronizeSelection(ytreeviewFixedPart, ytreeviewDynamicPart);

			GLib.Idle.Add(() =>
			{
				UpdateFixedPartWidthRequest();
				return false;
			});
		}

		private void ConfigureDynamicTreeView()
		{
			var weekStart = ViewModel.StartDate;
			var columnsConfig = FluentColumnsConfig<DriverScheduleRow>.Create();

			var dayNames = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

			for(int dayIndex = 0; dayIndex < 7; dayIndex++)
			{
				var date = weekStart.AddDays(dayIndex);
				var dayName = dayNames[dayIndex];
				var dayColumnTitle = date.Date == DateTime.Today
					? $"{ViewModel.GetShortDayString(date)} Сегодня"
					: ViewModel.GetShortDayString(date);

				columnsConfig.AddColumn(dayColumnTitle)
					.HeaderAlignment(0.5f)
					.AddComboRenderer(CreatePropertyExpression<DriverScheduleRow, CarEventType>($"{dayName}CarEventType"))
					.SetDisplayFunc(x => x == null ? "Нет" : x.ShortName)
					.FillItems(ViewModel.AvailableCarEventTypes)
					.Editing()
					.EditedEvent(OnDayComboEdited)
					.XAlign(0.5f);

				columnsConfig.AddColumn(" Адр У ")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(ConvertToObjectExpression<DriverScheduleRow, int>($"{dayName}MorningAddress"))
					.Adjustment(new Adjustment(0, 0, 1000, 1, 10, 0))
					.Editing()
					.XAlign(0.5f);

				columnsConfig.AddColumn(" Бут У ")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(ConvertToObjectExpression<DriverScheduleRow, int>($"{dayName}MorningBottles"))
					.Adjustment(new Adjustment(0, 0, 1000, 1, 10, 0))
					.Editing()
					.XAlign(0.5f);

				columnsConfig.AddColumn(" Адр В ")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(ConvertToObjectExpression<DriverScheduleRow, int>($"{dayName}EveningAddress"))
					.Adjustment(new Adjustment(0, 0, 1000, 1, 10, 0))
					.Editing()
					.XAlign(0.5f);

				columnsConfig.AddColumn(" Бут В ")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(ConvertToObjectExpression<DriverScheduleRow, int>($"{dayName}EveningBottles"))
					.Adjustment(new Adjustment(0, 0, 1000, 1, 10, 0))
					.Editing()
					.XAlign(0.5f);
			}

			columnsConfig.AddColumn(" Комментарий ")
				.HeaderAlignment(0.5f)
				.AddTextRenderer(node => node.EditedComment)
				.Editable()
				.XAlign(0f)
				.Finish();

			ytreeviewDynamicPart.ColumnsConfig = columnsConfig;

			ytreeviewDynamicPart.SetItemsSource(ViewModel.DriverScheduleRows);

			ytreeviewDynamicPart.BorderWidth = 0;
			ytreeviewDynamicPart.RulesHint = true;
			ytreeviewDynamicPart.EnableGridLines = TreeViewGridLines.Both;
			ytreeviewDynamicPart.ScrollEvent += OnTreeViewScroll;
			ytreeviewDynamicPart.KeyPressEvent += OnDynamicTreeViewKeyPress;
			ytreeviewDynamicPart.Selection.Changed += (sender, e) => SynchronizeSelection(ytreeviewDynamicPart, ytreeviewFixedPart);
		}

		private void ConfigureDriverScheduleScrolling()
		{
			scrolledwindowDriverSchedule.VscrollbarPolicy = PolicyType.Never;
			scrolledwindowDriverSchedule.HscrollbarPolicy = PolicyType.Never;

			GtkScrolledWindow.VscrollbarPolicy = PolicyType.Never;
			GtkScrolledWindow.HscrollbarPolicy = PolicyType.Always;

			GtkScrolledWindow1.VscrollbarPolicy = PolicyType.Automatic;
			GtkScrolledWindow1.HscrollbarPolicy = PolicyType.Automatic;

			GtkScrolledWindow1.Vadjustment.ValueChanged += (sender, args) =>
				SynchronizeDriverScheduleVerticalScroll(GtkScrolledWindow1, GtkScrolledWindow);

			GtkScrolledWindow.Hadjustment.ValueChanged += (sender, args) =>
				LockFixedPartHorizontalScroll();

			scrolledwindowDriverSchedule.SizeAllocated += (sender, args) =>
				UpdateDriverScheduleInnerScrollHeight(args.Allocation.Height);

			UpdateDriverScheduleInnerScrollHeight(scrolledwindowDriverSchedule.Allocation.Height);
		}

		private void UpdateFixedPartWidthRequest()
		{
			if(ytreeviewFixedPart == null || GtkScrolledWindow == null)
			{
				return;
			}

			var requisition = ytreeviewFixedPart.SizeRequest();
			if(requisition.Width > 0)
			{
				GtkScrolledWindow.WidthRequest = requisition.Width + 4;
			}
		}

		private void LockFixedPartHorizontalScroll()
		{
			if(_isLockingFixedPartHorizontalScroll || GtkScrolledWindow?.Hadjustment == null)
			{
				return;
			}

			_isLockingFixedPartHorizontalScroll = true;
			GtkScrolledWindow.Hadjustment.Value = GtkScrolledWindow.Hadjustment.Lower;
			_isLockingFixedPartHorizontalScroll = false;
		}

		private void UpdateDriverScheduleInnerScrollHeight(int availableHeight)
		{
			if(availableHeight <= 0)
			{
				return;
			}

			var scrollHeight = Math.Max(1, availableHeight - 2);
			GtkScrolledWindow.HeightRequest = scrollHeight;
			GtkScrolledWindow1.HeightRequest = scrollHeight;
		}

		private void SynchronizeDriverScheduleVerticalScroll(ScrolledWindow source, ScrolledWindow target)
		{
			if(_isSynchronizingDriverScheduleVerticalScroll)
			{
				return;
			}

			if(source?.Vadjustment == null || target?.Vadjustment == null)
			{
				return;
			}

			_isSynchronizingDriverScheduleVerticalScroll = true;

			var maxValue = Math.Max(target.Vadjustment.Lower, target.Vadjustment.Upper - target.Vadjustment.PageSize);
			target.Vadjustment.Value = Math.Min(source.Vadjustment.Value, maxValue);

			_isSynchronizingDriverScheduleVerticalScroll = false;
		}

		/// <summary>
		/// Создаёт expression для доступа к свойству по имени
		/// </summary>
		private static Expression<Func<T, TProperty>> CreatePropertyExpression<T, TProperty>(string propertyName)
		{
			var param = Expression.Parameter(typeof(T), "x");
			var property = Expression.PropertyOrField(param, propertyName);
			return Expression.Lambda<Func<T, TProperty>>(property, param);
		}

		/// <summary>
		/// Создаёт expression для доступа к свойству по имени с возвращаемым типом object
		/// </summary>
		private Expression<Func<T, object>> ConvertToObjectExpression<T, TProperty>(string propertyName)
		{
			var param = Expression.Parameter(typeof(T), "x");
			var property = Expression.PropertyOrField(param, propertyName);
			var converted = Expression.Convert(property, typeof(object));
			return Expression.Lambda<Func<T, object>>(converted, param);
		}

		private void OnDayComboEdited(object sender, EditedArgs args)
		{
			var node = ytreeviewDynamicPart.YTreeModel.NodeAtPath(new TreePath(args.Path));

			if(!(node is DriverScheduleRow driverScheduleNode))
			{
				return;
			}

			if(driverScheduleNode is DriverScheduleTotalAddressesRow ||
			   driverScheduleNode is DriverScheduleTotalBottlesRow)
			{
				args.RetVal = false;
				return;
			}

			ytreeviewDynamicPart.GetCursor(out TreePath path, out TreeViewColumn focusedColumn);

			if(focusedColumn == null)
			{
				return;
			}

			var columnIndex = Array.IndexOf(ytreeviewDynamicPart.Columns, focusedColumn);

			if(columnIndex < 0)
			{
				return;
			}

			int dayIndex = columnIndex / 5;

			if(dayIndex >= 0 && dayIndex < driverScheduleNode.Days.Length &&
			   driverScheduleNode.Days[dayIndex].HasActiveRouteList)
			{
				ViewModel.InteractiveService.ShowMessage(
					ImportanceLevel.Warning,
					$"За водителем {driverScheduleNode.DriverFullName} " +
					$"уже закреплен маршрутный лист на {ViewModel.GetShortDayString(driverScheduleNode.Days[dayIndex].Date)}");

				args.RetVal = false;
			}
		}

		/// <summary>
		/// Горизонтальный скролл
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void OnTreeViewScroll(object sender, ScrollEventArgs args)
		{
			if(!(sender is yTreeView treeView))
			{
				return;
			}

			var state = args.Event.State;
			bool isShiftPressed = (state & Gdk.ModifierType.ShiftMask) != 0;

			if(isShiftPressed)
			{
				var scrolledWindow = treeView.Parent as ScrolledWindow;
				if(scrolledWindow == null && treeView.Parent is Viewport viewport)
				{
					scrolledWindow = viewport.Parent as ScrolledWindow;
				}

				if(scrolledWindow != null && scrolledWindow.Hadjustment != null)
				{
					var hadj = scrolledWindow.Hadjustment;

					double step = hadj.StepIncrement / 2.5;
					if(args.Event.Direction == Gdk.ScrollDirection.Up ||
						args.Event.Direction == Gdk.ScrollDirection.Left)
					{
						hadj.Value = Math.Max(hadj.Lower, hadj.Value - step);
					}
					else if(args.Event.Direction == Gdk.ScrollDirection.Down ||
							 args.Event.Direction == Gdk.ScrollDirection.Right)
					{
						hadj.Value = Math.Min(hadj.Upper - hadj.PageSize, hadj.Value + step);
					}

					args.RetVal = true;
				}
			}
		}

		// Перемещение стрелками между FixedTreeView и DynamicTreeView
		[GLib.ConnectBefore]
		private void OnFixedTreeViewKeyPress(object o, KeyPressEventArgs args)
		{
			if(args.Event.Key == Gdk.Key.Right)
			{
				var treeView = (TreeView)o;

				treeView.GetCursor(out TreePath path, out TreeViewColumn focusedColumn);

				if(path != null && focusedColumn != null)
				{
					if(!IsLastVisibleColumn(treeView, focusedColumn))
					{
						args.RetVal = false;
						return;
					}
				}

				if(ytreeviewFixedPart.Selection.CountSelectedRows() > 0)
				{
					var selectedPath = ytreeviewFixedPart.Selection.GetSelectedRows()[0];
					ytreeviewDynamicPart.Selection.SelectPath(selectedPath);
					ytreeviewDynamicPart.SetCursor(selectedPath, ytreeviewDynamicPart.Columns[0], false);
					ytreeviewDynamicPart.GrabFocus();
					args.RetVal = true;
				}
			}
		}

		// Перемещение стрелками между DynamicTreeView и FixedTreeView
		[GLib.ConnectBefore]
		private void OnDynamicTreeViewKeyPress(object o, KeyPressEventArgs args)
		{
			if(args.Event.Key == Gdk.Key.Left)
			{
				var treeView = (TreeView)o;

				treeView.GetCursor(out TreePath path, out TreeViewColumn focusedColumn);

				if(path != null && focusedColumn != null)
				{
					var columns = treeView.Columns;
					var currentColumnIndex = Array.IndexOf(columns, focusedColumn);

					if(currentColumnIndex > 0)
					{
						args.RetVal = false;
						return;
					}
				}

				if(ytreeviewDynamicPart.Selection.CountSelectedRows() > 0)
				{
					var selectedPath = ytreeviewDynamicPart.Selection.GetSelectedRows()[0];
					ytreeviewFixedPart.Selection.SelectPath(selectedPath);

					var lastVisibleColumn = GetLastVisibleColumn(ytreeviewFixedPart);
					if(lastVisibleColumn != null)
					{
						ytreeviewFixedPart.SetCursor(selectedPath, lastVisibleColumn, false);
					}

					ytreeviewFixedPart.GrabFocus();
					args.RetVal = true;
				}
			}
		}

		private void SynchronizeSelection(TreeView source, TreeView target)
		{
			if(source.Selection.CountSelectedRows() > 0)
			{
				var selectedPath = source.Selection.GetSelectedRows()[0];
				target.Selection.SelectPath(selectedPath);
			}
		}

		private void RefreshTreeViews()
		{
			ytreeviewFixedPart.SetItemsSource(ViewModel.DriverScheduleRows);
			ytreeviewDynamicPart.SetItemsSource(ViewModel.DriverScheduleRows);

			ytreeviewFixedPart.QueueDraw();
			ytreeviewDynamicPart.QueueDraw();
		}

		private bool IsColumnVisibilityProperty(string propertyName) =>
			propertyName == nameof(DriverScheduleViewModel.ShowCarTypeOfUseColumn)
			|| propertyName == nameof(DriverScheduleViewModel.ShowCarOwnTypeColumn)
			|| propertyName == nameof(DriverScheduleViewModel.ShowDriverCarOwnTypeColumn)
			|| propertyName == nameof(DriverScheduleViewModel.ShowPhoneColumn)
			|| propertyName == nameof(DriverScheduleViewModel.ShowDistrictColumn)
			|| propertyName == nameof(DriverScheduleViewModel.ShowArrivalTimeColumn)
			|| propertyName == nameof(DriverScheduleViewModel.ShowLastModifiedDateTimeColumn);

		private void ApplyFixedColumnsVisibility()
		{
			if(ytreeviewFixedPart?.Columns == null || ytreeviewFixedPart.Columns.Length < _fixedColumnsCount)
			{
				return;
			}

			ytreeviewFixedPart.Columns[_carTypeOfUseColumnIndex].Visible = ViewModel.ShowCarTypeOfUseColumn;
			ytreeviewFixedPart.Columns[_carOwnTypeColumnIndex].Visible = ViewModel.ShowCarOwnTypeColumn;
			ytreeviewFixedPart.Columns[_driverCarOwnTypeColumnIndex].Visible = ViewModel.ShowDriverCarOwnTypeColumn;
			ytreeviewFixedPart.Columns[_phoneColumnIndex].Visible = ViewModel.ShowPhoneColumn;
			ytreeviewFixedPart.Columns[_districtColumnIndex].Visible = ViewModel.ShowDistrictColumn;
			ytreeviewFixedPart.Columns[_arrivalTimeColumnIndex].Visible = ViewModel.ShowArrivalTimeColumn;
			ytreeviewFixedPart.Columns[_lastModifiedDateTimeColumnIndex].Visible = ViewModel.ShowLastModifiedDateTimeColumn;
			ytreeviewFixedPart.Columns[_totalTitleColumnIndex].Visible = !ViewModel.ShowLastModifiedDateTimeColumn;

			GLib.Idle.Add(() =>
			{
				UpdateFixedPartWidthRequest();
				return false;
			});
		}

		private static string GetTotalTitle(DriverScheduleRow row)
		{
			var totalRow = row as DriverScheduleTotalRow;
			return totalRow?.TotalTitle ?? "";
		}

		private static bool IsLastVisibleColumn(TreeView treeView, TreeViewColumn column)
		{
			return column != null && column == GetLastVisibleColumn(treeView);
		}

		private static TreeViewColumn GetLastVisibleColumn(TreeView treeView)
		{
			return treeView?.Columns?.LastOrDefault(column => column.Visible);
		}
	}
}
