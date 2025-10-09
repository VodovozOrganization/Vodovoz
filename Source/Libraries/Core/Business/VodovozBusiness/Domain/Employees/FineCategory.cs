using QS.DomainModel.Entity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Employees;

namespace Vodovoz.Domain.Employees
{
	public class FineCategory : FineCategoryEntity, IArchivable, IValidatableObject
	{
		/*[Display(Name = "Административный")]
		Administrative,
		[Display(Name = "ГИБДД")]
		GIBDD,
		[Display(Name = "Кассовая дисциплина")]
		CashDiscipline,
		[Display(Name = "Ущерб компании")]
		CompanyDamage*/

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrEmpty(Name))
			{
				yield return new ValidationResult(
					"Название должно быть заполнено.", new[] { nameof(Name) });
			}
		}
	}
}
