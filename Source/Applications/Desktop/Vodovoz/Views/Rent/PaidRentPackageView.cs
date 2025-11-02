using QS.Navigation;
using QS.Views.GtkUI;
using Vodovoz.Domain;
using Vodovoz.ViewModels.ViewModels.Rent;

namespace Vodovoz.Views.Rent
{
	public partial class PaidRentPackageView : TabViewBase<PaidRentPackageViewModel>
	{
		public PaidRentPackageView(PaidRentPackageViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			buttonSave.Clicked += (sender, args) => ViewModel.SaveAndClose();
			buttonCancel.Clicked += (sender, args) => ViewModel.Close(false, CloseSource.Cancel);
			
			dataentryName.Binding.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text).InitializeFromSource();
			spinDeposit.Binding.AddBinding(ViewModel.Entity, e => e.Deposit, w => w.ValueAsDecimal).InitializeFromSource();
			spinPriceDaily.Binding.AddBinding(ViewModel.Entity, e => e.PriceDaily, w => w.ValueAsDecimal).InitializeFromSource();
			spinPriceMonthly.Binding.AddBinding(ViewModel.Entity, e => e.PriceMonthly, w => w.ValueAsDecimal).InitializeFromSource();

			entryDepositServiceNomenclature.ViewModel = ViewModel.DepositServiceNomenclatureViewModel;
			entryDailyRentServiceNomenclature.ViewModel = ViewModel.DailyRentServiceNomenclatureViewModel;
			entryLongTermRentServiceNomenclature.ViewModel = ViewModel.LongTermRentNomenclatureServiceViewModel;

			referenceEquipmentType.SubjectType = typeof(EquipmentKind);
			referenceEquipmentType.Binding.AddBinding(ViewModel.Entity, e => e.EquipmentKind, w => w.Subject).InitializeFromSource();
		}
	}
}
