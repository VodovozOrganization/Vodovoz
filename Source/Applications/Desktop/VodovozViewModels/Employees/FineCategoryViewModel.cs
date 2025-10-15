using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Extension;
using System;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Employees;

namespace Vodovoz.ViewModels.Employees
{
	public class FineCategoryViewModel : EntityTabViewModelBase<FineCategory>, IAskSaveOnCloseViewModel
	{
		private readonly IEntityUoWBuilder _uowBuilder;
		private readonly ICommonServices _commonServices;
		private readonly IGenericRepository<FineCategory> _fineCategoryRepository;

		public FineCategoryViewModel(
			IEntityUoWBuilder entityUoWBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation,
			IGenericRepository<FineCategory> genericRepository)
			: base(entityUoWBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			_uowBuilder = entityUoWBuilder ?? throw new ArgumentNullException(nameof(entityUoWBuilder));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_fineCategoryRepository = genericRepository ?? throw new ArgumentNullException(nameof(genericRepository));

			TabName = IsNew ? "Новая категория штрафа" : $"Категория штрафа: {Entity.Name}";
		}

		public bool IsNew => Entity.Id == 0;
		public bool CanEdit => PermissionResult.CanUpdate || (PermissionResult.CanCreate);
		public bool AskSaveOnClose => CanEdit;

		protected override bool BeforeSave()
		{
			var duplicate = _fineCategoryRepository
				.GetFirstOrDefault(UoW, x => x.Name == Entity.Name);

			if(duplicate != null)
			{
				_commonServices.InteractiveService.ShowMessage(
					QS.Dialog.ImportanceLevel.Warning,
					$"Категория с названием \"{Entity.Name}\" уже существует."
				);
				return false;
			}

			return base.BeforeSave();
		}
	}
}
