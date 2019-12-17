using System;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Tools.AdditionalAgreements;

namespace Vodovoz.ViewModels.Orders
{
	public class AddFixPriceActionViewModel : UoWWidgetViewModelBase, ICreationControl
	{
		public AddFixPriceActionViewModel(IUnitOfWork UoW, PromotionalSet promotionalSet, ICommonServices commonServices) : base(commonServices.InteractiveService)
		{
			CreateCommands();
			PromotionalSet = promotionalSet;
			CommonServices = commonServices;
			base.UoW = UoW;
		}

		public PromotionalSet PromotionalSet { get; set; }
		public ICommonServices CommonServices { get; set; }

		public event Action CancelCreation;

		private Nomenclature nomenclature;
		public Nomenclature Nomenclature {
			get => nomenclature;
			set => SetField(ref nomenclature, value);
		}

		private decimal price;
		public decimal Price {
			get => price;
			set => SetField(ref price, value);
		}

		#region Commands

		private void CreateCommands()
		{
			CreateAcceptCommand();
			CreateCancelCommand();
		}

		public DelegateCommand AcceptCommand;

		private void CreateAcceptCommand()
		{
			AcceptCommand = new DelegateCommand(
				() => {
					WaterFixedPriceGenerator waterFixedPriceGenerator = new WaterFixedPriceGenerator(UoW);
					var fixedPrices = waterFixedPriceGenerator.GenerateFixedPrices(Nomenclature.Id, Price);
					foreach(var fixedPrice in fixedPrices) {
						var newAction = new PromotionalSetActionFixPrice() {
							Nomenclature = fixedPrice.Nomenclature,
							Price = fixedPrice.Price,
							PromotionalSet = PromotionalSet
						};
						if(!CommonServices.ValidationService.GetValidator().Validate(newAction))
							return;
						PromotionalSet.ObservablePromotionalSetActions.Add(newAction);
					}
				},
				() => true);
		}

		public DelegateCommand CancelCommand;

		private void CreateCancelCommand()
		{
			CancelCommand = new DelegateCommand(
				() => CancelCreation?.Invoke(),
				() => true);
		}

		#endregion
	}
}
