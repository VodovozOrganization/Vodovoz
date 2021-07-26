using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Factories
{
	public class ValidationContextFactory : IValidationContextFactory
	{
		public ValidationContext CreateNewValidationContext<TEntity>(TEntity entity) => new ValidationContext(entity);
	}
}