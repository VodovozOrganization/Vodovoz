using System;
using System.ComponentModel;
using System.Data.Bindings.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Models
{
	public class CounterpartyFixedPricesModel : IFixedPricesModel
	{
		private readonly IUnitOfWork _uow;
		private readonly Counterparty _counterparty;
		private readonly INomenclatureFixedPriceController _fixedPriceController;

		public CounterpartyFixedPricesModel(
			IUnitOfWork uow,
			Counterparty counterparty,
			INomenclatureFixedPriceController fixedPriceController)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_counterparty = counterparty ?? throw new ArgumentNullException(nameof(counterparty));
			_fixedPriceController = fixedPriceController ?? throw new ArgumentNullException(nameof(fixedPriceController));
			counterparty.PropertyChanged += CounterpartyOnPropertyChanged;
		}
		
		
		private void CounterpartyOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName) {
				case nameof(_counterparty.ObservableNomenclatureFixedPrices):
					RaiseFixedPricesUpdated();
					break;
			}
		}

		public GenericObservableList<NomenclatureFixedPrice> FixedPrices => _counterparty.ObservableNomenclatureFixedPrices;

		public event EventHandler FixedPricesUpdated;

		private void RaiseFixedPricesUpdated()
		{
			FixedPricesUpdated?.Invoke(this, EventArgs.Empty);
		}
		
		public void AddFixedPrice(Nomenclature nomenclature, decimal fixedPrice, int minCount)
		{
			if(nomenclature == null) 
			{
				throw new ArgumentNullException(nameof(nomenclature));
			}

			_fixedPriceController.AddFixedPrice(_counterparty, nomenclature, fixedPrice, minCount);
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

			_fixedPriceController.DeleteFixedPrice(_counterparty, nomenclatureFixedPrice);
		}
	}
}
