using QS.Views.GtkUI;
using System.ComponentModel;
using Vodovoz.ViewModels.Counterparties;

namespace Vodovoz.Counterparties
{
	[ToolboxItem(true)]
	public partial class SubtypeView : TabViewBase<SubtypeViewModel>
	{
		public SubtypeView(SubtypeViewModel viewModel)
			: base(viewModel)
		{
			Build();

			Initialize();
		}

		private void Initialize()
		{
			ylabelIdValue.Binding
				.AddBinding(ViewModel.Entity, e => e.Id, w => w.Text)
				.InitializeFromSource();

			yentryName.Binding
				.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text)
				.InitializeFromSource();

			buttonSave.Clicked += (s, e) => ViewModel.SaveCommand.Execute();
			buttonCancel.Clicked += (s, e) => ViewModel.CloseCommand.Execute();
		}
	}
}
