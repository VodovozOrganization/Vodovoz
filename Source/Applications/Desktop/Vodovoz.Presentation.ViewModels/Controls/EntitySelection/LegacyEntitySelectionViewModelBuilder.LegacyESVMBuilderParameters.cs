using Autofac;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Tdi;
using System;

namespace Vodovoz.Presentation.ViewModels.Controls.EntitySelection
{
	public partial class LegacyEntitySelectionViewModelBuilder<TEntity>
		where TEntity : class, IDomainObject
	{
		public class LegacyESVMBuilderParameters : ILegacyESVMBuilderParameters
		{
			public LegacyESVMBuilderParameters(
				Func<ITdiTab> dialogTabFunc,
				IUnitOfWork unitOfWork,
				INavigationManager navigationManager,
				ILifetimeScope autofacScope)
			{
				DialogTabFunc = dialogTabFunc ?? throw new ArgumentNullException(nameof(dialogTabFunc));
				UnitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
				NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
				AutofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope));
			}

			public Func<ITdiTab> DialogTabFunc { get; }
			public IUnitOfWork UnitOfWork { get; }
			public INavigationManager NavigationManager { get; }
			public ILifetimeScope AutofacScope { get; }
		}
	}
}
