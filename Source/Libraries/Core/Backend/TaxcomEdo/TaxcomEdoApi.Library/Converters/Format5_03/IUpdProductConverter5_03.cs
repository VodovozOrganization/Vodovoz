using System.Collections.Generic;
using Edo.Contracts.Messages.Dto;
using Edo.Contracts.Xml.Documents.FormalizedDocuments.UPD;

namespace TaxcomEdoApi.Library.Converters.Format5_03
{
	public interface IUpdProductConverter5_03
	{
		ФайлДокументТаблСчФактСведТов[] ConvertProductsToUpdProducts(IEnumerable<ProductInfo> products);
	}
}
