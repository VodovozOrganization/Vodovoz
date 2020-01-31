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
	public class PhoneTypeJournalViewModel : SingleEntityJournalViewModelBase<PhoneType, PhoneTypeViewModel, PhoneTypeJournalNode>
	{
		public PhoneTypeJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices)
			: base(unitOfWorkFactory, commonServices)
		{
			this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));

			TabName = "Типы телефонов";

			UpdateOnChanges(typeof(PhoneType));
		}

		IUnitOfWorkFactory unitOfWorkFactory;

		protected override Func<IUnitOfWork, IQueryOver<PhoneType>> ItemsSourceQueryFunction => (uow) => {

			PhoneTypeJournalNode resultAlias = null;

			var query = uow.Session.QueryOver<PhoneType>()
				.SelectList(list => list
				.Select(x => x.Id).WithAlias(() => resultAlias.Id)
				.Select(x => x.Name).WithAlias(() => resultAlias.Name)
				.Select(x => x.PhoneAdditionalType).WithAlias(() => resultAlias.PhoneAdditionalType))
				.TransformUsing(Transformers.AliasToBean<PhoneTypeJournalNode>()).OrderBy(x => x.Id).Desc;

			query.Where(
			GetSearchCriterion<PhoneType>(
				x => x.Id,
				x => x.PhoneAdditionalType,
				x => x.Name
				)
			);

			return query;
		};

		protected override Func<PhoneTypeViewModel> CreateDialogFunction => () => new PhoneTypeViewModel(
			EntityUoWBuilder.ForCreate(),
			unitOfWorkFactory,
			commonServices
		);

		protected override Func<PhoneTypeJournalNode, PhoneTypeViewModel> OpenDialogFunction => node => new PhoneTypeViewModel(
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

	public class PhoneTypeJournalNode : JournalEntityNodeBase<PhoneType>
	{
		public string Name { get; set; }
		public PhoneAdditionalType PhoneAdditionalType { get; set; }
	}
}
