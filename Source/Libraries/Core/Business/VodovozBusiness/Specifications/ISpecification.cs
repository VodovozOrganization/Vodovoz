using System;
using System.Linq.Expressions;

namespace Vodovoz.Specifications
{
	public interface ISpecification<TEntity>
	{
		Expression <Func<TEntity,bool>> IsSatisfiedBy();
	}
}
