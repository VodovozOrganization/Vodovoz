using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Counterparties;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Clients.Nodes;
using Vodovoz.Domain.Client;
using VodovozBusiness.EntityRepositories.Counterparties;

namespace Vodovoz.Infrastructure.Persistance.Counterparties
{
	internal sealed class ConnectedCustomerRepository : IConnectedCustomerRepository
	{
		public IEnumerable<ConnectedCustomerInfoNode> GetConnectedCustomersInfo(IUnitOfWork uow, int counterpartyId, PersonType personType)
		{
			switch(personType)
			{
				case PersonType.legal:
					return GetConnectedCustomersToLegalCounterparty(uow, counterpartyId);
				case PersonType.natural:
					return GetConnectedCustomersToNaturalCounterparty(uow, counterpartyId);
			}
			
			return Enumerable.Empty<ConnectedCustomerInfoNode>();
		}

		public IEnumerable<LegalCounterpartyInfo> GetConnectedCustomers(IUnitOfWork uow, int counterpartyId)
		{
			var connectedLegalClients =
				from connectedCustomer in uow.Session.Query<ConnectedCustomer>()
				join legalCounterparty in uow.Session.Query<Counterparty>()
					on connectedCustomer.LegalCounterpartyId equals legalCounterparty.Id 
				where connectedCustomer.NaturalCounterpartyId == counterpartyId
				select new LegalCounterpartyInfo
				{
					ErpCounterpartyId = legalCounterparty.Id,
					Inn = legalCounterparty.INN,
					Kpp = legalCounterparty.KPP,
					FullName = legalCounterparty.Name,
					JurAddress = legalCounterparty.JurAddress,
					ConnectState = connectedCustomer.ConnectState.ToString()
				};

			return connectedLegalClients
				.Distinct()
				.ToList();
		}

		public ConnectedCustomer GetConnectedCustomer(IUnitOfWork uow, int legalCounterpartyId, int naturalCounterpartyId)
		{
			var connectedCustomers =
				from connectedCustomer in uow.Session.Query<ConnectedCustomer>()
				where connectedCustomer.NaturalCounterpartyId == naturalCounterpartyId
					&& connectedCustomer.LegalCounterpartyId == legalCounterpartyId
				select connectedCustomer;

			return connectedCustomers.FirstOrDefault();
		}

		private IEnumerable<ConnectedCustomerInfoNode> GetConnectedCustomersToNaturalCounterparty(IUnitOfWork uow, int counterpartyId)
		{
			var connectedCustomers =
				from connectedCustomer in uow.Session.Query<ConnectedCustomer>()
				join legalCounterparty in uow.Session.Query<Counterparty>()
					on connectedCustomer.LegalCounterpartyId equals legalCounterparty.Id 
				where connectedCustomer.NaturalCounterpartyId == counterpartyId
				select new ConnectedCustomerInfoNode
				{
					ConnectedCustomer = connectedCustomer,
					CounterpartyFullName = legalCounterparty.Name
				};

			return connectedCustomers
				.Distinct()
				.ToList();
		}
		
		private IEnumerable<ConnectedCustomerInfoNode> GetConnectedCustomersToLegalCounterparty(IUnitOfWork uow, int counterpartyId)
		{
			var connectedCustomers =
				from connectedCustomer in uow.Session.Query<ConnectedCustomer>()
				join naturalCounterparty in uow.Session.Query<Counterparty>()
					on connectedCustomer.NaturalCounterpartyId equals naturalCounterparty.Id
				where connectedCustomer.LegalCounterpartyId == counterpartyId
				select new ConnectedCustomerInfoNode
				{
					ConnectedCustomer = connectedCustomer,
					CounterpartyFullName = naturalCounterparty.Name,
				};

			return connectedCustomers
				.Distinct()
				.ToList();
		}
	}
}
