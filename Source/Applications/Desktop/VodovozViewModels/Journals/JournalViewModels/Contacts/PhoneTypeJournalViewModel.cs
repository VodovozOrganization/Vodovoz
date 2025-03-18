using System;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Core.Domain.Contacts;
using Vodovoz.EntityRepositories;
using Vodovoz.ViewModels;
using VodovozBusiness.Domain.Contacts;

namespace Vodovoz.Journals.JournalViewModels
{
	public class PhoneTypeJournalViewModel : SingleEntityJournalViewModelBase<PhoneType, PhoneTypeViewModel, PhoneTypeJournalNode>
	{
		public PhoneTypeJournalViewModel(
			IPhoneRepository phoneRepository,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices)
			: base(unitOfWorkFactory, commonServices)
		{
			this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			this.phoneRepository = phoneRepository ?? throw new ArgumentNullException(nameof(phoneRepository));

			TabName = "Типы телефонов";

			UpdateOnChanges(typeof(PhoneType));
		}

		IUnitOfWorkFactory unitOfWorkFactory;
		IPhoneRepository phoneRepository;

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
			phoneRepository,
			EntityUoWBuilder.ForCreate(),
			unitOfWorkFactory,
			commonServices
		);

		protected override Func<PhoneTypeJournalNode, PhoneTypeViewModel> OpenDialogFunction => node => new PhoneTypeViewModel(
			phoneRepository,
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
		public override string Title => Name;
		public string Name { get; set; }
		public PhonePurpose PhonePurpose { get; set; }
	}
}
