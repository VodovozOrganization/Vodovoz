using QS.Views.GtkUI;
using System.ComponentModel;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ViewModels.Counterparties;

namespace Vodovoz.Dialogs.Client
{
	[ToolboxItem(true)]
	public partial class TagDlg : TabViewBase<TagViewModel>
	{
		public TagDlg(TagViewModel viewModel)
			: base(viewModel)
		{
			Initialize();
		}

		private void Initialize()
		{
			entryName1.Binding
				.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text)
				.InitializeFromSource();

			ycolorbutton.Binding
				.AddBinding(ViewModel.Entity, e => e.ColorText, w => w.Color, new ColorTextToGdkColorConverter())
				.InitializeFromSource();
		}
	}
}
