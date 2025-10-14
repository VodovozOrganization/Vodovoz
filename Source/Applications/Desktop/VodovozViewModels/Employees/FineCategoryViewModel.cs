using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Extension;
using System;
using Vodovoz.Domain.Employees;

namespace Vodovoz.ViewModels.Employees
{
	public class FineCategoryViewModel : EntityTabViewModelBase<FineCategory>, IAskSaveOnCloseViewModel
	{
		private readonly IEntityUoWBuilder _uowBuilder;

		public FineCategoryViewModel(
			IEntityUoWBuilder entityUoWBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation)
			: base(entityUoWBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			_uowBuilder = entityUoWBuilder ?? throw new ArgumentNullException(nameof(entityUoWBuilder));

			TabName = IsNew ? "Новая категория штрафа" : Entity.Name;
		}

		public bool IsNew => Entity.Id == 0;
		public bool CanEdit => PermissionResult.CanUpdate || (PermissionResult.CanCreate);
		public bool AskSaveOnClose => CanEdit;
	}
}
