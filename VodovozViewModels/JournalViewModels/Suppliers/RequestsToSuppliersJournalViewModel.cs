using System;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.Config;
using QS.Services;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Suppliers;
using Vodovoz.FilterViewModels.Suppliers;
using Vodovoz.JournalNodes;

namespace Vodovoz.JournalViewModels.Suppliers
{
	public class RequestsToSuppliersJournalViewModel : FilterableSingleEntityJournalViewModelBase<RequestToSupplier, RequestToSupplierViewModel, RequestToSupplierJournalNode, RequestsToSuppliersFilterViewModel>
	{
		readonly RequestsToSuppliersFilterViewModel filterViewModel;
		readonly IEntityConfigurationProvider entityConfigurationProvider;
		readonly ICommonServices commonServices;

		public RequestsToSuppliersJournalViewModel(RequestsToSuppliersFilterViewModel filterViewModel, IEntityConfigurationProvider entityConfigurationProvider, ICommonServices commonServices) : base(filterViewModel, entityConfigurationProvider, commonServices)
		{
			this.commonServices = commonServices;
			this.entityConfigurationProvider = entityConfigurationProvider;
			this.filterViewModel = filterViewModel;
			TabName = "Журнал заявок поставщикам";
			SetOrder<RequestToSupplier>(c => c.Id, true);

			UpdateOnChanges(typeof(RequestToSupplier));
		}

		protected override Func<IQueryOver<RequestToSupplier>> ItemsSourceQueryFunction => () => {
			Employee authorAlias = null;
			Nomenclature nomenclaturesAlias = null;
			RequestToSupplierJournalNode resultAlias = null;

			var authorProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "GET_PERSON_NAME_WITH_INITIALS(?1, ?2, ?3)"),
				NHibernateUtil.String,
				Projections.Property(() => authorAlias.LastName),
				Projections.Property(() => authorAlias.Name),
				Projections.Property(() => authorAlias.Patronymic)
			);

			var query = UoW.Session.QueryOver<RequestToSupplier>()
								   .Left.JoinAlias(x => x.Creator, () => authorAlias)
								   .Left.JoinAlias(x => x.RequestingNomenclatures, () => nomenclaturesAlias)
								   ;
			if(FilterViewModel.RestrictNomenclature != null)
				query.Where(() => nomenclaturesAlias.Id == FilterViewModel.RestrictNomenclature.Id);

			if(FilterViewModel.RestrictStartDate.HasValue)
				query.Where(x => x.CreatingDate >= FilterViewModel.RestrictStartDate.Value);

			if(FilterViewModel.RestrictEndDate.HasValue)
				query.Where(o => o.CreatingDate <= FilterViewModel.RestrictEndDate.Value.AddDays(1).AddTicks(-1));

			query.Where(
				GetSearchCriterion<RequestToSupplier>(
					x => x.Id,
					x => x.Name
				)
			);

			var result = query.SelectList(list => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Name).WithAlias(() => resultAlias.Name)
					.Select(x => x.CreatingDate).WithAlias(() => resultAlias.Created)
					.Select(authorProjection).WithAlias(() => resultAlias.Author)
				)
				.TransformUsing(Transformers.AliasToBean<RequestToSupplierJournalNode>())
				.OrderBy(x => x.Id)
				.Desc;

			return result;
		};

		protected override Func<RequestToSupplierViewModel> CreateDialogFunction => throw new NotImplementedException();

		protected override Func<RequestToSupplierJournalNode, RequestToSupplierViewModel> OpenDialogFunction => throw new NotImplementedException();
	}
}
