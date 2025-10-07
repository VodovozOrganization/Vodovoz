using Gamma.Utilities;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Employees;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Employees
{
	public class EmployeeFineCategoryNode : PropertyChangedBase
	{
		private bool _selected;
		public virtual bool Selected
		{
			get => _selected;
			set => SetField(ref _selected, value);
		}

		public FineCategory FineCategory { get; }

		public string Title => FineCategory.GetEnumTitle();

		public EmployeeFineCategoryNode(FineCategory finecategory)
		{
			FineCategory = finecategory;
		}
	}
}
