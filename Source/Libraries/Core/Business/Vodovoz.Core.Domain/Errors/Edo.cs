using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Core.Domain.Errors
{
	public static class Edo
	{
		public static class TransferOrder
		{
			public static Error TransferOrderCreateDateMissing =>
				new Error(
				typeof(TransferOrder),
				nameof(TransferOrderCreateDateMissing),
				"Дата создания заказа перемещения товаров не указана");

			public static Error TransferOrderCreateSellerMissing =>
				new Error(
				typeof(TransferOrder),
				nameof(TransferOrderCreateSellerMissing),
				"Продавец не указан");

			public static Error TransferOrderCreateCustomerMissing =>
				new Error(
				typeof(TransferOrder),
				nameof(TransferOrderCreateCustomerMissing),
				"Покупатель не указан");
			
			public static Error TransferOrderDocumentOrganizationCounterMissing =>
				new Error(
					typeof(TransferOrder),
					nameof(TransferOrderDocumentOrganizationCounterMissing),
					"Нет счетчика документов для трансфера");
		}
	}
}
