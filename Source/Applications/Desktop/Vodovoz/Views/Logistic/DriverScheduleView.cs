using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using Gamma.Widgets.Additions;
using Gtk;
using QS.Views.GtkUI;
using System;
using System.Collections.Generic;
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

			ytreeviewSubdivison.ColumnsConfig = FluentColumnsConfig<SubdivisionNode>.Create()
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
				.AddColumn(" Адр У ")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.MorningAddress)
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
					.AddNumericRenderer(node => node.EveningAddress)
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
					//.AddComboRenderer(node => node.DaysByIndex[dayIndex].CarEventType) // Такая же ошибка, как и с выражением выше
					.SetDisplayFunc(x => x == null ? "Нет" : x.ShortName)
					.FillItems(ViewModel.AvailableCarEventTypes)
					.Editing()
					.XAlign(0.5f);

				/*columnsConfig.AddColumn(" Адр У ")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => (object)(ViewModel.GetDayMorningAddress(node, dayIndex) ?? 0))
					.Adjustment(new Adjustment(0, 0, 1000, 1, 10, 0))
					.Editing()
					.XAlign(0.5f);

				columnsConfig.AddColumn(" Бут У ")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => (object)(ViewModel.GetDayMorningBottles(node, dayIndex) ?? 0))
					.Adjustment(new Adjustment(0, 0, 1000, 1, 10, 0))
					.Editing()
					.XAlign(0.5f);

				columnsConfig.AddColumn(" Адр В ")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => (object)(ViewModel.GetDayEveningAddress(node, dayIndex) ?? 0))
					.Adjustment(new Adjustment(0, 0, 1000, 1, 10, 0))
					.Editing()
					.XAlign(0.5f);

				columnsConfig.AddColumn(" Бут В ")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => (object)(ViewModel.GetDayEveningBottles(node, dayIndex) ?? 0))
					.Adjustment(new Adjustment(0, 0, 1000, 1, 10, 0))
					.Editing()
					.XAlign(0.5f);*/
			}

			columnsConfig.AddColumn("").Finish();

			ytreeviewDynamicPart.ColumnsConfig = columnsConfig;

			foreach(var driver in ViewModel.DriverScheduleRows)
			{
				if(driver.DaysByIndex == null || driver.DaysByIndex.Count == 0)
				{
					var weekDays = new List<DateTime>();
					for(int i = 0; i < 7; i++)
					{
						weekDays.Add(weekStart.AddDays(i));
					}
					driver.InitializeDays(weekDays);
				}
			}

			ytreeviewDynamicPart.SetItemsSource(ViewModel.DriverScheduleRows);

			ytreeviewDynamicPart.BorderWidth = 0;
			ytreeviewDynamicPart.RulesHint = true;
			ytreeviewDynamicPart.EnableGridLines = TreeViewGridLines.Both;
			ytreeviewDynamicPart.ScrollEvent += OnTreeViewScroll;
			ytreeviewDynamicPart.KeyPressEvent += OnDynamicTreeViewKeyPress;
		}

		private Expression<Func<DriverScheduleDatasetNode, CarEventType>> GetDayCarEventTypeExpression(int dayIndex)
		{
			var param = Expression.Parameter(typeof(DriverScheduleDatasetNode), "x");

			// x.DaysByIndex
			var daysByIndex = Expression.PropertyOrField(param, nameof(DriverScheduleDatasetNode.DaysByIndex));

			// индекс (константа)
			var index = Expression.Constant(dayIndex, typeof(int));

			// indexer property "Item" у Dictionary<int, DayScheduleNode>
			var dictType = typeof(Dictionary<int, DayScheduleNode>);
			var indexer = dictType.GetProperty("Item", new Type[] { typeof(int) });
			if(indexer == null)
				throw new InvalidOperationException("Indexer property not found on Dictionary<int, DayScheduleNode>.");

			// x.DaysByIndex[dayIndex]
			var indexedAccess = Expression.Property(daysByIndex, indexer, index);

			// .CarEventType
			var carEventProp = typeof(DayScheduleNode).GetProperty(nameof(DayScheduleNode.CarEventType));
			if(carEventProp == null)
				throw new InvalidOperationException("Property CarEventType not found on DayScheduleNode.");

			var final = Expression.Property(indexedAccess, carEventProp);

			return Expression.Lambda<Func<DriverScheduleDatasetNode, CarEventType>>(final, param);
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

		/*[GLib.ConnectBefore]
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
					var driverIndex = selectedPath.Indices[0];

					// Переходим на первый день выбранного водителя в динамической части
					var firstDayPathForDriver = new TreePath(new int[] { driverIndex * 7 });

					ytreeviewDynamicPart.Selection.SelectPath(firstDayPathForDriver);
					ytreeviewDynamicPart.SetCursor(firstDayPathForDriver, ytreeviewDynamicPart.Columns[0], false);
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
					var dayRowIndex = selectedPath.Indices[0];

					// Вычисляем индекс водителя: дневной индекс / 7 дней в неделе
					var driverIndex = dayRowIndex / 7;
					var driverPath = new TreePath(new int[] { driverIndex });

					ytreeviewFixedPart.Selection.SelectPath(driverPath);
					ytreeviewFixedPart.SetCursor(driverPath, ytreeviewFixedPart.Columns[ytreeviewFixedPart.Columns.Length - 1], false);
					ytreeviewFixedPart.GrabFocus();
					args.RetVal = true;
				}
			}
		}

		private void RefreshTreeViews()
		{
			ytreeviewFixedPart.SetItemsSource(ViewModel.DriverScheduleRows);

			var rows = ViewModel.DriverScheduleRows.SelectMany(x => x.DaysSchedule).ToList();
			ytreeviewDynamicPart.SetItemsSource(rows);

			ytreeviewFixedPart.QueueDraw();
			ytreeviewDynamicPart.QueueDraw();
		}*/
	}
}
