using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
namespace Vodovoz.Domain.Payments
{
	public class CategoryExpense : PropertyChangedBase
	{
		public virtual int Id { get; set; }

		string name;
		[Display(Name = "Название расхода")]
		public virtual string Name {
			get => name;
			set => SetField(ref name, value);
		}

		public CategoryExpense() { }
	}
}
