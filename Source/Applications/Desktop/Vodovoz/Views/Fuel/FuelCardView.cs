using QS.Views.GtkUI;
using Vodovoz.ViewModels.Fuel.FuelCards;
namespace Vodovoz.Views.Fuel
{
	public partial class FuelCardView : TabViewBase<FuelCardViewModel>
	{
		public FuelCardView(FuelCardViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			yvboxMain.Sensitive = ViewModel.CanCreateOrUpdate;

			yentryCardNumber.Binding
				.AddBinding(ViewModel.Entity, e => e.CardNumber, w => w.Text)
				.InitializeFromSource();
			yentryCardNumber.Changed += (s, e) => ViewModel.ResetFuelCardIdCommand.Execute();

			yentryCardId.Binding
				.AddBinding(ViewModel.Entity, e => e.CardId, w => w.Text)
				.InitializeFromSource();

			ycheckbuttonIsArchived.Binding
				.AddBinding(ViewModel.Entity, e => e.IsArchived, w => w.Active)
				.InitializeFromSource();

			ybuttonGetCardId.Binding
				.AddBinding(ViewModel, vm => vm.IsCanSetCardId, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonSave.BindCommand(ViewModel.SaveCommand);
			ybuttonCancel.BindCommand(ViewModel.CancelCommand);
			ybuttonGetCardId.BindCommand(ViewModel.GetCardIdCommand);
		}
	}
}
