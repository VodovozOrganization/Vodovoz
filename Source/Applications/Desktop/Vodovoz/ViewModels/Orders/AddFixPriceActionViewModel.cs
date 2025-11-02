using Autofac;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using System;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Tools;
using Vodovoz.ViewModels.Dialogs.Goods;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.ViewModels.Orders
{
	public class AddFixPriceActionViewModel : UoWWidgetViewModelBase, ICreationControl, IDisposable
	{
		private Nomenclature _nomenclature;
		private decimal _price;
		private bool _isForZeroDebt;
		private DialogViewModelBase _container;
		private readonly ILifetimeScope _lifetimeScope;
		private readonly INomenclatureSettings _nomenclatureSettings;
		private readonly INomenclatureRepository _nomenclatureRepository;

		public AddFixPriceActionViewModel(
			ILifetimeScope lifetimeScope,
			IUnitOfWork uow,
			PromotionalSet promotionalSet,
			INomenclatureSettings nomenclatureSettings,
			INomenclatureRepository nomenclatureRepository,
			ICommonServices commonServices)
		{
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));

			CreateCommands();
			PromotionalSet = promotionalSet;
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			CommonServices = commonServices;
			UoW = uow;
		}

		public DialogViewModelBase Container
		{
			get => _container;
			set
			{
				if(_container != null)
				{
					return;
				}

				_container = value;

				NomenclatureViewModel = new CommonEEVMBuilderFactory<AddFixPriceActionViewModel>(_container, this, UoW, _container.NavigationManager, _lifetimeScope)
					.ForProperty(x => x.Nomenclature)
					.UseViewModelJournalAndAutocompleter<NomenclaturesJournalViewModel, NomenclatureFilterViewModel>(filter =>
					{
						filter.RestrictCategory = NomenclatureCategory.water;
					})
					.UseViewModelDialog<NomenclatureViewModel>()
					.Finish();
			}
		}

		public IEntityEntryViewModel NomenclatureViewModel { get; private set; }

		public PromotionalSet PromotionalSet { get; set; }
		public ICommonServices CommonServices { get; set; }

		public event Action CancelCreation;

		public Nomenclature Nomenclature
		{
			get => _nomenclature;
			set => SetField(ref _nomenclature, value);
		}

		public decimal Price
		{
			get => _price;
			set => SetField(ref _price, value);
		}

		public bool IsForZeroDebt
		{
			get => _isForZeroDebt;
			set => SetField(ref _isForZeroDebt, value);
		}

		#region Commands

		private void CreateCommands()
		{
			CreateAcceptCommand();
			CreateCancelCommand();
		}

		public DelegateCommand AcceptCommand { get; private set; }
		public DelegateCommand CancelCommand { get; private set; }

		private void CreateAcceptCommand()
		{
			AcceptCommand = new DelegateCommand(
				() =>
				{
					var validatableAction = new PromotionalSetActionFixPrice
					{
						Nomenclature = Nomenclature,
						Price = Price,
						PromotionalSet = PromotionalSet,
						IsForZeroDebt = IsForZeroDebt
					};

					if(!CommonServices.ValidationService.Validate(validatableAction))
					{
						return;
					}

					var waterFixedPriceGenerator = new WaterFixedPriceGenerator(UoW, _nomenclatureSettings, _nomenclatureRepository);
					var fixedPrices = waterFixedPriceGenerator.GenerateFixedPrices(Nomenclature.Id, Price);

					foreach(var fixedPrice in fixedPrices)
					{
						var newAction = new PromotionalSetActionFixPrice
						{
							Nomenclature = fixedPrice.Nomenclature,
							Price = fixedPrice.Price,
							PromotionalSet = PromotionalSet,
							IsForZeroDebt = IsForZeroDebt
						};

						if(!CommonServices.ValidationService.Validate(newAction))
						{
							return;
						}

						PromotionalSet.ObservablePromotionalSetActions.Add(newAction);
					}
				},
				() => true);
		}

		private void CreateCancelCommand()
		{
			CancelCommand = new DelegateCommand(
				() => CancelCreation?.Invoke(),
				() => true);
		}

		public void Dispose()
		{
			_container = null;
		}

		#endregion
	}
}
