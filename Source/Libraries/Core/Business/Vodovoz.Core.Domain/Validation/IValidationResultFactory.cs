using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Validation
{
	public interface IValidationResultFactory<TEntity>
		where TEntity : class
	{
		ValidationResult CreateForNullProperty(string propertyName);
	}
}
