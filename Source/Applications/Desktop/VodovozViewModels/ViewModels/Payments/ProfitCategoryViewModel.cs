using System;
using System.Linq;
using System.Windows.Input;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Extension;
using Vodovoz.Core.Domain.Payments;
using Vodovoz.Core.Domain.Repositories;

namespace Vodovoz.ViewModels.ViewModels.Payments
{
	public class ProfitCategoryViewModel : EntityTabViewModelBase<ProfitCategory>, IAskSaveOnCloseViewModel
	{
		private readonly IGenericRepository<ProfitCategory> _profitCategoryRepository;

		public ProfitCategoryViewModel(
			IEntityUoWBuilder uoWBuilder,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			IGenericRepository<ProfitCategory> profitCategoryRepository,
			INavigationManager navigation) : base(uoWBuilder, uowFactory, commonServices, navigation)
		{
			_profitCategoryRepository = profitCategoryRepository ?? throw new ArgumentNullException(nameof(profitCategoryRepository));
			Initialize();
		}
		
		public bool CanEdit { get; private set; }
		public ICommand SaveCommand { get; private set; }
		public ICommand CancelCommand { get; private set; }
		public bool AskSaveOnClose => CanEdit;

		private void Initialize()
		{
			var saveCommand = new DelegateCommand(SaveAndClose);
			saveCommand.CanExecuteChangedWith(this, x => x.CanEdit);
			SaveCommand = saveCommand;
			
			var cancelCommand = new DelegateCommand(() => Close(false, CloseSource.Cancel));
			CancelCommand = cancelCommand;

			CanEdit = (Entity.Id == 0 && PermissionResult.CanCreate) || PermissionResult.CanUpdate;
		}

		protected override bool BeforeSave()
		{
			if(string.IsNullOrEmpty(Entity.Name))
			{
				return true;
			}

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot("Проверка на дубли"))
			{
				var duplicates = _profitCategoryRepository
					.Get(uow, x => x.Name == Entity.Name && x.Id != Entity.Id)
					.ToArray();

				if(!duplicates.Any())
				{
					return true;
				}

				if(duplicates.Length > 1)
				{
					ShowErrorMessage("Уже сохранено несколько категорий доходов с таким же именем! Обратитесь в отдел разработки");
					return false;
				}

				var duplicate = duplicates[0];

				if(duplicate.IsArchive)
				{
					ShowWarningMessage(
						$"Есть заархивированная категория дохода с похожим именем, Код: {duplicate.Id}. Разархивируйте существующую категорию");
					return false;
				}

				ShowWarningMessage($"Уже есть категория дохода с похожим именем, Код: {duplicate.Id}. Нельзя создавать дубли");
				return false;
			}
		}
	}
}
