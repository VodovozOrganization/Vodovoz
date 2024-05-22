using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;

namespace Vodovoz.EntityRepositories.Goods
{
	public class NomenclatureFixedPriceRepository : INomenclatureFixedPriceRepository
	{
		
		public IEnumerable<NomenclatureFixedPrice> GetEmployeesNomenclatureFixedPrices(IUnitOfWork uow)
		{
			var result = from fixedPrice in uow.Session.Query<NomenclatureFixedPrice>()
				where fixedPrice.IsEmployeeFixedPrice
				select fixedPrice;
			
			return result.ToArray();
		}
		
		public IEnumerable<NomenclatureFixedPrice> GetEmployeesNomenclatureFixedPricesByCounterpartyId(IUnitOfWork uow, int counterpartyId)
		{
			var counterpartiesFixedPrices =
				from fixedPrice in uow.Session.Query<NomenclatureFixedPrice>()
				join counterparty in uow.Session.Query<Counterparty>()
					on fixedPrice.Counterparty.Id equals counterparty.Id
				join employee in uow.Session.Query<Employee>()
					on counterparty.Id equals employee.Counterparty.Id
				where counterparty.Id == counterpartyId
				select fixedPrice;

			return counterpartiesFixedPrices.ToArray();
		}

		public IEnumerable<NomenclatureFixedPrice> GetAllFixedPricesFromEmployeeCounterparties(IUnitOfWork uow)
		{
			var counterpartiesFixedPrices =
				from fixedPrice in uow.Session.Query<NomenclatureFixedPrice>()
				join counterparty in uow.Session.Query<Counterparty>()
					on fixedPrice.Counterparty.Id equals counterparty.Id
				join employee in uow.Session.Query<Employee>()
					on counterparty.Id equals employee.Counterparty.Id
				select fixedPrice;

			return counterpartiesFixedPrices.ToArray();
		}
		
		public IEnumerable<NomenclatureFixedPrice> GetAllFixedPricesFromEmployeeCounterpartiesDeliveryPoints(IUnitOfWork uow)
		{
			var deliveryPointFixedPrices =
				from fixedPrice in uow.Session.Query<NomenclatureFixedPrice>()
				join deliveryPoint in uow.Session.Query<DeliveryPoint>()
					on fixedPrice.DeliveryPoint.Id equals deliveryPoint.Id
				join employee in uow.Session.Query<Employee>()
					on deliveryPoint.Counterparty.Id equals employee.Counterparty.Id
				select fixedPrice;

			return deliveryPointFixedPrices.ToArray();
		}
		
		public IEnumerable<int> GetEmployeeCounterpartiesIds(IUnitOfWork uow)
		{
			var counterpartiesIds =
				from counterparty in uow.Session.Query<Counterparty>()
				join employee in uow.Session.Query<Employee>()
					on counterparty.Id equals employee.Counterparty.Id
				select counterparty.Id;

			return counterpartiesIds.ToArray();
		}
		
		public IEnumerable<int> GetEmployeeCounterpartiesDeliveryPointsIds(IUnitOfWork uow)
		{
			var deliveryPointIds =
				from deliveryPoint in uow.Session.Query<DeliveryPoint>()
				join employee in uow.Session.Query<Employee>()
					on deliveryPoint.Counterparty.Id equals employee.Counterparty.Id
				select deliveryPoint.Id;

			return deliveryPointIds.ToArray();
		}
		
		public IReadOnlyList<NomenclatureFixedPrice> GetFixedPricesFor19LWater(IUnitOfWork uow)
		{
			var result =
				from fixedPrice in uow.Session.Query<NomenclatureFixedPrice>()
				join nomenclature in uow.Session.Query<Nomenclature>()
					on fixedPrice.Nomenclature.Id equals nomenclature.Id
					where nomenclature.Category == NomenclatureCategory.water
						&& nomenclature.TareVolume == TareVolume.Vol19L
				select fixedPrice;

			return result.ToList();
		}
	}
}
