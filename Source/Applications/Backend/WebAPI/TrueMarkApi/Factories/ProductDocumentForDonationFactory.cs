using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using TrueMarkApi.Dto.Documents;
using Order = Vodovoz.Domain.Orders.Order;

namespace TrueMarkApi.Factories
{
	/// <summary>
	/// Бартер
	/// </summary>
	public class ProductDocumentForDonationFactory : IProductDocumentFactory
	{
		private readonly string _organizationInn;
		private readonly Order _order;

		public ProductDocumentForDonationFactory(string organizationInn, Order order)
		{
			_organizationInn = organizationInn;
			_order = order;
		}
		public string CreateProductDocument()
		{
			var accountableItems = _order.OrderItems.Where(oi =>
					oi.Nomenclature.IsAccountableInTrueMark
					&& oi.ActualCount > 0)
				.GroupBy(oi => oi.Nomenclature.Gtin)
				.Select(gp => new ProductDto { Gtin = gp.Key, GtinQuantity = gp.Sum(s => (int)s.ActualCount).ToString() })
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
			var serializedProductDocumentBytes = Encoding.UTF8.GetBytes(serializedProductDocument);

			return Convert.ToBase64String(serializedProductDocumentBytes);
		}
	}
}
