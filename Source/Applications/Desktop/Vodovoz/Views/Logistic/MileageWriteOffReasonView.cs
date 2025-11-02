using QS.Views.GtkUI;
using Vodovoz.ViewModels.Logistic.MileagesWriteOff;
namespace Vodovoz.Views.Logistic
{
	public partial class MileageWriteOffReasonView : TabViewBase<MileageWriteOffReasonViewModel>
	{
		public MileageWriteOffReasonView(MileageWriteOffReasonViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			yvboxMain.Sensitive = ViewModel.CanCreateOrUpdate;

			ycheckbuttonIsArchived.Binding
				.AddBinding(ViewModel.Entity, vm => vm.IsArchived, w => w.Active)
				.InitializeFromSource();

			yentryName.Binding
				.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text)
				.InitializeFromSource();

			ytextviewDescription.Binding
				.AddBinding(ViewModel.Entity, e => e.Description, w => w.Buffer.Text)
				.InitializeFromSource();

			ybuttonSave.BindCommand(ViewModel.SaveCommand);

			ybuttonCancel.BindCommand(ViewModel.CancelCommand);
		}
	}
}
