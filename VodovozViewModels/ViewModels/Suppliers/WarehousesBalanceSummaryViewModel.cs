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
		public BalanceSummaryRow[] SummaryRows { get; set; }
	}

	public class BalanceBean
	{
		public int NomId { get; set; }
		public int WarehouseId { get; set; }
		public decimal Amount { get; set; }
		public decimal MinCount { get; set; }
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

		public double AlgoTime { get; set; }

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

			var nomsSet = _nomsFilter.ParameterSets.FirstOrDefault(x => x.ParameterName == "nomenclatures");
			var noms = nomsSet?.GetIncludedParameters().ToList();
			var groups = _nomsFilter.ParameterSets.FirstOrDefault(x => x.ParameterName == "product_groups")?
				.GetIncludedParameters().ToList();
			var wars = _warsFilter.ParameterSets
				.FirstOrDefault(ps => ps.ParameterName == "warehouses")?.GetIncludedParameters().ToList();

			var report = new BalanceSummaryReport
			{
				WarehousesTitles = new List<string>(),
				SummaryRows = new BalanceSummaryRow[noms?.Count ?? 0]
			};
			report.WarehousesTitles = wars?.Select(x => x.Title).ToList();

			Nomenclature nomAlias = null;
			WarehouseMovementOperation inAlias = null;
			WarehouseMovementOperation woAlias = null;
			BalanceBean resultAlias = null;
			var nomsIds = noms?.Select(x => (int) x.Value).ToList();
			var warsIds = wars?.Select(x => (int) x.Value).ToList();
			var groupsIds = groups?.Select(x => (int) x.Value).ToList();

			var incomeQuery = UoW.Session.QueryOver(() => inAlias)
				.Fetch(SelectMode.Fetch, () => inAlias.Nomenclature)
				.Fetch(SelectMode.Fetch, () => inAlias.IncomingWarehouse)
				.Where(() => inAlias.OperationTime <= endDate)
				.Left.JoinAlias(() => inAlias.Nomenclature, () => nomAlias)
				.Where(Restrictions.In(Projections.Property(() => nomAlias.Id), nomsIds))
				//.Where(Restrictions.In(Projections.Property(() => nomAlias.ProductGroup.Id), groupsIds))
				.Where(Restrictions.In(Projections.Property(() => inAlias.IncomingWarehouse.Id), warsIds))
				.SelectList(list => list
					.SelectGroup(() => inAlias.IncomingWarehouse.Id).WithAlias(() => resultAlias.WarehouseId)
					.SelectGroup(() => nomAlias.Id).WithAlias(() => resultAlias.NomId)
					.Select(() => nomAlias.MinStockCount).WithAlias(() => resultAlias.MinCount)
					.Select(Projections.Sum(Projections.Property(() => inAlias.Amount))).WithAlias(() => resultAlias.Amount)
				)
				.OrderBy(() => inAlias.Nomenclature.Id).Asc
				.ThenBy(() => inAlias.IncomingWarehouse.Id).Asc
				.TransformUsing(Transformers.AliasToBean<BalanceBean>());

			var writeoffQuery = UoW.Session.QueryOver(() => woAlias)
				.Fetch(SelectMode.Fetch, () => woAlias.Nomenclature)
				.Fetch(SelectMode.Fetch, () => woAlias.WriteoffWarehouse)
				.Where(() => woAlias.OperationTime <= endDate)
				.Left.JoinAlias(() => woAlias.Nomenclature, () => nomAlias)
				.Where(Restrictions.In(Projections.Property(() => woAlias.Nomenclature.Id), nomsIds))
				//.Where(Restrictions.In(Projections.Property(() => nomAlias.ProductGroup.Id), groupsIds))
				.Where(Restrictions.In(Projections.Property(() => woAlias.WriteoffWarehouse.Id), warsIds))
				.SelectList(list => list
					.SelectGroup(() => woAlias.WriteoffWarehouse.Id).WithAlias(() => resultAlias.WarehouseId)
					.SelectGroup(() => nomAlias.Id).WithAlias(() => resultAlias.NomId)
					.Select(Projections.Sum(Projections.Property(() => woAlias.Amount))).WithAlias(() => resultAlias.Amount)
				)
				.OrderBy(() => nomAlias.Id).Asc
				.ThenBy(() => woAlias.WriteoffWarehouse.Id).Asc
				.TransformUsing(Transformers.AliasToBean<BalanceBean>());

			var inResult = await incomeQuery.ListAsync<BalanceBean>(cancellationToken);
			var woResult = await writeoffQuery.ListAsync<BalanceBean>(cancellationToken);

			//Кол-во списаний != кол-во начислений, используется два счетчика
			var addedCounter = 0;
			var removedCounter = 0;
			var start = DateTime.Now;
			for(var nomsCounter = 0; nomsCounter < noms?.Count; nomsCounter++)
			{
				cancellationToken.ThrowIfCancellationRequested();
				var row = new BalanceSummaryRow
				{
					NomId = (int) noms[nomsCounter].Value,
					NomTitle = noms[nomsCounter].Title,
					Separate = new decimal[wars?.Count ?? 0]
				};
				//Т.к. для ТМЦ может не быть incomingWarMovOps, то не будет MinStockCount. Дозагрузка сильно увеличивает время формирования
				//при этом дозагрузка зачастую бессмысленна, т.к. поле MinStockCount != 0 у малого кол-ва номенклатур.
				if(addedCounter < inResult.Count && inResult[addedCounter].NomId == row.NomId)
				{
					row.Min = inResult[addedCounter].MinCount;
				}
				else
				{
					row.Min = 0;
				}

				for(var warsCounter = 0; warsCounter < wars?.Count; warsCounter++)
				{//Т.к. данные запроса упорядочены, тут реализован доступ по индексам
					if(addedCounter == inResult.Count || removedCounter == woResult.Count)
					{
						/*Кол-во данных в запросе может не совпадать с кол-вом выбранных номенклатур. Несмотря на то, что счетчики
						 * увеличиваются лишь при совпадении, иногда кидается OutOfBounds, так что пришлось вставить break.
						 * На итоговые данные это не влияет */
						break;
					}
					var warId = (int) wars[warsCounter].Value;
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

				report.SummaryRows[nomsCounter] = row;
			}
			AlgoTime = (DateTime.Now - start).TotalSeconds;


			return await new ValueTask<BalanceSummaryReport>(report);
		}

		public void ShowWarning(string message)
		{
			ShowWarningMessage(message);
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
