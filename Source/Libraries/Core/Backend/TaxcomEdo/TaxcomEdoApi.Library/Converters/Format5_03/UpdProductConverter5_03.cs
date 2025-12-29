using System;
using System.Collections.Generic;
using System.Linq;
using Core.Infrastructure;
using Edo.Contracts.Messages.Dto;
using Taxcom.Client.Api.Document.DocumentByFormat1115131_5_03;
using TaxcomEdo.Contracts.Goods;
using TaxcomEdo.Contracts.Orders;

namespace TaxcomEdoApi.Library.Converters.Format5_03
{
	public class UpdProductConverter5_03 : IUpdProductConverter5_03
	{
		public FajlDokumentTablSchFaktSvedTov[] ConvertOrderItemsToUpdProducts(
			IList<OrderItemInfoForEdo> orderItems, IEnumerable<SpecialNomenclatureInfoForEdo> counterpartySpecialNomenclatures)
		{
			var products = new List<FajlDokumentTablSchFaktSvedTov>();

			for(var i = 0; i < orderItems.Count; i++)
			{
				var product = ConvertOrderItemToUpdProduct(orderItems[i], counterpartySpecialNomenclatures, i + 1);
				products.Add(product);
			}

			return products.ToArray();
		}

		public FajlDokumentTablSchFaktSvedTov[] ConvertProductsToUpdProducts(IEnumerable<ProductInfo> products)
		{
			var updProducts = new List<FajlDokumentTablSchFaktSvedTov>();
			var i = 0;

			foreach(var product in products)
			{
				var updProduct = ConvertOrderItemToUpdProduct(product, i + 1);
				updProducts.Add(updProduct);
			}

			return updProducts.ToArray();
		}

		private FajlDokumentTablSchFaktSvedTov ConvertOrderItemToUpdProduct(
			OrderItemInfoForEdo orderItemInfoForEdo, IEnumerable<SpecialNomenclatureInfoForEdo> counterpartySpecialNomenclatures, int row)
		{
			var count = Math.Round(orderItemInfoForEdo.CurrentCount, orderItemInfoForEdo.NomenclatureInfoForEdo?.MeasurementUnitInfoForEdo?.Digits ?? 2);
			
			var product = new FajlDokumentTablSchFaktSvedTov
			{
				Akciz = new SumAkcizTip
				{
					Item = SumAkcizTipBezAkciz.bezakciza
				},
				
				NomStr = row.ToString(),
				NaimTov = orderItemInfoForEdo.NomenclatureInfoForEdo?.OfficialName,
				OKEI_Tov = orderItemInfoForEdo.NomenclatureInfoForEdo.MeasurementUnitInfoForEdo.OKEI,
				NaimEdIzm = orderItemInfoForEdo.NomenclatureInfoForEdo.MeasurementUnitInfoForEdo.Name,
				KolTov = count,
				CenaTov = orderItemInfoForEdo.PriceWithoutVat,
				StTovBezNDS = orderItemInfoForEdo.SumWithoutVat,
				NalSt = GetProductTaxRate(orderItemInfoForEdo.ValueAddedTax),
				StTovUchNal = orderItemInfoForEdo.ActualSum,
				CenaTovSpecified = true,
				KolTovSpecified = true,
				StTovBezNDSSpecified = true,
				
				DopSvedTov = new FajlDokumentTablSchFaktSvedTovDopSvedTov
				{
					KodTov = GetProductCode(counterpartySpecialNomenclatures, orderItemInfoForEdo.NomenclatureInfoForEdo.Id)
				}
			};

			if(!string.IsNullOrWhiteSpace(orderItemInfoForEdo.NomenclatureInfoForEdo?.Gtin))
			{
				product.DopSvedTov.NomSredIdentTov = new[]
				{
					new FajlDokumentTablSchFaktSvedTovDopSvedTovNomSredIdentTov
					{
						ItemsElementName = new[]
						{
							ItemsChoiceType.NomUpak
						},
						Items = new[]
						{
							new PackageNumberConverter().ConvertGtinToPackageNumberUpd(orderItemInfoForEdo.NomenclatureInfoForEdo.Gtin, count)
						}
					}
				};
			}
			
			product.SumNal = new SumNDSTip
			{
				Item = GetTax(orderItemInfoForEdo, product.NalSt)
			};

			return product;
		}
		
		private FajlDokumentTablSchFaktSvedTov ConvertOrderItemToUpdProduct(ProductInfo product, int row)
		{
			var count = product.Count;
			
			var updProduct = new FajlDokumentTablSchFaktSvedTov
			{
				Akciz = new SumAkcizTip
				{
					Item = SumAkcizTipBezAkciz.bezakciza
				},
				
				NomStr = row.ToString(),
				NaimTov = product.Name,
				OKEI_Tov = product.OKEI,
				NaimEdIzm = product.UnitName,
				KolTov = count,
				CenaTov = product.PriceWithoutVat,
				CenaTovSpecified = true,
				KolTovSpecified = true,
				NalSt = GetProductTaxRate(product.ValueAddedTax),
				StTovUchNal = product.Sum,
				StTovBezNDS = product.SumWithoutVat,
				StTovBezNDSSpecified = true,
				DopSvedTov = new FajlDokumentTablSchFaktSvedTovDopSvedTov
				{
					KodTov = product.Code
				}
			};

			if(product.EconomicLifeFacts != null && product.EconomicLifeFacts.Any())
			{
				updProduct.InfPolFHZh2 = product.EconomicLifeFacts
					.Select(x => new TekstInfTip { Identif = x.Id, Znachen = x.Value })
					.ToArray();
			}

			if(product.TrueMarkCodes.Any())
			{
				var codesWithoutTransport = product.TrueMarkCodes
					.Where(x => x.TransportCode.IsNullOrWhiteSpace());

				var codesPerTransport = product.TrueMarkCodes
					.Where(x => !x.TransportCode.IsNullOrWhiteSpace())
					.GroupBy(x => x.TransportCode);

				var sredIdentTovList = new List<FajlDokumentTablSchFaktSvedTovDopSvedTovNomSredIdentTov>();

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

				updProduct.DopSvedTov.NomSredIdentTov = sredIdentTovList.ToArray();
			}
			
			updProduct.SumNal = new SumNDSTip
			{
				Item = GetTax(product, updProduct.NalSt)
			};

			return updProduct;
		}

		private IEnumerable<FajlDokumentTablSchFaktSvedTovDopSvedTovNomSredIdentTov> CreateSredIdentTov(
			IEnumerable<ProductCodeInfo> codes, 
			string transportCode = null
			)
		{
			if(!transportCode.IsNullOrWhiteSpace())
			{
				return new []{ new FajlDokumentTablSchFaktSvedTovDopSvedTovNomSredIdentTov { IdentTransUpak = transportCode} };
			}

			var identificationData = new List<FajlDokumentTablSchFaktSvedTovDopSvedTovNomSredIdentTov>();
			var groupedCodesByType = codes.ToLookup(x => x.IsGroup);

			foreach(var groupCodesByType in groupedCodesByType)
			{
				var identificationInfo = new FajlDokumentTablSchFaktSvedTovDopSvedTovNomSredIdentTov();
				var items = new List<(ItemsChoiceType Type, string Code)>();
				
				items.AddRange(
					from code in groupCodesByType
					let itemType = code.IsGroup
						? ItemsChoiceType.NomUpak
						: ItemsChoiceType.KIZ
					select (itemType, code.IndividualOrGroupCode));

				identificationInfo.ItemsElementName = items.Select(x => x.Type).ToArray();
				identificationInfo.Items = items.Select(x => x.Code).ToArray();
				identificationData.Add(identificationInfo);
			}

			return identificationData;
		}

		private string GetProductCode(IEnumerable<SpecialNomenclatureInfoForEdo> counterpartySpecialNomenclatures, int nomenclatureId)
		{
			var specialNomenclature = counterpartySpecialNomenclatures.SingleOrDefault(x => x.NomenclatureId == nomenclatureId);

			return specialNomenclature != null
				? specialNomenclature.SpecialId.ToString()
				: nomenclatureId.ToString();
		}

		private FajlDokumentTablSchFaktSvedTovNalSt GetProductTaxRate(decimal? orderItemTax)
		{
			switch(orderItemTax)
			{
				case null:
				case 0m:
					return FajlDokumentTablSchFaktSvedTovNalSt.bezNDS;
				case 0.10m:
					return FajlDokumentTablSchFaktSvedTovNalSt.Item10;
				case 0.18m:
					throw new InvalidOperationException("Не должно быть НДС 18% в УПД формата 5.03");
				case 0.20m:
					return FajlDokumentTablSchFaktSvedTovNalSt.Item20;
				case 0.22m:
					return FajlDokumentTablSchFaktSvedTovNalSt.Item22;
				default:
					return FajlDokumentTablSchFaktSvedTovNalSt.bezNDS;
			}
		}

		private object GetTax(OrderItemInfoForEdo orderItemInfoForEdo, FajlDokumentTablSchFaktSvedTovNalSt taxRate)
		{
			if(taxRate == FajlDokumentTablSchFaktSvedTovNalSt.bezNDS)
			{
				return SumNDSTipBezNDS.bezNDS;
			}

			return orderItemInfoForEdo.IncludeNDS;
		}
		
		private object GetTax(ProductInfo product, FajlDokumentTablSchFaktSvedTovNalSt taxRate)
		{
			if(taxRate == FajlDokumentTablSchFaktSvedTovNalSt.bezNDS)
			{
				return SumNDSTipBezNDS.bezNDS;
			}

			return product.IncludeVat;
		}
	}
}
