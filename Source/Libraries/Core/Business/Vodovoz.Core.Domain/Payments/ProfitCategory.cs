using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Repositories;

namespace Vodovoz.Core.Domain.Payments
{
	/// <summary>
	/// Категория дохода
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "категории доходов",
		Nominative = "категория дохода",
		Genitive = "категорию дохода",
		GenitivePlural = "категорий доходов",
		Prepositional = "категории дохода",
		PrepositionalPlural = "категориях доходов")]
	[EntityPermission]
	[HistoryTrace]
	public class ProfitCategory : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private string _name;
		private bool _isArchive;

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

		/// <summary>
		/// Архивный
		/// </summary>
		[Display(Name = "Архивный")]
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			var uowFactory = validationContext.GetRequiredService<IUnitOfWorkFactory>();
			var profitCategoryRepository = validationContext.GetRequiredService<IGenericRepository<ProfitCategory>>();

			if(string.IsNullOrWhiteSpace(Name))
			{
				yield return new ValidationResult("Наименование должно быть заполнено");
			}
		}
	}
}
