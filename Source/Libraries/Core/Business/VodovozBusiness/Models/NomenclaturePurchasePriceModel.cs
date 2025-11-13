using QS.Services;
using System;
using System.Linq;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Models
{
	public class NomenclaturePurchasePriceModel : INomenclaturePurchasePriceModel
	{
		private readonly ICurrentPermissionService _permissionService;

		public NomenclaturePurchasePriceModel(ICurrentPermissionService permissionService)
		{
			_permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
		}

		public NomenclaturePurchasePrice CreatePrice(Nomenclature nomenclature, DateTime startDate)
		{
			if(!CanCreatePrice(nomenclature, startDate))
			{
				throw new InvalidOperationException($"Невозможно создать цену на дату {startDate}, так как есть активная цена с более поздней датой начала");
			}

			CloseActivePrice(nomenclature, startDate);
			var newPrice = new NomenclaturePurchasePrice();
			newPrice.Nomenclature = nomenclature;
			newPrice.StartDate = startDate;
			nomenclature.PurchasePrices.Add(newPrice);
			return newPrice;
		}

		public NomenclaturePurchasePrice CreatePrice(Nomenclature nomenclature, DateTime startDate, decimal price)
		{
			var newPrice = CreatePrice(nomenclature, startDate);
			newPrice.PurchasePrice = price;
			return newPrice;
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

		public bool CanCreatePrice(Nomenclature nomenclature, DateTime startDate, decimal newPrice)
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

			if(newPrice == 0 || (activePrice != null && activePrice.PurchasePrice == newPrice))
			{
				return false;
			}

			return true;
		}

		public void ChangeDate(Nomenclature nomenclature, NomenclaturePurchasePrice price, DateTime startDate)
		{
			if(!CanChangeDate(nomenclature, price, startDate))
			{
				throw new InvalidOperationException($"Невозможно изменить дату цены, так как дата {startDate} меньше даты начала предыдущей цены или больше даты окончания текущей цены");
			}

			var previousPrice = nomenclature.PurchasePrices.Where(x => x.EndDate < price.StartDate).OrderByDescending(x => x.EndDate).FirstOrDefault();
			if(previousPrice != null)
			{
				previousPrice.EndDate = GetCloseTime(startDate);
			}

			price.StartDate = startDate;
		}

		public bool CanChangeDate(Nomenclature nomenclature, NomenclaturePurchasePrice price, DateTime startDate)
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

			if(previousPrice.StartDate >= startDate)
			{
				return false;
			}

			if(_permissionService.ValidatePresetPermission("can_change_nomenclature_price_date") && previousPrice.StartDate < startDate)
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

		public NomenclaturePurchasePrice GetActivePrice(Nomenclature nomenclature)
		{
			var unclosedPrices = nomenclature.PurchasePrices.Where(x => x.EndDate == null);
			var unclosedPriceCount = unclosedPrices.Count();
			if(unclosedPriceCount > 1)
			{
				throw new InvalidOperationException($"Существует несколько открытых цен, невозможно определить какой датой закрывать каждый из них.");
			}

			return unclosedPrices.FirstOrDefault();
		}

		public NomenclaturePurchasePrice GetPrice(DateTime date, Nomenclature nomenclature)
		{
			if(nomenclature is null)
			{
				throw new ArgumentNullException(nameof(nomenclature));
			}

			var prices = nomenclature.PurchasePrices
				.Where(x => x.StartDate <= date)
				.Where(x => x.EndDate == null || x.EndDate.Value >= date);
			if(prices.Count() > 1)
			{
				throw new InvalidOperationException($"Невозможно получить цену. На дату {date} в номенклатуре {nomenclature.Name} найдены несколько цен закупки или себестоимости.");
			}

			return prices.SingleOrDefault();
		}

		private DateTime GetCloseTime(DateTime startDate)
		{
			return startDate.AddMilliseconds(-1);
		}
	}
}
