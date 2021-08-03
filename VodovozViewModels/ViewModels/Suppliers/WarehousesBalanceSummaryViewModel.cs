using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using NHibernate.Util;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Store;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using Vodovoz.ViewModels.Reports;

namespace Vodovoz.ViewModels.ViewModels.Suppliers
{
	public class BalanceSummaryRow
	{
		public int NomId { get; set; }
		public string NomTitle { get; set; }
		public decimal Min { get; set; }
		public decimal Common => Separate.Sum();
		public decimal Diff => Common - Min;
		public decimal[] Separate { get; set; }
	}

	public class BalanceSummaryReport
	{
		public List<string> WarehousesTitles { get; set; }
		public List<BalanceSummaryRow> SummaryRows { get; set; }
	}

	public class BalanceBean
	{
		public int NomId { get; set; }
		public int WarehouseId { get; set; }
		public decimal? Amount { get; set; }
	}

	public class WarehousesBalanceSummaryViewModel : DialogTabViewModelBase
	{
		private SelectableParameterReportFilterViewModel _nomsViewModel;
		private SelectableParameterReportFilterViewModel _warsViewModel;
		private SelectableParametersReportFilter _nomsFilter;
		private SelectableParametersReportFilter _warsFilter;

		private bool _isGenerating = false;
		private BalanceSummaryReport _report;

		public WarehousesBalanceSummaryViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigation)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			TabName = "Остатки по складам";
		}

		#region Свойства

		public DateTime? EndDate { get; set; } = DateTime.Today;

		public bool AllNoms { get; set; }
		public bool GtZNoms { get; set; }
		public bool LeZNoms { get; set; }
		public bool LtMinNoms { get; set; }
		public bool GeMinNoms { get; set; }

		public bool AllWars { get; set; }
		public bool GtZWars { get; set; }
		public bool LeZWars { get; set; }
		public bool LtMinWars { get; set; }
		public bool GeMinWars { get; set; }

		public SelectableParameterReportFilterViewModel NomsViewModel => _nomsViewModel ?? (_nomsViewModel = CreateNomsViewModel());

		public SelectableParameterReportFilterViewModel WarsViewModel => _warsViewModel ?? (_warsViewModel = CreateWarsViewModel());

		/*public bool CanSave
		{
			get => _canSave;
			set => SetField(ref _canSave, value);
		}

		public bool IsSaving
		{
			get => _isSaving;
			set
			{
				SetField(ref _isSaving, value);
				CanSave = !IsSaving;
			}
		}*/

		public bool IsGenerating
		{
			get => _isGenerating;
			set => SetField(ref _isGenerating, value);
		}

		public CancellationTokenSource ReportGenerationCancelationTokenSource { get; set; }

		public BalanceSummaryReport Report
		{
			get => _report;
			set => SetField(ref _report, value);
		}

		#endregion

		public async Task<BalanceSummaryReport> ActionGenerateReport(CancellationToken cancellationToken)
		{
			/*if(!SelectedIndicators.Any())
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Не выбрано ни 1 показателя");
				return null;
			}*/

			try
			{
				return await Generate(EndDate, cancellationToken);
			}
			finally
			{
				UoW.Session.Clear();
			}
		}

		private async Task<BalanceSummaryReport> Generate(
			DateTime? endDate,
			CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			endDate = endDate?.AddHours(23).AddMinutes(59).AddSeconds(59);

			var report = new BalanceSummaryReport {WarehousesTitles = new List<string>(), SummaryRows = new List<BalanceSummaryRow>()};
			var wars = _warsFilter.ParameterSets.FirstOrDefault(ps => ps.ParameterName == "warehouses")?.GetIncludedParameters().ToList();

			report.WarehousesTitles = wars?.Select(x => x.Title).ToList();
			var nomsSet = _nomsFilter.ParameterSets.FirstOrDefault(x => x.ParameterName == "nomenclatures");
			var noms = nomsSet?.GetIncludedParameters().ToList();
			var groupsSet = _nomsFilter.ParameterSets.FirstOrDefault(x => x.ParameterName == "product_groups");
			var groupsIds = groupsSet?.GetSelectedValues().ToList();
			var includeGroups = groupsSet?.FilterType == SelectableFilterType.Include;
//FIXME учесть группы
//FIXME учесть ДАТУ

			Nomenclature nomAlias = null;
			Warehouse warAlias = null;
			WarehouseMovementOperation inAlias = null;
			WarehouseMovementOperation woAlias = null;
			BalanceBean resultAlias = null;
			var nomsIds = noms?.Select(x => (int) x.Value).ToList();
			var warsIds = wars?.Select(x => (int) x.Value).ToList();

			var incomeQuery = UoW.Session.QueryOver(() => inAlias)
				.Fetch(SelectMode.Fetch, () => inAlias.Nomenclature)
				.Fetch(SelectMode.Fetch, () => inAlias.IncomingWarehouse)
				.Where(() => inAlias.OperationTime < endDate)
				.Left.JoinAlias(() => inAlias.Nomenclature, () => nomAlias)
				.Left.JoinAlias(() => inAlias.IncomingWarehouse, () => warAlias)
				.Where(Restrictions.In(Projections.Property(() => nomAlias.Id), nomsIds))
				.Where(Restrictions.In(Projections.Property(() => warAlias.Id), warsIds))
				.SelectList(list => list
					.SelectGroup(() => warAlias.Id).WithAlias(() => resultAlias.WarehouseId)
					.SelectGroup(() => nomAlias.Id).WithAlias(() => resultAlias.NomId)
					.Select(Projections.Sum(Projections.Property(() => inAlias.Amount))).WithAlias(() => resultAlias.Amount)
				).TransformUsing(Transformers.AliasToBean<BalanceBean>());

			var writeoffQuery = UoW.Session.QueryOver(() => woAlias)
				.Fetch(SelectMode.Fetch, () => woAlias.Nomenclature)
				.Fetch(SelectMode.Fetch, () => woAlias.WriteoffWarehouse)
				.Where(() => woAlias.OperationTime < endDate)
				.Left.JoinAlias(() => woAlias.Nomenclature, () => nomAlias)
				.Left.JoinAlias(() => woAlias.WriteoffWarehouse, () => warAlias)
				.Where(Restrictions.In(Projections.Property(() => nomAlias.Id), nomsIds))
				.Where(Restrictions.In(Projections.Property(() => warAlias.Id), warsIds))
				.SelectList(list => list
					.SelectGroup(() => warAlias.Id).WithAlias(() => resultAlias.WarehouseId)
					.SelectGroup(() => nomAlias.Id).WithAlias(() => resultAlias.NomId)
					.Select(Projections.Sum(Projections.Property(() => woAlias.Amount))).WithAlias(() => resultAlias.Amount)
				).TransformUsing(Transformers.AliasToBean<BalanceBean>());

			var inResult = await incomeQuery.ListAsync<BalanceBean>(cancellationToken);
			var woResult = await writeoffQuery.ListAsync<BalanceBean>(cancellationToken);

			/*var incomes = incomeQuery.List();
			UoW.Session.Clear();
			var writeoffs = writeoffQuery.List();*/

			foreach(var nom in noms)
			{
				cancellationToken.ThrowIfCancellationRequested();
				var row = new BalanceSummaryRow();
				row.NomId = (int) nom.Value;
				row.NomTitle = nom.Title;
				row.Separate = new decimal[wars?.Count ?? 0];
				row.Min = UoW.Session.QueryOver<Nomenclature>().Where(n => n.Id == row.NomId).List().Select(x => x.MinStockCount).First();

				var i = 0;

				foreach(var war in wars)
				{
					cancellationToken.ThrowIfCancellationRequested();
					/*var added = incomes
						.Where(x => x.Nomenclature.Id == (int)nom.Value && x.IncomingWarehouse.Id == (int)war.Value)
						.Sum(x => x.Amount);
					var removed = writeoffs
						.Where(x => x.Nomenclature.Id == (int)nom.Value && x.WriteoffWarehouse.Id == (int)war.Value)
						.Sum(x => x.Amount);*/
					/*var added = incomeQuery.List<decimal?>().FirstOrDefault();
					var removed = writeoffQuery.List<decimal?>().FirstOrDefault();*/

					/*var incomeQuery = UoW.Session.QueryOver(() => inAlias)
						.Left.JoinAlias(() => inAlias.Nomenclature, () => nomAlias)
						.Left.JoinAlias(() => inAlias.IncomingWarehouse, () => warAlias)
						.Where(Restrictions.Eq(Projections.Property(() => nomAlias.Id), nom.Value))
						.Where(Restrictions.Eq(Projections.Property(() => warAlias.Id), war.Value));
					var writeoffQuery = UoW.Session.QueryOver(() => woAlias)
						.Left.JoinAlias(() => woAlias.Nomenclature, () => nomAlias)
						.Left.JoinAlias(() => woAlias.WriteoffWarehouse, () => warAlias)
						.Where(Restrictions.Eq(Projections.Property(() => nomAlias.Id), nom.Value))
						.Where(Restrictions.Eq(Projections.Property(() => warAlias.Id), war.Value));

					incomeQuery.Select(Projections.Sum(Projections.Property(() => inAlias.Amount)));
					writeoffQuery.Select(Projections.Sum(Projections.Property(() => woAlias.Amount)));
					
					var added = incomeQuery.List<decimal?>().FirstOrDefault();
					var removed = writeoffQuery.List<decimal?>().FirstOrDefault();

					row.Separate[i] = (added ?? 0) - (removed ?? 0);*/

					var added = inResult
						.Where(x => x.NomId == (int)nom.Value && x.WarehouseId == (int)war.Value)
						.Select(x => x.Amount).FirstOrDefault();
					var removed = woResult
						.Where(x => x.NomId == (int)nom.Value && x.WarehouseId == (int)war.Value)
						.Select(x => x.Amount).FirstOrDefault();
					row.Separate[i] = (added ?? 0) - (removed ?? 0);
					i++;
				}

				report.SummaryRows.Add(row);
			}

			return await new ValueTask<BalanceSummaryReport>(report);
		}

		public void ShowWarning(string message)
		{
			ShowWarningMessage(message);
		}

		private SelectableParameterReportFilterViewModel CreateNomsViewModel()
		{
			_nomsFilter = new SelectableParametersReportFilter(UoW);
			var nomenclatureTypeParam = _nomsFilter.CreateParameterSet("Типы номенклатур", "nomenclature_type",
				new ParametersEnumFactory<NomenclatureCategory>());

			var nomenclatureParam = _nomsFilter.CreateParameterSet("Номенклатуры", "nomenclatures",
				new ParametersFactory(UoW, (filters) =>
				{
					SelectableEntityParameter<Nomenclature> resultAlias = null;
					var query = UoW.Session.QueryOver<Nomenclature>().Where(x => !x.IsArchive);
					if(filters != null && EnumerableExtensions.Any(filters))
					{
						foreach(var f in filters)
						{
							var filterCriterion = f();
							if(filterCriterion != null)
							{
								query.Where(filterCriterion);
							}
						}
					}

					query.SelectList(list => list
						.Select(x => x.Id).WithAlias(() => resultAlias.EntityId)
						.Select(x => x.OfficialName).WithAlias(() => resultAlias.EntityTitle)
					).TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<Nomenclature>>());
					return query.List<SelectableParameter>();
				})
			);

			nomenclatureParam.AddFilterOnSourceSelectionChanged(nomenclatureTypeParam,
				() =>
				{
					var selectedValues = nomenclatureTypeParam.GetSelectedValues();
					return !EnumerableExtensions.Any(selectedValues)
						? null
						: Restrictions.On<Nomenclature>(x => x.Category).IsIn(nomenclatureTypeParam.GetSelectedValues().ToArray());
				}
			);

			UoW.Session.QueryOver<ProductGroup>().Fetch(SelectMode.Fetch, x => x.Childs).List();

			_nomsFilter.CreateParameterSet("Группы товаров", "product_groups",
				new RecursiveParametersFactory<ProductGroup>(UoW, (filters) =>
					{
						var query = UoW.Session.QueryOver<ProductGroup>();
						if(filters != null && EnumerableExtensions.Any(filters))
						{
							foreach(var f in filters)
							{
								query.Where(f());
							}
						}

						return query.List();
					},
					x => x.Name,
					x => x.Childs)
			);

			return new SelectableParameterReportFilterViewModel(_nomsFilter);
		}

		private SelectableParameterReportFilterViewModel CreateWarsViewModel()
		{
			_warsFilter = new SelectableParametersReportFilter(UoW);

			_warsFilter.CreateParameterSet("Склады", "warehouses", new ParametersFactory(UoW, (filters) =>
			{
				SelectableEntityParameter<Warehouse> resultAlias = null;
				var query = UoW.Session.QueryOver<Warehouse>().Where(x => !x.IsArchive);
				if(filters != null && EnumerableExtensions.Any(filters))
				{
					foreach(var f in filters)
					{
						var filterCriterion = f();
						if(filterCriterion != null)
						{
							query.Where(filterCriterion);
						}
					}
				}

				query.SelectList(list => list
					.Select(x => x.Id).WithAlias(() => resultAlias.EntityId)
					.Select(x => x.Name).WithAlias(() => resultAlias.EntityTitle)
				).TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<Warehouse>>());
				return query.List<SelectableParameter>();
			}));

			return new SelectableParameterReportFilterViewModel(_warsFilter);
		}
	}
}
