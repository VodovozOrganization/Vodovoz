using Gtk;
using QS.Views.GtkUI;
using QSWidgetLib;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Edo;
using Vodovoz.ViewModels.Journals.JournalNodes.Edo;
using Key = Gdk.Key;

namespace Vodovoz.Filters.GtkViews
{
	public partial class EdoDeviationFilterView : FilterViewBase<EdoDeviationFilterViewModel>
	{
		public EdoDeviationFilterView(EdoDeviationFilterViewModel filterViewModel) : base(filterViewModel)
		{
			Build();

			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			yentryOrder.KeyReleaseEvent += OnKeyReleased;
			yentryOrder.ValidationMode = ValidationType.numeric;
			yentryOrder.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.OrderId, w => w.Text, new NullableIntToStringConverter())
				.InitializeFromSource();

			yentryEdoTask.KeyReleaseEvent += OnKeyReleased;
			yentryEdoTask.ValidationMode = ValidationType.numeric;
			yentryEdoTask.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.TaskId, w => w.Text, new NullableIntToStringConverter())
				.InitializeFromSource();

			yentrySourceId.KeyReleaseEvent += OnKeyReleased;
			yentrySourceId.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.ProblemSourceName, w => w.Text)
				.InitializeFromSource();

			datepickerDeliveryDate.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.DeliveryDateFrom, w => w.StartDateOrNull)
				.AddBinding(vm => vm.DeliveryDateTo, w => w.EndDateOrNull)
				.InitializeFromSource();

			yenumEdoRowType.ItemsEnum = typeof(EdoDeviationJournalNodeType);
			yenumEdoRowType.AddEnumToHideList(EdoDeviationJournalNodeType.Order);
			yenumEdoRowType.ShowSpecialStateAll = true;
			yenumEdoRowType.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.RowType, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			yenumEdoTaskType.ItemsEnum = typeof(EdoTaskType);
			yenumEdoTaskType.AddEnumToHideList(
				EdoTaskType.InformalOrderDocument,
				EdoTaskType.SaveCode,
				EdoTaskType.BulkAccounting,
				EdoTaskType.Withdrawal);
			yenumEdoTaskType.ShowSpecialStateAll = true;
			yenumEdoTaskType.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.EdoTaskType, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			yenumDeviationType.ItemsEnum = typeof(EdoDeviationType);
			yenumDeviationType.ShowSpecialStateAll = true;
			yenumDeviationType.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.DeviationType, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			yenumEdoTaskStatus.ItemsEnum = typeof(EdoTaskStatus);
			yenumEdoTaskStatus.ShowSpecialStateAll = true;
			yenumEdoTaskStatus.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.EdoTaskStatus, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			yenumTaskProblemState.ItemsEnum = typeof(TaskProblemState);
			yenumTaskProblemState.ShowSpecialStateAll = true;
			yenumTaskProblemState.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.State, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			ybuttonHelp.BindCommand(ViewModel.HelpCommand);
		}

		private void OnKeyReleased(object sender, KeyReleaseEventArgs args)
		{
			if(args.Event.Key == Key.Return)
			{
				ViewModel.Update();
			}
		}

		public override void Dispose()
		{
			yentryOrder.KeyReleaseEvent -= OnKeyReleased;
			yentryEdoTask.KeyReleaseEvent -= OnKeyReleased;
			yentrySourceId.KeyReleaseEvent -= OnKeyReleased;

			base.Dispose();
		}
	}
}
