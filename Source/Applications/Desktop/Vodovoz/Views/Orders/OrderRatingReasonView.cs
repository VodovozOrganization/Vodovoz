using QS.Views.Dialog;
using Vodovoz.ViewModels.ViewModels.Orders;

namespace Vodovoz.Views.Orders
{
	public partial class OrderRatingReasonView : DialogViewBase<OrderRatingReasonViewModel>
	{
		public OrderRatingReasonView(OrderRatingReasonViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}
		
		private void Configure()
		{
			btnSave.BindCommand(ViewModel.SaveAndCloseCommand);
			btnCancel.BindCommand(ViewModel.CloseCommand);
				
			lblIdTitle.Binding
				.AddBinding(ViewModel, vm => vm.CanShowId, w => w.Visible)
				.InitializeFromSource();
			
			lblId.Selectable = true;
			lblId.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.IdToString, w => w.LabelProp)
				.AddBinding(vm => vm.CanShowId, w => w.Visible)
				.InitializeFromSource();
			
			entryReason.Binding
				.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.IsEditable)
				.InitializeFromSource();
			
			chkForRatingOne.Binding
				.AddBinding(ViewModel.Entity, e => e.IsForOneStarRating, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			
			chkForRatingTwo.Binding
				.AddBinding(ViewModel.Entity, e => e.IsForTwoStarRating, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			
			chkForRatingThree.Binding
				.AddBinding(ViewModel.Entity, e => e.IsForThreeStarRating, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			
			chkForRatingFour.Binding
				.AddBinding(ViewModel.Entity, e => e.IsForFourStarRating, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			
			chkForRatingFive.Binding
				.AddBinding(ViewModel.Entity, e => e.IsForFiveStarRating, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			
			chkIsArchive.Binding
				.AddBinding(ViewModel.Entity, e => e.IsArchive, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
		}
	}
}
