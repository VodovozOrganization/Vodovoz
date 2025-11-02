using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace Vodovoz.Core.Domain.Payments
{
	/// <summary>
	/// Категория дохода
	/// </summary>
	public class ProfitCategory : PropertyChangedBase, IDomainObject
	{
		private string _name;

		public virtual int Id { get; set; }

		/// <summary>
		/// Название
		/// </summary>
		[Display(Name = "Название дохода")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}
	}
}
