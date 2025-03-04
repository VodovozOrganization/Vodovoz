using QS.Views.Dialog;
using QSWidgetLib;
using Vodovoz.ViewModels.ViewModels.Goods;

namespace Vodovoz.Views.Goods
{
	public partial class GroupGtinView : DialogViewBase<GroupGtinViewModel>
	{
		public GroupGtinView(GroupGtinViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			validatedGtin.ValidationMode = ValidationType.numeric;
			validatedGtin.Binding
				.AddBinding(ViewModel.GroupGtin, g => g.GtinNumber, w => w.Text)
				.InitializeFromSource();

			yspinbuttonCount.Binding
				.AddBinding(ViewModel.GroupGtin, g => g.CodesCount, w => w.ValueAsInt)
				.InitializeFromSource();

			buttonOk.BindCommand(ViewModel.CloseCommand);
		}
	}
}
