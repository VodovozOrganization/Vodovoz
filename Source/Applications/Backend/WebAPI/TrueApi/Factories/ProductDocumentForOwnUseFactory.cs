using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using TrueApi.Dto.Documents;
using Vodovoz.Domain.Orders;

namespace TrueApi.Factories
{
	public class ProductDocumentForOwnUseFactory : IProductDocumentFactory
	{
		private readonly string _organizationInn;
		private readonly Order _order;

		public ProductDocumentForOwnUseFactory(string organizationInn, Order order)
		{
			_organizationInn = organizationInn;
			_order = order;
		}

		public string CreateProductDocument()
		{
			var accountableItems = _order.OrderItems.Where(oi => oi.Nomenclature.IsAccountableInChestniyZnak);

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
				Products = accountableItems.Select(ai =>
					new Product
					{
						Gtin = ai.Nomenclature.Gtin,
						GtinQuantity = ((int)ai.Count).ToString()
					}).ToList()
			};

			var serializedProductDocument = JsonSerializer.Serialize(productDocument);
			var serializedProductDocumentBytes = Encoding.UTF8.GetBytes(serializedProductDocument);
			return Convert.ToBase64String(serializedProductDocumentBytes);
		}
	}
}
