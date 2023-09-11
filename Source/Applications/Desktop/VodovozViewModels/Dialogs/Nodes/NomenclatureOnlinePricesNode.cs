using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.ViewModels.Dialogs.Nodes
{
	public class NomenclatureOnlinePricesNode
	{
		public decimal MinCount
		{
			get
			{
				if(MobileAppNomenclatureOnlinePrice != null)
				{
					return MobileAppNomenclatureOnlinePrice.NomenclaturePrice.MinCount;
				}
				
				return KulerSaleWebSiteNomenclatureOnlinePrice != null
					? KulerSaleWebSiteNomenclatureOnlinePrice.NomenclaturePrice.MinCount
					: default(decimal);
			}
		}
		public NomenclatureOnlinePrice MobileAppNomenclatureOnlinePrice { get; set; }
		public NomenclatureOnlinePrice VodovozWebSiteNomenclatureOnlinePrice { get; set; }
		public NomenclatureOnlinePrice KulerSaleWebSiteNomenclatureOnlinePrice { get; set; }
		public decimal? NomenclaturePrice => MobileAppNomenclatureOnlinePrice?.NomenclaturePrice.Price;
		public decimal? KulerSalePrice => KulerSaleWebSiteNomenclatureOnlinePrice?.NomenclaturePrice.Price;

		public string MobileAppPriceWithoutDiscountString
		{
			get => MobileAppNomenclatureOnlinePrice?.PriceWithoutDiscount.ToString();
			set
			{
				if(MobileAppNomenclatureOnlinePrice is null)
				{
					return;
				}
				
				if(string.IsNullOrWhiteSpace(value))
				{
					MobileAppNomenclatureOnlinePrice.PriceWithoutDiscount = null;
					return;
				}

				MobileAppNomenclatureOnlinePrice.PriceWithoutDiscount = decimal.Parse(value);
			}
		} 

		public string VodovozWebSitePriceWithoutDiscountString
		{
			get => VodovozWebSiteNomenclatureOnlinePrice?.PriceWithoutDiscount.ToString();
			set
			{
				if(VodovozWebSiteNomenclatureOnlinePrice is null)
				{
					return;
				}
				
				if(string.IsNullOrWhiteSpace(value))
				{
					VodovozWebSiteNomenclatureOnlinePrice.PriceWithoutDiscount = null;
					return;
				}

				VodovozWebSiteNomenclatureOnlinePrice.PriceWithoutDiscount = decimal.Parse(value);
			}
		}

		public string KulerSaleWebSitePriceWithoutDiscountString
		{
			get => KulerSaleWebSiteNomenclatureOnlinePrice?.PriceWithoutDiscount.ToString();
			set
			{
				if(KulerSaleWebSiteNomenclatureOnlinePrice is null)
				{
					return;
				}
				
				if(string.IsNullOrWhiteSpace(value))
				{
					KulerSaleWebSiteNomenclatureOnlinePrice.PriceWithoutDiscount = null;
					return;
				}

				KulerSaleWebSiteNomenclatureOnlinePrice.PriceWithoutDiscount = decimal.Parse(value);
			}
		}
		public bool CanChangeMobileAppPriceWithoutDiscount => NomenclaturePrice.HasValue;
		public bool CanChangeVodovozWebSitePriceWithoutDiscount => NomenclaturePrice.HasValue;
		public bool CanChangeKulerSaleWebSitePriceWithoutDiscount => KulerSalePrice.HasValue;
	}
}
