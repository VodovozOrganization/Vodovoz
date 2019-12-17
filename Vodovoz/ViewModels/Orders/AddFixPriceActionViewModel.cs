using System;
using QS.Commands;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.Orders
{
	public class AddFixPriceActionViewModel : UoWWidgetViewModelBase, ICreationControl
	{
		public AddFixPriceActionViewModel(PromotionalSet promotionalSet, ICommonServices commonServices) : base(commonServices.InteractiveService)
		{
			CreateCommands();
			PromotionalSet = promotionalSet;
			CommonServices = commonServices;
		}

		public PromotionalSet PromotionalSet { get; set; }
		public ICommonServices CommonServices { get; set; }

		public event Action<PromotionalSetActionBase> AcceptCreation;
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
					var newAction = new PromotionalSetActionFixPrice {
						Nomenclature = Nomenclature,
						Price = Price,
						PromotionalSet = PromotionalSet
					};
					if(!CommonServices.ValidationService.GetValidator().Validate(newAction))
						return;
					AcceptCreation?.Invoke(newAction);
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
