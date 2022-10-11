using System;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels
{
	public class UndeliveryProblemSourceViewModel : EntityTabViewModelBase<UndeliveryProblemSource>
	{
		public UndeliveryProblemSourceViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices) : base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			TabName = "Источники проблем";
		}
	}
}
