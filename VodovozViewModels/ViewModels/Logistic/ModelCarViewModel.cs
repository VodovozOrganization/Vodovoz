using System;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class ModelCarViewModel : EntityTabViewModelBase<CarModel>
	{
		public ModelCarViewModel(IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices)
			: base(uowBuilder, unitOfWorkFactory, commonServices)
		{
		}
		
		public IEmployeePostsJournalFactory EmployeePostsJournalFactory { get; }
	}
}
