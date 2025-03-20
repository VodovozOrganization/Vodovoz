using Gamma.GtkWidgets;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.TrueMark.CodesPool;
using static Vodovoz.ViewModels.TrueMark.CodesPool.CodesPoolViewModel;
namespace Vodovoz.Views.TrueMark
{
	public partial class CodesPoolView : TabViewBase<CodesPoolViewModel>
	{
		public CodesPoolView(CodesPoolViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			ytreeviewData.ColumnsConfig = ColumnsConfigFactory.Create<CodesPoolDataNode>()
				.AddColumn("GTIN").AddTextRenderer(node => node.Gtin)
				.AddColumn("Количество в пуле").AddTextRenderer(node => node.CountInPool.ToString())
				.AddColumn("Продано вчера").AddTextRenderer(node => node.SoldYesterdayCount.ToString())
				.AddColumn("Номенклатуры").AddTextRenderer(node => node.NomenclatureName)
				.Finish();

			ybuttonLoadCodesToPool.BindCommand(ViewModel.LoadCodesToPoolCommand);
			ybutton2.BindCommand(ViewModel.RefreshCommand);
		}
	}
}
