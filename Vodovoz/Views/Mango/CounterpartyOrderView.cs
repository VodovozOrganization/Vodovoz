using System;
using Gamma.GtkWidgets;
using QS.Views;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModels.Mango;

namespace Vodovoz.Views.Mango
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CounterpartyOrderView : ViewBase<CounterpartyOrderViewModel>
	{
		public CounterpartyOrderView(CounterpartyOrderViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}
		void Configure()
		{
			OrderYTreeView.ColumnsConfig = ColumnsConfigFactory.Create<Order>()
			.AddColumn("Номер")
			.AddNumericRenderer(order => order.Id)
			.AddColumn("Дата")
			.AddTextRenderer(order => order.DeliveryDate.HasValue ? order.DeliveryDate.Value.ToString("dd.MM.yy") : String.Empty)
			.AddColumn("Статус")
			.AddTextRenderer(order => order.OrderStatus.ToString())
			.AddColumn("Адрес")
			.AddTextRenderer(order => order.DeliveryPoint.CompiledAddress)
			.Finish();

			CounterpartyYButton.Clicked+= PressEvent_CounterpartyYButton;
			OrderYTreeView.RowActivated += SelectCursorRow_OrderYTreeView;

			CounterpartyYButton.Label = ViewModel.Client.Name;
			CommYEntry.Text = ViewModel.Client.Comment;


			OrderYTreeView.SetItemsSource<Order>(ViewModel.LatestOrder);
		}
		#region Events
		private void PressEvent_CounterpartyYButton(object sender , EventArgs e)
		{
			ViewModel.OpenMoreInformationAboutCounterparty();
		}

		private void SelectCursorRow_OrderYTreeView(object sender, EventArgs e)
		{
			var row = OrderYTreeView.GetSelectedObject<Order>();
			ViewModel.OpenMoreInformationAboutOrder(row.Id);
		}
		#endregion
	}
}