using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.ViewModels.Dialogs.Nodes
{
	/// <summary>
	/// Нода онлайн цен ИПЗ
	/// </summary>
	public class NomenclatureOnlinePricesNode
	{
		/// <summary>
		/// Минимальное количество, при котором будет действовать цена
		/// </summary>
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
		
		/// <summary>
		/// Цена для МП
		/// </summary>
		public NomenclatureOnlinePrice MobileAppNomenclatureOnlinePrice { get; set; }
		
		/// <summary>
		/// Цена для сайта ВВ
		/// </summary>
		public NomenclatureOnlinePrice VodovozWebSiteNomenclatureOnlinePrice { get; set; }
		
		/// <summary>
		/// Цена для сайта Кулер сэйл
		/// </summary>
		public NomenclatureOnlinePrice KulerSaleWebSiteNomenclatureOnlinePrice { get; set; }
		
		/// <summary>
		/// Цена для ИИ бота
		/// </summary>
		public NomenclatureOnlinePrice AiBotNomenclatureOnlinePrice { get; set; }
		
		/// <summary>
		/// Прайсовая цена
		/// </summary>
		public decimal? NomenclaturePrice => MobileAppNomenclatureOnlinePrice?.NomenclaturePrice.Price;
		
		/// <summary>
		/// Альтернативная цена(для Кулер сэйл)
		/// </summary>
		public decimal? KulerSalePrice => KulerSaleWebSiteNomenclatureOnlinePrice?.NomenclaturePrice.Price;

		/// <summary>
		/// Цена без скидки для МП
		/// </summary>
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

		/// <summary>
		/// Цена без скидки для сайта ВВ
		/// </summary>
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

		/// <summary>
		/// Цена без скидки для сайта Кулер сэйл
		/// </summary>
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
		
		/// <summary>
		/// Цена без скидки для ИИ бота
		/// </summary>
		public string AiBotPriceWithoutDiscountString
		{
			get => AiBotNomenclatureOnlinePrice?.PriceWithoutDiscount.ToString();
			set
			{
				if(AiBotNomenclatureOnlinePrice is null)
				{
					return;
				}
				
				if(string.IsNullOrWhiteSpace(value))
				{
					AiBotNomenclatureOnlinePrice.PriceWithoutDiscount = null;
					return;
				}

				AiBotNomenclatureOnlinePrice.PriceWithoutDiscount = decimal.Parse(value);
			}
		}
		
		/// <summary>
		/// Возможность изменять цену без скидки для МП
		/// </summary>
		public bool CanChangeMobileAppPriceWithoutDiscount => NomenclaturePrice.HasValue;
		
		/// <summary>
		/// Возможность изменять цену без скидки для сайта ВВ
		/// </summary>
		public bool CanChangeVodovozWebSitePriceWithoutDiscount => NomenclaturePrice.HasValue;
		
		/// <summary>
		/// Возможность изменять цену без скидки для сайта Кулер сэйл
		/// </summary>
		public bool CanChangeKulerSaleWebSitePriceWithoutDiscount => KulerSalePrice.HasValue;
		
		/// <summary>
		/// Возможность изменять цену без скидки для ИИ бота
		/// </summary>
		public bool CanChangeAiBotPriceWithoutDiscount => NomenclaturePrice.HasValue;
	}
}
