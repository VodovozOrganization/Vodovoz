using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Factories
{
	public interface IValidationContextFactory
	{
		ValidationContext CreateNewValidationContext<TEntity>(TEntity entity);
		ValidationContext CreateNewValidationContext<TEntity>(TEntity entity, IDictionary<object, object> items);
	}
}
