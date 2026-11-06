using QS.Views.GtkUI;
using Vodovoz.ViewModels.Journals.FilterViewModels.Edo;
using Vodovoz.ViewModels.Journals.JournalNodes.Edo;

namespace Vodovoz.Filters.GtkViews
{
	/// <summary>
	/// Фильтр журнала сохранённых УПД.
	/// </summary>
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EdoUpdFilterView : FilterViewBase<EdoUpdFilterViewModel>
	{
		/// <summary>Создаёт представление фильтра.</summary>
		/// <param name="viewModel">Модель фильтра.</param>
		public EdoUpdFilterView(EdoUpdFilterViewModel viewModel) : base(viewModel)
		{
			Build();
			dateRange.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.DateFrom, widget => widget.StartDateOrNull)
				.AddBinding(vm => vm.DateTo, widget => widget.EndDateOrNull).InitializeFromSource();
			yenumDocumentType.ItemsEnum = typeof(EdoUpdDocumentType);
			yenumDocumentType.ShowSpecialStateNot = true;
			yenumDocumentType.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.DocumentType, widget => widget.SelectedItemOrNull).InitializeFromSource();
			yentryInn.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CustomerInn, widget => widget.Text).InitializeFromSource();
		}
	}
}
