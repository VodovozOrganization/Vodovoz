using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.Extensions
{
	public static class NomenclatureOnlineParametersExtensions
	{
		public static void AddNewNomenclatureOnlinePrice(this NomenclatureOnlineParameters parameters, NomenclatureOnlinePrice price)
		{
			if(price is null)
			{
				return;
			}

			price.NomenclatureOnlineParameters = parameters;
			parameters.NomenclatureOnlinePrices.Add(price);
		}
		
		public static void RemoveNomenclatureOnlinePrice(this NomenclatureOnlineParameters parameters, NomenclatureOnlinePrice price)
		{
			if(price is null)
			{
				return;
			}

			if(!parameters.NomenclatureOnlinePrices.Contains(price))
			{
				return;
			}

			parameters.NomenclatureOnlinePrices.Remove(price);
		}
	}
}
