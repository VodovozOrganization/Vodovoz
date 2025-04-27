using System;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.FastPayments;
using Vodovoz.Settings.Orders;
using Vodovoz.Settings.Organizations;
using VodovozBusiness.Domain.Settings;

namespace Vodovoz.Application.Orders.Services
{
	/// <summary>
	/// Базовый класс для подбора организации по типу доставки
	/// </summary>
	public abstract class OrganizationForOrderByDelivery
	{
		private readonly IFastPaymentRepository _fastPaymentRepository;

		protected OrganizationForOrderByDelivery(
			IOrganizationSettings organizationSettings,
			IOrderSettings orderSettings,
			IFastPaymentRepository fastPaymentRepository,
			IOrganizationForOrderFromSet organizationForOrderFromSet)
		{
			_fastPaymentRepository = fastPaymentRepository ?? throw new ArgumentNullException(nameof(fastPaymentRepository));
			OrganizationForOrderFromSet =
				organizationForOrderFromSet ?? throw new ArgumentNullException(nameof(organizationForOrderFromSet));
			OrganizationSettings = organizationSettings ?? throw new ArgumentNullException(nameof(organizationSettings));
			OrderSettings = orderSettings ?? throw new ArgumentNullException(nameof(orderSettings));
		}
		
		protected IOrganizationSettings OrganizationSettings { get; }
		protected IOrderSettings OrderSettings { get; }
		protected IOrganizationForOrderFromSet OrganizationForOrderFromSet { get; }

		protected int GetOrganizationId(
			IUnitOfWork uow,
			IOrganizations settingsOrganizations,
			TimeSpan requestTime,
			int? onlineOrderNumber)
		{
			if(!onlineOrderNumber.HasValue)
			{
				return GetOrganizationIdFromSet(settingsOrganizations, requestTime);
			}

			var fastPayment = _fastPaymentRepository.GetPerformedFastPaymentByExternalId(uow, onlineOrderNumber.Value);
			
			if(fastPayment is null)
			{
				return GetOrganizationIdFromSet(settingsOrganizations, requestTime);
			}

			return fastPayment.Organization?.Id ?? GetOrganizationIdFromSet(settingsOrganizations, requestTime);
		}

		private int GetOrganizationIdFromSet(IOrganizations settingsOrganizations, TimeSpan requestTime)
		{
			return OrganizationForOrderFromSet.GetOrganizationForOrderFromSet(requestTime, settingsOrganizations).Id;
		}
	}
}
