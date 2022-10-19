using System;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.Orders
{
	public class ReturnTareReasonViewModel : EntityTabViewModelBase<ReturnTareReason>
	{
		public ReturnTareReasonViewModel(IEntityUoWBuilder uowBuilder,
										IUnitOfWorkFactory unitOfWorkFactory,
										ICommonServices commonServices) : base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			if(uowBuilder.IsNewEntity)
				TabName = "Создание новой причины забора тары";
			else
			    TabName = $"{Entity.Title}";
		}
	}
}
