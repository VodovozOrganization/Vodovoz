using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.ViewModels;
using System;
using Vodovoz.Domain.Goods;

namespace Vodovoz.ViewModels.ViewModels.Goods
{
	public class NomenclaturePurchasePriceViewModel : TabViewModelBase, ISingleUoWDialog
	{
		private readonly bool _isNewEntity;

		public NomenclaturePurchasePriceViewModel(
			IUnitOfWork uow,
			Nomenclature nomenclature,
			ICommonServices commonServices)
			: base((commonServices ?? throw new ArgumentNullException(nameof(commonServices))).InteractiveService, null)
		{
			UoW = uow ?? throw new ArgumentNullException(nameof(uow));

			_isNewEntity = true;

			if(nomenclature == null)
			{
				throw new ArgumentNullException(nameof(nomenclature));
			}

			Entity = new NomenclaturePurchasePrice();
			Entity.Nomenclature = nomenclature;
			Title = $"Цена закупки ({Entity.Nomenclature.Name})";
		}

		public NomenclaturePurchasePriceViewModel(
			IUnitOfWork uow,
			NomenclaturePurchasePrice nomenclaturePurchasePrice,
			ICommonServices commonServices)
			: base((commonServices ?? throw new ArgumentNullException(nameof(commonServices))).InteractiveService, null)
		{
			UoW = uow ?? throw new ArgumentNullException(nameof(uow));
			Entity = nomenclaturePurchasePrice ?? throw new ArgumentNullException(nameof(nomenclaturePurchasePrice));
			Title = $"Цена закупки ({Entity.Nomenclature.Name})";
		}

		public bool CanEdit => _isNewEntity;

		public void Save()
		{
			//Сохраняется только при создании, изменять цены закупки нельзя
			if(!_isNewEntity)
			{
				return;
			}

			OnPurchasePriceCreated?.Invoke(this, Entity);
			Close(false, CloseSource.Save);
		}

		public IUnitOfWork UoW { get; }
		public NomenclaturePurchasePrice Entity { get; }
		public event EventHandler<NomenclaturePurchasePrice> OnPurchasePriceCreated;
	}
}
