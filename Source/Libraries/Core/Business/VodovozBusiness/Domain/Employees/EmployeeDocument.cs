using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Employees;

namespace Vodovoz.Domain.Employees
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "документы сотрудника",
		Nominative = "документ сотрудника")]
	[EntityPermission]
	public class EmployeeDocument: EmployeeDocumentEntity, IValidatableObject
	{
		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrEmpty(PassportSeria))
			{
				yield return new ValidationResult("Серия должна быть заполнена", new[] { "PassportSeria" });
			}

			if(string.IsNullOrEmpty(PassportNumber))
			{
				yield return new ValidationResult("Номер должен быть заполнен", new[] { "PassportNumber" });
			}
		}

	}
}
