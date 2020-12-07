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
		private readonly IUnitOfWork uow;
		private readonly Counterparty counterparty;
		private readonly NomenclatureFixedPriceController fixedPriceController;

		public CounterpartyFixedPricesModel(IUnitOfWork uow, Counterparty counterparty, NomenclatureFixedPriceController fixedPriceController)
		{
			this.uow = uow ?? throw new ArgumentNullException(nameof(uow));
			this.counterparty = counterparty ?? throw new ArgumentNullException(nameof(counterparty));
			this.fixedPriceController = fixedPriceController ?? throw new ArgumentNullException(nameof(fixedPriceController));
			counterparty.PropertyChanged += CounterpartyOnPropertyChanged;
		}
		
		
		private void CounterpartyOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName) {
				case nameof(counterparty.ObservableNomenclatureFixedPrices):
					RaiseFixedPricesUpdated();
					break;
			}
		}

		public GenericObservableList<NomenclatureFixedPrice> FixedPrices => counterparty.ObservableNomenclatureFixedPrices;

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

			fixedPriceController.AddOrUpdateFixedPrice(uow, counterparty, nomenclature, fixedPrice);
		}

		public void RemoveFixedPrice(NomenclatureFixedPrice nomenclatureFixedPrice)
		{
			if(nomenclatureFixedPrice == null) {
				throw new ArgumentNullException(nameof(nomenclatureFixedPrice));
			}

			fixedPriceController.DeleteFixedPrice(counterparty, nomenclatureFixedPrice);
		}
	}
}
