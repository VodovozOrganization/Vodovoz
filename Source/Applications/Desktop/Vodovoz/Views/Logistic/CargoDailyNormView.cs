using Gamma.ColumnConfig;
using Gtk;
using QS.Navigation;
using QS.Views.Dialog;
using Vodovoz.ViewModels.Dialogs.Logistic;

namespace Vodovoz.Views.Logistic
{
	public partial class CargoDailyNormView : DialogViewBase<CargoDailyNormViewModel>
	{
		public CargoDailyNormView(CargoDailyNormViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			ybuttonSave.Clicked += (sender, args) => ViewModel.SaveAndClose();
			ybuttonCancel.Clicked += (sender, args) => ViewModel.Close(false, CloseSource.Cancel);

			ytreeviewCarTypeNorms.ColumnsConfig = FluentColumnsConfig<CargoDailyNormNode>.Create()
				.AddColumn("Тип авто").AddEnumRenderer(x => x.CarTypeOfUse)
				.AddColumn("Кол-во").AddNumericRenderer(x => x.Amount)
					.Adjustment(new Adjustment(0, 0, 100000, 1, 1, 1)).Editing().Digits(2)
				.AddColumn("").AddTextRenderer(x => x.Postfix)
				.Finish();

			ytreeviewCarTypeNorms.ItemsDataSource = ViewModel.CargoDailyNormNodes;
		}
	}
}
