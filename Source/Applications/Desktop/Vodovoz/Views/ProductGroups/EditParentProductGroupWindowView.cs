using QS.Project.Dialogs.GtkUI;
using QS.Views.Dialog;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Representations;
using Vodovoz.ViewModels.ViewModels.Goods;

namespace Vodovoz.Views.ProductGroups
{
	public partial class EditParentProductGroupWindowView : DialogViewBase<EditParentProductGroupWindowViewModel>
	{
		public EditParentProductGroupWindowView(EditParentProductGroupWindowViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			btnMove.Clicked += (sender, e) => ViewModel.MoveToParentGroupCommand.Execute();

			entryParentProductGroup.JournalButtons = Buttons.None;
			entryParentProductGroup.RepresentationModel = new ProductGroupVM(ViewModel.UoW, new ProductGroupFilterViewModel());
			entryParentProductGroup.Binding.AddBinding(ViewModel, vm => vm.ParentProductGroup, w => w.Subject).InitializeFromSource();
		}
	}
}
