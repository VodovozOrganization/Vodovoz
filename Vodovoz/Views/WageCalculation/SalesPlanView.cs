using QS.Views.GtkUI;
using Vodovoz.ViewModels.WageCalculation;

namespace Vodovoz.Views.WageCalculation
{
	public partial class SalesPlanView : TabViewBase<SalesPlanViewModel>
	{
		public SalesPlanView(SalesPlanViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureWidget();
		}

		protected void ConfigureWidget()
		{
			labelName.Visible = yentryName.Visible = false;
			btnSave.Clicked += (sender, e) => ViewModel.SaveAndClose();
			btnCancel.Clicked += (sender, e) => ViewModel.Close(false);
			yentryName.Binding.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text).InitializeFromSource();
			chkIsArchive.Binding.AddBinding(ViewModel.Entity, s => s.IsArchive, w => w.Active).InitializeFromSource();
			entryFullBottlesToSell.Binding.AddBinding(ViewModel.Entity, e => e.FullBottleToSell, w => w.ValueAsInt).InitializeFromSource();
			entryEmptyBottlesToTake.Binding.AddBinding(ViewModel.Entity, e => e.EmptyBottlesToTake, w => w.ValueAsInt).InitializeFromSource();
		}
	}
}