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
				.GroupBy(oi => oi.Nomenclature.Gtin)
				.Select(gp => new ProductDto { Gtin = gp.Key, GtinQuantity = gp.Sum(s => (int)s.ActualCount).ToString() })
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
