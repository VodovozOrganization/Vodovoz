﻿using System;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.EntityFactories;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using QS.DomainModel.UoW;

namespace Vodovoz.Domain
{
	public class NomenclatureFixedPriceController : INomenclatureFixedPriceProvider 
    {
        private readonly INomenclatureFixedPriceFactory nomenclatureFixedPriceFactory;
        private readonly WaterFixedPricesGenerator waterFixedPricesGenerator;

        public NomenclatureFixedPriceController(
			INomenclatureFixedPriceFactory nomenclatureFixedPriceFactory,
            WaterFixedPricesGenerator waterFixedPricesGenerator) 
		{
            this.nomenclatureFixedPriceFactory = nomenclatureFixedPriceFactory ??
            	throw new ArgumentNullException(nameof(nomenclatureFixedPriceFactory));
            this.waterFixedPricesGenerator = waterFixedPricesGenerator ??
            	throw new ArgumentNullException(nameof(waterFixedPricesGenerator));
        }
        
        public bool ContainsFixedPrice(Order order, Nomenclature nomenclature) 
        {
            if (order.DeliveryPoint != null)
                return ContainsFixedPrice(order.DeliveryPoint, nomenclature);

            return ContainsFixedPrice(order.Client, nomenclature);
        }

        public bool TryGetFixedPrice(Order order, Nomenclature nomenclature, out decimal fixedPrice) 
        {
            NomenclatureFixedPrice nomenclatureFixedPrice; 
            
            if (order.DeliveryPoint != null) {
                nomenclatureFixedPrice =
                    order.DeliveryPoint.NomenclatureFixedPrices.SingleOrDefault(x =>
                        x.Nomenclature.Id == nomenclature.Id);

                if (nomenclatureFixedPrice != null) {
                    fixedPrice = nomenclatureFixedPrice.Price;
                    return true;
                }
            }
            else {
                nomenclatureFixedPrice = 
                    order.Client.ObservableNomenclatureFixedPrices.SingleOrDefault(x =>
                        x.Nomenclature.Id == nomenclature.Id);
                
                if (nomenclatureFixedPrice != null) {
                    fixedPrice = nomenclatureFixedPrice.Price;
                    return true;
                }
            }

            fixedPrice = default(int);
            return false;
        }

        public bool ContainsFixedPrice(Counterparty counterparty, Nomenclature nomenclature) => 
            counterparty.ObservableNomenclatureFixedPrices.Any(x => x.Nomenclature.Id == nomenclature.Id);

        public bool TryGetFixedPrice(Counterparty counterparty, Nomenclature nomenclature, out decimal fixedPrice) 
        {
            var nomenclatureFixedPrice = counterparty.ObservableNomenclatureFixedPrices
            	.SingleOrDefault(x => x.Nomenclature.Id == nomenclature.Id);

            return CheckFixedPrice(out fixedPrice, nomenclatureFixedPrice);
        }

        public bool ContainsFixedPrice(DeliveryPoint deliveryPoint, Nomenclature nomenclature) => 
            deliveryPoint.ObservableNomenclatureFixedPrices.Any(x => x.Nomenclature == nomenclature);

        public bool TryGetFixedPrice(DeliveryPoint deliveryPoint, Nomenclature nomenclature, out decimal fixedPrice) 
        {
            var nomenclatureFixedPrice = deliveryPoint.ObservableNomenclatureFixedPrices
            	.SingleOrDefault(x => x.Nomenclature.Id == nomenclature.Id);

            return CheckFixedPrice(out fixedPrice, nomenclatureFixedPrice);
        }

        private bool CheckFixedPrice(out decimal fixedPrice, NomenclatureFixedPrice nomenclatureFixedPrice) 
        {
            if (nomenclatureFixedPrice != null) {
                fixedPrice = nomenclatureFixedPrice.Price;
                return true;
            }

            fixedPrice = default(int);
            return false;
        }
        
        public void AddOrUpdateFixedPrice(IUnitOfWork uow, DeliveryPoint deliveryPoint, Nomenclature nomenclature, decimal fixedPrice = 0, int minCount = 0, int nomenclatureFixedPriceId = 0)
		{
			if(uow == null)
			{
				throw new ArgumentNullException(nameof(uow));
			}

			if(deliveryPoint == null)
			{
				throw new ArgumentNullException(nameof(deliveryPoint));
			}

			if(nomenclature == null)
			{
				throw new ArgumentNullException(nameof(nomenclature));
			}
			
			if(nomenclature.Category == NomenclatureCategory.water)
			{
				AddOrUpdateWaterFixedPrice(uow, deliveryPoint, nomenclature, fixedPrice, minCount);
			}
            else 
			{
                throw new NotSupportedException("Не поддерживается.");
            }
        }
        
        public void AddOrUpdateFixedPrice(IUnitOfWork uow, Counterparty counterparty, Nomenclature nomenclature, decimal fixedPrice = 0, int minCount = 0, int nomenclatureFixedPriceId = 0) 
		{
			if(uow == null)
			{
				throw new ArgumentNullException(nameof(uow));
			}

			if(counterparty == null)
			{
				throw new ArgumentNullException(nameof(counterparty));
			}

			if(nomenclature == null)
			{
				throw new ArgumentNullException(nameof(nomenclature));
			}

			if(nomenclature.Category == NomenclatureCategory.water)
			{
				AddOrUpdateWaterFixedPrice(uow, counterparty, nomenclature, fixedPrice, minCount, nomenclatureFixedPriceId);
			}
            else 
			{
                throw new NotSupportedException("Не поддерживается.");
            }
        }

        public void DeleteFixedPrice(DeliveryPoint deliveryPoint, NomenclatureFixedPrice nomenclatureFixedPrice) 
        {
            if (deliveryPoint.ObservableNomenclatureFixedPrices.Contains(nomenclatureFixedPrice)) {
                deliveryPoint.ObservableNomenclatureFixedPrices.Remove(nomenclatureFixedPrice);
            }
        }
        
        public void DeleteFixedPrice(Counterparty counterparty, NomenclatureFixedPrice nomenclatureFixedPrice) 
        {
            if (counterparty.ObservableNomenclatureFixedPrices.Contains(nomenclatureFixedPrice)) {
                counterparty.ObservableNomenclatureFixedPrices.Remove(nomenclatureFixedPrice);
            }
        }

  //      private void AddOrUpdateWaterFixedPrice(IUnitOfWork uow,  DeliveryPoint deliveryPoint, Nomenclature nomenclature, decimal fixedPrice, int minCount) 
  //      {
  //          var fixedPrices = waterFixedPricesGenerator.GenerateFixedPricesForAllWater(nomenclature.Id, fixedPrice);

  //          foreach (var pricePair in fixedPrices) {
  //              var foundFixedPrice = deliveryPoint.NomenclatureFixedPrices.SingleOrDefault(x => x.Nomenclature.Id == pricePair.Key);
  //              if (foundFixedPrice == null) {
  //                  var newNomenclature = uow.GetById<Nomenclature>(pricePair.Key);
  //                  var nomenclatureFixedPrice = CreateNewNomenclatureFixedPrice(newNomenclature, pricePair.Value);
  //                  nomenclatureFixedPrice.DeliveryPoint = deliveryPoint;
		//			nomenclatureFixedPrice.MinCount = minCount;
		//			deliveryPoint.ObservableNomenclatureFixedPrices.Add(nomenclatureFixedPrice);
  //              } else {
  //                  foundFixedPrice.Price = pricePair.Value;
		//			foundFixedPrice.MinCount = minCount;

		//		}
  //          }
		//}

		private void AddOrUpdateWaterFixedPrice(IUnitOfWork uow, DeliveryPoint deliveryPoint, Nomenclature nomenclature, decimal fixedPrice = 0, int minCount = 0, int nomenclatureFixedPriceId = 0)
		{
			var foundFixedPrice = deliveryPoint.NomenclatureFixedPrices.SingleOrDefault(x => x.Id == nomenclatureFixedPriceId);

			if(foundFixedPrice == null && fixedPrice == 0 && minCount == 0)
			{
				var nomenclatureFixedPrice = CreateNewNomenclatureFixedPrice(nomenclature, fixedPrice, minCount);
				nomenclatureFixedPrice.DeliveryPoint = deliveryPoint;
				nomenclatureFixedPrice.MinCount = minCount;
				deliveryPoint.ObservableNomenclatureFixedPrices.Add(nomenclatureFixedPrice);
			}
			else
			{
				foundFixedPrice.Price = fixedPrice;
				foundFixedPrice.MinCount = minCount;
			}
		}

		private void AddOrUpdateWaterFixedPrice(IUnitOfWork uow, Counterparty counterparty, Nomenclature nomenclature, decimal fixedPrice = 0, int minCount = 0, int nomenclatureFixedPriceId = 0)
		{
			var foundFixedPrice = counterparty.NomenclatureFixedPrices.SingleOrDefault(x => x.Id == nomenclatureFixedPriceId);

			if(foundFixedPrice == null && fixedPrice == 0 && minCount == 0)
			{
				var nomenclatureFixedPrice = CreateNewNomenclatureFixedPrice(nomenclature, fixedPrice, minCount);
				nomenclatureFixedPrice.Counterparty = counterparty;
				nomenclatureFixedPrice.MinCount = minCount;
				counterparty.ObservableNomenclatureFixedPrices.Add(nomenclatureFixedPrice);
			}
			else
			{
				foundFixedPrice.Price = fixedPrice;
				foundFixedPrice.MinCount = minCount;
			}	
		}

		//private void AddOrUpdateWaterFixedPrice(IUnitOfWork uow, Counterparty counterparty, Nomenclature nomenclature, decimal fixedPrice, int minCount)
		//{
		//	var fixedPrices = waterFixedPricesGenerator.GenerateFixedPricesForAllWater(nomenclature.Id, fixedPrice);

		//	foreach(var pricePair in fixedPrices)
		//	{
		//		var foundFixedPrice = counterparty.NomenclatureFixedPrices.SingleOrDefault(x => x.Nomenclature.Id == pricePair.Key && x.MinCount == minCount);
		//		if(foundFixedPrice == null)
		//		{
		//			var newNomenclature = uow.GetById<Nomenclature>(pricePair.Key);
		//			var nomenclatureFixedPrice = CreateNewNomenclatureFixedPrice(newNomenclature, pricePair.Value);
		//			nomenclatureFixedPrice.Counterparty = counterparty;
		//			nomenclatureFixedPrice.MinCount = minCount;
		//			counterparty.ObservableNomenclatureFixedPrices.Add(nomenclatureFixedPrice);
		//		}
		//		else
		//		{
		//			foundFixedPrice.Price = pricePair.Value;
		//			foundFixedPrice.MinCount = minCount;
		//		}
		//	}
		//}

		private NomenclatureFixedPrice CreateNewNomenclatureFixedPrice(Nomenclature nomenclature, decimal fixedPrice, int minCount = 0) 
        {
            var nomenclatureFixedPrice = nomenclatureFixedPriceFactory.Create();
            nomenclatureFixedPrice.Nomenclature = nomenclature;
            nomenclatureFixedPrice.Price = fixedPrice;
			nomenclatureFixedPrice.MinCount = minCount;

			return nomenclatureFixedPrice;
        }
    }
}
