using System;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Contacts;
using Vodovoz.EntityRepositories;
using Vodovoz.ViewModels;

namespace Vodovoz.Journals.JournalViewModels
{
	public class PhoneTypeJournalViewModel : SingleEntityJournalViewModelBase<PhoneType, PhoneTypeViewModel, PhoneTypeJournalNode>
	{
		public PhoneTypeJournalViewModel(
			EntitiesJournalActionsViewModel journalActionsViewModel,
			IPhoneRepository phoneRepository,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices)
			: base(journalActionsViewModel, unitOfWorkFactory, commonServices)
		{
			this.phoneRepository = phoneRepository ?? throw new ArgumentNullException(nameof(phoneRepository));

			TabName = "Типы телефонов";

			UpdateOnChanges(typeof(PhoneType));
		}

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
			UnitOfWorkFactory,
			CommonServices
		);

		protected override Func<JournalEntityNodeBase, PhoneTypeViewModel> OpenDialogFunction => node => new PhoneTypeViewModel(
			phoneRepository,
			EntityUoWBuilder.ForOpen(node.Id),
			UnitOfWorkFactory,
			CommonServices
		);

		protected override void InitializeJournalActionsViewModel()
		{
			EntitiesJournalActionsViewModel.Initialize(
				SelectionMode, EntityConfigs, this, HideJournal, OnItemsSelected,
				true, true, true, false);
		}
	}

	public class PhoneTypeJournalNode : JournalEntityNodeBase<PhoneType>
	{
		public string Name { get; set; }
		public PhonePurpose PhonePurpose { get; set; }
	}
}
