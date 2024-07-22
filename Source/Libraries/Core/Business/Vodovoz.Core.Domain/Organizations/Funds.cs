using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;

namespace Vodovoz.Core.Domain.Organizations
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "Формы денежных средств",
		Nominative = "Форма денежных средств",
		GenitivePlural = "Формы денежных средств")]
	[EntityPermission]
	public class Funds : PropertyChangedBase, INamedDomainObject
	{
		private string _name;
		private AccountFillType _defaultAccountFillType;

		public virtual int Id { get; set; }

		[Display(Name = "Название")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}
		
		[Display(Name = "Заполнение данных по расчетному счету по умолчанию")]
		public virtual AccountFillType DefaultAccountFillType
		{
			get => _defaultAccountFillType;
			set => SetField (ref _defaultAccountFillType, value);
		}
	}
}
