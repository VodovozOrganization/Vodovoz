using QS.ViewModels;
using System;
using Vodovoz.Core.Domain.Goods;

namespace Vodovoz.ViewModels.ViewModels.Goods
{
	public class NomenclaturePurchasePriceViewModel : WidgetViewModelBase
	{
		public NomenclaturePurchasePrice Entity { get; }

		public NomenclaturePurchasePriceViewModel(NomenclaturePurchasePrice entity)
		{
			Entity = entity ?? throw new ArgumentNullException(nameof(entity));
			Entity.PropertyChanged += Entity_PropertyChanged;
		}

		private void Entity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(Entity.PurchasePrice):
					OnPropertyChanged(nameof(PurchasePrice));
					break;
				case nameof(Entity.StartDate):
					OnPropertyChanged(nameof(StartDate));
					OnPropertyChanged(nameof(StartDateTitle));
					break;
				case nameof(Entity.EndDate):
					OnPropertyChanged(nameof(EndDate));
					OnPropertyChanged(nameof(EndDateTitle));
					break;
				default:
					break;
			}
		}

		public bool CanEditPrice => Entity.Id == 0;

		public string Nomenclature => Entity.Nomenclature.Name;

		public DateTime StartDate
		{
			get => Entity.StartDate;
			set => Entity.StartDate = value;
		}

		public string StartDateTitle => StartDate.ToString("dd.MM.yyyy HH:mm");

		public DateTime? EndDate
		{
			get => Entity.EndDate;
			set => Entity.EndDate = value;
		}
		public string EndDateTitle => EndDate.HasValue ? EndDate.Value.ToString("dd.MM.yyyy HH:mm") : string.Empty;

		public decimal PurchasePrice
		{
			get => Entity.PurchasePrice;
			set
			{
				if(!CanEditPrice)
				{
					OnPropertyChanged(nameof(PurchasePrice));
					return;
				}
				Entity.PurchasePrice = value;
			}
		}
	}
}
