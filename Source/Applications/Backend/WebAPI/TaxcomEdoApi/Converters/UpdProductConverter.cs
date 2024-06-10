using System;
using System.Collections.Generic;
using System.Linq;
using Taxcom.Client.Api.Document.DocumentByFormat1115131;
using Vodovoz.Domain.Orders;

namespace TaxcomEdoApi.Converters
{
	public class UpdProductConverter : IUpdProductConverter
	{
		public FajlDokumentTablSchFaktSvedTov[] ConvertOrderItemsToUpdProducts(IList<OrderItem> orderItems)
		{
			var products = new List<FajlDokumentTablSchFaktSvedTov>();

			for(var i = 0; i < orderItems.Count; i++)
			{
				var product = ConvertOrderItemToUpdProduct(orderItems[i], i + 1);
				products.Add(product);
			}

			return products.ToArray();
		}
		
		private FajlDokumentTablSchFaktSvedTov ConvertOrderItemToUpdProduct(OrderItem orderItem, int row)
		{
			var count = Math.Round(orderItem.CurrentCount, orderItem.Nomenclature?.Unit?.Digits ?? 2);
			
			var product = new FajlDokumentTablSchFaktSvedTov
			{
				Akciz = new SumAkcizTip
				{
					Item = SumAkcizTipBezAkciz.bezakciza
				},
				
				NomStr = row.ToString(),
				CenaTov = orderItem.PriceWithoutVat,
				CenaTovSpecified = true,
				KolTov = count,
				KolTovSpecified = true,
				NaimTov = orderItem.Nomenclature?.OfficialName,
				NalSt = GetProductTaxRate(orderItem.ValueAddedTax),
				StTovUchNal = orderItem.ActualSum,
				StTovBezNDS = orderItem.SumWithoutVat,
				StTovBezNDSSpecified = true,
				OKEI_Tov = orderItem.Nomenclature.Unit.OKEI,
				DopSvedTov = new FajlDokumentTablSchFaktSvedTovDopSvedTov
				{
					NaimEdIzm = orderItem.Nomenclature.Unit.Name,
					KodTov = GetProductCode(orderItem.Order, orderItem.Nomenclature.Id)
				}
			};

			if(!string.IsNullOrWhiteSpace(orderItem.Nomenclature?.Gtin))
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
							new PackageNumberConverter().ConvertGtinToPackageNumberUpd(orderItem.Nomenclature.Gtin, count)
						}
					}
				};
			}
			product.SumNal = new SumNDSTip
			{
				Item = GetTax(orderItem, product.NalSt)
			};

			return product;
		}

		private string GetProductCode(Order order, int nomenclatureId)
		{
			var specialNomenclature = order.Client.SpecialNomenclatures.SingleOrDefault(x => x.Nomenclature.Id == nomenclatureId);

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

		private object GetTax(OrderItem orderItem, FajlDokumentTablSchFaktSvedTovNalSt taxRate)
		{
			if(taxRate == FajlDokumentTablSchFaktSvedTovNalSt.bezNDS)
			{
				return SumNDSTipBezNDS.bezNDS;
			}

			return orderItem.IncludeNDS;
		}
	}
}
