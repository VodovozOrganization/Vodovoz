using NPOI.SS.Formula.Functions;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using System;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Orders;
using Vodovoz.Settings.Nomenclature;

namespace VodovozBusiness.Factories.Edo
{
	public class OrderUpdOperationFactory
	{
		private readonly IUnitOfWork _uow;
		private readonly IGenericRepository<NomenclatureEntity> _nomenclatureRepository;
		private readonly INomenclatureSettings _nomenclatureSettings;

		public OrderUpdOperationFactory(
			IUnitOfWork uow,
			IGenericRepository<NomenclatureEntity> nomenclatureRepository,
			INomenclatureSettings nomenclatureSettings)
		{
			_uow = uow ?? throw new System.ArgumentNullException(nameof(uow));
			_nomenclatureRepository = nomenclatureRepository ?? throw new System.ArgumentNullException(nameof(nomenclatureRepository));
			_nomenclatureSettings = nomenclatureSettings ?? throw new System.ArgumentNullException(nameof(nomenclatureSettings));
		}

		public static OrderUpdOperation CreateOrderUpdOperation(Order order)
		{
			if(order.DeliveryDate is null)
			{
				throw new InvalidOperationException("Нельзя сделать УПД без даты заказа");
			}

			if(order.Client.PersonType == PersonType.natural)
			{
				throw new InvalidOperationException("Нельзя сделать УПД для физического лица");
			}

			var client = order.Client;
			var contract = order.Contract;
			var deliveryPoint = order.DeliveryPoint;
			var organization = contract.Organization;

			var orderUpdOperation = new OrderUpdOperation();

			orderUpdOperation.OrderId = order.Id;
			orderUpdOperation.OrderDeliveryDate = order.DeliveryDate.Value;
			orderUpdOperation.CounterpartyExternalOrderId = order.CounterpartyExternalOrderId;
			orderUpdOperation.IsOrderForOwnNeeds = client.ReasonForLeaving == ReasonForLeaving.ForOwnNeeds;

			orderUpdOperation.ClientContractNumber = default(int);
			orderUpdOperation.ClientContractDate = default(DateTime);

			orderUpdOperation.ClientName = client.UseSpecialDocFields && !string.IsNullOrWhiteSpace(client.SpecialCustomer) ? client.SpecialCustomer : client.FullName;
			orderUpdOperation.ClientAddress = client.JurAddress;
			orderUpdOperation.ClientInn = client.INN;
			orderUpdOperation.ClientKpp = client.UseSpecialDocFields && !string.IsNullOrWhiteSpace(client.PayerSpecialKPP) ? client.PayerSpecialKPP : client.KPP;

			orderUpdOperation.ConsigneeName = client.FullName;
			orderUpdOperation.ConsigneeInn = client.INN;

			switch(client.CargoReceiverSource)
			{
				case CargoReceiverSource.FromDeliveryPoint:
					orderUpdOperation.ConsigneeAddress = deliveryPoint != null ? deliveryPoint.ShortAddress : client.JurAddress;
					orderUpdOperation.ConsigneeKpp = deliveryPoint?.KPP ?? client.KPP;
					orderUpdOperation.ConsigneeSummary = string.Empty;
					break;
				default:
					break;
			}

		orderUpdOperation.UseSpecialDocFields = default(bool);
			orderUpdOperation.SpecialCargoReceiver = default(string);
			orderUpdOperation.SpecialCustomerName = default(string);
			orderUpdOperation.SpecialContractNumber = default(string);
			orderUpdOperation.PayerSpecialKpp = default(string);
			orderUpdOperation.SpecialGovContract = default(string);
			orderUpdOperation.SpecialDeliveryAddress = default(string);

			orderUpdOperation.OrganizationName = organization.Name;
			orderUpdOperation.OrganizationAddress = organization.ActiveOrganizationVersion.JurAddress;
			orderUpdOperation.OrganizationInn = organization.INN;
			orderUpdOperation.OrganizationKpp = organization.KPP;
			orderUpdOperation.OrganizationTaxcomEdoAccountId = organization.TaxcomEdoAccountId;

			orderUpdOperation.BuhLastName = default(string);
			orderUpdOperation.BuhName = default(string);
			orderUpdOperation.BuhPatronymic = default(string);
			orderUpdOperation.LeaderLastName = default(string);
			orderUpdOperation.LeaderName = default(string);
			orderUpdOperation.LeaderPatronymic = default(string);
			orderUpdOperation.BottlesInFact = default(string);
			orderUpdOperation.IsSelfDelivery = default(bool);
			orderUpdOperation.CargoReceiver = default(string);
			orderUpdOperation.ClientInnKpp = default(string);
			orderUpdOperation.PaymentsInfo = default(string);
			orderUpdOperation.Goods = new ObservableList<OrderUpdOperationProduct>();

			return orderUpdOperation;
		}

		//var isSpecialFieldsUsedAndNotEmpty = client.UseSpecialDocFields
		//   && !string.IsNullOrWhiteSpace(client.SpecialContractName)
		//   && !string.IsNullOrWhiteSpace(client.SpecialContractNumber)
		//   && client.SpecialContractDate.HasValue;
	}
}
