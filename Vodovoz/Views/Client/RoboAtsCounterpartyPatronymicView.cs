using QS.Navigation;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Contacts;
using Vodovoz.ViewModels.ViewModels.Counterparty;

namespace Vodovoz.Views.Client
{
	public partial class RoboAtsCounterpartyPatronymicView : TabViewBase<RoboAtsCounterpartyPatronymicViewModel>
	{
		public RoboAtsCounterpartyPatronymicView(RoboAtsCounterpartyPatronymicViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			yentryPatronymic.Binding.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text).InitializeFromSource();
			yentryAccent.Binding.AddBinding(ViewModel.Entity, e => e.Accent, w => w.Text).InitializeFromSource();

			buttonSave.Clicked += (sender, args) => ViewModel.SaveAndClose();
			buttonCancel.Clicked += (sender, args) => ViewModel.Close(true, CloseSource.Cancel);
		}
	}
}
