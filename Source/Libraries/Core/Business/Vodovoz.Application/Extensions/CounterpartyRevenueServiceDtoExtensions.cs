using RevenueService.Client.Dto;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Services;

namespace Vodovoz.Application.Extensions
{
	public static class CounterpartyRevenueServiceDtoExtensions
	{
		public static CounterpartyRevenueServiceInfo ToCounterpartyRevenueServiceInfo(this  CounterpartyRevenueServiceDto counterpartyRevenueServiceDto)
		{
			var result = new CounterpartyRevenueServiceInfo
			{
				INN = counterpartyRevenueServiceDto.Inn,
				KPP = counterpartyRevenueServiceDto.Kpp,
				Name = counterpartyRevenueServiceDto.ShortName ?? counterpartyRevenueServiceDto.FullName,
				FullName = counterpartyRevenueServiceDto.FullName ?? counterpartyRevenueServiceDto.ShortName,
				LegalAddress = counterpartyRevenueServiceDto.Address,
				Phones = Enumerable.Empty<string>(),
				Emails = Enumerable.Empty<string>()
			};

			if((counterpartyRevenueServiceDto.Opf ?? string.Empty).Length > 0
				&& (counterpartyRevenueServiceDto.OpfFull ?? string.Empty).Length > 0)
			{
				result.TypeOfOwnership = counterpartyRevenueServiceDto.Opf;
				result.Name = $"{counterpartyRevenueServiceDto.Opf} {result.Name}";
				result.FullName = $"{counterpartyRevenueServiceDto.Opf} {result.FullName}";
			}

			if(counterpartyRevenueServiceDto.Opf == "ИП")
			{
				result.SignatoryFIO = string.Empty;

				result.Surname = counterpartyRevenueServiceDto.PersonSurname ?? string.Empty;
				result.FirstName = counterpartyRevenueServiceDto.PersonName ?? string.Empty;
				result.Patronymic = counterpartyRevenueServiceDto.PersonPatronymic ?? string.Empty;
			}
			else
			{
				result.SignatoryFIO = counterpartyRevenueServiceDto.TitlePersonFullName;

				result.Surname = string.Empty;
				result.FirstName = string.Empty;
				result.Patronymic = string.Empty;
			}

			if(counterpartyRevenueServiceDto.Phones != null && counterpartyRevenueServiceDto.Phones.Length != 0)
			{
				result.Phones = counterpartyRevenueServiceDto.Phones;
			}

			if(counterpartyRevenueServiceDto.Emails != null && counterpartyRevenueServiceDto.Emails.Length != 0)
			{
				result.Emails = counterpartyRevenueServiceDto.Emails;
			}

			return result;
		}

		public static IEnumerable<CounterpartyRevenueServiceInfo> ToCounterpartyRevenueServiceInfo(
			this IEnumerable<CounterpartyRevenueServiceDto> counterpartyRevenueServiceDtos)
		{
			var result = new List<CounterpartyRevenueServiceInfo>();

			foreach(var dto in counterpartyRevenueServiceDtos)
			{
				result.Add(dto.ToCounterpartyRevenueServiceInfo());
			}

			return result;
		}
	}
}
