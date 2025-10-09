using NHibernate.Type;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Extension;
using System;
using System.Reflection;
using Vodovoz.Domain.Employees;

namespace Vodovoz.ViewModels.Employees
{
	public class FineCategoryViewModel : EntityTabViewModelBase<FineCategory>, IAskSaveOnCloseViewModel
	{
		private readonly IEntityUoWBuilder _uowBuilder;
		public FineCategoryViewModel(
			IEntityUoWBuilder entityUoWBuilder,
			Type entityType,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation = null)
			: base(entityUoWBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			_uowBuilder = entityUoWBuilder ?? throw new ArgumentNullException(nameof(entityUoWBuilder));

			TabName = entityType.GetCustomAttribute<AppellativeAttribute>(true)?.Nominative;
		}
		public bool CanEdit => PermissionResult.CanUpdate || (PermissionResult.CanCreate);
		public bool AskSaveOnClose => CanEdit;
	}
}
