using System;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain;
namespace Vodovoz.ViewModels.Employees
{
	public class FineTemplateViewModel : EntityTabViewModelBase<FineTemplate>
	{
		public FineTemplateViewModel(IEntityUoWBuilder uoWBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices) : base(uoWBuilder, unitOfWorkFactory, commonServices)
		{
			TabName = "Шаблон основания штрафа";
		}

		public bool CanEdit => PermissionResult.CanUpdate;
	}
}
