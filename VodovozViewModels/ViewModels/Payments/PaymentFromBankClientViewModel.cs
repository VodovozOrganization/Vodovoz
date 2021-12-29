using System;
using System.Collections.Generic;
using Autofac;
using QS.Commands;
using QS.ViewModels;
using Vodovoz.Domain.Payments;
using QS.Project.Domain;
using QS.Services;
using QS.DomainModel.UoW;
using QS.Navigation;
using Vodovoz.EntityRepositories.Payments;
using Vodovoz.Services;

namespace Vodovoz.ViewModels.ViewModels.Payments
{
	public class PaymentFromBankClientViewModel : EntityTabViewModelBase<Payment>
	{
		private DelegateCommand _saveAndOpenManualPaymentMatchingCommand;
		
		public PaymentFromBankClientViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			IProfitCategoryRepository profitCategoryRepository,
			IProfitCategoryProvider profitCategoryProvider,
			ILifetimeScope scope) : base(uowBuilder, uowFactory, commonServices, navigationManager)
		{
			if(profitCategoryRepository == null)
			{
				throw new ArgumentNullException(nameof(profitCategoryRepository));
			}
			if(profitCategoryProvider == null)
			{
				throw new ArgumentNullException(nameof(profitCategoryProvider));
			}
			Scope = scope ?? throw new ArgumentNullException(nameof(scope));

			Configure(profitCategoryRepository, profitCategoryProvider);
		}
		
		public IEnumerable<ProfitCategory> ProfitCategories { get; private set; }
		public ILifetimeScope Scope { get; }

		public DelegateCommand SaveAndOpenManualPaymentMatchingCommand =>
			_saveAndOpenManualPaymentMatchingCommand ?? (_saveAndOpenManualPaymentMatchingCommand = new DelegateCommand(
					() =>
					{
						if(Save(false))
						{
							NavigationManager.OpenViewModel<ManualPaymentMatchingViewModel, IEntityUoWBuilder>(
								this, EntityUoWBuilder.ForOpen(Entity.Id));
						}
					}
				)
			);

		protected override void BeforeSave()
		{
			Entity.FillPropertiesFromCounterparty();
		}

		private void Configure(IProfitCategoryRepository profitCategoryRepository, IProfitCategoryProvider profitCategoryProvider)
		{
			ProfitCategories = profitCategoryRepository.GetAllProfitCategories(UoW);
			Entity.Date = DateTime.Today;
			Entity.ProfitCategory = profitCategoryRepository.GetProfitCategory(UoW, profitCategoryProvider.GetDefaultProfitCategory());
			Entity.IsManualCreated = true;
		}
	}
}
