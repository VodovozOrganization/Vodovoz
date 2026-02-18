using QS.Views.Dialog;
using QSWidgetLib;
using Vodovoz.ViewModels.ViewModels.Goods;

namespace Vodovoz.Views.Goods
{
	public partial class GtinView : DialogViewBase<GtinViewModel>
	{
		public GtinView(GtinViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			validatedGtin.ValidationMode = ValidationType.numeric;
			validatedGtin.Binding.AddBinding(ViewModel.Gtin, g => g.GtinNumber, w => w.Text).InitializeFromSource();

			yspinbuttonPriority.Binding
				.AddBinding(ViewModel.Gtin, g => g.Priority, w => w.ValueAsUint)
				.InitializeFromSource();

			buttonOk.BindCommand(ViewModel.CloseCommand);
		}
	}
}
