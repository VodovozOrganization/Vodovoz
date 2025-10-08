using QS.DomainModel.Entity;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Employees
{
	public class EmployeeFineCategoryNode : PropertyChangedBase
	{
		public EmployeeFineCategoryNode(string fineCategoryName)
		{
			FineCategoryName = fineCategoryName;
		}

		private bool _selected;
		public virtual bool Selected
		{
			get => _selected;
			set => SetField(ref _selected, value);
		}

		public string FineCategoryName { get; }
	}
}
