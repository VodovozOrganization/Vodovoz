using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Factories
{
	public class ValidationContextFactory : IValidationContextFactory
	{
		public ValidationContext CreateNewValidationContext<TEntity>(TEntity entity) => new ValidationContext(entity);
		public ValidationContext CreateNewValidationContext<TEntity>(TEntity entity, IDictionary<object, object> items) =>
			new ValidationContext(entity, items);
	}
}
