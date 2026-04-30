using CustomerOrders.Contracts.InfoMessages;

namespace CustomerOrdersApi.Library.V5.Factories
{
	/// <inheritdoc/>
	public class WarningMessageFactoryV5 : IWarningMessageFactoryV5
	{
		public WarningMessage CreateDistrictNotFoundMessage()
		{
			return WarningMessage.Create(
				nameof(WarningMessageType.DeliveryChanged),
				Messages.DeliveryChanged.Title,
				"Не найден логистический район");
		}

		/// <inheritdoc/>
		public WarningMessage CreateDeliveryChangedMessage(string message)
		{
			return WarningMessage.Create(
				nameof(WarningMessageType.DeliveryChanged),
				Messages.DeliveryChanged.Title,
				$"Платная доставка, {message}");
		}
		
		/// <inheritdoc/>
		public WarningMessage CreatePromoSetUnavailableMessage()
		{
			return WarningMessage.Create(
				nameof(WarningMessageType.PromoSetInvalid),
				Messages.Unavailable.PromoSet,
				Messages.Unavailable.PromoSetForNewClientOnlyForNewClients);
		}
		
		/// <inheritdoc/>
		public WarningMessage CreatePromoCodeUnavailableMessage()
		{
			return WarningMessage.Create(
				nameof(WarningMessageType.PromoCodeInvalid),
				Messages.Unavailable.PromoCode,
				Messages.Unavailable.PromoCodeUnavailable);
		}
		
		/// <inheritdoc/>
		public WarningMessage CreatePriceChangedMessage()
		{
			return WarningMessage.Create(
				nameof(WarningMessageType.PriceChanged),
				Messages.PriceChanged.Title,
				Messages.PriceChanged.Description);
		}
		
		/// <inheritdoc/>
		public WarningMessage CreateOutOfStockMessage()
		{
			return WarningMessage.Create(
				nameof(WarningMessageType.ItemOutOfStock),
				Messages.OutOfStock.ItemOutOfStock,
				Messages.OutOfStock.SomeProductsUnavailableToOrder);
		}
		
		/// <inheritdoc/>
		public WarningMessage CreateAllOutOfStockMessage()
		{
			return WarningMessage.Create(
				nameof(WarningMessageType.AllItemsOutOfStock),
				Messages.OutOfStock.ItemOutOfStock,
				Messages.OutOfStock.AllProductsUnavailableToOrder,
				nameof(WarningMessageButtonType.GoToSameCatalog));
		}
	}
}
