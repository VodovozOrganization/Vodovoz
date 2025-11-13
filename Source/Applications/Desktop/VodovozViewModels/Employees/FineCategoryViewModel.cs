using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Extension;
using System;
using System.Windows.Input;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Repositories;

namespace Vodovoz.ViewModels.Employees
{
	public class FineCategoryViewModel : EntityTabViewModelBase<FineCategory>, IAskSaveOnCloseViewModel
	{
		private readonly IGenericRepository<FineCategory> _fineCategoryRepository;

		public FineCategoryViewModel(
			IEntityUoWBuilder entityUoWBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation,
			IGenericRepository<FineCategory> genericRepository)
			: base(entityUoWBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			if(entityUoWBuilder == null)
			{
				throw new ArgumentNullException(nameof(entityUoWBuilder));
			}
			if(unitOfWorkFactory == null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}
			if(commonServices == null)
			{
				throw new ArgumentNullException(nameof(commonServices));
			}
			if(navigation == null)
			{
				throw new ArgumentNullException(nameof(navigation));
			}

			_fineCategoryRepository = genericRepository ?? throw new ArgumentNullException(nameof(genericRepository));

			TabName = IsNew ? "Новая категория штрафа" : $"Категория штрафа: {Entity.Name}";

			SaveCommand = new DelegateCommand(() => SaveAndClose());
			CancelCommand = new DelegateCommand(() => Close(AskSaveOnClose, CloseSource.Cancel));
		}

		public ICommand SaveCommand { get; }
		public ICommand CancelCommand { get; }

		public bool IsNew => Entity.Id == 0;
		public bool CanEdit => PermissionResult.CanCreate && IsNew || PermissionResult.CanUpdate;
		public bool AskSaveOnClose => CanEdit;

		protected override bool BeforeSave()
		{
			var duplicate = _fineCategoryRepository
				.GetFirstOrDefault(UoW, x => x.Name == Entity.Name && x.Id != Entity.Id);

			if(duplicate != null)
			{
				CommonServices.InteractiveService.ShowMessage(
					QS.Dialog.ImportanceLevel.Warning,
					$"Категория с названием \"{Entity.Name}\" уже существует."
				);
				return false;
			}

			return base.BeforeSave();
		}
	}
}
