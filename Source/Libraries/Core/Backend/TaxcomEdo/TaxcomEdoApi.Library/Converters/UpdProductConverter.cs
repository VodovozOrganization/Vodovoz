using System;
using System.Collections.Generic;
using System.Linq;
using Edo.Contracts.Messages.Dto;
using Taxcom.Client.Api.Document.DocumentByFormat1115131;
using TaxcomEdo.Contracts.Goods;
using TaxcomEdo.Contracts.Orders;

namespace TaxcomEdoApi.Library.Converters
{
	public class UpdProductConverter : IUpdProductConverter
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
					NaimEdIzm = orderItemInfoForEdo.NomenclatureInfoForEdo.MeasurementUnitInfoForEdo.Name,
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
				CenaTov = product.PriceWithoutVat,
				CenaTovSpecified = true,
				KolTov = count,
				KolTovSpecified = true,
				NaimTov = product.Name,
				NalSt = GetProductTaxRate(product.ValueAddedTax),
				StTovUchNal = product.Sum,
				StTovBezNDS = product.SumWithoutVat,
				StTovBezNDSSpecified = true,
				OKEI_Tov = product.OKEI,
				DopSvedTov = new FajlDokumentTablSchFaktSvedTovDopSvedTov
				{
					NaimEdIzm = product.UnitName,
					KodTov = product.Code
				}
			};

			if(product.TrueMarkCodes.Any())
			{
				updProduct.DopSvedTov.NomSredIdentTov = new[]
				{
					new FajlDokumentTablSchFaktSvedTovDopSvedTovNomSredIdentTov
					{
						ItemsElementName = new[]
						{
							ItemsChoiceType.KIZ
						},
						Items = product.TrueMarkCodes.ToArray()
					}
				};
			}
			
			updProduct.SumNal = new SumNDSTip
			{
				Item = GetTax(product, updProduct.NalSt)
			};

			return updProduct;
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
					return FajlDokumentTablSchFaktSvedTovNalSt.Item18;
				case 0.20m:
					return FajlDokumentTablSchFaktSvedTovNalSt.Item20;
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
