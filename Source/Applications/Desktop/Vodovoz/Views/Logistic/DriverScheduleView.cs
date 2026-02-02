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

		private void ConfigureFixedColumnsTreeView()
		{
			while(ytreeview1.Columns.Length > 0)
			{
				ytreeview1.RemoveColumn(ytreeview1.Columns[0]);
			}

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
				/*.AddColumn("Район")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.District.Name ?? "")*/
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
					.XAlign(0.5f)
				.AddColumn("")
				.Finish();

			ytreeview1.ColumnsConfig = columnsConfig;

			ytreeview1.SetItemsSource(ViewModel.Drivers);

			ytreeview1.RulesHint = true; // Чередование строк
			ytreeview1.EnableGridLines = TreeViewGridLines.Both; // Сетка

			// Настройка сортировки (по умолчанию по ФИО)
			ytreeview1.Columns[3].Clickable = true;
			ytreeview1.Columns[3].SortColumnId = 0;
		}
	}
}
