using QS.Views.GtkUI;
using System.ComponentModel;
using Gamma.Utilities;
using Vodovoz.ViewModels.AdministrationTools;
using static Vodovoz.ViewModels.AdministrationTools.RevenueServiceMassCounterpartyUpdateToolViewModel;

namespace Vodovoz.AdministrationTools
{
	[ToolboxItem(true)]
	public partial class RevenueServiceMassCounterpartyUpdateToolView : TabViewBase<RevenueServiceMassCounterpartyUpdateToolViewModel>
	{
		public RevenueServiceMassCounterpartyUpdateToolView(RevenueServiceMassCounterpartyUpdateToolViewModel viewModel)
			: base(viewModel)
		{
			Build();

			Initialize();
		}

		private void Initialize()
		{
			datePeriodPicker.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			ConfigureTreeView();

			ybuttonSearch.Clicked += (s, e) => ViewModel.SearchCounterpartiesCommand.Execute();
			ybuttonUpdateCounterparties.Clicked += OnYbuttonUpdateCounterpartiesClicked;
		}

		private void OnYbuttonUpdateCounterpartiesClicked(object sender, System.EventArgs e)
		{
			Gtk.Application.Invoke((s, _) =>
			{
				ViewModel.UpdateCounterpartiesRevenueServiceInformationCommand.Execute();
				ViewModel.ShowLastErrorsAndClearCommand.Execute();
			});
		}

		private void ConfigureTreeView()
		{
			ytreeview1.CreateFluentColumnsConfig<CounterpartyUpdateRow>()
				.AddColumn("✔️")
				.AddToggleRenderer(x => x.Selected)
				.AddColumn("Наименование")
				.AddTextRenderer(x => x.Name)
				.AddColumn("ИНН")
				.AddTextRenderer(x => x.INN)
				.AddColumn("КПП")
				.AddTextRenderer(x => x.KPP)
				.AddColumn("Архивный")
				.AddToggleRenderer(x => x.IsArchive).Editing(false)
				.AddColumn("Статус в ФНС")
				.AddTextRenderer(x => x.RevenueStatus == null ? "Неизвестно" : x.RevenueStatus.GetEnumTitle())
				.AddColumn("Дата последней продажи")
				.AddDateRenderer(x => x.LastSale)
				.AddColumn("")
				.Finish();

			ytreeview1.ItemsDataSource = ViewModel.CounterpartiesRows;
		}
	}
}
