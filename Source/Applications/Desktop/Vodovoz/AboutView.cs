using System.ComponentModel;
using QS.Views.Dialog;
using Vodovoz.ViewModels;

namespace Vodovoz
{
	[ToolboxItem(true)]
	public partial class AboutView : DialogViewBase<AboutViewModel>
	{
		public AboutView(AboutViewModel viewModel)
			: base(viewModel)
		{
			Build();

			ylabelProgramName.Binding
				.AddBinding(ViewModel, vm => vm.ProgramNameFormatted, w => w.LabelProp)
				.InitializeFromSource();

			ylabelDescription.Binding
				.AddBinding(ViewModel, vm => vm.Description, w => w.LabelProp)
				.InitializeFromSource();

			ylabelWikiWebsite.Binding
				.AddBinding(ViewModel, vm => vm.WikiWebsiteFormatted, w => w.LabelProp)
				.InitializeFromSource();

			ylabelCopyright.Binding
				.AddBinding(ViewModel, vm => vm.Copyright, w => w.LabelProp)
				.InitializeFromSource();

			ylabelWebsite.Binding
				.AddBinding(ViewModel, vm => vm.WebsiteFormatted, w => w.LabelProp)
				.InitializeFromSource();

			ybuttonAuthors.BindCommand(ViewModel.OpenAuthorsCommand);
			ybuttonClose.BindCommand(ViewModel.CloseCommand);
		}
	}
}
