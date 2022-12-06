using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using Vodovoz.Domain.Employees;
using Vodovoz.ViewModels.Employees;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Employees
{
	public class EmployeeRegistrationsJournalViewModel
		: EntityJournalViewModelBase<EmployeeRegistration, EmployeeRegistrationViewModel, EmployeeRegistrationsJournalNode>
	{
		public EmployeeRegistrationsJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			ICurrentPermissionService currentPermissionService,
			IDeleteEntityService deleteEntityService = null)
			: base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			TabName = "Журнал видов оформлений сотрудников";
		}

		protected override IQueryOver<EmployeeRegistration> ItemsQuery(IUnitOfWork uow)
		{
			EmployeeRegistration employeeRegistrationAlias = null;
			EmployeeRegistrationsJournalNode resultAlias = null;

			var query = uow.Session.QueryOver(() => employeeRegistrationAlias)
				.SelectList(list => list
					.Select(er => er.Id).WithAlias(() => resultAlias.Id)
					.Select(er => er.RegistrationType).WithAlias(() => resultAlias.RegistrationType)
					.Select(er => er.PaymentForm).WithAlias(() => resultAlias.PaymentForm)
					.Select(er => er.TaxRate).WithAlias(() => resultAlias.TaxRate))
				.TransformUsing(Transformers.AliasToBean<EmployeeRegistrationsJournalNode>());

			return query;
		}
	}
}
