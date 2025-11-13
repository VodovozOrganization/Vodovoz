using System.Collections.Generic;
using Edo.Contracts.Messages.Dto;
using Taxcom.Client.Api.Document.DocumentByFormat1115131_5_03;
using TaxcomEdo.Contracts.Goods;
using TaxcomEdo.Contracts.Orders;

namespace TaxcomEdoApi.Library.Converters.Format5_03
{
	public interface IUpdProductConverter5_03
	{
		FajlDokumentTablSchFaktSvedTov[] ConvertOrderItemsToUpdProducts(
			IList<OrderItemInfoForEdo> orderItems, IEnumerable<SpecialNomenclatureInfoForEdo> counterpartySpecialNomenclatures);
		FajlDokumentTablSchFaktSvedTov[] ConvertProductsToUpdProducts(IEnumerable<ProductInfo> products);
	}
}
