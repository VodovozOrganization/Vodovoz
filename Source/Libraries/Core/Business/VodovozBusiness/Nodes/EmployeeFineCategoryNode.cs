using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Employees;

namespace VodovozBusiness.Nodes
{
	public class EmployeeFineCategoryNode : PropertyChangedBase
	{
		public EmployeeFineCategoryNode(FineCategory fineCategory)
		{
			FineCategory = fineCategory;
		}

		private bool _selected;
		public virtual bool Selected
		{
			get => _selected;
			set => SetField(ref _selected, value);
		}
		public FineCategory FineCategory { get; }
		public string FineCategoryName { get => FineCategory.Name; }
		public int FineCategoryId { get => FineCategory.Id; }
	}
}
