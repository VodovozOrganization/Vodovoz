using System;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Core.Domain.Contacts;
using Vodovoz.EntityRepositories;
using Vodovoz.Domain.Contacts;
using Vodovoz.ViewModels;
using VodovozBusiness.Domain.Contacts;

namespace Vodovoz.Journals.JournalViewModels
{
	public class PhoneTypeJournalViewModel : SingleEntityJournalViewModelBase<PhoneType, PhoneTypeViewModel, PhoneTypeJournalNode>
	{
		public PhoneTypeJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices)
			: base(unitOfWorkFactory, commonServices)
		{
			TabName = "Типы телефонов";
			UpdateOnChanges(typeof(PhoneType));
		}

		protected override Func<IUnitOfWork, IQueryOver<PhoneType>> ItemsSourceQueryFunction => (uow) => {

			PhoneTypeJournalNode resultAlias = null;

			var query = uow.Session.QueryOver<PhoneType>()
				.SelectList(list => list
				.Select(x => x.Id).WithAlias(() => resultAlias.Id)
				.Select(x => x.Name).WithAlias(() => resultAlias.Name)
				.Select(x => x.PhonePurpose).WithAlias(() => resultAlias.PhonePurpose))
				.TransformUsing(Transformers.AliasToBean<PhoneTypeJournalNode>()).OrderBy(x => x.Id).Desc;

			query.Where(
			GetSearchCriterion<PhoneType>(
				x => x.Id,
				x => x.PhonePurpose,
				x => x.Name
				)
			);

			return query;
		};

		protected override Func<PhoneTypeViewModel> CreateDialogFunction => () => new PhoneTypeViewModel(
			EntityUoWBuilder.ForCreate(),
			UnitOfWorkFactory,
			commonServices
		);

		protected override Func<PhoneTypeJournalNode, PhoneTypeViewModel> OpenDialogFunction => node => new PhoneTypeViewModel(
			EntityUoWBuilder.ForOpen(node.Id),
			UnitOfWorkFactory,
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

	public class PhoneTypeJournalNode : JournalEntityNodeBase<PhoneType>
	{
		public override string Title => Name;
		public string Name { get; set; }
		public PhonePurpose PhonePurpose { get; set; }
	}
}
