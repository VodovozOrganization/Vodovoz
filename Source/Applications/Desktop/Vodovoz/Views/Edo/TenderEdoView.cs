using Gamma.ColumnConfig;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Edo;

namespace Vodovoz.Views.Edo
{
	public partial class TenderEdoView : TabViewBase<TenderEdoViewModel>
	{
		public TenderEdoView(TenderEdoViewModel viewModel)
			: base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			ylabelGeneratedInfo.Binding
				.AddBinding(ViewModel, vm => vm.Info, w => w.Text)
				.InitializeFromSource();

			ytreeViewCodes.ColumnsConfig = FluentColumnsConfig<string>.Create()
				.AddColumn("Код")
				.AddTextRenderer(c => c)
				.AddColumn("")
				.Finish();

			ytreeViewCodes.ItemsDataSource = ViewModel.Codes;

			ybuttonExportCodes.BindCommand(ViewModel.ExportCodesCommand);
		}
	}
}
