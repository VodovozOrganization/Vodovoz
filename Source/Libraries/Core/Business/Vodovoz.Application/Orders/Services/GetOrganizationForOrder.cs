using System;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.FastPayments;
using Vodovoz.Settings.Orders;
using Vodovoz.Settings.Organizations;

namespace Vodovoz.Application.Orders.Services
{
	public abstract class GetOrganizationForOrder
	{
		private readonly IFastPaymentRepository _fastPaymentRepository;

		protected GetOrganizationForOrder(
			IOrganizationSettings organizationSettings,
			IOrderSettings orderSettings,
			IFastPaymentRepository fastPaymentRepository)
		{
			_fastPaymentRepository = fastPaymentRepository ?? throw new ArgumentNullException(nameof(fastPaymentRepository));
			OrganizationSettings = organizationSettings ?? throw new ArgumentNullException(nameof(organizationSettings));
			OrderSettings = orderSettings ?? throw new ArgumentNullException(nameof(orderSettings));
		}
		
		protected IOrganizationSettings OrganizationSettings { get; }
		protected IOrderSettings OrderSettings { get; }
		
		protected int GetOrganizationId(
			IUnitOfWork uow,
			int settingsOrganizationId,
			int? onlineOrderId)
		{
			if(!onlineOrderId.HasValue)
			{
				return settingsOrganizationId;
			}

			var fastPayment = _fastPaymentRepository.GetPerformedFastPaymentByExternalId(uow, onlineOrderId.Value);
			
			if(fastPayment is null)
			{
				return settingsOrganizationId;
			}

			return fastPayment.Organization?.Id ?? settingsOrganizationId;
		}
		
		protected int GetOrganizationIdForOnlinePayment(
			IUnitOfWork uow,
			PaymentFrom paymentFrom,
			int defaultOrganizationId,
			int? onlineOrderId)
		{
			if(paymentFrom == null)
			{
				return OrganizationSettings.VodovozNorthOrganizationId;
			}

			if(OrderSettings.PaymentsByCardFromAvangard.Contains(paymentFrom.Id))
			{
				if(!onlineOrderId.HasValue)
				{
					return GetPaymentFromOrganisationIdOrDefault(paymentFrom, defaultOrganizationId);
				}

				var fastPayment = _fastPaymentRepository.GetPerformedFastPaymentByExternalId(uow, onlineOrderId.Value);
				if(fastPayment == null)
				{
					return GetPaymentFromOrganisationIdOrDefault(paymentFrom, defaultOrganizationId);
				}

				return fastPayment.Organization?.Id ?? OrganizationSettings.VodovozNorthOrganizationId;
			}

			return GetPaymentFromOrganisationIdOrDefault(paymentFrom, defaultOrganizationId);
		}

		protected int GetOrganizationIdForByCard(
			IUnitOfWork uow,
			PaymentFrom paymentFrom,
			DateTime? orderCreateDate,
			int? onlineOrderId)
		{
			if(paymentFrom == null)
			{
				return OrganizationSettings.VodovozNorthOrganizationId;
			}
			
			if(OrderSettings.PaymentsByCardFromForNorthOrganization.Contains(paymentFrom.Id)
			   && orderCreateDate.HasValue
			   && orderCreateDate.Value < new DateTime(2022, 08, 30, 13, 00, 00))
			{
				return OrganizationSettings.VodovozNorthOrganizationId;
			}

			if(OrderSettings.PaymentsByCardFromAvangard.Contains(paymentFrom.Id))
			{
				if(!onlineOrderId.HasValue)
				{
					return GetPaymentFromOrganisationIdOrDefault(paymentFrom);
				}

				var fastPayment = _fastPaymentRepository.GetPerformedFastPaymentByExternalId(uow, onlineOrderId.Value);
				if(fastPayment == null)
				{
					return GetPaymentFromOrganisationIdOrDefault(paymentFrom);
				}

				return fastPayment.Organization?.Id ?? OrganizationSettings.VodovozNorthOrganizationId;
			}

			return GetPaymentFromOrganisationIdOrDefault(paymentFrom);
		}

		private int GetPaymentFromOrganisationIdOrDefault(PaymentFrom paymentFrom, int? defaultOrganizationId = null) =>
			paymentFrom.OrganizationForOnlinePayments?.Id
				?? defaultOrganizationId
				?? OrganizationSettings.VodovozNorthOrganizationId;
	}
}
