using System.Linq;
using System.Text.Json;
using TrueMark.Contracts.Documents;
using Order = Vodovoz.Domain.Orders.Order;

namespace TrueMarkWorker.Factories
{
	/// <summary>
	/// Бартер
	/// </summary>
	public class ProductDocumentForDonationFactory : IDocumentFactory
	{
		private readonly string _organizationInn;
		private readonly Order _order;

		public ProductDocumentForDonationFactory(string organizationInn, Order order)
		{
			_organizationInn = organizationInn;
			_order = order;
		}
		public string CreateDocument()
		{
			var accountableItems = _order.OrderItems.Where(oi =>
					oi.Nomenclature.IsAccountableInTrueMark
					&& oi.ActualCount > 0)
				.Select(oi => new ProductDto
				{
					Gtin = oi.Nomenclature.Gtin,
					GtinQuantity = ((int)oi.ActualCount).ToString(),
					// ProductCost - Обязательный, если указан ИНН покупателя. Если «action» = «DONATION» и поле «product_cost» заполнено, то значение должно быть «0.00»
					ProductCost =
						string.IsNullOrWhiteSpace(_order.Client.INN)
						? null
						: "0"
				})
				.ToList();

			var productDocument = new ProductDocumentDto
			{
				Inn = _organizationInn,
				BuyerInn = _order.Client.INN,
				Action = "DONATION",
				ActionDate = _order.DeliveryDate.Value,
				DocumentType = "CONSIGNMENT_NOTE",
				DocumentNumber = _order.Id.ToString(),
				DocumentDate = _order.DeliveryDate.Value,
				Products = accountableItems
			};

			var serializedProductDocument = JsonSerializer.Serialize(productDocument);

			return serializedProductDocument;
		}
	}
}
