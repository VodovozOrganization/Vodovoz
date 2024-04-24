using QS.Navigation;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Orders;
namespace Vodovoz.Views.Orders
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class UndeliveryView : TabViewBase<UndeliveryViewModel>
	{

		public UndeliveryView(UndeliveryViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureWidget();
		}

		private void ConfigureWidget()
		{
			//если недовоз новый, то не можем оставлять комментарии
			if(ViewModel.Entity.Id > 0)
			{
				vboxDicussions.Add(new UndeliveryDiscussionsView(ViewModel.UndeliveryDiscussionsViewModel));
				vboxDicussions.ShowAll();
			}
			else
			{
				hpanedUndelivery.Position = 0;
			}

			undeliveredOrderView.WidgetViewModel = ViewModel.UndeliveredOrderViewModel;

			buttonSave.Clicked += (s, e) => ViewModel.SaveAndClose();
			buttonCancel.Clicked += (s, e) => ViewModel.Close(true, CloseSource.Cancel);
		}
	}
}
