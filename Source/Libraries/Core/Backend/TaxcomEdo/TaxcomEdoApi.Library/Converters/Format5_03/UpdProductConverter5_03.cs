using System;
using System.Collections.Generic;
using System.Linq;
using Core.Infrastructure;
using Edo.Contracts.Messages.Dto;
using Edo.Contracts.Xml.Documents.FormalizedDocuments;
using Edo.Contracts.Xml.Documents.FormalizedDocuments.UPD;

namespace TaxcomEdoApi.Library.Converters.Format5_03
{
	public class UpdProductConverter5_03 : IUpdProductConverter5_03
	{
		public ФайлДокументТаблСчФактСведТов[] ConvertProductsToUpdProducts(IEnumerable<ProductInfo> products)
		{
			var updProducts = new List<ФайлДокументТаблСчФактСведТов>();
			var i = 0;

			foreach(var product in products)
			{
				var updProduct = ConvertOrderItemToUpdProduct(product, i + 1);
				updProducts.Add(updProduct);
			}

			return updProducts.ToArray();
		}

		private ФайлДокументТаблСчФактСведТов ConvertOrderItemToUpdProduct(ProductInfo product, int row)
		{
			var count = product.Count;
			
			var updProduct = new ФайлДокументТаблСчФактСведТов
			{
				Акциз = new СумАкцизТип
				{
					Item = СумАкцизТипБезАкциз.безакциза
				},
				
				НомСтр = row.ToString(),
				НаимТов = product.Name,
				ОКЕИ_Тов = product.OKEI,
				НаимЕдИзм = product.UnitName,
				КолТов = count,
				ЦенаТов = product.PriceWithoutVat,
				ЦенаТовSpecified = true,
				КолТовSpecified = true,
				НалСт = GetProductTaxRate(product.ValueAddedTax),
				СтТовУчНал = product.Sum,
				СтТовБезНДС = product.SumWithoutVat,
				СтТовБезНДСSpecified = true,
				ДопСведТов = new ФайлДокументТаблСчФактСведТовДопСведТов
				{
					КодТов = product.Code
				}
			};

			if(product.EconomicLifeFacts != null && product.EconomicLifeFacts.Any())
			{
				updProduct.ИнфПолФХЖ2 = product.EconomicLifeFacts
					.Select(x => new TextInformation { Key = x.Id, Value = x.Value })
					.ToArray();
			}

			if(product.TrueMarkCodes.Any())
			{
				var codesWithoutTransport = product.TrueMarkCodes
					.Where(x => x.TransportCode.IsNullOrWhiteSpace());

				var codesPerTransport = product.TrueMarkCodes
					.Where(x => !x.TransportCode.IsNullOrWhiteSpace())
					.GroupBy(x => x.TransportCode);

				var sredIdentTovList = new List<ФайлДокументТаблСчФактСведТовДопСведТовНомСредИдентТов>();

				if(codesWithoutTransport.Any())
				{
					var sredIdentTov = CreateSredIdentTov(codesWithoutTransport);
					sredIdentTovList.AddRange(sredIdentTov);
				}

				foreach(var codesPerTransportItem in codesPerTransport)
				{
					if(codesPerTransportItem.Any())
					{
						var transportCode = codesPerTransportItem.Key;
						var sredIdentTov = CreateSredIdentTov(codesWithoutTransport, transportCode);
						sredIdentTovList.AddRange(sredIdentTov);
					}
				}

				updProduct.ДопСведТов.НомСредИдентТов = sredIdentTovList.ToArray();
			}
			
			updProduct.СумНал = new СумНДСТип
			{
				Item = GetTax(product, updProduct.НалСт)
			};

			return updProduct;
		}

		private IEnumerable<ФайлДокументТаблСчФактСведТовДопСведТовНомСредИдентТов> CreateSredIdentTov(
			IEnumerable<ProductCodeInfo> codes, 
			string transportCode = null
			)
		{
			if(!transportCode.IsNullOrWhiteSpace())
			{
				return new []{ new ФайлДокументТаблСчФактСведТовДопСведТовНомСредИдентТов { ИдентТрансУпак = transportCode} };
			}

			var identificationData = new List<ФайлДокументТаблСчФактСведТовДопСведТовНомСредИдентТов>();
			var groupedCodesByType = codes.ToLookup(x => x.IsGroup);

			foreach(var groupCodesByType in groupedCodesByType)
			{
				var identificationInfo = new ФайлДокументТаблСчФактСведТовДопСведТовНомСредИдентТов();
				var items = new List<(ItemsChoiceType Type, string Code)>();
				
				items.AddRange(
					from code in groupCodesByType
					let itemType = code.IsGroup
						? ItemsChoiceType.НомУпак
						: ItemsChoiceType.КИЗ
					select (itemType, code.IndividualOrGroupCode));

				identificationInfo.ItemsElementName = items.Select(x => x.Type).ToArray();
				identificationInfo.Items = items.Select(x => x.Code).ToArray();
				identificationData.Add(identificationInfo);
			}

			return identificationData;
		}

		private ФайлДокументТаблСчФактСведТовНалСт GetProductTaxRate(decimal? orderItemTax)
		{
			switch(orderItemTax)
			{
				case null:
				case 0m:
					return ФайлДокументТаблСчФактСведТовНалСт.безНДС;
				case 0.10m:
					return ФайлДокументТаблСчФактСведТовНалСт.Item10;
				case 0.18m:
					throw new InvalidOperationException("Не должно быть НДС 18% в УПД формата 5.03");
				case 0.20m:
					return ФайлДокументТаблСчФактСведТовНалСт.Item20;
				case 0.22m:
					return ФайлДокументТаблСчФактСведТовНалСт.Item22;
				default:
					return ФайлДокументТаблСчФактСведТовНалСт.безНДС;
			}
		}
		
		private object GetTax(ProductInfo product, ФайлДокументТаблСчФактСведТовНалСт taxRate)
		{
			if(taxRate == ФайлДокументТаблСчФактСведТовНалСт.безНДС)
			{
				return СумНДСТипБезНДС.безНДС;
			}

			return product.IncludeVat;
		}
	}
}
