using System;
using System.Collections.Generic;
using System.Linq;
using Dadata.Model;
using RevenueService.Client.Dto;
using Vodovoz.Core.Domain.Clients;

namespace RevenueService.Client.Extensions
{
	public static class RevenueExtensions
	{
		/// <summary>
		/// Конвертация статусов контрагента в налоговой из DaData в RevenueStatus 
		/// </summary>
		public static RevenueStatus ConvertToRevenueStatus(this PartyStatus status)
		{
			switch(status)
			{
				case PartyStatus.ACTIVE:
					return RevenueStatus.Active;
				case PartyStatus.LIQUIDATING:
					return RevenueStatus.Liquidating;
				case PartyStatus.LIQUIDATED:
					return RevenueStatus.Liquidated;
				case PartyStatus.REORGANIZING:
					return RevenueStatus.Reorganizing;
				case PartyStatus.BANKRUPT:
					return RevenueStatus.Bankrupt;
				default:
					throw new ArgumentOutOfRangeException($"Неизвестный статус {status}");
			}
		}

		/// <summary>
		/// Самая свежая информация о контрагенте 
		/// </summary>
		/// <param name="counterpartyInfoList"></param>
		/// <returns></returns>
		public static CounterpartyRevenueServiceDto GetLastByDateInformation(this IList<CounterpartyRevenueServiceDto> counterpartyInfoList)
		{
			return counterpartyInfoList?
				.OrderBy(x => x.StateDate)
				.LastOrDefault();
		}
	}
}
