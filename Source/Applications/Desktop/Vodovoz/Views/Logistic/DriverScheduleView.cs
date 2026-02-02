using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using Gamma.Widgets.Additions;
using Gtk;
using QS.Views.GtkUI;
using System;
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
		public DriverScheduleView(DriverScheduleViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();

			ViewModel.PropertyChanged += (sender, e) =>
			{
				if(e.PropertyName == nameof(DriverScheduleViewModel.DriverScheduleRows))
				{
					RefreshTreeViews();
				}
			};
		}

		private void Configure()
		{
			leftsidepanel5.Panel = yvboxFilters;

			var weekPicker = new MonthPickerView(ViewModel.WeekPickerViewModel);
			weekPicker.Show();
			ViewModel.WeekPickerViewModel.DateEntryWidthRequest = -1;
			yhboxWeek.Add(weekPicker);

			buttonSave.Binding
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			buttonSave.BindCommand(ViewModel.SaveCommand);

			buttonCancel.Clicked += (sender, e) => ViewModel.Close(ViewModel.AskSaveOnClose, QS.Navigation.CloseSource.Cancel);
			buttonCancel.BindCommand(ViewModel.CancelCommand);

			ybuttonExport.BindCommand(ViewModel.ExportlCommand);
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

			ytreeviewSubdivison.ColumnsConfig = FluentColumnsConfig<DriverScheduleNode>.Create()
				.AddColumn("Подразделение").AddTextRenderer(x => x.SubdivisionName)
				.AddColumn("").AddToggleRenderer((x => x.Selected))
				.Finish();

			ytreeviewSubdivison.Binding
				.AddBinding(ViewModel, x => x.Subdivisions, x => x.ItemsDataSource)
				.InitializeFromSource();

			ybuttonApplyFilters.BindCommand(ViewModel.ApplyFiltersCommand);

			ConfigureFixedTreeView();
			ConfigureDynamicTreeView();
		}

		private void ConfigureFixedTreeView()
		{
			var columnsConfig = FluentColumnsConfig<DriverScheduleDatasetNode>.Create()
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
					.AddComboRenderer(node => node.DeliverySchedule)
					.SetDisplayFunc(x => x.Name)
					.FillItems(ViewModel.AvailableDeliverySchedules)
					.Editing()
					.XAlign(0.5f)
				.AddColumn("Адр У")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.MorningAddress)
					.Adjustment(new Adjustment(0, 0, 1000, 1, 1, 1))
					.Editing()
					.XAlign(0.5f)
				.AddColumn("Бут У")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.MorningBottles)
					.Adjustment(new Adjustment(0, 0, 1000, 1, 1, 1))
					.Editing()
					.XAlign(0.5f)
				.AddColumn("Адр В")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.EveningAddress)
					.Adjustment(new Adjustment(0, 0, 1000, 1, 1, 1))
					.Editing()
					.XAlign(0.5f)
				.AddColumn("Бут В")
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
				//.AddColumn("")
				.Finish();

			ytreeviewFixedPart.ColumnsConfig = columnsConfig;
			ytreeviewFixedPart.SetItemsSource(ViewModel.DriverScheduleRows);

			ytreeviewFixedPart.BorderWidth = 0;
			ytreeviewFixedPart.RulesHint = true;
			ytreeviewFixedPart.EnableGridLines = TreeViewGridLines.Both;
			ytreeviewFixedPart.KeyPressEvent += OnFixedTreeViewKeyPress;
		}

		private void ConfigureDynamicTreeView()
		{
			var weekStart = GetStartOfWeek(DateTime.Today);
			var columnsConfig = FluentColumnsConfig<DriverScheduleDatasetNode>.Create();

			for(int dayIndex = 0; dayIndex < 7; dayIndex++)
			{
				var date = weekStart.AddDays(dayIndex);

				columnsConfig.AddColumn($"{GetShortDayString(date)}")
					.HeaderAlignment(0.5f)
					.AddComboRenderer(GetDayCarEventTypeExpression(dayIndex))
					.SetDisplayFunc(x => x == null ? "Нет" : x.ShortName)
					.FillItems(ViewModel.AvailableCarEventTypes)
					.Editing()
					.XAlign(0.5f);

				columnsConfig.AddColumn("АдрУ")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(GetDayMorningAddressExpression(dayIndex))
					.Adjustment(new Adjustment(0, 0, 1000, 1, 10, 0))
					.Editing()
					.XAlign(0.5f);

				columnsConfig.AddColumn("БутУ")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(GetDayMorningBottlesExpression(dayIndex))
					.Adjustment(new Adjustment(0, 0, 1000, 1, 10, 0))
					.Editing()
					.XAlign(0.5f);

				columnsConfig.AddColumn("АдрВ")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(GetDayEveningAddressExpression(dayIndex))
					.Adjustment(new Adjustment(0, 0, 1000, 1, 10, 0))
					.Editing()
					.XAlign(0.5f);

				columnsConfig.AddColumn("БутВ")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(GetDayEveningBottlesExpression(dayIndex))
					.Adjustment(new Adjustment(0, 0, 1000, 1, 10, 0))
					.Editing()
					.XAlign(0.5f);
			}

			columnsConfig
				.AddColumn("Комментарии")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.Comment)
					.XAlign(0.5f)
				.AddColumn("")
				.Finish();

			ytreeviewDynamicPart.ColumnsConfig = columnsConfig;
			ytreeviewDynamicPart.SetItemsSource(ViewModel.DriverScheduleRows);

			ytreeviewDynamicPart.BorderWidth = 0;
			ytreeviewDynamicPart.RulesHint = true;
			ytreeviewDynamicPart.EnableGridLines = TreeViewGridLines.Both;
			ytreeviewDynamicPart.ScrollEvent += OnTreeViewScroll;
			ytreeviewDynamicPart.KeyPressEvent += OnDynamicTreeViewKeyPress;
		}

		private string FormatDayCarEventType(DriverScheduleDatasetNode node, int dayIndex)
		{
			var eventType = GetDayCarEventType(node, dayIndex);
			return eventType == null ? "Нет" : eventType.ShortName;
		}

		private CarEventType GetDayCarEventType(DriverScheduleDatasetNode node, int dayIndex)
		{
			switch(dayIndex)
			{
				case 0: return node.MondayCarEventType;
				case 1: return node.TuesdayCarEventType;
				case 2: return node.WednesdayCarEventType;
				case 3: return node.ThursdayCarEventType;
				case 4: return node.FridayCarEventType;
				case 5: return node.SaturdayCarEventType;
				case 6: return node.SundayCarEventType;
				default: return null;
			}
		}

		private Expression<Func<DriverScheduleDatasetNode, CarEventType>> GetDayCarEventTypeExpression(int dayIndex)
		{
			switch(dayIndex)
			{
				case 0: return x => x.MondayCarEventType;
				case 1: return x => x.TuesdayCarEventType;
				case 2: return x => x.WednesdayCarEventType;
				case 3: return x => x.ThursdayCarEventType;
				case 4: return x => x.FridayCarEventType;
				case 5: return x => x.SaturdayCarEventType;
				case 6: return x => x.SundayCarEventType;
				default: throw new ArgumentOutOfRangeException(nameof(dayIndex));
			}
		}

		private Expression<Func<DriverScheduleDatasetNode, object>> GetDayMorningAddressExpression(int dayIndex)
		{
			switch(dayIndex)
			{
				case 0: return x => (object)x.MondayMorningAddress;
				case 1: return x => (object)x.TuesdayMorningAddress;
				case 2: return x => (object)x.WednesdayMorningAddress;
				case 3: return x => (object)x.ThursdayMorningAddress;
				case 4: return x => (object)x.FridayMorningAddress;
				case 5: return x => (object)x.SaturdayMorningAddress;
				case 6: return x => (object)x.SundayMorningAddress;
				default: throw new ArgumentOutOfRangeException(nameof(dayIndex));
			}
		}

		private Expression<Func<DriverScheduleDatasetNode, object>> GetDayMorningBottlesExpression(int dayIndex)
		{
			switch(dayIndex)
			{
				case 0: return x => (object)x.MondayMorningBottles;
				case 1: return x => (object)x.TuesdayMorningBottles;
				case 2: return x => (object)x.WednesdayMorningBottles;
				case 3: return x => (object)x.ThursdayMorningBottles;
				case 4: return x => (object)x.FridayMorningBottles;
				case 5: return x => (object)x.SaturdayMorningBottles;
				case 6: return x => (object)x.SundayMorningBottles;
				default: throw new ArgumentOutOfRangeException(nameof(dayIndex));
			}
		}

		private Expression<Func<DriverScheduleDatasetNode, object>> GetDayEveningAddressExpression(int dayIndex)
		{
			switch(dayIndex)
			{
				case 0: return x => (object)x.MondayEveningAddress;
				case 1: return x => (object)x.TuesdayEveningAddress;
				case 2: return x => (object)x.WednesdayEveningAddress;
				case 3: return x => (object)x.ThursdayEveningAddress;
				case 4: return x => (object)x.FridayEveningAddress;
				case 5: return x => (object)x.SaturdayEveningAddress;
				case 6: return x => (object)x.SundayEveningAddress;
				default: throw new ArgumentOutOfRangeException(nameof(dayIndex));
			}
		}

		private Expression<Func<DriverScheduleDatasetNode, object>> GetDayEveningBottlesExpression(int dayIndex)
		{
			switch(dayIndex)
			{
				case 0: return x => (object)x.MondayEveningBottles;
				case 1: return x => (object)x.TuesdayEveningBottles;
				case 2: return x => (object)x.WednesdayEveningBottles;
				case 3: return x => (object)x.ThursdayEveningBottles;
				case 4: return x => (object)x.FridayEveningBottles;
				case 5: return x => (object)x.SaturdayEveningBottles;
				case 6: return x => (object)x.SundayEveningBottles;
				default: throw new ArgumentOutOfRangeException(nameof(dayIndex));
			}
		}

		private DateTime GetStartOfWeek(DateTime date)
		{
			int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
			return date.AddDays(-diff).Date;
		}

		private string GetShortDayString(DateTime date)
		{
			string dayOfWeek;
			switch(date.DayOfWeek)
			{
				case DayOfWeek.Monday:
					dayOfWeek = "Пн";
					break;
				case DayOfWeek.Tuesday:
					dayOfWeek = "Вт";
					break;
				case DayOfWeek.Wednesday:
					dayOfWeek = "Ср";
					break;
				case DayOfWeek.Thursday:
					dayOfWeek = "Чт";
					break;
				case DayOfWeek.Friday:
					dayOfWeek = "Пт";
					break;
				case DayOfWeek.Saturday:
					dayOfWeek = "Сб";
					break;
				case DayOfWeek.Sunday:
					dayOfWeek = "Вс";
					break;
				default:
					dayOfWeek = "";
					break;
			}

			return $"{dayOfWeek}, {date:dd.MM.yyyy}";
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

					double step = hadj.StepIncrement;
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
					var columns = treeView.Columns;
					var currentColumnIndex = Array.IndexOf(columns, focusedColumn);

					if(currentColumnIndex < columns.Length - 1)
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
					ytreeviewFixedPart.SetCursor(selectedPath, ytreeviewFixedPart.Columns[ytreeviewFixedPart.Columns.Length - 1], false);
					ytreeviewFixedPart.GrabFocus();
					args.RetVal = true;
				}
			}
		}

		private void RefreshTreeViews()
		{
			ytreeviewFixedPart.SetItemsSource(ViewModel.DriverScheduleRows);
			ytreeviewDynamicPart.SetItemsSource(ViewModel.DriverScheduleRows);

			ytreeviewFixedPart.QueueDraw();
			ytreeviewDynamicPart.QueueDraw();
		}
	}
}
