using System;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.EntityFactories;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain {
    public class NomenclatureFixedPriceController : INomenclatureFixedPriceProvider {
        private readonly NomenclatureFixedPriceFactory nomenclatureFixedPriceFactory;
        private readonly WaterFixedPricesGenerator waterFixedPricesGenerator;

        public NomenclatureFixedPriceController(NomenclatureFixedPriceFactory nomenclatureFixedPriceFactory,
                                                WaterFixedPricesGenerator waterFixedPricesGenerator) {
            this.nomenclatureFixedPriceFactory = nomenclatureFixedPriceFactory ??
                                                 throw new ArgumentNullException(nameof(nomenclatureFixedPriceFactory));
            this.waterFixedPricesGenerator = waterFixedPricesGenerator ??
                                             throw new ArgumentNullException(nameof(waterFixedPricesGenerator));
        }
        
        public bool ContainsFixedPrice(OrderBase order, Nomenclature nomenclature) {
            if (order.DeliveryPoint != null)
                return ContainsFixedPrice(order.DeliveryPoint, nomenclature);

            return ContainsFixedPrice(order.Counterparty, nomenclature);
        }

        public bool TryGetFixedPrice(OrderBase order, Nomenclature nomenclature, out decimal fixedPrice) {
            NomenclatureFixedPrice nomenclatureFixedPrice; 
            
            if (order.DeliveryPoint != null) {
                nomenclatureFixedPrice =
                    order.DeliveryPoint.ObservableNomenclatureFixedPrices.SingleOrDefault(x =>
                        x.Nomenclature.Id == nomenclature.Id);

                if (nomenclatureFixedPrice != null) {
                    fixedPrice = nomenclatureFixedPrice.FixedPrice;
                    return true;
                }
            }
            else {
                nomenclatureFixedPrice = 
                    order.Counterparty.ObservableNomenclatureFixedPrices.SingleOrDefault(x =>
                        x.Nomenclature.Id == nomenclature.Id);
                
                if (nomenclatureFixedPrice != null) {
                    fixedPrice = nomenclatureFixedPrice.FixedPrice;
                    return true;
                }
            }

            fixedPrice = default(int);
            return false;
        }

        public bool ContainsFixedPrice(Counterparty counterparty, Nomenclature nomenclature) => 
            counterparty.ObservableNomenclatureFixedPrices.Any(x => x.Nomenclature.Id == nomenclature.Id);

        public bool TryGetFixedPrice(Counterparty counterparty, Nomenclature nomenclature, out decimal fixedPrice) {
            var nomenclatureFixedPrice =
                counterparty.ObservableNomenclatureFixedPrices.SingleOrDefault(x =>
                    x.Nomenclature.Id == nomenclature.Id);

            return CheckFixedPrice(out fixedPrice, nomenclatureFixedPrice);
        }

        public bool ContainsFixedPrice(DeliveryPoint deliveryPoint, Nomenclature nomenclature) => 
            deliveryPoint.ObservableNomenclatureFixedPrices.Any(x => x.Nomenclature == nomenclature);

        public bool TryGetFixedPrice(DeliveryPoint deliveryPoint, Nomenclature nomenclature, out decimal fixedPrice) {
            var nomenclatureFixedPrice =
                deliveryPoint.ObservableNomenclatureFixedPrices.SingleOrDefault(x =>
                    x.Nomenclature.Id == nomenclature.Id);

            return CheckFixedPrice(out fixedPrice, nomenclatureFixedPrice);
        }

        private bool CheckFixedPrice(out decimal fixedPrice, NomenclatureFixedPrice nomenclatureFixedPrice) {
            if (nomenclatureFixedPrice != null) {
                fixedPrice = nomenclatureFixedPrice.FixedPrice;
                return true;
            }

            fixedPrice = default(int);
            return false;
        }

        public void AddOrUpdateFixedPrice(DeliveryPoint deliveryPoint, Nomenclature nomenclature, decimal fixedPrice) {
            if (!ContainsFixedPrice(deliveryPoint, nomenclature)) {
                var nomenclatureFixedPrice = CreateNewNomenclatureFixedPrice(nomenclature, fixedPrice);
                nomenclatureFixedPrice.DeliveryPoint = deliveryPoint;
                
                deliveryPoint.ObservableNomenclatureFixedPrices.Add(nomenclatureFixedPrice);
            }
            else {
                UpdateFixedPrice(deliveryPoint, nomenclature, fixedPrice);
            }
        }
        
        public void AddOrUpdateFixedPrice(Counterparty counterparty, Nomenclature nomenclature, decimal fixedPrice) {
            if (!ContainsFixedPrice(counterparty, nomenclature)) {
                var nomenclatureFixedPrice = CreateNewNomenclatureFixedPrice(nomenclature, fixedPrice);
                nomenclatureFixedPrice.Counterparty = counterparty;
                
                counterparty.ObservableNomenclatureFixedPrices.Add(nomenclatureFixedPrice);
            }
            else {
                UpdateFixedPrice(counterparty, nomenclature, fixedPrice);
            }
        }

        public void DeleteFixedPrice(DeliveryPoint deliveryPoint, NomenclatureFixedPrice nomenclatureFixedPrice) {
            var fixedPrice =
                deliveryPoint.ObservableNomenclatureFixedPrices.SingleOrDefault(x => x.Id == nomenclatureFixedPrice.Id);

            if (fixedPrice != null)
                deliveryPoint.ObservableNomenclatureFixedPrices.Remove(fixedPrice);
        }
        
        public void DeleteFixedPrice(Counterparty counterparty, NomenclatureFixedPrice nomenclatureFixedPrice) {
            var fixedPrice =
                counterparty.ObservableNomenclatureFixedPrices.SingleOrDefault(x => x.Id == nomenclatureFixedPrice.Id);

            if (fixedPrice != null)
                counterparty.ObservableNomenclatureFixedPrices.Remove(fixedPrice);
        }
        
        private void AddOrUpdateWaterFixedPrice(DeliveryPoint deliveryPoint, Nomenclature nomenclature, decimal fixedPrice) {
            if (!ContainsFixedPrice(deliveryPoint, nomenclature)) {
                AddWaterFixedPrice(deliveryPoint, nomenclature, fixedPrice);
            }
            else {
                UpdateFixedPrice(deliveryPoint, nomenclature, fixedPrice);
            }
        }
        
        private void AddOrUpdateWaterFixedPrice(Counterparty counterparty, Nomenclature nomenclature, decimal fixedPrice) {
            if (!ContainsFixedPrice(counterparty, nomenclature)) {
                AddWaterFixedPrice(counterparty, nomenclature, fixedPrice);
            }
            else {
                UpdateFixedPrice(counterparty, nomenclature, fixedPrice);
            }
        }
        
        private NomenclatureFixedPrice CreateNewNomenclatureFixedPrice(Nomenclature nomenclature, decimal fixedPrice) {
            var nomenclatureFixedPrice = nomenclatureFixedPriceFactory.Create();
            nomenclatureFixedPrice.Nomenclature = nomenclature;
            nomenclatureFixedPrice.FixedPrice = fixedPrice;

            return nomenclatureFixedPrice;
        }

        private void AddWaterFixedPrice(DeliveryPoint deliveryPoint, Nomenclature nomenclature, decimal fixedPrice) {
            var fixedPrices = waterFixedPricesGenerator.GenerateFixedPricesForAllWater(nomenclature, fixedPrice);

            foreach (var price in fixedPrices) {
                if (!ContainsFixedPrice(deliveryPoint, price.Key)) {
                    var nomenclatureFixedPrice = CreateNewNomenclatureFixedPrice(price.Key, price.Value);
                    nomenclatureFixedPrice.DeliveryPoint = deliveryPoint;

                    deliveryPoint.ObservableNomenclatureFixedPrices.Add(nomenclatureFixedPrice);
                }
            }
        }
        
        private void AddWaterFixedPrice(Counterparty counterparty, Nomenclature nomenclature, decimal fixedPrice) {
            var fixedPrices = waterFixedPricesGenerator.GenerateFixedPricesForAllWater(nomenclature, fixedPrice);
           
            foreach (var price in fixedPrices) {
                if (!ContainsFixedPrice(counterparty, price.Key)) {
                    var nomenclatureFixedPrice = CreateNewNomenclatureFixedPrice(price.Key, price.Value);
                    nomenclatureFixedPrice.Counterparty = counterparty;

                    counterparty.ObservableNomenclatureFixedPrices.Add(nomenclatureFixedPrice);
                }
            }
        }

        private void UpdateFixedPrice(DeliveryPoint deliveryPoint, Nomenclature nomenclature, decimal fixedPrice) {
            var nomenclatureFixedPrice =
                deliveryPoint.ObservableNomenclatureFixedPrices.SingleOrDefault(x =>
                    x.Nomenclature.Id == nomenclature.Id);

            if (nomenclatureFixedPrice != null && nomenclatureFixedPrice.FixedPrice != fixedPrice)
                nomenclatureFixedPrice.FixedPrice = fixedPrice;
        }
        
        private void UpdateFixedPrice(Counterparty counterparty, Nomenclature nomenclature, decimal fixedPrice) {
            var nomenclatureFixedPrice =
                counterparty.ObservableNomenclatureFixedPrices.SingleOrDefault(x =>
                    x.Nomenclature.Id == nomenclature.Id);

            if (nomenclatureFixedPrice != null && nomenclatureFixedPrice.FixedPrice != fixedPrice)
                nomenclatureFixedPrice.FixedPrice = fixedPrice;
        }
    }
}