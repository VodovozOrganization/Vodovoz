using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using TrueMark.Contracts.Documents;
using Vodovoz.Domain.Orders;

namespace TrueMarkWorker.Factories
{
	public class ProductDocumentForOwnUseFactory : IDocumentFactory
	{
		private readonly string _organizationInn;
		private readonly Order _order;

		public ProductDocumentForOwnUseFactory(string organizationInn, Order order)
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
					ProductCost =
						string.IsNullOrWhiteSpace(_order.Client.INN)
						? null
						: ((oi.Price * (oi.ActualCount ?? oi.Count) - oi.DiscountMoney) * 100).ToString("F2")
				})
				.ToList();

			var productDocument = new ProductDocumentDto
			{
				Inn = _organizationInn,
				BuyerInn = _order.Client.INN,
				Action = "OWN_USE",
				ActionDate = _order.DeliveryDate.Value,
				DocumentType = "OTHER",
				DocumentNumber = _order.Id.ToString(),
				DocumentDate = _order.DeliveryDate.Value,
				PrimaryDocumentCustomName = "UTD",
				Products = accountableItems
			};

			var serializedProductDocument = JsonSerializer.Serialize(productDocument);

			return serializedProductDocument;
		}
	}
}
