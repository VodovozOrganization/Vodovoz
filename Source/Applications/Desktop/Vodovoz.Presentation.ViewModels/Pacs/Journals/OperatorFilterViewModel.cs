using QS.Project.Filter;

namespace Vodovoz.Presentation.ViewModels.Pacs.Journals
{
	public partial class OperatorFilterViewModel : FilterViewModelBase<OperatorFilterViewModel>
	{
		private OperatorIsWorkingFilteringModeEnum _operatorIsWorkingFilteringMode;

		public OperatorIsWorkingFilteringModeEnum OperatorIsWorkingFilteringMode
		{
			get => _operatorIsWorkingFilteringMode;
			set => UpdateFilterField(ref _operatorIsWorkingFilteringMode, value);
		}
	}
}
