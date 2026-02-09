using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using Gamma.Widgets.Additions;
using Gtk;
using QS.Dialog;
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
					ConfigureDynamicTreeView();
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

			ConfigureFixedTreeView();
			ConfigureDynamicTreeView();
		}

		private void ConfigureFixedTreeView()
		{
			var columnsConfig = FluentColumnsConfig<DriverScheduleNode>.Create()
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
			var weekStart = ViewModel.StartDate;
			var columnsConfig = FluentColumnsConfig<DriverScheduleNode>.Create();

			var dayNames = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

			for(int dayIndex = 0; dayIndex < 7; dayIndex++)
			{
				var date = weekStart.AddDays(dayIndex);
				var dayName = dayNames[dayIndex];

				columnsConfig.AddColumn($"{ViewModel.GetShortDayString(date)}")
					.HeaderAlignment(0.5f)
					.AddComboRenderer(CreatePropertyExpression<DriverScheduleNode, CarEventType>($"{dayName}CarEventType"))
					.SetDisplayFunc(x => x == null ? "Нет" : x.ShortName)
					.FillItems(ViewModel.AvailableCarEventTypes)
					.Editing()
					.EditedEvent(OnDayComboEdited)
					.XAlign(0.5f);

				columnsConfig.AddColumn(" Адр У ")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(ConvertToObjectExpression<DriverScheduleNode, int>($"{dayName}MorningAddress"))
					.Adjustment(new Adjustment(0, 0, 1000, 1, 10, 0))
					.Editing()
					.XAlign(0.5f);

				columnsConfig.AddColumn(" Бут У ")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(ConvertToObjectExpression<DriverScheduleNode, int>($"{dayName}MorningBottles"))
					.Adjustment(new Adjustment(0, 0, 1000, 1, 10, 0))
					.Editing()
					.XAlign(0.5f);

				columnsConfig.AddColumn(" Адр В ")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(ConvertToObjectExpression<DriverScheduleNode, int>($"{dayName}EveningAddress"))
					.Adjustment(new Adjustment(0, 0, 1000, 1, 10, 0))
					.Editing()
					.XAlign(0.5f);

				columnsConfig.AddColumn(" Бут В ")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(ConvertToObjectExpression<DriverScheduleNode, int>($"{dayName}EveningBottles"))
					.Adjustment(new Adjustment(0, 0, 1000, 1, 10, 0))
					.Editing()
					.XAlign(0.5f);
			}

			columnsConfig.AddColumn("Комментарий")
				.HeaderAlignment(0.5f)
				.AddTextRenderer(node => node.Comment)
				.Editable()
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

			if(!(node is DriverScheduleNode driverScheduleNode))
			{
				return;
			}

			if(driverScheduleNode is DriverScheduleTotalAddressesRow ||
			   driverScheduleNode is DriverScheduleTotalBottlesRow)
			{
				args.RetVal = false;
				return;
			}

			if(driverScheduleNode.HasActiveRouteList)
			{
				ViewModel.InteractiveService.ShowMessage(
					ImportanceLevel.Warning,
					$"За водителем {driverScheduleNode.DriverFullName} " +
					$"уже закреплен маршрутный лист");

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
