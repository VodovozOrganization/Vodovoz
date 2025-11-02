using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Tdi;
using System;

namespace Vodovoz.Presentation.ViewModels.Controls.EntitySelection
{
	public interface ILegacyESVMBuilderParameters
	{
		Func<ITdiTab> DialogTabFunc { get; }
		IUnitOfWork UnitOfWork { get; }
		INavigationManager NavigationManager { get; }
		ILifetimeScope AutofacScope { get; }
	}
}
