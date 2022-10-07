using QS.Views.GtkUI;
using Vodovoz.Domain.Documents.DriverTerminal;
using Vodovoz.ViewModels.ViewModels.Employees;

namespace Vodovoz.Views.Employees
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TerminalManagementView : WidgetViewBase<TerminalManagementViewModel>
	{
		public TerminalManagementView()
		{
			this.Build();
		}

		protected override void ConfigureWidget()
		{
			labelTerminalInfo.Binding.AddBinding(ViewModel, vm => vm.Title, w => w.LabelProp).InitializeFromSource();
			buttonGiveout.Binding.AddFuncBinding(ViewModel,
				vm => vm.DocumentTypeToCreate == AttachedTerminalDocumentType.Giveout, w => w.Visible).InitializeFromSource();

			buttonReturn.Binding.AddFuncBinding(ViewModel,
				vm => vm.DocumentTypeToCreate == AttachedTerminalDocumentType.Return, w => w.Visible).InitializeFromSource();

			buttonGiveout.Clicked += (sender, args) => { ViewModel.GiveoutTerminal(); };
			buttonReturn.Clicked += (sender, args) => { ViewModel.ReturnTerminal(); };
		}
	}
}
