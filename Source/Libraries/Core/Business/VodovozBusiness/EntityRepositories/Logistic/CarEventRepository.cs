using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.EntityRepositories.Logistic
{
	public class CarEventRepository : ICarEventRepository
	{
		public IEnumerable<CarEvent> Get(IUnitOfWork unitOfWork, Expression<Func<CarEvent, bool>> predicate = null)
		{
			if(predicate is null)
			{
				return unitOfWork.Session.Query<CarEvent>().ToList();
			}

			return unitOfWork.Session.Query<CarEvent>()
				.Where(predicate)
				.ToList();
		}
	}
}
