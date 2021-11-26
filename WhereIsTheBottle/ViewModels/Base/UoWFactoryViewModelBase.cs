using System;
using QS.DomainModel.UoW;
using QS.ViewModels;

namespace WhereIsTheBottle.ViewModels
{
	public abstract class UoWFactoryViewModelBase : ViewModelBase
	{
		public UoWFactoryViewModelBase(IUnitOfWorkFactory unitOfWorkFactory)
		{
			UnitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
		}

		protected IUnitOfWorkFactory UnitOfWorkFactory { get; set; }
	}
}