using QS.Views.Dialog;
using System.ComponentModel;
using Vodovoz.ViewModels;
namespace Vodovoz
{
	[ToolboxItem(true)]
	public partial class AboutAuthorsView : DialogViewBase<AboutAuthorsViewModel>
	{
		public AboutAuthorsView(AboutAuthorsViewModel viewModel) : base(viewModel)
		{
			Build();

			HeightRequest = 450;
			WidthRequest = 350;

			ytextviewAuthors.Editable = false;
			ytextviewAuthors.Binding
				.AddBinding(ViewModel, vm => vm.Authors, w => w.Buffer.Text)
				.InitializeFromSource();

			ybuttonClose.BindCommand(ViewModel.CloseCommand);
		}
	}
}
