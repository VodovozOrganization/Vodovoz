using Dadata.Model;

namespace RevenueService.Client.Extensions
{
	public static class PartyStatusExtensions
	{
		public static string GetUserFriendlyName(this PartyStatus status)
		{
			switch(status)
			{
				case PartyStatus.ACTIVE:
					return "Действующий";
				case PartyStatus.LIQUIDATING:
					return "Ликвидируется";
				case PartyStatus.LIQUIDATED:
					return "Ликвидирован";
				case PartyStatus.REORGANIZING:
					return "Банкротство";
				case PartyStatus.BANKRUPT:
					return "в процессе присоединения к другому юрлицу, с последующей ликвидацией";
				default:
					return "Неизвестно";
			}
		}
	}
}
