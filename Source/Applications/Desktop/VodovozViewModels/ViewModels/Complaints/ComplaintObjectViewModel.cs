using System;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Complaints;

namespace Vodovoz.ViewModels.ViewModels.Complaints
{
	public class ComplaintObjectViewModel : EntityTabViewModelBase<ComplaintObject>
	{
		public ComplaintObjectViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory uowFactory, ICommonServices commonServices)
			: base(uowBuilder, uowFactory, commonServices)
		{
			if(Entity.Id == 0)
			{
				Entity.CreateDate = DateTime.Now;
			}
		}
	}
}
