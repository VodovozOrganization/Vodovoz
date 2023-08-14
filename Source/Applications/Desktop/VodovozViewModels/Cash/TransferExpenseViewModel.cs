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
	public class TransferExpenseViewModel : EntityTabViewModelBase<Expense>
	{
		private readonly IDomainEntityNodeInMemoryCacheRepository<FinancialExpenseCategory> _financialExpenseCategoryNodeInMemoryCacheRepository;

		public TransferExpenseViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation,
			IDomainEntityNodeInMemoryCacheRepository<FinancialExpenseCategory> financialExpenseCategoryNodeInMemoryCacheRepository)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			_financialExpenseCategoryNodeInMemoryCacheRepository = financialExpenseCategoryNodeInMemoryCacheRepository
				?? throw new ArgumentNullException(nameof(financialExpenseCategoryNodeInMemoryCacheRepository));

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

		public IDomainEntityNodeInMemoryCacheRepository<FinancialExpenseCategory> FinancialExpenseCategoryNodeInMemoryCacheRepository => _financialExpenseCategoryNodeInMemoryCacheRepository;

		public override bool Save(bool close)
		{
			return true;
		}
	}
}
