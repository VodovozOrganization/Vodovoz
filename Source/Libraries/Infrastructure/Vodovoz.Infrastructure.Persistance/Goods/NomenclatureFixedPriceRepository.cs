using System.Collections.Generic;
using System.Linq;
using NHibernate;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories.Goods;

namespace Vodovoz.Infrastructure.Persistance.Goods
{
	internal sealed class NomenclatureFixedPriceRepository : INomenclatureFixedPriceRepository
	{
		public IEnumerable<NomenclatureFixedPrice> GetEmployeesNomenclatureFixedPrices(IUnitOfWork uow)
		{
			var result = from fixedPrice in uow.Session.Query<NomenclatureFixedPrice>()
						 where fixedPrice.IsEmployeeFixedPrice
						 orderby fixedPrice.Nomenclature.Id
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

		public IEnumerable<int> GetWorkingEmployeeCounterpartiesIds(IUnitOfWork uow)
		{
			var counterpartiesIds =
				from counterparty in uow.Session.Query<Counterparty>()
				join employee in uow.Session.Query<Employee>()
					on counterparty.Id equals employee.Counterparty.Id
				where employee.Status != EmployeeStatus.IsFired && employee.Status != EmployeeStatus.OnCalculation
				select counterparty.Id;

			return counterpartiesIds.ToArray();
		}

		public IEnumerable<int> GetWorkingEmployeeCounterpartiesDeliveryPointsIds(IUnitOfWork uow)
		{
			var deliveryPointIds =
				from deliveryPoint in uow.Session.Query<DeliveryPoint>()
				join employee in uow.Session.Query<Employee>()
					on deliveryPoint.Counterparty.Id equals employee.Counterparty.Id
				where employee.Status != EmployeeStatus.IsFired && employee.Status != EmployeeStatus.OnCalculation
				select deliveryPoint.Id;

			return deliveryPointIds.ToArray();
		}

		public IReadOnlyList<NomenclatureFixedPrice> GetFixedPricesFor19LWater(IUnitOfWork uow)
		{
			Nomenclature nomenclatureAlias = null;

			var result = uow.Session.QueryOver<NomenclatureFixedPrice>()
				.JoinAlias(fp => fp.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Category == NomenclatureCategory.water)
				.And(() => nomenclatureAlias.TareVolume == TareVolume.Vol19L)
				.Fetch(SelectMode.Fetch, fp => fp.DeliveryPoint)
				.Fetch(SelectMode.Fetch, fp => fp.Counterparty);

			return result.List() as List<NomenclatureFixedPrice>;
		}
	}
}
