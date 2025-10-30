using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Core.Domain.Payments
{
	/// <summary>
	/// Контрагенты, платежи от которых не подпадают под распределение
	/// </summary>
	[EntityPermission]
	[HistoryTrace]
	[
		Appellative(
			Gender = GrammaticalGender.Masculine,
			NominativePlural = "контрагенты, которые не участвуют в распределении",
			Nominative = "контрагент, который не участвует в распределении",
			Accusative = "контрагента, который не участвует в распределении",
			Genitive = "контрагента, который не участвует в распределении",
			GenitivePlural = "контрагентов, которые не участвуют в распределени"
		)
	]
	public class NotAllocatedCounterparty : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private const int _innMaxLenght = 12;
		private string _inn;
		private string _name;
		private bool _isArchive;
		private ProfitCategory _profitCategory;

		public virtual int Id { get; set; }

		/// <summary>
		/// Инн контрагента
		/// </summary>
		[Display(Name = "Инн контрагента")]
		public virtual string Inn
		{
			get => _inn;
			set => SetField(ref _inn, value);
		}
		
		/// <summary>
		/// Название
		/// </summary>
		[Display(Name = "Название")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}
		
		/// <summary>
		/// Инн контрагента
		/// </summary>
		[Display(Name = "Архивный")]
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}
		
		/// <summary>
		/// Категория прихода, выставляемая в функционале загрузки платежей по умолчанию
		/// </summary>
		[Display(Name = "Категория прихода")]
		public virtual ProfitCategory ProfitCategory
		{
			get => _profitCategory;
			set => SetField(ref _profitCategory, value);
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(ProfitCategory is null)
			{
				yield return new ValidationResult("Необходимо заполнить категорию прихода");
			}
			
			if(string.IsNullOrWhiteSpace(Inn))
			{
				yield return new ValidationResult("Необходимо заполнить инн контрагента");
			}
			else if(Inn.Length > _innMaxLenght)
			{
				yield return new ValidationResult($"Инн контрагента не может быть больше {_innMaxLenght}");
			}
		}
	}
}
