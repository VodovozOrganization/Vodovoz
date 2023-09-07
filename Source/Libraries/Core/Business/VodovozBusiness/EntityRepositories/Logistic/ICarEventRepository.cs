using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.EntityRepositories.Logistic
{
	public interface ICarEventRepository
	{
		IEnumerable<CarEvent> Get(IUnitOfWork unitOfWork, Expression<Func<CarEvent, bool>> predicate = null);
	}
}