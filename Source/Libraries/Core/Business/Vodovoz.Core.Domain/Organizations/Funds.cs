using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Core.Domain.Organizations
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "Формы денежных средств",
		Nominative = "Форма денежных средств",
		GenitivePlural = "Формы денежных средств")]
	[EntityPermission]
	[HistoryTrace]
	public class Funds : PropertyChangedBase, INamedDomainObject
	{
		private string _name;
		private AccountFillType _defaultAccountFillType;
		private bool _isArchive;

		public virtual int Id { get; set; }

		[Display(Name = "Название")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}
		
		[Display(Name = "Архивная")]
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField (ref _isArchive, value);
		}
		
		[Display(Name = "Заполнение данных по расчетному счету по умолчанию")]
		public virtual AccountFillType DefaultAccountFillType
		{
			get => _defaultAccountFillType;
			set => SetField (ref _defaultAccountFillType, value);
		}
	}
}
