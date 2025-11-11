using System.Linq;
using Gamma.ColumnConfig;
using Gdk;
using Gtk;
using QS.Navigation;
using QS.Views.Dialog;
using RevenueService.Client.Dto;
using RevenueService.Client.Extensions;
using Vodovoz.Extensions;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.ViewModels.Counterparty;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.Views.Client
{
	[WindowSize(800, 600)]
	public partial class CounterpartyDetailsFromRevenueServiceView : DialogViewBase<CounterpartyDetailsFromRevenueServiceViewModel>
	{
		private static readonly Color _redColor = GdkColors.DangerText;

		public CounterpartyDetailsFromRevenueServiceView(CounterpartyDetailsFromRevenueServiceViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureView();
		}

		private void ConfigureView()
		{
			lblMessage.Binding
				.AddBinding(ViewModel, vm => vm.Message, w => w.LabelProp)
				.InitializeFromSource();

			btnReplaceDetails.Binding
				.AddFuncBinding(ViewModel, vm => vm.SelectedNode != null, w => w.Sensitive)
				.InitializeFromSource();

			btnReplaceDetails.Clicked += (sender, args) => ViewModel.ReplaceDetailsCommand.Execute();

			btnExportToExcel.Binding
				.AddFuncBinding(ViewModel, vm => vm.Nodes.Any(), w => w.Sensitive)
				.InitializeFromSource();

			btnExportToExcel.Clicked += (sender, args) => ViewModel.ExportToExcelCommand.Execute();

			btnCancel.Clicked += (sender, args) => ViewModel.Close(false, CloseSource.Cancel);

			ConfigureTreeDetails();
		}

		private void ConfigureTreeDetails()
		{
			treeViewDetails.ColumnsConfig = FluentColumnsConfig<CounterpartyRevenueServiceDto>.Create()
				.AddColumn("№").AddNumericRenderer(n => ViewModel.Nodes.IndexOf(n) + 1)
				.AddColumn("КПП").AddTextRenderer(n => n.Kpp)
				.AddColumn("Наименование").AddTextRenderer(n => n.Name)
				.AddColumn("ФИО").AddTextRenderer(n => n.TitlePersonFullName)
				.AddColumn("Адрес").AddTextRenderer(n => n.Address).WrapWidth(400).WrapMode(WrapMode.WordChar)
				.AddColumn("Головная/\nФилиал").AddTextRenderer(n => n.BranchTypeString)
				.AddColumn("Статус в налоговой)").AddTextRenderer(n => n.RevenueStatusName)
				.AddColumn("")
				.RowCells().AddSetter<CellRendererText>((c, n) => c.ForegroundGdk = n.IsActive ? Color.Zero : _redColor)
				.Finish();

			treeViewDetails.Binding.AddBinding(ViewModel, vm => vm.SelectedNode, w => w.SelectedRow).InitializeFromSource();

			treeViewDetails.ItemsDataSource = ViewModel.Nodes;
		}
	}
}
