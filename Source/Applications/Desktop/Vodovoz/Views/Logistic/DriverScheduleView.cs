using Gamma.ColumnConfig;
using Gtk;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Logistic.DriverSchedule;

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
			ConfigureFixedColumnsTreeView();
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
