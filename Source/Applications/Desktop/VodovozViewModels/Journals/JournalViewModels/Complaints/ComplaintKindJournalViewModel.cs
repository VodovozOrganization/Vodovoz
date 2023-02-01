﻿using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using System;
using System.Linq;
using Autofac;
using NHibernate.Criterion;
using QS.Project.DB;
using QS.Project.Journal.EntitySelector;
using Vodovoz.Domain.Complaints;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Complaints;
using Vodovoz.ViewModels.Journals.FilterViewModels.Complaints;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Journals.JournalNodes.Complaints;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Complaints
{
	public class ComplaintKindJournalViewModel : FilterableSingleEntityJournalViewModelBase<ComplaintKind, ComplaintKindViewModel, ComplaintKindJournalNode, ComplaintKindJournalFilterViewModel>
	{
		private readonly IEntityAutocompleteSelectorFactory _employeeSelectorFactory;
		private readonly ISalesPlanJournalFactory _salesPlanJournalFactory;
		private readonly INomenclatureJournalFactory _nomenclatureSelectorFactory;
		private readonly ILifetimeScope _scope;

		public ComplaintKindJournalViewModel(
			ComplaintKindJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IEntityAutocompleteSelectorFactory employeeSelectorFactory,
			ISalesPlanJournalFactory salesPlanJournalFactory, 
			INomenclatureJournalFactory nomenclatureSelectorFactory,
			ILifetimeScope scope)
			: base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			_employeeSelectorFactory = employeeSelectorFactory ?? throw new ArgumentNullException(nameof(employeeSelectorFactory));
			_salesPlanJournalFactory = salesPlanJournalFactory ?? throw new ArgumentNullException(nameof(salesPlanJournalFactory));
			_nomenclatureSelectorFactory = nomenclatureSelectorFactory ?? throw new ArgumentNullException(nameof(nomenclatureSelectorFactory));
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));

			TabName = "Виды рекламаций";

			UpdateOnChanges(
				typeof(ComplaintKind),
				typeof(ComplaintObject)
				);
		}

		protected override Func<IUnitOfWork, IQueryOver<ComplaintKind>> ItemsSourceQueryFunction => (uow) =>
		{
			ComplaintKind complaintKindAlias = null;
			ComplaintObject complaintObjectAlias = null;
			ComplaintKindJournalNode resultAlias = null;
			Subdivision subdivisionsAlias = null;

			var itemsQuery = uow.Session.QueryOver(() => complaintKindAlias)
				.Left.JoinAlias(x => x.ComplaintObject, () => complaintObjectAlias)
				.Left.JoinAlias(x => x.Subdivisions, () => subdivisionsAlias);

			if(FilterViewModel.ComplaintObject != null)
			{
				itemsQuery.Where(x => x.ComplaintObject.Id == FilterViewModel.ComplaintObject.Id);
			}

			itemsQuery.Where(GetSearchCriterion(
				() => complaintKindAlias.Id,
				() => complaintKindAlias.Name,
				() => complaintObjectAlias.Name)
			);

			var subdivisionsProjection = CustomProjections.GroupConcat(
				() => subdivisionsAlias.ShortName,
				orderByExpression: () => subdivisionsAlias.ShortName,
				separator: ", "
			);

			itemsQuery.SelectList(list => list
					.SelectGroup(() => complaintKindAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => complaintKindAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(() => complaintKindAlias.IsArchive).WithAlias(() => resultAlias.IsArchive)
					.Select(() => complaintObjectAlias.Name).WithAlias(() => resultAlias.ComplaintObject)
					.Select(subdivisionsProjection).WithAlias(() => resultAlias.Subdivisions)
				)
				.TransformUsing(Transformers.AliasToBean<ComplaintKindJournalNode>());

			return itemsQuery;
		};

		protected override void CreateNodeActions()
		{
			CreateDefaultAddActions();
			CreateDefaultEditAction();
			CreateDefaultSelectAction();
		}

		protected override Func<ComplaintKindViewModel> CreateDialogFunction => () =>
			new ComplaintKindViewModel(EntityUoWBuilder.ForCreate(), UnitOfWorkFactory, commonServices, _employeeSelectorFactory, Refresh, 
				_salesPlanJournalFactory, _nomenclatureSelectorFactory, _scope.BeginLifetimeScope());

		protected override Func<ComplaintKindJournalNode, ComplaintKindViewModel> OpenDialogFunction =>
			(node) => new ComplaintKindViewModel(EntityUoWBuilder.ForOpen(node.Id), UnitOfWorkFactory, commonServices, _employeeSelectorFactory, Refresh,
				_salesPlanJournalFactory, _nomenclatureSelectorFactory, _scope.BeginLifetimeScope());
	}
}
