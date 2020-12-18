using System;
using System.ComponentModel;
using System.Data.Bindings.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Models
{
    public class DeliveryPointFixedPricesModel : IFixedPricesModel
    {
        private readonly IUnitOfWork uow;
        private readonly DeliveryPoint deliveryPoint;
        private readonly NomenclatureFixedPriceController fixedPriceController;

        public DeliveryPointFixedPricesModel(IUnitOfWork uow, DeliveryPoint deliveryPoint, NomenclatureFixedPriceController fixedPriceController)
        {
            this.uow = uow ?? throw new ArgumentNullException(nameof(uow));
            this.deliveryPoint = deliveryPoint ?? throw new ArgumentNullException(nameof(deliveryPoint));
            this.fixedPriceController = fixedPriceController ?? throw new ArgumentNullException(nameof(fixedPriceController));
            deliveryPoint.PropertyChanged += DeliveryPointOnPropertyChanged;
        }

        private void DeliveryPointOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName) {
                case nameof(deliveryPoint.ObservableNomenclatureFixedPrices):
                    RaiseFixedPricesUpdated();
                    break;
            }
        }

        public GenericObservableList<NomenclatureFixedPrice> FixedPrices => deliveryPoint.ObservableNomenclatureFixedPrices;
		
        public event EventHandler FixedPricesUpdated;

        private void RaiseFixedPricesUpdated()
        {
            FixedPricesUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void AddOrUpdateFixedPrice(Nomenclature nomenclature, decimal fixedPrice)
        {
            if(nomenclature == null) {
                throw new ArgumentNullException(nameof(nomenclature));
            }

            fixedPriceController.AddOrUpdateFixedPrice(uow, deliveryPoint, nomenclature, fixedPrice);
        }

        public void RemoveFixedPrice(NomenclatureFixedPrice nomenclatureFixedPrice)
        {
            if(nomenclatureFixedPrice == null) {
                throw new ArgumentNullException(nameof(nomenclatureFixedPrice));
            }

            fixedPriceController.DeleteFixedPrice(deliveryPoint, nomenclatureFixedPrice);
        }
    }
}