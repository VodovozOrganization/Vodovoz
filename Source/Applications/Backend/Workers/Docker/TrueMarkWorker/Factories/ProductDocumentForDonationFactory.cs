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
				.Select(p => new ProductDto
				{
					Gtin = p.Nomenclature.Gtin,
					GtinQuantity = ((int)p.ActualCount).ToString(),
					ProductCost = ((((p.Price * p.Count) - p.DiscountMoney) / p.Count) * 100).ToString(),
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
