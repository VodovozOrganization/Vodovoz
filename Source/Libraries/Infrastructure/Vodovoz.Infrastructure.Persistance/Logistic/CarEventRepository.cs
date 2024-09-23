using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.EntityRepositories.Logistic;

namespace Vodovoz.Infrastructure.Persistance.Logistic
{
	internal sealed class CarEventRepository : ICarEventRepository
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

		public CarEvent GetLastCarTechnicalCheckupEvent(IUnitOfWork uow, int carId, int technicalCheckupEventTypeId)
		{
			return
				(
					from ce in uow.Session.Query<CarEvent>()
					join c in uow.GetAll<Car>() on ce.Car.Id equals c.Id
					where c.Id == carId
						&& ce.CarEventType.Id == technicalCheckupEventTypeId
						&& ce.CarTechnicalCheckupEndingDate.HasValue
					orderby ce.CarTechnicalCheckupEndingDate descending
					select ce
				).FirstOrDefault();
		}

		public IList<CarEvent> GetCarEventsByFine(IUnitOfWork uow, int fineId)
		{
			CarEvent carEventAlias = null;
			Fine finesAlias = null;

			return uow.Session.QueryOver(() => carEventAlias)
				.JoinAlias(() => carEventAlias.Fines, () => finesAlias)
				.Where(() => finesAlias.Id == fineId)
				.List();
		}

		public IQueryable<int> GetCarEventIdsByWriteOffDocument(IUnitOfWork uow, int documentId)
		{
			return uow.Session.Query<CarEvent>()
				.Where(x => x.WriteOffDocument.Id == documentId)
				.Select(x => x.Id);
		}
	}
}
