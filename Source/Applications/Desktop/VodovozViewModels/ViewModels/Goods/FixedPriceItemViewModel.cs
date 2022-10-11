using System;
using System.ComponentModel;
using QS.ViewModels;
using Vodovoz.Domain.Goods;
using Vodovoz.Models;

namespace Vodovoz.ViewModels.ViewModels.Goods
{
    public class FixedPriceItemViewModel : ViewModelBase
    {
        public NomenclatureFixedPrice NomenclatureFixedPrice { get; }
        private readonly IFixedPricesModel fixedPriceModel;

        public FixedPriceItemViewModel(NomenclatureFixedPrice fixedPrice, IFixedPricesModel fixedPriceModel)
        {
            NomenclatureFixedPrice = fixedPrice ?? throw new ArgumentNullException(nameof(fixedPrice));
            this.fixedPriceModel = fixedPriceModel ?? throw new ArgumentNullException(nameof(fixedPriceModel));
			
            fixedPrice.PropertyChanged += FixedPriceOnPropertyChanged;
        }

        private void FixedPriceOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName) {
                case nameof(NomenclatureFixedPrice.Nomenclature):
                    NomenclatureFixedPrice.Nomenclature.PropertyChanged -= NomenclatureOnPropertyChanged;
                    NomenclatureFixedPrice.Nomenclature.PropertyChanged += NomenclatureOnPropertyChanged;
                    OnPropertyChanged(nameof(NomenclatureTitle));
                    break;
                case nameof(NomenclatureFixedPrice.Price):
                    OnPropertyChanged(nameof(FixedPrice));
                    break;
            }
        }
		
        private void NomenclatureOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName) {
                case nameof(NomenclatureFixedPrice.Nomenclature.Name):
                    OnPropertyChanged(nameof(NomenclatureTitle));
                    break;
            }
        }

        public string NomenclatureTitle => NomenclatureFixedPrice.Nomenclature.Name;

        public decimal FixedPrice {
            get => NomenclatureFixedPrice.Price;
            set => fixedPriceModel.AddOrUpdateFixedPrice(NomenclatureFixedPrice.Nomenclature, value);
        }
    }
}