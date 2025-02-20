using System;
using QS.DomainModel.UoW;
using Vodovoz.EntityRepositories.FastPayments;
using Vodovoz.Settings.Orders;
using Vodovoz.Settings.Organizations;
using VodovozBusiness.Domain.Settings;

namespace Vodovoz.Application.Orders.Services
{
	public abstract class GetOrganizationForOrder
	{
		private readonly IFastPaymentRepository _fastPaymentRepository;

		protected GetOrganizationForOrder(
			IOrganizationSettings organizationSettings,
			IOrderSettings orderSettings,
			IFastPaymentRepository fastPaymentRepository,
			OrganizationForOrderFromSet organizationForOrderFromSet)
		{
			_fastPaymentRepository = fastPaymentRepository ?? throw new ArgumentNullException(nameof(fastPaymentRepository));
			OrganizationForOrderFromSet =
				organizationForOrderFromSet ?? throw new ArgumentNullException(nameof(organizationForOrderFromSet));
			OrganizationSettings = organizationSettings ?? throw new ArgumentNullException(nameof(organizationSettings));
			OrderSettings = orderSettings ?? throw new ArgumentNullException(nameof(orderSettings));
		}
		
		protected IOrganizationSettings OrganizationSettings { get; }
		protected IOrderSettings OrderSettings { get; }
		protected OrganizationForOrderFromSet OrganizationForOrderFromSet { get; }

		protected int GetOrganizationId(
			IUnitOfWork uow,
			IOrganizations settingsOrganizations,
			int orderId,
			int? onlineOrderId)
		{
			if(!onlineOrderId.HasValue)
			{
				return GetOrganizationIdFromSet(settingsOrganizations, orderId);
			}

			var fastPayment = _fastPaymentRepository.GetPerformedFastPaymentByExternalId(uow, onlineOrderId.Value);
			
			if(fastPayment is null)
			{
				return GetOrganizationIdFromSet(settingsOrganizations, orderId);
			}

			return fastPayment.Organization?.Id ?? GetOrganizationIdFromSet(settingsOrganizations, orderId);
		}

		private int GetOrganizationIdFromSet(IOrganizations settingsOrganizations, int orderId)
		{
			return OrganizationForOrderFromSet.GetOrganizationForOrderFromSet(orderId, settingsOrganizations).Id;
		}
	}
}
