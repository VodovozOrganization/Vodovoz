using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
namespace Vodovoz.Domain.Payments
{
	public class ProfitCategory : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		string name;
		[Display(Name = "Название дохода")]
		public virtual string Name {
			get => name;
			set => SetField(ref name, value);
		}

		public ProfitCategory() { }
	}
}
