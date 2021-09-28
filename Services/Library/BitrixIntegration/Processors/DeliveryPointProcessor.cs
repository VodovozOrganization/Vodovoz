using Bitrix.DTO;
using QS.DomainModel.UoW;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Common;

namespace BitrixIntegration.Processors
{
	public class DeliveryPointProcessor : IDeliveryPointProcessor
	{
		public DeliveryPoint ProcessDeliveryPoint(IUnitOfWork uow, Deal deal, Counterparty counterparty)
		{
			//ЗАГЛУШКА, ПОКА НЕ БУДЕТ РЕАЛИЗОВАН ТОЧНЫЙ АДРЕСС В БИТРИКС
			return counterparty.DeliveryPoints.FirstOrDefault();

			//Парсинг координат
			Coordinate coordinate = Coordinate.Parse(deal.Coordinates);
		}
	}
}
