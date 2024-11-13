using Gamma.GtkWidgets;
using QS.Views;
using System.ComponentModel;
using Vodovoz.Domain.Client;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ViewModels.ReportsParameters.Bottles;

namespace Vodovoz.ReportsParameters.Bottles
{
	public partial class ClientsByDeliveryPointCategoryAndActivityKindsReport : ViewBase<ClientsByDeliveryPointCategoryAndActivityKindsReportViewModel>
	{
		private readonly ClientsByDeliveryPointCategoryAndActivityKindsReportViewModel _viewModel;

		public ClientsByDeliveryPointCategoryAndActivityKindsReport(ClientsByDeliveryPointCategoryAndActivityKindsReportViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		void ConfigureDlg()
		{
			specCmbDeliveryPointCategory.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.DeliveryPointCategories, w => w.ItemsList)
				.AddBinding(vm => vm.DeliveryPointCategory, w => w.SelectedItem)
				.InitializeFromSource();

			enumCmbPaymentType.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.PaymentTypeType, w => w.ItemsEnum)
				.AddBinding(vm => vm.PaymentType, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			specCmbSubstring.SetRenderTextFunc<CounterpartyActivityKind>(x => x.Name);
			specCmbSubstring.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.ActivityKinds, w => w.ItemsList)
				.AddBinding(vm => vm.ActivityKind, w => w.SelectedItem)
				.InitializeFromSource();

			yTreeSubstrings.ColumnsConfig = ColumnsConfigFactory.Create<SubstringToSearch>()
				.AddColumn("Выбрать").AddToggleRenderer(n => n.Selected).Editing()
				.AddColumn("Название").AddTextRenderer(n => n.Substring)
				.Finish();

			srlWinSubstrings.Visible = !ViewModel.SubstringsVisible;
			yTreeSubstrings.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.SubstringsToSearch, w => w.ItemsDataSource)
				.AddBinding(vm => vm.SubstringsVisible, w => w.Visible, new BooleanInvertedConverter())
				.InitializeFromSource();

			yEntSubstring.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.SubstringsVisible, w => w.Visible)
				.AddBinding(vm => vm.SubstringToSearch, w => w.Text)
				.InitializeFromSource();


			dtrngPeriod.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			buttonRun.BindCommand(ViewModel.GenerateReportCommand);

			ViewModel.PropertyChanged += ViewModelPropertyChanged;
		}

		private void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(ViewModel.SubstringsVisible):
					srlWinSubstrings.Visible = !ViewModel.SubstringsVisible;
					break;
				default:
					break;
			}
		}
	}
}
