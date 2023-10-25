using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using System;
using Vodovoz.Domain;
using Vodovoz.Tools;
using Vodovoz.ViewModels.Employees;
using static Vodovoz.ViewModels.Journals.JournalViewModels.Employees.FineTemplateJournalViewModel;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Employees
{
	public class FineTemplateJournalViewModel
		: EntityJournalViewModelBase<
			FineTemplate,
			FineTemplateViewModel,
			FineTemplateJournalNode>
	{
		public FineTemplateJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			IDeleteEntityService deleteEntityService = null,
			ICurrentPermissionService currentPermissionService = null)
			: base(
				  unitOfWorkFactory,
				  interactiveService,
				  navigationManager,
				  deleteEntityService,
				  currentPermissionService)
		{
			TabName = $"Журнал {typeof(FineTemplate).GetClassUserFriendlyName().GenitivePlural}";
		}

		protected override IQueryOver<FineTemplate> ItemsQuery(IUnitOfWork unitOfWork)
		{
			FineTemplateJournalNode resultAlias = null;

			FineTemplate fineTemplateAlias = null;

			return unitOfWork.Session.QueryOver(() => fineTemplateAlias)
				.SelectList(list =>
					list.Select(() => fineTemplateAlias.Id).WithAlias(() => resultAlias.Id)
						.Select(() => fineTemplateAlias.Reason).WithAlias(() => resultAlias.Reason))
				.TransformUsing(Transformers.AliasToBean(typeof(FineTemplateJournalNode)));
		}

		public class FineTemplateJournalNode : JournalEntityNodeBase<FineTemplate>
		{
			public override string Title => Reason;

			public string Reason { get; set; }
		}
	}
}
