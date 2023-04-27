﻿using System;
using CustomerAppsApi.Library.Dto;
using Vodovoz.Domain.Client;
using Vodovoz.Factories;

namespace CustomerAppsApi.Factories
{
	public class CounterpartyModelFactory
	{
		private readonly IRegisteredNaturalCounterpartyDtoFactory _registeredNaturalCounterpartyDtoFactory;
		private readonly IExternalCounterpartyMatchingFactory _externalCounterpartyMatchingFactory;
		private readonly IExternalCounterpartyFactory _externalCounterpartyFactory;

		public CounterpartyModelFactory(
			IRegisteredNaturalCounterpartyDtoFactory registeredNaturalCounterpartyDtoFactory,
			IExternalCounterpartyMatchingFactory externalCounterpartyMatchingFactory,
			IExternalCounterpartyFactory externalCounterpartyFactory)
		{
			_registeredNaturalCounterpartyDtoFactory =
				registeredNaturalCounterpartyDtoFactory ?? throw new ArgumentNullException(nameof(registeredNaturalCounterpartyDtoFactory));
			_externalCounterpartyMatchingFactory =
				externalCounterpartyMatchingFactory ?? throw new ArgumentNullException(nameof(externalCounterpartyMatchingFactory));
			_externalCounterpartyFactory =
				externalCounterpartyFactory ?? throw new ArgumentNullException(nameof(externalCounterpartyFactory));
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

		public CounterpartyManualHandlingDto CreateNeedManualHandlingCounterpartyDto(
			CounterpartyContactInfoDto counterpartyContactInfoDto, CounterpartyFrom counterpartyFrom)
		{
			var matchingEntity = _externalCounterpartyMatchingFactory.CreateNewExternalCounterpartyMatching(
				counterpartyContactInfoDto.ExternalCounterpartyId,
				counterpartyContactInfoDto.PhoneNumber,
				counterpartyFrom);

			var counterpartyIdentificationDto = CreateNeedManualHandlingCounterpartyIdentificationDto();
			
			return new CounterpartyManualHandlingDto(counterpartyIdentificationDto, matchingEntity);
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

		public ExternalCounterparty CreateExternalCounterparty(CounterpartyFrom counterpartyFrom) =>
			_externalCounterpartyFactory.CreateNewExternalCounterparty(counterpartyFrom);
		
		public ExternalCounterparty CopyToOtherExternalCounterparty(ExternalCounterparty copyingCounterparty, Guid externalCounterpartyId)
		{
			switch(copyingCounterparty.CounterpartyFrom)
			{
				case CounterpartyFrom.MobileApp:
					return new WebSiteCounterparty
					{
						Email = copyingCounterparty.Email,
						Phone = copyingCounterparty.Phone,
						ExternalCounterpartyId = externalCounterpartyId
					};
				case CounterpartyFrom.WebSite:
					return new MobileAppCounterparty
					{
						Email = copyingCounterparty.Email,
						Phone = copyingCounterparty.Phone,
						ExternalCounterpartyId = externalCounterpartyId
					};
				default:
					return null;
			}
		}

		#endregion

		#region ExternalCounterpartyMatching

		public ExternalCounterpartyMatching CreateNewExternalCounterpartyMatching(Guid externalCounterpartyId, string phoneNumber,
			CounterpartyFrom counterpartyFrom)
		{
			return _externalCounterpartyMatchingFactory.CreateNewExternalCounterpartyMatching(
				externalCounterpartyId, phoneNumber, counterpartyFrom);
		}

		#endregion
	}
}
