using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Employees
{
	/// <summary>
	/// Категория штрафов
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		Accusative = "категорию штрафов",
		AccusativePlural = "категории штрафов",
		Genitive = "категории штрафов",
		GenitivePlural = "категорий штрафов",
		Nominative = "категория штрафов",
		NominativePlural = "категории штрафов",
		Prepositional = "категории штрафов",
		PrepositionalPlural = "категориях штрафов")]
	[EntityPermission]
	[HistoryTrace]
	public class FineCategory : PropertyChangedBase, INamedDomainObject, IArchivable, IValidatableObject
	{
		private int _id;
		private string _name;
		private bool _isArchive;

		/// <summary>
		/// Идентификатор
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
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		/// <summary>
		/// Архивная
		/// </summary>
		[Display(Name = "Архивная")]
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrEmpty(Name))
			{
				yield return new ValidationResult(
					"Название должно быть заполнено.", new[] { nameof(Name) });
			}
			if(!string.IsNullOrEmpty(Name) && Name.Length > 127)
			{
				yield return new ValidationResult(
					"Длина названия не должна превышать 127 символов.", new[] { nameof(Name) });
			}
		}
	}
}
