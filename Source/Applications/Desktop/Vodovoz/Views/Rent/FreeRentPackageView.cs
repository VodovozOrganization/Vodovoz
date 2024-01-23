using QS.Navigation;
using QS.Views.GtkUI;
using Vodovoz.Domain;
using Vodovoz.ViewModels.ViewModels.Rent;

namespace Vodovoz.Views.Rent
{
	public partial class FreeRentPackageView : TabViewBase<FreeRentPackageViewModel>
	{
		public FreeRentPackageView(FreeRentPackageViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			buttonSave.Clicked += (sender, e) => ViewModel.SaveAndClose();
			buttonCancel.Clicked += (sender, e) => ViewModel.Close(false, CloseSource.Cancel);

			dataentryName.Binding
				.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text)
				.InitializeFromSource();

			spinDeposit.Binding
				.AddBinding(ViewModel.Entity, e => e.Deposit, w => w.ValueAsDecimal)
				.InitializeFromSource();

			spinMinWaterAmount.Binding
				.AddBinding(ViewModel.Entity, e => e.MinWaterAmount, w => w.ValueAsInt)
				.InitializeFromSource();

			//entryDepositService.SetEntityAutocompleteSelectorFactory(ViewModel.DepositServiceSelectorFactory);

			entryDepositService.Binding
				.AddBinding(ViewModel.Entity, e => e.DepositService, w => w.Subject)
				.InitializeFromSource();

			referenceEquipmentType.SubjectType = typeof(EquipmentKind);
			referenceEquipmentType.Binding
				.AddBinding(ViewModel.Entity, e => e.EquipmentKind, w => w.Subject)
				.InitializeFromSource();

			ycheckbuttonArchive.Binding
				.AddBinding(ViewModel.Entity, e => e.IsArchive, w => w.Active)
				.InitializeFromSource();
		}
	}
}
