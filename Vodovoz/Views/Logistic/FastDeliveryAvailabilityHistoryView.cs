using Gamma.ColumnConfig;
using QS.Views.GtkUI;
using Vodovoz.Domain.Logistic.FastDelivery;
using Vodovoz.ViewModels.ViewModels.Logistic;
using Vodovoz.ViewWidgets.Logistics;

namespace Vodovoz.Views.Logistic
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class FastDeliveryAvailabilityHistoryView : TabViewBase<FastDeliveryAvailabilityHistoryViewModel>
	{
		public FastDeliveryAvailabilityHistoryView(FastDeliveryAvailabilityHistoryViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			var fastDeliveryVerificationView = new FastDeliveryVerificationView(ViewModel.FastDeliveryVerificationViewModel);
			fastDeliveryVerificationView.ShowAll();
			/*yhboxVerification.PackStart(fastDeliveryVerificationView, true, true, 0);

			ytextviewLogisticiaComment.Binding.AddBinding(ViewModel.Entity, e => e.LogisticianComment, w => w.Buffer.Text)
				.InitializeFromSource();

			ybuttonSaveLogisticiaComment.Clicked += (sender, args) => ViewModel.SaveLogisticiaCommentCommand.Execute();*/

			ConfigureOrderItems();
		}

		private void ConfigureOrderItems()
		{
			/*treeViewNomenclatures.ColumnsConfig = FluentColumnsConfig<FastDeliveryOrderItemsHistory>.Create()
				.AddColumn("Товар").HeaderAlignment(0.5f).AddTextRenderer(x => x.Nomenclature.Name)
				.AddColumn("Кол-во").MinWidth(75).HeaderAlignment(0.5f).AddNumericRenderer(node => node.Count)
				.Finish();

			treeViewNomenclatures.ItemsDataSource = ViewModel.Entity.OrderItemsHistoryItems;*/
		}
	}
}
