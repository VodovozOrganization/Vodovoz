using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Core.Domain.Organizations
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Расчетные счета банковских выписок",
		Nominative = "Расчетный счет банковской выписки",
		GenitivePlural = "Расчетных счетов банковской выписки")]
	[EntityPermission]
	[HistoryTrace]
	public class BusinessAccount : PropertyChangedBase, INamedDomainObject, IValidatableObject
	{
		private const int _numberMaxChars = 45;
		private string _name;
		private string _number;
		private string _bank;
		private int? _subdivisionId;
		private bool _isArchive;
		private BusinessActivity _businessActivity;
		private Funds _funds;
		private AccountFillType _accountFillType;

		public virtual int Id { get; set; }

		[Display(Name = "Название")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}
		
		[Display(Name = "Номер")]
		public virtual string Number
		{
			get => _number;
			set => SetField (ref _number, value);
		}

		[Display(Name = "Банк")]
		public virtual string Bank
		{
			get => _bank;
			set => SetField (ref _bank, value);
		}
		
		[Display(Name = "Id подразделения для получения баланса")]
		public virtual int? SubdivisionId
		{
			get => _subdivisionId;
			set => SetField (ref _subdivisionId, value);
		}
		
		[Display(Name = "Архивный")]
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField (ref _isArchive, value);
		}

		[Display(Name = "Направление деятельности")]
		public virtual BusinessActivity BusinessActivity
		{
			get => _businessActivity;
			set => SetField (ref _businessActivity, value);
		}
		
		[Display(Name = "Форма денежных средств")]
		public virtual Funds Funds
		{
			get => _funds;
			set => SetField (ref _funds, value);
		}
		
		[Display(Name = "Заполнение")]
		public virtual AccountFillType AccountFillType
		{
			get => _accountFillType;
			set => SetField (ref _accountFillType, value);
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Funds is null)
			{
				yield return new ValidationResult("Нужно выбрать форму денежных средств");
			}
			
			if(BusinessActivity is null)
			{
				yield return new ValidationResult("Направление деятельности должно быть заполнено");
			}

			if(!string.IsNullOrWhiteSpace(Number) && Number.Length > _numberMaxChars)
			{
				yield return new ValidationResult(
					$"Номер расчетного счета не может быть больше {_numberMaxChars}. Сейчас превышение на {Number.Length - _numberMaxChars}");
			}
		}
	}
}
