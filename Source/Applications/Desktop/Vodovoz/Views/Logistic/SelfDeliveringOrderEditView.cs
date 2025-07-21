using QS.Views.GtkUI;
using Vodovoz.ViewModels.Logistic;
namespace Vodovoz.Views.Logistic
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SelfDeliveringOrderEditView : TabViewBase<SelfDeliveringOrderEditViewModel>
	{
		public SelfDeliveringOrderEditView(SelfDeliveringOrderEditViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
		}
	}
}
