using System;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Tools;

namespace Vodovoz.ViewModels.Orders
{
	public class AddFixPriceActionViewModel : UoWWidgetViewModelBase, ICreationControl
	{
		public IEntityAutocompleteSelectorFactory NomenclatureSelectorFactory { get; }
		
		public AddFixPriceActionViewModel(IUnitOfWork UoW, 
		                                  PromotionalSet promotionalSet, 
		                                  ICommonServices commonServices,
		                                  IEntityAutocompleteSelectorFactory nomenclatureSelectorFactory) 
		{
			NomenclatureSelectorFactory = nomenclatureSelectorFactory ??
			                              throw new ArgumentNullException(nameof(nomenclatureSelectorFactory));
			
			CreateCommands();
			PromotionalSet = promotionalSet;
			CommonServices = commonServices;
			this.UoW = UoW;
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

		private bool isForZeroDebt;
		public bool IsForZeroDebt {
			get => isForZeroDebt;
			set => SetField(ref isForZeroDebt, value);
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
					var validatableAction = new PromotionalSetActionFixPrice {
						Nomenclature = Nomenclature,
						Price = Price,
						PromotionalSet = PromotionalSet,
						IsForZeroDebt = IsForZeroDebt
					};
					if(!CommonServices.ValidationService.Validate(validatableAction))
						return;

					WaterFixedPriceGenerator waterFixedPriceGenerator = new WaterFixedPriceGenerator(UoW);
					var fixedPrices = waterFixedPriceGenerator.GenerateFixedPrices(Nomenclature.Id, Price);
					foreach(var fixedPrice in fixedPrices) {
						var newAction = new PromotionalSetActionFixPrice {
							Nomenclature = fixedPrice.Nomenclature,
							Price = fixedPrice.Price,
							PromotionalSet = PromotionalSet,
							IsForZeroDebt = IsForZeroDebt
						};
						if(!CommonServices.ValidationService.Validate(newAction))
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
