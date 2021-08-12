using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Multi;
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
		public List<decimal> Separate { get; set; }
	}

	public class BalanceSummaryReport
	{
		public DateTime EndDate { get; set; }
		public List<string> WarehousesTitles { get; set; }
		public List<BalanceSummaryRow> SummaryRows { get; set; }
	}

	public class BalanceBean
	{
		public int NomId { get; set; }
		public int WarehouseId { get; set; }
		public decimal Amount { get; set; }
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

		public bool AllNoms { get; set; } = true;
		public bool GtZNoms { get; set; }
		public bool LeZNoms { get; set; }
		public bool LtMinNoms { get; set; }
		public bool GeMinNoms { get; set; }

		public bool AllWars { get; set; } = true;
		public bool GtZWars { get; set; }
		public bool LeZWars { get; set; }
		public bool LtMinWars { get; set; }
		public bool GeMinWars { get; set; }

		public SelectableParameterReportFilterViewModel NomsViewModel => _nomsViewModel ?? (_nomsViewModel = CreateNomsViewModel());

		public SelectableParameterReportFilterViewModel WarsViewModel => _warsViewModel ?? (_warsViewModel = CreateWarsViewModel());

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

		public void ShowWarning(string message)
		{
			ShowWarningMessage(message);
		}

		public async Task<BalanceSummaryReport> ActionGenerateReport(CancellationToken cancellationToken)
		{
			try
			{
				return await Generate(EndDate ?? DateTime.Today, cancellationToken);
			}
			finally
			{
				UoW.Session.Clear();
			}
		}

		private async Task<BalanceSummaryReport> Generate(
			DateTime endDate,
			CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			endDate = endDate.AddHours(23).AddMinutes(59).AddSeconds(59);

			var nomsSet = _nomsFilter.ParameterSets.FirstOrDefault(x => x.ParameterName == "nomenclatures");
			var noms = nomsSet?.GetIncludedParameters()?.ToList();
			var typesSet = _nomsFilter.ParameterSets.FirstOrDefault(x => x.ParameterName == "nomenclature_type");
			var types = typesSet?.GetIncludedParameters()?.ToList();
			var groupsSet = _nomsFilter.ParameterSets.FirstOrDefault(x => x.ParameterName == "product_groups");
			var groups = groupsSet?.GetIncludedParameters()?.ToList();
			var wars = _warsFilter.ParameterSets
				.FirstOrDefault(ps => ps.ParameterName == "warehouses")?.GetIncludedParameters()?.ToList();

			Nomenclature nomAlias = null;
			WarehouseMovementOperation inAlias = null;
			WarehouseMovementOperation woAlias = null;
			BalanceBean resultAlias = null;
			var warsIds = wars?.Select(x => (int)x.Value).ToArray();
			var groupsIds = groups?.Select(x => (int)x.Value).ToArray();
			var groupsSelected = groups?.Any() ?? false;
			var typesSelected = types?.Any() ?? false;
			var nomsSelected = noms?.Any() ?? false;
			var allNomsSelected = noms?.Count == nomsSet?.Parameters.Count;
			if(groupsSelected)
			{
				var nomsInGroupsIds = (List<int>)await UoW.Session.QueryOver(() => nomAlias)
					.Where(Restrictions.In(Projections.Property(() => nomAlias.ProductGroup.Id), groupsIds))
					.AndNot(() => nomAlias.IsArchive)
					.Select(n => n.Id).ListAsync<int>(cancellationToken);
				if(nomsSelected)
				{
					noms = noms.Where(x => nomsInGroupsIds.Contains((int)x.Value)).ToList();
				}
				else
				{
					noms?.AddRange(nomsSet.Parameters.Where(x => nomsInGroupsIds.Contains((int)x.Value)).ToList());
				}
			}

			if(typesSelected && !nomsSelected)
			{
				noms?.AddRange(nomsSet.Parameters);
			}

			var nomsIds = noms?.Select(x => (int)x.Value).ToArray();

			var report = new BalanceSummaryReport
			{
				EndDate = endDate,
				WarehousesTitles = wars?.Select(x => x.Title).ToList(),
				SummaryRows = new List<BalanceSummaryRow>()
			};

			#region Запросы

			var inQuery = UoW.Session.QueryOver(() => inAlias)
				.Where(() => inAlias.OperationTime <= endDate)
				.Inner.JoinAlias(x => x.Nomenclature, () => nomAlias)
				.Where(Restrictions.In(Projections.Property(() => inAlias.IncomingWarehouse.Id), warsIds))
				.SelectList(list => list
					.SelectGroup(() => inAlias.IncomingWarehouse.Id).WithAlias(() => resultAlias.WarehouseId)
					.SelectGroup(() => nomAlias.Id).WithAlias(() => resultAlias.NomId)
					.Select(Projections.Sum(Projections.Property(() => inAlias.Amount))).WithAlias(() => resultAlias.Amount)
				)
				.OrderBy(() => nomAlias.Id).Asc
				.ThenBy(() => inAlias.IncomingWarehouse.Id).Asc
				.TransformUsing(Transformers.AliasToBean<BalanceBean>());

			var woQuery = UoW.Session.QueryOver(() => woAlias)
				.Where(() => woAlias.OperationTime <= endDate)
				.Inner.JoinAlias(x => x.Nomenclature, () => nomAlias)
				.Where(Restrictions.In(Projections.Property(() => woAlias.WriteoffWarehouse.Id), warsIds))
				.SelectList(list => list
					.SelectGroup(() => woAlias.WriteoffWarehouse.Id).WithAlias(() => resultAlias.WarehouseId)
					.SelectGroup(() => nomAlias.Id).WithAlias(() => resultAlias.NomId)
					.Select(Projections.Sum(Projections.Property(() => woAlias.Amount))).WithAlias(() => resultAlias.Amount)
				)
				.OrderBy(() => nomAlias.Id).Asc
				.ThenBy(() => woAlias.WriteoffWarehouse.Id).Asc
				.TransformUsing(Transformers.AliasToBean<BalanceBean>());

			var msQuery = UoW.Session.QueryOver(() => nomAlias)
				.Select(n => n.MinStockCount);

			if(typesSelected)
			{
				var typesIds = types.Select(x => (int)x.Value).ToArray();
				inQuery.Where(Restrictions.In(Projections.Property(() => nomAlias.Category), typesIds)).AndNot(() => nomAlias.IsArchive);
				woQuery.Where(Restrictions.In(Projections.Property(() => nomAlias.Category), typesIds)).AndNot(() => nomAlias.IsArchive);
				msQuery.Where(Restrictions.In(Projections.Property(() => nomAlias.Category), typesIds))
					.AndNot(() => nomAlias.IsArchive);
			}

			if(nomsSelected && !allNomsSelected)
			{
				inQuery.Where(Restrictions.In(Projections.Property(() => inAlias.Nomenclature.Id), nomsIds));
				woQuery.Where(Restrictions.In(Projections.Property(() => woAlias.Nomenclature.Id), nomsIds));
				msQuery.Where(Restrictions.In(Projections.Property(() => nomAlias.Id), nomsIds));
			}

			#endregion

			var batch = UoW.Session.CreateQueryBatch();
			var inFuture = batch.AddAsFuture<BalanceBean>(inQuery);
			var woFuture = batch.AddAsFuture<BalanceBean>(woQuery);
			var msFuture = batch.AddAsFuture<decimal>(msQuery);

			cancellationToken.ThrowIfCancellationRequested();
			var inResult = inFuture.ToArray();
			var woResult = woFuture.ToArray();
			var msResult = msFuture.ToArray();

			//Кол-во списаний != кол-во начислений, используется два счетчика
			var addedCounter = 0;
			var removedCounter = 0;
			for(var nomsCounter = 0; nomsCounter < noms?.Count; nomsCounter++)
			{
				cancellationToken.ThrowIfCancellationRequested();
				var row = new BalanceSummaryRow
				{
					NomId = (int)noms[nomsCounter].Value,
					NomTitle = noms[nomsCounter].Title,
					Separate = new List<decimal>(),
					Min = msResult[nomsCounter]
				};

				for(var warsCounter = 0; warsCounter < wars?.Count; warsCounter++)
				{
					row.Separate.Add(0);
					//Т.к. данные запросов упорядочены, тут реализован доступ по индексам
					if(addedCounter == inResult.Length || removedCounter == woResult.Length)
					{
						/*Кол-во данных в запросе может не совпадать с кол-вом выбранных номенклатур и складов. Несмотря на то, что счетчики
						 * увеличиваются лишь при совпадении, иногда кидается OutOfBounds, так что пришлось вставить continue.
						 * На итоговые данные это не влияет */
						continue;
					}

					var warId = (int)wars[warsCounter].Value;
					var tempIn = inResult[addedCounter];
					if(tempIn.WarehouseId == warId && tempIn.NomId == row.NomId)
					{
						row.Separate[warsCounter] = tempIn.Amount;
						addedCounter++;
					}

					var tempWo = woResult[removedCounter];
					if(tempWo.WarehouseId == warId && tempWo.NomId == row.NomId)
					{
						row.Separate[warsCounter] -= tempWo.Amount;
						removedCounter++;
					}
				}

				AddRow(ref report, row);
			}

			RemoveWarehousesByFilterCondition(ref report, cancellationToken);

			return await new ValueTask<BalanceSummaryReport>(report);
		}

		private void RemoveWarehousesByFilterCondition(ref BalanceSummaryReport report, CancellationToken cancellationToken)
		{
			if(AllWars)
			{
				return;
			}

			for(var warCounter = 0; warCounter < report.WarehousesTitles.Count; warCounter++)
			{
				if(GtZWars && report.SummaryRows.FirstOrDefault(row => row.Separate[warCounter] > 0) == null
				|| LeZWars && report.SummaryRows.FirstOrDefault(row => row.Separate[warCounter] <= 0) == null
				|| LtMinWars && report.SummaryRows.FirstOrDefault(row => row.Min < row.Separate[warCounter]) == null
				|| GeMinWars && report.SummaryRows.FirstOrDefault(row => row.Min >= row.Separate[warCounter]) == null)
				{
					RemoveWarehouseByIndex(ref report, ref warCounter, cancellationToken);
				}
			}
		}

		private void RemoveWarehouseByIndex(ref BalanceSummaryReport report, ref int warCounter, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			report.WarehousesTitles.RemoveAt(warCounter);
			var i = warCounter;
			report.SummaryRows.ForEach(row => row.Separate.RemoveAt(i));
			warCounter--;
		}

		private void AddRow(ref BalanceSummaryReport report, BalanceSummaryRow row)
		{
			if(AllNoms
			|| GtZNoms && row.Separate.FirstOrDefault(war => war > 0) > 0
			|| LeZNoms && row.Separate.FirstOrDefault(war => war <= 0) <= 0
			|| LtMinNoms && row.Separate.FirstOrDefault(war => war < row.Min) < row.Min
			|| GeMinNoms && row.Separate.FirstOrDefault(war => war >= row.Min) >= row.Min)
			{
				report.SummaryRows.Add(row);
			}
		}

		#region Настройка фильтров

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

		#endregion
	}
}
