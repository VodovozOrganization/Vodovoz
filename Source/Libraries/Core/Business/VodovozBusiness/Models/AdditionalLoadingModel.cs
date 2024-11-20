using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Flyers;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.Settings.Delivery;

namespace Vodovoz.Models
{
	public class AdditionalLoadingModel : IAdditionalLoadingModel
	{
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IFlyerRepository _flyerRepository;
		private readonly IDeliveryRulesSettings _deliveryRulesSettings;
		private readonly IStockRepository _stockRepository;
		private IList<Flyer> _activeFlyers;
		private IDictionary<int, decimal> _flyersInStock;

		public AdditionalLoadingModel(IEmployeeRepository employeeRepository, IFlyerRepository flyerRepository,
			IDeliveryRulesSettings deliveryRulesSettings, IStockRepository stockRepository)
		{
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_flyerRepository = flyerRepository ?? throw new ArgumentNullException(nameof(flyerRepository));
			_deliveryRulesSettings = deliveryRulesSettings ?? throw new ArgumentNullException(nameof(deliveryRulesSettings));
			_stockRepository = stockRepository ?? throw new ArgumentNullException(nameof(stockRepository));
		}

		public AdditionalLoadingDocument CreateAdditionLoadingDocument(IUnitOfWork uow, RouteList routeList)
		{
			if(uow == null)
			{
				throw new ArgumentNullException(nameof(uow));
			}
			if(routeList == null)
			{
				throw new ArgumentNullException(nameof(routeList));
			}
			if(routeList.AdditionalLoadingDocument != null)
			{
				throw new InvalidOperationException("Может быть создан только один документ запаса");
			}

			var availableWeight = routeList.Car.CarModel.MaxWeight - routeList.GetTotalWeight();
			var availableVolume = routeList.Car.CarModel.MaxVolume - routeList.GetTotalVolume();
			if(availableWeight < 0 || availableVolume < 0)
			{
				return null;
			}

			var document = new AdditionalLoadingDocument
			{
				Author = _employeeRepository.GetEmployeeForCurrentUser(uow),
				CreationDate = DateTime.Now,
			};

			foreach(var item in CreateAdditionalLoadingItems(uow, availableWeight, availableVolume, routeList.Date))
			{
				item.AdditionalLoadingDocument = document;
				document.Items.Add(item);
			}
			return document.Items.Any() ? document : null;
		}

		private IList<AdditionalLoadingDocumentItem> CreateAdditionalLoadingItems(IUnitOfWork uow, decimal availableWeight,
			decimal availableVolume, DateTime routelistDate)
		{
			var distributions = uow.GetAll<AdditionalLoadingNomenclatureDistribution>().ToList();
			IList<AdditionalLoadingDocumentItem> items = new List<AdditionalLoadingDocumentItem>();

			//Сначала расчитывам по грузоподъёмности
			foreach(var distribution in distributions)
			{
				if(distribution.Nomenclature.Weight <= 0 || distribution.Nomenclature.Volume <= 0)
				{
					continue;
				}
				var item = new AdditionalLoadingDocumentItem
				{
					Nomenclature = distribution.Nomenclature,
					//Доступный вес в МЛ * процент распределения номенклатуры / вес номенклатуры
					Amount = Math.Floor(availableWeight * distribution.Percent / 100 / distribution.Nomenclature.Weight)
				};
				items.Add(item);
			}
			RemoveZeroAmountItemsIfNeededByWeight(items, availableWeight);
			AddFlyers(items, uow, routelistDate);

			var hasVolumeExcess = availableVolume < items.Sum(x => x.Amount * x.Nomenclature.Volume);
			if(!hasVolumeExcess)
			{
				_activeFlyers = null;
				_flyersInStock = null;
				return items;
			}

			//Если расчитали по грузоподъёмности, но не хватило места по объёму, то расчитываем по объёму
			items.Clear();
			foreach(var distribution in distributions)
			{
				if(distribution.Nomenclature.Weight <= 0 || distribution.Nomenclature.Volume <= 0)
				{
					continue;
				}
				var item = new AdditionalLoadingDocumentItem
				{
					Nomenclature = distribution.Nomenclature,
					Amount = Math.Floor(availableVolume * distribution.Percent / 100 / distribution.Nomenclature.Volume)
				};
				items.Add(item);
			}

			RemoveZeroAmountItemsIfNeededByVolume(items, availableVolume);
			AddFlyers(items, uow, routelistDate);
			_activeFlyers = null;
			_flyersInStock = null;

			return items;
		}

		public void ReloadActiveFlyers(IUnitOfWork uow, RouteList routelist, DateTime previousRoutelistDate)
		{
			if(routelist.Status != RouteListStatus.New)
			{
				return;
			}
			if(routelist.AdditionalLoadingDocument == null)
			{
				return;
			}
			
			var activeFlyersForPreviousDate = _flyerRepository.GetAllActiveFlyersByDate(uow, previousRoutelistDate);
			var items = routelist.AdditionalLoadingDocument.ObservableItems;

			foreach(var previousActiveFlyer in activeFlyersForPreviousDate)
			{
				var flyer = items.SingleOrDefault(x => x.Nomenclature.Id == previousActiveFlyer.FlyerNomenclature.Id);

				if(flyer != null)
				{
					items.Remove(flyer);
				}
			}

			_activeFlyers = null;
			_flyersInStock = null;
			AddFlyers(items, uow, routelist.Date);

			foreach(var item in routelist.AdditionalLoadingDocument.ObservableItems)
			{
				if(item.AdditionalLoadingDocument == null)
				{
					item.AdditionalLoadingDocument = routelist.AdditionalLoadingDocument;
				}
			}
		}

		private void AddFlyers(IList<AdditionalLoadingDocumentItem> items, IUnitOfWork uow, DateTime routelistDate)
		{
			var additionalFlyersEnabled = _deliveryRulesSettings.AdditionalLoadingFlyerAdditionEnabled;
			var additionalFlyersForNewCounterpartiesEnabled = _deliveryRulesSettings.FlyerForNewCounterpartyEnabled;

			if(!additionalFlyersEnabled
			&& !additionalFlyersForNewCounterpartiesEnabled)
			{
				return;
			}

			var water19LCount = items
				.Where(x => x.Nomenclature.TareVolume == TareVolume.Vol19L && x.Nomenclature.Category == NomenclatureCategory.water)
				.Sum(x => x.Amount);

			var flyerAmount = (int)water19LCount / _deliveryRulesSettings.BottlesCountForFlyer;

			var flyerForNewCounterpartiesAmount = (int)water19LCount / _deliveryRulesSettings.FlyerForNewCounterpartyBottlesCount;

			if(flyerAmount == 0 && flyerForNewCounterpartiesAmount == 0)
			{
				return;
			}

			if(_activeFlyers == null)
			{
				_activeFlyers = _flyerRepository.GetAllActiveFlyersByDate(uow, routelistDate);
			}
			if(_flyersInStock == null)
			{
				_flyersInStock = _stockRepository.NomenclatureInStock(uow, _activeFlyers.Select(x => x.FlyerNomenclature.Id).ToArray());
			}

			if(additionalFlyersEnabled)
			{
				AddFlyers(items, flyerAmount);
			}

			if(additionalFlyersForNewCounterpartiesEnabled)
			{
				AddFlyersForNewCounterparties(items, flyerForNewCounterpartiesAmount);
			}
		}

		private void AddFlyersForNewCounterparties(IList<AdditionalLoadingDocumentItem> items, int flyerForNewCounterpartiesAmount)
		{
			foreach(var flyer in _activeFlyers)
			{
				if(!flyer.IsForFirstOrder)
				{
					continue;
				}
				if(items.Any(x => x.Nomenclature.Id == flyer.FlyerNomenclature.Id))
				{
					continue;
				}
				var amount = Math.Min(_flyersInStock[flyer.FlyerNomenclature.Id], flyerForNewCounterpartiesAmount);
				if(amount == 0)
				{
					continue;
				}
				items.Add(new AdditionalLoadingDocumentItem
				{
					Nomenclature = flyer.FlyerNomenclature,
					Amount = amount
				});
			}
		}

		private void AddFlyers(IList<AdditionalLoadingDocumentItem> items, int flyerAmount)
		{
			foreach(var flyer in _activeFlyers)
			{
				if(flyer.IsForFirstOrder)
				{
					continue;
				}
				if(items.Any(x => x.Nomenclature.Id == flyer.FlyerNomenclature.Id))
				{
					continue;
				}
				var amount = Math.Min(_flyersInStock[flyer.FlyerNomenclature.Id], flyerAmount);
				if(amount == 0)
				{
					continue;
				}
				items.Add(new AdditionalLoadingDocumentItem
				{
					Nomenclature = flyer.FlyerNomenclature,
					Amount = amount
				});
			}
		}

		/// <summary>
		///  Если можем погрузить одну единицу товара, для которого алгорим распределения посчитал кол-во равное  &lt; 1, то грузим.
		///  Иначе удаляем айтем из списка. Для веса
		/// </summary>
		private static void RemoveZeroAmountItemsIfNeededByWeight(IList<AdditionalLoadingDocumentItem> items, decimal availableWeight)
		{
			foreach(var item in items.ToList())
			{
				if(item.Amount != 0)
				{
					continue;
				}

				var currentAvialableWeight = availableWeight - items.Sum(x => x.Amount * x.Nomenclature.Weight);
				if(currentAvialableWeight - item.Nomenclature.Weight > 0)
				{
					item.Amount = 1;
				}
				else
				{
					items.Remove(item);
				}
			}
		}

		/// <summary>
		///  Если можем погрузить одну единицу товара, для которого алгорим распределения посчитал кол-во равное  &lt; 1, то грузим.
		///  Иначе удаляем айтем из списка. Для объёма
		/// </summary>
		private static void RemoveZeroAmountItemsIfNeededByVolume(IList<AdditionalLoadingDocumentItem> items, decimal availableVolume)
		{
			foreach(var item in items.ToList())
			{
				if(item.Amount != 0)
				{
					continue;
				}

				var currentAvailableVolume = availableVolume - items.Sum(x => x.Amount * x.Nomenclature.Volume);
				if(currentAvailableVolume - item.Nomenclature.Volume > 0)
				{
					item.Amount = 1;
				}
				else
				{
					items.Remove(item);
				}
			}
		}
	}
}
