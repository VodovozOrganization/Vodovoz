using System;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Core.Domain.Contacts;
using Vodovoz.Domain.Contacts;
using Vodovoz.EntityRepositories;
using Vodovoz.ViewModels;

namespace Vodovoz.Journals.JournalViewModels
{
	public class EmailTypeJournalViewModel : SingleEntityJournalViewModelBase<EmailType, EmailTypeViewModel, EmailTypeJournalNode>
	{
		public EmailTypeJournalViewModel
		(
			IEmailRepository emailRepository,
			IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices)
			: base(unitOfWorkFactory, commonServices)
		{
			this.emailRepository = emailRepository ?? throw new ArgumentNullException(nameof(emailRepository));
			this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));

			TabName = "Типы e-mail адресов";
			UpdateOnChanges(typeof(EmailType));
		}

		IEmailRepository emailRepository;
		IUnitOfWorkFactory unitOfWorkFactory;

		protected override Func<IUnitOfWork, IQueryOver<EmailType>> ItemsSourceQueryFunction => (uow) => {

			EmailTypeJournalNode resultAlias = null;

			var query = uow.Session.QueryOver<EmailType>()
				.SelectList(list => list
				.Select(x => x.Id).WithAlias(() => resultAlias.Id)
				.Select(x => x.Name).WithAlias(() => resultAlias.Name)
				.Select(x => x.EmailPurpose).WithAlias(() => resultAlias.EmailPurpose))
				.TransformUsing(Transformers.AliasToBean<EmailTypeJournalNode>()).OrderBy(x => x.Id).Desc;

			query.Where(
			GetSearchCriterion<EmailType>(
				x => x.Id,
				x => x.EmailPurpose,
				x => x.Name
				)
			);

			return query;
		};

		protected override Func<EmailTypeViewModel> CreateDialogFunction => () => new EmailTypeViewModel(
			emailRepository,
			EntityUoWBuilder.ForCreate(),
			unitOfWorkFactory,
			commonServices
		);

		protected override Func<EmailTypeJournalNode, EmailTypeViewModel> OpenDialogFunction => node => new EmailTypeViewModel(
			emailRepository,
			EntityUoWBuilder.ForOpen(node.Id),
			unitOfWorkFactory,
			commonServices
		);

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
			CreateDefaultAddActions();
			CreateDefaultEditAction();
		}
	}

	public class EmailTypeJournalNode : JournalEntityNodeBase<EmailType>
	{
		public override string Title => Name;
		public string Name { get; set; }
		public EmailPurpose EmailPurpose { get; set; }
	}
}
