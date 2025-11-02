using QS.Views.GtkUI;
using QSWidgetLib;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Edo;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EdoProcessFilterView : FilterViewBase<EdoProcessFilterViewModel>
	{
		public EdoProcessFilterView(EdoProcessFilterViewModel filterViewModel) : base(filterViewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			yentryOrder.ValidationMode = ValidationType.numeric;
			yentryOrder.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.OrderId, w => w.Text, new NullableIntToStringConverter())
				.InitializeFromSource();

			datepickerDeliveryDate.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.DeliveryDateFrom, w => w.StartDateOrNull)
				.AddBinding(vm => vm.DeliveryDateTo, w => w.EndDateOrNull)
				.InitializeFromSource();

			yenumRequestSource.ItemsEnum = typeof(CustomerEdoRequestSource);
			yenumRequestSource.HiddenItems = new object[] { CustomerEdoRequestSource.None };
			yenumRequestSource.ShowSpecialStateNot = true;
			yenumRequestSource.Binding.AddSource(ViewModel)
				.AddBinding(e => e.RequestSource, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			yenumEdoTaskType.ItemsEnum = typeof(EdoTaskType);
			yenumEdoTaskType.ShowSpecialStateNot = true;
			yenumEdoTaskType.Binding.AddSource(ViewModel)
				.AddBinding(e => e.EdoTaskType, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			yenumEdoTaskStatus.ItemsEnum = typeof(EdoTaskStatus);
			yenumEdoTaskStatus.ShowSpecialStateNot = true;
			yenumEdoTaskStatus.Binding.AddSource(ViewModel)
				.AddBinding(e => e.EdoTaskStatus, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			yenumDocumentStage.ItemsEnum = typeof(DocumentEdoTaskStage);
			yenumDocumentStage.ShowSpecialStateNot = true;
			yenumDocumentStage.Binding.AddSource(ViewModel)
				.AddBinding(e => e.DocumentTaskStage, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			yenumReceiptStage.ItemsEnum = typeof(EdoReceiptStatus);
			yenumReceiptStage.ShowSpecialStateNot = true;
			yenumReceiptStage.Binding.AddSource(ViewModel)
				.AddBinding(e => e.ReceiptTaskStage, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			ycheckAllTransfersComplete.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.AllTransfersComplete, w => w.Active)
				.InitializeFromSource();

			ycheckHasProblemTransfers.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.HasTransferProblem, w => w.Active)
				.InitializeFromSource();
		}
	}
}
