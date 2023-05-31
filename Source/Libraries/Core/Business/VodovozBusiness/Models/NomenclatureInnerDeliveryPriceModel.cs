using QS.Services;
using System;
using System.Linq;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Models
{
	public class NomenclatureInnerDeliveryPriceModel : INomenclatureInnerDeliveryPriceModel
	{
		private readonly ICurrentPermissionService _permissionService;

		public NomenclatureInnerDeliveryPriceModel(ICurrentPermissionService permissionService)
		{
			_permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
		}

		public NomenclatureInnerDeliveryPrice CreatePrice(Nomenclature nomenclature, DateTime startDate)
		{
			if(!CanCreatePrice(nomenclature, startDate))
			{
				throw new InvalidOperationException($"Невозможно создать цену на дату {startDate}, так как есть активная цена с более поздней датой начала");
			}

			CloseActivePrice(nomenclature, startDate);
			var newPrice = new NomenclatureInnerDeliveryPrice();
			newPrice.Nomenclature = nomenclature;
			newPrice.StartDate = startDate;
			nomenclature.ObservableInnerDeliveryPrices.Add(newPrice);
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

		public void ChangeDate(Nomenclature nomenclature, NomenclatureInnerDeliveryPrice price, DateTime startDate)
		{
			if(!CanChangeDate(nomenclature, price, startDate))
			{
				throw new InvalidOperationException($"Невозможно изменить дату цены, так как дата {startDate} меньше даты начала предыдущей цены или больше даты окончания текущей цены");
			}

			var previousPrice = nomenclature.InnerDeliveryPrices.Where(x => x.EndDate < price.StartDate).OrderByDescending(x => x.EndDate).FirstOrDefault();
			if(previousPrice != null)
			{
				previousPrice.EndDate = GetCloseTime(startDate);
			}

			price.StartDate = startDate;
		}

		public bool CanChangeDate(Nomenclature nomenclature, NomenclatureInnerDeliveryPrice price, DateTime startDate)
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

			var previousPrice = nomenclature.InnerDeliveryPrices.Where(x => x.EndDate < price.StartDate).OrderByDescending(x => x.EndDate).FirstOrDefault();
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

		private NomenclatureInnerDeliveryPrice GetActivePrice(Nomenclature nomenclature)
		{
			var unclosedPrices = nomenclature.InnerDeliveryPrices.Where(x => x.EndDate == null);
			var unclosedPriceCount = unclosedPrices.Count();
			if(unclosedPriceCount > 1)
			{
				throw new InvalidOperationException($"Существует несколько открытых цен, невозможно определить какой датой закрывать каждый из них.");
			}

			return unclosedPrices.FirstOrDefault();
		}

		public NomenclatureInnerDeliveryPrice GetPrice(DateTime date, Nomenclature nomenclature)
		{
			if(nomenclature is null)
			{
				throw new ArgumentNullException(nameof(nomenclature));
			}

			var prices = nomenclature.InnerDeliveryPrices
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
