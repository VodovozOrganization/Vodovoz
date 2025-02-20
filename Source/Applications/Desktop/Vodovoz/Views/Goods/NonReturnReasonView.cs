using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Goods;

namespace Vodovoz.Views.Goods
{
	public partial class NonReturnReasonView : TabViewBase<NonReturnReasonViewModel>
	{
		public NonReturnReasonView(NonReturnReasonViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			yentryName.Binding.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text).InitializeFromSource();
			ycheckbuttonNeedForfeit.Binding.AddBinding(ViewModel.Entity, e => e.NeedForfeit, w => w.Active).InitializeFromSource();

			buttonSave.BindCommand(ViewModel.SaveCommand);
			buttonCancel.BindCommand(ViewModel.CloseCommand);
		}
	}
}
