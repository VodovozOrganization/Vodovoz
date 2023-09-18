using Dadata.Model;

namespace RevenueService.Client.Extensions
{
	public static class PartyStatusExtensions
	{
		public static string GetUserFriendlyName(this PartyStatus status)
		{
			switch(status)
			{
				case Dadata.Model.PartyStatus.ACTIVE:
					return "Действующий";
				case Dadata.Model.PartyStatus.LIQUIDATING:
					return "Ликвидируется";
				case Dadata.Model.PartyStatus.LIQUIDATED:
					return "Ликвидирован";
				case Dadata.Model.PartyStatus.REORGANIZING:
					return "Банкротство";
				case Dadata.Model.PartyStatus.BANKRUPT:
					return "в процессе присоединения к другому юрлицу, с последующей ликвидацией";
				default:
					return "Неизвестно";
			}
		}
	}
}
