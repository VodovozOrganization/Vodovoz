using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Orders
{
	/// <summary>
	/// Интернет-магазин
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Masculine,
        NominativePlural = "интернет-магазины",
        Nominative = "интернет-магазин",
        Prepositional = "интернет-магазине",
        PrepositionalPlural = "интернет-магазинах"
    )]
    public class OnlineStore : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private string _name;

		/// <summary>
		/// Код
		/// </summary>
		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Название
		/// </summary>
		[Display(Name = "Название")]
		public virtual string Name {
            get => _name;
            set => SetField(ref _name, value);
        }
    }
}
