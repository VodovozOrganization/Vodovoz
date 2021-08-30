using QS.Commands;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using System;
using System.Linq;
using Vodovoz.Domain.Goods;

namespace Vodovoz.ViewModels.ViewModels.Goods
{
	public class NomenclaturePurchasePricesViewModel : EntityWidgetViewModelBase<Nomenclature>
	{
		private readonly ITdiTab _tab;
		private DateTime? _startDate;
		private DelegateCommand _changePurchasePriceCommand;
		private DelegateCommand<NomenclaturePurchasePrice> _changePurchasePriceStartDateCommand;
		private DelegateCommand<NomenclaturePurchasePrice> _openPurchasePriceCommand;

		public NomenclaturePurchasePricesViewModel(
			Nomenclature entity,
			ITdiTab tab,
			IUnitOfWork uow,
			ICommonServices commonServices) : base(entity, commonServices)
		{
			_tab = tab ?? throw new ArgumentNullException(nameof(tab));
			UoW = uow ?? throw new ArgumentNullException(nameof(uow));
		}

		private NomenclaturePurchasePrice GetPreviousPurchasePrice(DateTime date) =>
			Entity.ObservablePurchasePrices
			.Where(x => x.EndDate != null)
			.Where(x => x.EndDate <= date)
			.OrderByDescending(x => x.EndDate)
			.FirstOrDefault();

		private bool CanChangePurchasePrice => (StartDate.HasValue && Entity.CheckStartDateForNewPurchasePrice(StartDate.Value));

		[PropertyChangedAlso(nameof(CanChangePurchasePrice))]
		public virtual DateTime? StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value, () => StartDate);
		}

		#region Commands

		public DelegateCommand ChangePurchasePriceCommand
		{
			get
			{
				if(_changePurchasePriceCommand == null)
				{
					_changePurchasePriceCommand = new DelegateCommand(
						() =>
						{
							var nomenclaturePurchasePriceViewModel = new NomenclaturePurchasePriceViewModel(UoW, Entity, CommonServices);
							nomenclaturePurchasePriceViewModel.OnPurchasePriceCreated += (sender, purchasePrice) => Entity.ChangePurchasePrice(purchasePrice, StartDate.Value);

							_tab.TabParent.AddSlaveTab(_tab, nomenclaturePurchasePriceViewModel);
						},
						() => CanChangePurchasePrice
					);
					_changePurchasePriceCommand.CanExecuteChangedWith(this, x => x.CanChangePurchasePrice);
				}

				return _changePurchasePriceCommand;
			}
		}

		public DelegateCommand<NomenclaturePurchasePrice> ChangePurchasePriceStartDateCommand
		{
			get
			{
				if(_changePurchasePriceStartDateCommand == null)
				{
					_changePurchasePriceStartDateCommand = new DelegateCommand<NomenclaturePurchasePrice>(
						(node) =>
						{
							if(!CommonServices.InteractiveService.Question(
								"Внимание! Будет произведено изменение даты цены закупки." +
								" Продолжить?", "Внимание!"))
							{
								return;
							}

							var previousPurchasePrice = GetPreviousPurchasePrice(node.StartDate);
							if(previousPurchasePrice != null)
							{
								previousPurchasePrice.EndDate = StartDate.Value.AddTicks(-1);
							}
							node.StartDate = StartDate.Value;
						},
						(node) =>
						{
							if(node == null || !StartDate.HasValue)
							{
								return false;
							}
							var previousPurchasePriceByDate = GetPreviousPurchasePrice(StartDate.Value);
							var previousPurchasePriceBySelectedParameter = GetPreviousPurchasePrice(node.StartDate);

							bool noConflictWithEndDate = !node.EndDate.HasValue || node.EndDate.Value > StartDate;
							bool noConflictWithPreviousStartDate = (previousPurchasePriceByDate == null && previousPurchasePriceBySelectedParameter == null) || (previousPurchasePriceBySelectedParameter != null && previousPurchasePriceBySelectedParameter.StartDate < StartDate);

							return StartDate.HasValue && noConflictWithEndDate && noConflictWithPreviousStartDate;
						}
					);
					_changePurchasePriceStartDateCommand.CanExecuteChangedWith(this, x => x.StartDate);
				}

				return _changePurchasePriceStartDateCommand;
			}
		}

		public DelegateCommand<NomenclaturePurchasePrice> OpenPurchasePriceCommand
		{
			get
			{
				if(_openPurchasePriceCommand == null)
				{
					_openPurchasePriceCommand = new DelegateCommand<NomenclaturePurchasePrice>(
						(node) =>
						{
							NomenclaturePurchasePriceViewModel nomenclaturePurchasePriceViewModel = new NomenclaturePurchasePriceViewModel(UoW, node, CommonServices);
							_tab.TabParent.AddTab(nomenclaturePurchasePriceViewModel, _tab);
						},
						(node) => node != null
					);
				}

				return _openPurchasePriceCommand;
			}
		}

		#endregion Commands
	}
}
