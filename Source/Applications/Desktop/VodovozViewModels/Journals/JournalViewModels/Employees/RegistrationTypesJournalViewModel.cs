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
	public class RegistrationTypesJournalViewModel
		: EntityJournalViewModelBase<RegistrationType, RegistrationTypeViewModel, RegistrationTypeJournalNode>
	{
		public RegistrationTypesJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			ICurrentPermissionService currentPermissionService,
			IDeleteEntityService deleteEntityService = null)
			: base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			TabName = "Журнал видов оформлений сотрудников";
		}

		protected override IQueryOver<RegistrationType> ItemsQuery(IUnitOfWork uow)
		{
			RegistrationType registrationTypeAlias = null;
			RegistrationTypeJournalNode resultAlias = null;

			var query = uow.Session.QueryOver(() => registrationTypeAlias)
				.SelectList(list => list
					.Select(er => er.Id).WithAlias(() => resultAlias.Id)
					.Select(er => er.Name).WithAlias(() => resultAlias.Name))
				.TransformUsing(Transformers.AliasToBean<RegistrationTypeJournalNode>());

			return query;
		}
	}
}
