using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Factories
{
	public interface IValidationContextFactory
	{
		ValidationContext CreateNewValidationContext<TEntity>(TEntity entity);
	}
}