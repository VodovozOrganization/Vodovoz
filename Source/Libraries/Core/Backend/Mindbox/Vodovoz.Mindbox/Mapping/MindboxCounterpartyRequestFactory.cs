using System;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;
using Vodovoz.Mindbox.Contracts;

namespace Vodovoz.Mindbox.Mapping
{
	/// <summary>
	/// Формирует тело запроса на создание контрагента в Mindbox
	/// </summary>
	public interface IMindboxCounterpartyRequestFactory
	{
		MindboxCounterpartyRequest Create(Counterparty counterparty);
	}

	/// <inheritdoc />
	public sealed class MindboxCounterpartyRequestFactory : IMindboxCounterpartyRequestFactory
	{
		/// <inheritdoc />
		public MindboxCounterpartyRequest Create(Counterparty counterparty)
		{
			if(counterparty == null)
			{
				throw new ArgumentNullException(nameof(counterparty));
			}

			var request = new MindboxCounterpartyRequest
			{
				ErpCounterpartyId = counterparty.Id,
				CounterpartyForm = MindboxCounterpartyMapping.GetCounterpartyForm(counterparty),
				CounterpartyType = MindboxCounterpartyMapping.GetCounterpartyType(counterparty),
				IsNet = counterparty.IsChainStore,
				SourcePointOfContact = MindboxCounterpartyMapping.GetSourcePointOfContact(counterparty),
				MobilePhone = MindboxCounterpartyMapping.GetMobilePhone(counterparty),
				Email = MindboxCounterpartyMapping.GetEmail(counterparty)
			};

			if(counterparty.PersonType == PersonType.natural)
			{
				request.PhysicalFields = new MindboxPhysicalFields
				{
					LastName = counterparty.Surname,
					FirstName = counterparty.FirstName,
					MiddleName = counterparty.Patronymic
				};
			}
			else
			{
				request.LegalFields = new MindboxLegalFields
				{
					OwnershipForm = counterparty.TypeOfOwnership,
					CounterpartyName = counterparty.Name,
					CounterpartyFullName = string.IsNullOrWhiteSpace(counterparty.FullName)
						? counterparty.Name
						: counterparty.FullName,
					PaymentForm = MindboxCounterpartyMapping.GetPaymentForm(counterparty.PaymentMethod),
					INN = counterparty.INN,
					KPP = counterparty.KPP
				};
			}

			var deliveryPoint = MindboxCounterpartyMapping.GetDeliveryPoint(counterparty);

			if(deliveryPoint != null)
			{
				request.DeliveryPointFields = new MindboxDeliveryPointFields
				{
					LocalityType = deliveryPoint.LocalityType,
					LocalityTypeShort = deliveryPoint.LocalityTypeShort,
					City = deliveryPoint.City,
					StreetType = deliveryPoint.StreetType,
					StreetTypeShort = deliveryPoint.StreetTypeShort,
					Street = deliveryPoint.Street,
					Building = deliveryPoint.Building,
					Floor = deliveryPoint.Floor,
					Entrance = deliveryPoint.Entrance,
					Room = deliveryPoint.Room,
					Latitude = deliveryPoint.Latitude.HasValue ? (double?)deliveryPoint.Latitude.Value : null,
					Longitude = deliveryPoint.Longitude.HasValue ? (double?)deliveryPoint.Longitude.Value : null,
					DeliveryPointCategoryId = deliveryPoint.Category?.Id ?? 0,
					OnlineComment = deliveryPoint.OnlineComment,
					Intercom = deliveryPoint.Intercom,
					CityFiasGuid = deliveryPoint.CityFiasGuid?.ToString(),
					StreetFiasGuid = deliveryPoint.StreetFiasGuid?.ToString(),
					BuildingFiasGuid = deliveryPoint.BuildingFiasGuid?.ToString(),
					CounterpartyErpId = counterparty.Id,
					// В ERP источник точки доставки отдельно не хранится.
					Source = 0
				};
			}

			return request;
		}
	}
}
