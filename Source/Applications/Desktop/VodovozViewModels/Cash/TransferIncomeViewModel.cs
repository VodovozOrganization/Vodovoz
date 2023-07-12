using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using System;
using Vodovoz.CachingRepositories.Common;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;

namespace Vodovoz.ViewModels.Cash
{
	public class TransferIncomeViewModel : EntityTabViewModelBase<Income>
	{
		private IDomainEntityNodeInMemoryCacheRepository<FinancialIncomeCategory> _financialIncomeCategoryNodeInMemoryCacheRepository;

		public TransferIncomeViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation,
			IDomainEntityNodeInMemoryCacheRepository<FinancialIncomeCategory> financialIncomeCategoryNodeInMemoryCacheRepository)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			_financialIncomeCategoryNodeInMemoryCacheRepository = financialIncomeCategoryNodeInMemoryCacheRepository
				?? throw new ArgumentNullException(nameof(financialIncomeCategoryNodeInMemoryCacheRepository));

			if(UoWGeneric.IsNew)
			{
				commonServices.InteractiveService.ShowMessage(ImportanceLevel.Error,
					"Для данного диалога невозможно создание новой сущности",
					"Ошибка");

				FailInitialize = true;

				return;
			}

			if(navigation is null)
			{
				throw new ArgumentNullException(nameof(navigation));
			}

			HasChanges = false;

			CloseCommand = new DelegateCommand(() => Close(false, CloseSource.Self));
		}

		public DelegateCommand CloseCommand { get; }

		public IDomainEntityNodeInMemoryCacheRepository<FinancialIncomeCategory> FinancialIncomeCategoryNodeInMemoryCacheRepository => _financialIncomeCategoryNodeInMemoryCacheRepository;

		public override bool Save(bool close)
		{
			return true;
		}
	}
}
