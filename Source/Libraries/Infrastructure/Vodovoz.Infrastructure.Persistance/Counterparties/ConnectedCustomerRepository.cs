using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Counterparties;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Clients.Nodes;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
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

		public IEnumerable<LegalCounterpartyInfo> GetConnectedCustomers(IUnitOfWork uow, int counterpartyId, string phone)
		{
			var connectedLegalClients =
				from connectedCustomer in uow.Session.Query<ConnectedCustomer>()
				join legalCounterparty in uow.Session.Query<Counterparty>()
					on connectedCustomer.LegalCounterpartyId equals legalCounterparty.Id
				join connectedPhone in uow.Session.Query<Phone>()
					on connectedCustomer.NaturalCounterpartyPhoneId equals connectedPhone.Id
				where connectedPhone.Counterparty.Id == counterpartyId
					&& connectedPhone.DigitsNumber == phone
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

		public ConnectedCustomer GetConnectedCustomer(IUnitOfWork uow, int legalCounterpartyId, int naturalCounterpartyId, string phone)
		{
			var connectedCustomers =
				from connectedCustomer in uow.Session.Query<ConnectedCustomer>()
				join connectedPhone in uow.Session.Query<Phone>()
					on connectedCustomer.NaturalCounterpartyPhoneId equals connectedPhone.Id
				where connectedPhone.Counterparty.Id == naturalCounterpartyId
					&& connectedPhone.DigitsNumber == phone
					&& connectedCustomer.LegalCounterpartyId == legalCounterpartyId
				select connectedCustomer;

			return connectedCustomers.FirstOrDefault();
		}

		public ConnectedCustomer GetConnectedCustomer(IUnitOfWork uow, int legalCounterpartyId, int phoneId)
		{
			var connectedCustomers =
				from connectedCustomer in uow.Session.Query<ConnectedCustomer>()
				join connectedPhone in uow.Session.Query<Phone>()
					on connectedCustomer.NaturalCounterpartyPhoneId equals connectedPhone.Id
				where connectedPhone.Id == phoneId
				      && connectedCustomer.LegalCounterpartyId == legalCounterpartyId
				select connectedCustomer;

			return connectedCustomers.FirstOrDefault();
		}

		public IEnumerable<PhoneInfo> GetConnectedCustomerPhones(IUnitOfWork uow, int legalCounterpartyId, int naturalCounterpartyId)
		{
			var connectedCustomerPhones =
				from connectedCustomer in uow.Session.Query<ConnectedCustomer>()
				join connectedPhone in uow.Session.Query<Phone>()
					on connectedCustomer.NaturalCounterpartyPhoneId equals connectedPhone.Id
				where connectedCustomer.LegalCounterpartyId == legalCounterpartyId
				select new PhoneInfo
				{
					ErpPhoneId = connectedPhone.Id,
					Number = $"+7{ connectedPhone.DigitsNumber }",
					ConnectState = connectedCustomer.ConnectState.ToString()
				};

			return connectedCustomerPhones.ToList();
		}

		private IEnumerable<ConnectedCustomerInfoNode> GetConnectedCustomersToNaturalCounterparty(IUnitOfWork uow, int counterpartyId)
		{
			var connectedCustomers =
				from connectedCustomer in uow.Session.Query<ConnectedCustomer>()
				join legalCounterparty in uow.Session.Query<Counterparty>()
					on connectedCustomer.LegalCounterpartyId equals legalCounterparty.Id
				join connectedPhone in uow.Session.Query<Phone>()
					on connectedCustomer.NaturalCounterpartyPhoneId equals connectedPhone.Id
				where connectedPhone.Counterparty.Id == counterpartyId
				select new ConnectedCustomerInfoNode
				{
					ConnectedCustomer = connectedCustomer,
					CounterpartyFullName = legalCounterparty.Name,
					CounterpartyId = legalCounterparty.Id,
					PhoneNumber = connectedPhone.Number
				};

			return connectedCustomers
				.Distinct()
				.ToList();
		}
		
		private IEnumerable<ConnectedCustomerInfoNode> GetConnectedCustomersToLegalCounterparty(IUnitOfWork uow, int counterpartyId)
		{
			var connectedCustomers =
				from connectedCustomer in uow.Session.Query<ConnectedCustomer>()
				join connectedPhone in uow.Session.Query<Phone>()
					on connectedCustomer.NaturalCounterpartyPhoneId equals connectedPhone.Id
				join naturalCounterparty in uow.Session.Query<Counterparty>()
					on connectedPhone.Counterparty.Id equals naturalCounterparty.Id
				where connectedCustomer.LegalCounterpartyId == counterpartyId
				select new ConnectedCustomerInfoNode
				{
					ConnectedCustomer = connectedCustomer,
					CounterpartyFullName = naturalCounterparty.Name,
					CounterpartyId = naturalCounterparty.Id,
					PhoneNumber = connectedPhone.Number
				};

			return connectedCustomers
				.Distinct()
				.ToList();
		}
	}
}
