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
        private readonly IUnitOfWork _uow;
        private readonly DeliveryPoint _deliveryPoint;
        private readonly NomenclatureFixedPriceController _fixedPriceController;

        public DeliveryPointFixedPricesModel(IUnitOfWork uow, DeliveryPoint deliveryPoint, NomenclatureFixedPriceController fixedPriceController)
        {
            this._uow = uow ?? throw new ArgumentNullException(nameof(uow));
            this._deliveryPoint = deliveryPoint ?? throw new ArgumentNullException(nameof(deliveryPoint));
            this._fixedPriceController = fixedPriceController ?? throw new ArgumentNullException(nameof(fixedPriceController));
            deliveryPoint.PropertyChanged += DeliveryPointOnPropertyChanged;
        }

        private void DeliveryPointOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName) {
                case nameof(_deliveryPoint.ObservableNomenclatureFixedPrices):
                    RaiseFixedPricesUpdated();
                    break;
            }
        }

        public GenericObservableList<NomenclatureFixedPrice> FixedPrices => _deliveryPoint.ObservableNomenclatureFixedPrices;
		
        public event EventHandler FixedPricesUpdated;

        private void RaiseFixedPricesUpdated()
        {
            FixedPricesUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void AddFixedPrice(Nomenclature nomenclature, decimal fixedPrice, int minCount, int nomenclatureFixedPriceId)
        {
            if(nomenclature == null) {
                throw new ArgumentNullException(nameof(nomenclature));
            }

            _fixedPriceController.AddFixedPrice(_uow, _deliveryPoint, nomenclature, fixedPrice, minCount, nomenclatureFixedPriceId);
		}

		public void UpdateFixedPrice(NomenclatureFixedPrice nomenclatureFixedPrice, decimal fixedPrice, int minCount)
		{
			if(nomenclatureFixedPrice is null)
			{
				throw new ArgumentNullException(nameof(nomenclatureFixedPrice));
			}

			_fixedPriceController.UpdateFixedPrice(nomenclatureFixedPrice, fixedPrice, minCount);
		}

		public void RemoveFixedPrice(NomenclatureFixedPrice nomenclatureFixedPrice)
        {
            if(nomenclatureFixedPrice == null) {
                throw new ArgumentNullException(nameof(nomenclatureFixedPrice));
            }

            _fixedPriceController.DeleteFixedPrice(_deliveryPoint, nomenclatureFixedPrice);
        }
    }
}
