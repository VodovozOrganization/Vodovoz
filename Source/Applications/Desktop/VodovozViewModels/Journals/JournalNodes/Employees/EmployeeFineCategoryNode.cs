using Gamma.Utilities;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Employees;
using Gamma.Utilities;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Employees
{
	public class EmployeeFineCategoryNode : PropertyChangedBase
	{
		public EmployeeFineCategoryNode(FineCategory finecategory)
		{
			FineCategory = finecategory;
		}

		private bool _selected;
		public virtual bool Selected
		{
			get => _selected;
			set => SetField(ref _selected, value);
		}

		public FineCategory? FineCategory { get; }

		public string FineCategoryName 
		{ 
			get => FineCategory != null 
				? FineCategory.GetEnumTitle() 
				: ""; 
		}
	}
}
