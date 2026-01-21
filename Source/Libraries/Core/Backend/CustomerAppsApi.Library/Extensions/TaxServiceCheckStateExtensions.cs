using System;
using Dadata.Model;
using Vodovoz.Core.Domain.Clients.Accounts;

namespace CustomerAppsApi.Library.Extensions
{
	public static class TaxServiceCheckStateExtensions
	{
		public static TaxServiceCheckState ToTaxServiceCheckState(this PartyStatus source)
		{
			switch(source)
			{
				case PartyStatus.ACTIVE:
					return TaxServiceCheckState.Done;
				case PartyStatus.LIQUIDATING:
					return TaxServiceCheckState.IsLiquidating;
				case PartyStatus.LIQUIDATED:
					return TaxServiceCheckState.IsLiquidated;
				case PartyStatus.REORGANIZING:
					return TaxServiceCheckState.IsReorganizing;
				case PartyStatus.BANKRUPT:
					return TaxServiceCheckState.IsBankrupt;
				default:
					throw new ArgumentOutOfRangeException(nameof(source), "Неизвестный статус при проверке в ФНС");
			}
		}
	}
}
