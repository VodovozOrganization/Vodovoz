using System;
using System.Linq;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Models
{
	public class NomenclatureCostPurchasePriceModel
	{
		public void CreatePrice(Nomenclature nomenclature, DateTime startDate)
		{
			if(!CanCreatePrice(nomenclature, startDate))
			{
				throw new InvalidOperationException($"Невозможно создать цену на дату {startDate}, так как есть активная цена с более поздней датой начала");
			}

			CloseActivePrice(nomenclature, startDate);
			var newPrice = new NomenclatureCostPurchasePrice();
			newPrice.Nomenclature = nomenclature;
			newPrice.StartDate = startDate;
			nomenclature.ObservablePurchasePrices.Add(newPrice);
		}

		public bool CanCreatePrice(Nomenclature nomenclature, DateTime startDate)
		{
			if(nomenclature is null)
			{
				throw new ArgumentNullException(nameof(nomenclature));
			}
			var activePrice = GetActivePrice(nomenclature);
			if(activePrice != null && activePrice.StartDate >= startDate)
			{
				return false;
			}

			return true;
		}

		public void ChangeDate(Nomenclature nomenclature, NomenclatureCostPurchasePrice price, DateTime startDate)
		{
			if(!CanChangeDate(nomenclature, price, startDate))
			{
				throw new InvalidOperationException($"Невозможно изменить дату цены, так как дата {startDate} меньше даты начала предидущей цены или больше даты окончания текущей цены");
			}

			var previousPrice = nomenclature.PurchasePrices.Where(x => x.EndDate < startDate).OrderByDescending(x => x.EndDate).FirstOrDefault();
			if(previousPrice != null)
			{
				previousPrice.EndDate = GetCloseTime(startDate);
			}

			price.StartDate = startDate;
		}

		public bool CanChangeDate(Nomenclature nomenclature, NomenclatureCostPurchasePrice price, DateTime startDate)
		{
			if(nomenclature is null)
			{
				throw new ArgumentNullException(nameof(nomenclature));
			}

			if(price is null)
			{
				throw new ArgumentNullException(nameof(price));
			}

			if(price.EndDate.HasValue && startDate > price.EndDate.Value)
			{
				return false;
			}

			var previousPrice = nomenclature.PurchasePrices.Where(x => x.EndDate < price.StartDate).OrderByDescending(x => x.EndDate).FirstOrDefault();
			if(previousPrice == null)
			{
				return true;
			}

			if(previousPrice.EndDate > startDate)
			{
				return false;
			}

			return true;
		}

		public void CloseActivePrice(Nomenclature nomenclature, DateTime startDate)
		{
			var activePrice = GetActivePrice(nomenclature);
			if(activePrice == null)
			{
				return;
			}

			activePrice.EndDate = GetCloseTime(startDate);
		}

		private NomenclatureCostPurchasePrice GetActivePrice(Nomenclature nomenclature)
		{
			var unclosedPrices = nomenclature.PurchasePrices.Where(x => x.EndDate == null);
			var unclosedPriceCount = unclosedPrices.Count();
			if(unclosedPriceCount > 1)
			{
				throw new InvalidOperationException($"Существует несколько открытых цен, невозможно определить какой датой закрывать каждый из них.");
			}

			return unclosedPrices.FirstOrDefault();
		}

		private DateTime GetCloseTime(DateTime startDate)
		{
			return startDate.AddMilliseconds(-1);
		}
	}
}
