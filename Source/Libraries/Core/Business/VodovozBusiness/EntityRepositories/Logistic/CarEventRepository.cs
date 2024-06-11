using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;

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

		public CarEvent GetLastTechInspectCarEvent(IUnitOfWork uow, int carId, int techInspectCarEventTypeId)
		{
			return
				(
					from ce in uow.Session.Query<CarEvent>()
					join c in uow.GetAll<Car>() on ce.Car.Id equals c.Id
					where c.Id == carId
						&& ce.CarEventType.Id == techInspectCarEventTypeId
					orderby ce.StartDate descending
					select ce
				).FirstOrDefault();
		}

		public CarEvent GetCarEventByFine(IUnitOfWork uow, int fineId)
		{
			CarEvent carEventAlias = null;
			Fine finesAlias = null;

			return uow.Session.QueryOver(() => carEventAlias)
				.JoinAlias(() => carEventAlias.Fines, () => finesAlias)
				.Where(() => finesAlias.Id == fineId)
				.Take(1)
				.SingleOrDefault();
		}
	}
}
