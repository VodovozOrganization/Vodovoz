using System;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain;
namespace Vodovoz.ViewModels.Employees
{
	public class FineTemplateViewModel : EntityTabViewModelBase<FineTemplate>
	{
		public FineTemplateViewModel(IEntityConstructorParam ctorParam, ICommonServices commonServices) : base(ctorParam, commonServices)
		{
			TabName = "Шаблон основания штрафа";
		}

		public bool CanEdit => PermissionResult.CanUpdate;
	}
}
