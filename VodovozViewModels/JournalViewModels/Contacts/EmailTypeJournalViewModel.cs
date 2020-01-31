using System;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain.Contacts;
using Vodovoz.ViewModels;

namespace Vodovoz.JournalViewModels
{
	public class EmailTypeJournalViewModel : SingleEntityJournalViewModelBase<EmailType, EmailTypeViewModel, EmailTypeJournalNode>
	{
		public EmailTypeJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices)
			: base(unitOfWorkFactory, commonServices)
		{
			this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));

			TabName = "Типы e-mail адресов";

			UpdateOnChanges(typeof(EmailType));
		}

		IUnitOfWorkFactory unitOfWorkFactory;

		protected override Func<IUnitOfWork, IQueryOver<EmailType>> ItemsSourceQueryFunction => (uow) => {

			EmailTypeJournalNode resultAlias = null;

			var query = uow.Session.QueryOver<EmailType>()
				.SelectList(list => list
				.Select(x => x.Id).WithAlias(() => resultAlias.Id)
				.Select(x => x.Name).WithAlias(() => resultAlias.Name)
				.Select(x => x.EmailAdditionalType).WithAlias(() => resultAlias.EmailAdditionalType))
				.TransformUsing(Transformers.AliasToBean<EmailTypeJournalNode>()).OrderBy(x => x.Id).Desc;

			query.Where(
			GetSearchCriterion<EmailType>(
				x => x.Id,
				x => x.EmailAdditionalType,
				x => x.Name
				)
			);

			return query;
		};

		protected override Func<EmailTypeViewModel> CreateDialogFunction => () => new EmailTypeViewModel(
			EntityUoWBuilder.ForCreate(),
			unitOfWorkFactory,
			commonServices
		);

		protected override Func<EmailTypeJournalNode, EmailTypeViewModel> OpenDialogFunction => node => new EmailTypeViewModel(
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
		public string Name { get; set; }
		public EmailAdditionalType EmailAdditionalType { get; set; }
	}
}
