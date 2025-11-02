using QS.Views.GtkUI;
using Vodovoz.ViewModels.Logistic.MileagesWriteOff;

namespace Vodovoz.Views.Logistic
{
	public partial class MileageWriteOffView : TabViewBase<MileageWriteOffViewModel>
	{
		public MileageWriteOffView(MileageWriteOffViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			yvboxMain.Sensitive = ViewModel.CanCreateOrUpdate;

			datepickerCreateDate.Binding
				.AddBinding(ViewModel.Entity, e => e.CreationDate, w => w.DateOrNull)
				.InitializeFromSource();

			datepickerWriteOffDate.Binding
				.AddBinding(ViewModel.Entity, e => e.WriteOffDate, w => w.DateOrNull)
				.InitializeFromSource();

			yspinbuttonDistance.Binding
				.AddBinding(ViewModel.Entity, e => e.DistanceKm, w => w.ValueAsDecimal)
				.InitializeFromSource();

			yspinbuttonLitersOutlayed.Binding
				.AddBinding(ViewModel.Entity, e => e.LitersOutlayed, w => w.ValueAsDecimal)
				.InitializeFromSource();

			entityentryAuthor.ViewModel = ViewModel.AuthorEntryViewModel;

			entityentryCar.ViewModel = ViewModel.CarEntryViewModel;
			entityentryCar.ViewModel.ChangedByUser += (s, e) => ViewModel.CarChangedByUserCommand.Execute();

			entityentryDriver.ViewModel = ViewModel.DriverEntryViewModel;

			entityentryWriteOffReason.ViewModel = ViewModel.WriteOffReasonEntryViewModel;
			
			ytextviewComment.Binding
				.AddBinding(ViewModel.Entity, e => e.Comment, w => w.Buffer.Text)
				.InitializeFromSource();

			ybuttonSave.BindCommand(ViewModel.SaveCommand);
			ybuttonCancel.BindCommand(ViewModel.CancelCommand);
		}
	}
}
