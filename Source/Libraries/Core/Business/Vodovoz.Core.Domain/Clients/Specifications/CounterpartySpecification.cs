using Core.Infrastructure.Specifications;
using System;
using System.Linq.Expressions;

namespace Vodovoz.Core.Domain.Clients.Specifications
{
	public class CounterpartySpecification : ExpressionSpecification<CounterpartyEntity>
	{
		public CounterpartySpecification(Expression<Func<CounterpartyEntity, bool>> expression) : base(expression)
		{
		}

		public static CounterpartySpecification ById(int id)
		{
			return new CounterpartySpecification(x => x.Id == id);
		}

		public static CounterpartySpecification NonEmptyInn()
		{
			return new CounterpartySpecification(x => !string.IsNullOrWhiteSpace(x.INN));
		}
	}
}
