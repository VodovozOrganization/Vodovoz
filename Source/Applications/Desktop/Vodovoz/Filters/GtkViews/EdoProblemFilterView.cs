using QS.Views.GtkUI;
using QSWidgetLib;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Edo;

namespace Vodovoz.Filters.GtkViews
{
	public partial class EdoProblemFilterView : FilterViewBase<EdoProblemFilterViewModel>
	{
		public EdoProblemFilterView(EdoProblemFilterViewModel filterViewModel) : base(filterViewModel)
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
			
			yentryEdoTask.ValidationMode = ValidationType.numeric;
			yentryEdoTask.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.TaskId, w => w.Text, new NullableIntToStringConverter())
				.InitializeFromSource();
			
			yentrySourceId.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.SourceId, w => w.Text)
				.InitializeFromSource();

			datepickerDeliveryDate.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.DeliveryDateFrom, w => w.StartDateOrNull)
				.AddBinding(vm => vm.DeliveryDateTo, w => w.EndDateOrNull)
				.InitializeFromSource();
			
			yenumEdoTaskStatus.ItemsEnum = typeof(EdoTaskStatus);
			yenumEdoTaskStatus.ShowSpecialStateNot = true;
			yenumEdoTaskStatus.Binding.AddSource(ViewModel)
				.AddBinding(e => e.EdoTaskStatus, w => w.SelectedItemOrNull)
				.InitializeFromSource();
			
			yenumTaskProblemState.ItemsEnum = typeof(TaskProblemState);
			yenumTaskProblemState.ShowSpecialStateNot = true;
			yenumTaskProblemState.Binding.AddSource(ViewModel)
				.AddBinding(e => e.TaskProblemState, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			ycheckHasProblemItems.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.HasProblemItems, w => w.Active)
				.InitializeFromSource();

			ycheckHasProblemItemGtins.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.HasProblemItemGtins, w => w.Active)
				.InitializeFromSource();
		}
	}
}
