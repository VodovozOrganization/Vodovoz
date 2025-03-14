using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Validation
{
	public interface IValidationResultFactory<TEntity>
		where TEntity : class
	{
		ValidationResult CreateForDateNotInRange(string propertyName, DateTime minimalDate, DateTime maximalDate, DateTime? date);
		ValidationResult CreateForNullProperty(string propertyName);
		ValidationResult CreateForLeZero(string propertyName);
	}
}
