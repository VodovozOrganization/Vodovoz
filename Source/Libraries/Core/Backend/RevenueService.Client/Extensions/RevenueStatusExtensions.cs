using System;
using Dadata.Model;
using Vodovoz.Core.Domain.Clients;

namespace RevenueService.Client.Extensions
{
	public static class RevenueStatusExtensions
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
	}
}
