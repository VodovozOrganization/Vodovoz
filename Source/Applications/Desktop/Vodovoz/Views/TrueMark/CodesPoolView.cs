using Gamma.GtkWidgets;
using Gdk;
using Gtk;
using QS.Views.GtkUI;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.TrueMark.CodesPool;
using static Vodovoz.ViewModels.TrueMark.CodesPool.CodesPoolViewModel;

namespace Vodovoz.Views.TrueMark
{
	public partial class CodesPoolView : TabViewBase<CodesPoolViewModel>
	{
		private readonly Color _isNotEnoughCodesColor = GdkColors.DangerBase;
		private readonly Color _baseColor = GdkColors.PrimaryBase;

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
				.AddColumn("Продано вчера").AddTextRenderer(node => node.SoldYesterday.ToString())
				.AddColumn("Нехватка по заказам").AddTextRenderer(node => node.MissingCodesInOrdersCount.ToString())
				.AddColumn("Номенклатуры").AddTextRenderer(node => node.Nomenclatures)
				.RowCells().AddSetter<CellRenderer>((cell, node) =>
				{
					cell.CellBackgroundGdk = node.IsNotEnoughCodes ? _isNotEnoughCodesColor : _baseColor;
				})
				.Finish();

			ytreeviewData.Binding
				.AddBinding(ViewModel, vm => vm.CodesPoolData, w => w.ItemsDataSource)
				.InitializeFromSource();

			ybuttonLoadCodesToPool.BindCommand(ViewModel.LoadCodesToPoolCommand);
			ybuttonRefresh.BindCommand(ViewModel.RefreshCommand);
		}
	}
}
