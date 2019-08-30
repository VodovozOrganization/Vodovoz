using System;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.Config;
using QS.Project.Domain;
using QS.Services;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Suppliers;
using Vodovoz.EntityRepositories.Suppliers;
using Vodovoz.FilterViewModels.Suppliers;
using Vodovoz.Infrastructure.Services;
using Vodovoz.JournalNodes;
using Vodovoz.ViewModels.Suppliers;

namespace Vodovoz.JournalViewModels.Suppliers
{
	public class RequestsToSuppliersJournalViewModel : FilterableSingleEntityJournalViewModelBase<RequestToSupplier, RequestToSupplierViewModel, RequestToSupplierJournalNode, RequestsToSuppliersFilterViewModel>
	{
		readonly RequestsToSuppliersFilterViewModel filterViewModel;
		readonly IEntityConfigurationProvider entityConfigurationProvider;
		readonly ICommonServices commonServices;
		readonly ISupplierPriceItemsRepository supplierPriceItemsRepository;
		readonly IEmployeeService employeeService;

		public RequestsToSuppliersJournalViewModel(
			RequestsToSuppliersFilterViewModel filterViewModel,
			IEntityConfigurationProvider entityConfigurationProvider,
			ICommonServices commonServices,
			IEmployeeService employeeService,
			ISupplierPriceItemsRepository supplierPriceItemsRepository
		) : base(filterViewModel, entityConfigurationProvider, commonServices)
		{
			this.employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			this.supplierPriceItemsRepository = supplierPriceItemsRepository ?? throw new ArgumentNullException(nameof(supplierPriceItemsRepository));
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			this.entityConfigurationProvider = entityConfigurationProvider ?? throw new ArgumentNullException(nameof(entityConfigurationProvider));
			this.filterViewModel = filterViewModel;
			TabName = "Журнал заявок поставщикам";
			SetOrder(c => c.Id, true);

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
								   .Left.JoinAlias(x => x.RequestingNomenclatureItems, () => nomenclaturesAlias)
								   ;

			if(FilterViewModel?.RestrictNomenclature != null) {
				var subquery = QueryOver.Of<RequestToSupplierItem>()
										.Where(r => r.Nomenclature.Id == FilterViewModel.RestrictNomenclature.Id)
										.Select(r => r.RequestToSupplier.Id);
				query.WithSubquery.WhereProperty(r => r.Id).In(subquery);
			}

			if(FilterViewModel != null && FilterViewModel.RestrictStartDate.HasValue)
				query.Where(x => x.CreatingDate >= FilterViewModel.RestrictStartDate.Value);

			if(FilterViewModel != null && FilterViewModel.RestrictEndDate.HasValue)
				query.Where(o => o.CreatingDate <= FilterViewModel.RestrictEndDate.Value.AddDays(1).AddTicks(-1));

			query.Where(
				GetSearchCriterion<RequestToSupplier>(
					x => x.Id,
					x => x.Name
				)
			);

			var result = query.SelectList(list => list
					.SelectGroup(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Name).WithAlias(() => resultAlias.Name)
					.Select(x => x.CreatingDate).WithAlias(() => resultAlias.Created)
					.Select(authorProjection).WithAlias(() => resultAlias.Author)
				)
				.TransformUsing(Transformers.AliasToBean<RequestToSupplierJournalNode>())
				.OrderBy(x => x.Id)
				.Desc;

			return result;
		};

		protected override Func<RequestToSupplierViewModel> CreateDialogFunction => () => new RequestToSupplierViewModel(
			EntityConstructorParam.ForCreate(),
			commonServices,
			entityConfigurationProvider,
			employeeService,
			supplierPriceItemsRepository
		);

		protected override Func<RequestToSupplierJournalNode, RequestToSupplierViewModel> OpenDialogFunction => n => new RequestToSupplierViewModel(
			EntityConstructorParam.ForOpen(n.Id),
			commonServices,
			entityConfigurationProvider,
			employeeService,
			supplierPriceItemsRepository
		);
	}
}