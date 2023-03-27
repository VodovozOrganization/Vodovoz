using System;
using CustomerAppsApi.Dto;
using CustomerAppsApi.Models;
using Vodovoz.Domain.Client;

namespace CustomerAppsApi.Factories
{
	public class CounterpartyModelFactory
	{
		private readonly RegisteredNaturalCounterpartyDtoFactory _registeredNaturalCounterpartyDtoFactory;

		public CounterpartyModelFactory(RegisteredNaturalCounterpartyDtoFactory registeredNaturalCounterpartyDtoFactory)
		{
			_registeredNaturalCounterpartyDtoFactory =
				registeredNaturalCounterpartyDtoFactory ?? throw new ArgumentNullException(nameof(registeredNaturalCounterpartyDtoFactory));
		}

		#region CounterpartyIdentificationDto

		public CounterpartyIdentificationDto CreateErrorCounterpartyIdentificationDto(string error)
		{
			return new CounterpartyIdentificationDto
			{
				ErrorDescription = error,
				CounterpartyIdentificationStatus = CounterpartyIdentificationStatus.Error
			};
		}

		public CounterpartyIdentificationDto CreateNotFoundCounterpartyIdentificationDto()
		{
			return new CounterpartyIdentificationDto
			{
				CounterpartyIdentificationStatus = CounterpartyIdentificationStatus.CounterpartyNotFound
			};
		}

		public CounterpartyIdentificationDto CreateNeedManualHandlingCounterpartyIdentificationDto()
		{
			return new CounterpartyIdentificationDto
			{
				CounterpartyIdentificationStatus = CounterpartyIdentificationStatus.NeedManualHandling
			};
		}

		public CounterpartyIdentificationDto CreateSuccessCounterpartyIdentificationDto(ExternalCounterparty externalCounterparty)
		{
			return new CounterpartyIdentificationDto
			{
				RegisteredNaturalCounterpartyDto =
					_registeredNaturalCounterpartyDtoFactory.CreateNewRegisteredNaturalCounterpartyDto(externalCounterparty),
				CounterpartyIdentificationStatus = externalCounterparty.Email != null
					? CounterpartyIdentificationStatus.Success
					: CounterpartyIdentificationStatus.CounterpartyRegisteredWithoutEmail
			};
		}
		
		public CounterpartyIdentificationDto CreateRegisteredCounterpartyIdentificationDto(ExternalCounterparty externalCounterparty)
		{
			return new CounterpartyIdentificationDto
			{
				RegisteredNaturalCounterpartyDto =
					_registeredNaturalCounterpartyDtoFactory.CreateNewRegisteredNaturalCounterpartyDto(externalCounterparty),
				CounterpartyIdentificationStatus =
					externalCounterparty.Email != null
						? CounterpartyIdentificationStatus.CounterpartyRegistered
						: CounterpartyIdentificationStatus.CounterpartyRegisteredWithoutEmail
			};
		}

		#endregion

		#region CounterpartyRegistrationDto

		public CounterpartyRegistrationDto CreateErrorCounterpartyRegistrationDto(string error)
		{
			return new CounterpartyRegistrationDto
			{
				ErrorDescription = error,
				CounterpartyRegistrationStatus = CounterpartyRegistrationStatus.Error
			};
		}
		
		public CounterpartyRegistrationDto CreateRegisteredCounterpartyRegistrationDto(int counterpartyId)
		{
			return new CounterpartyRegistrationDto
			{
				CounterpartyRegistrationStatus = CounterpartyRegistrationStatus.CounterpartyRegistered,
				ErpCounterpartyId = counterpartyId
			};
		}
		
		#endregion

		#region CounterpartyUpdateDto

		public CounterpartyUpdateDto CreateErrorCounterpartyUpdateDto(string error)
		{
			return new CounterpartyUpdateDto
			{
				ErrorDescription = error,
				CounterpartyUpdateStatus = CounterpartyUpdateStatus.Error
			};
		}
		
		public CounterpartyUpdateDto CreateNotFoundCounterpartyUpdateDto()
		{
			return new CounterpartyUpdateDto
			{
				CounterpartyUpdateStatus = CounterpartyUpdateStatus.CounterpartyNotFound
			};
		}

		#endregion

		#region ExternalCounterparty

		public ExternalCounterparty CreateExternalCounterparty(CounterpartyFrom counterpartyFrom)
		{
			switch(counterpartyFrom)
			{
				case CounterpartyFrom.MobileApp:
					return new MobileAppCounterparty();
				case CounterpartyFrom.WebSite:
					return new WebSiteCounterparty();
				default:
					throw new ArgumentOutOfRangeException(nameof(counterpartyFrom), counterpartyFrom, null);
			}
		}
		
		public ExternalCounterparty CopyToOtherExternalCounterparty(ExternalCounterparty copyingCounterparty, Guid externalCounterpartyId)
		{
			switch(copyingCounterparty.CounterpartyFrom)
			{
				case CounterpartyFrom.MobileApp:
					return new WebSiteCounterparty
					{
						Email = copyingCounterparty.Email,
						Phone = copyingCounterparty.Phone,
						ExternalCounterpartyId = externalCounterpartyId,
						IsArchive = copyingCounterparty.IsArchive
					};
				case CounterpartyFrom.WebSite:
					return new MobileAppCounterparty
					{
						Email = copyingCounterparty.Email,
						Phone = copyingCounterparty.Phone,
						ExternalCounterpartyId = externalCounterpartyId,
						IsArchive = copyingCounterparty.IsArchive
					};
				default:
					return null;
			}
		}

		#endregion
	}
}
