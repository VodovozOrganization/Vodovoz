using System.Linq;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Employees;

namespace Vodovoz.ViewModels.ViewModels.Employees
{
	public class PremiumRaskatGAZelleViewModel : EntityTabViewModelBase<PremiumRaskatGAZelle>
	{
		public PremiumRaskatGAZelleViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory uowFactory, ICommonServices commonServices)
			: base(uowBuilder, uowFactory, commonServices)
		{
			TabName = Entity.Title;
		}

		public string EmployeeFullName => Entity.Items.FirstOrDefault().Employee.FullName;
	}
}
