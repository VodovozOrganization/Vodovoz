using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using QS.DomainModel.UoW;
using QS.Models;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories.Goods.BottleAnalytics;
using WhereIsTheBottle.Controls;

namespace WhereIsTheBottle.Models.MainContent
{
	public class GeneralAssetModel : UoWFactoryModelBase
	{
		private readonly ICommonBottleAnalyticsRepository _commonBottleAnalyticsRepository;
		private readonly IGeneralAssetBottleAnalyticsRepository _generalAssetBottleAnalyticsRepository;

		public GeneralAssetModel(IUnitOfWorkFactory unitOfWorkFactory, ICommonBottleAnalyticsRepository commonBottleAnalyticsRepository,
			IGeneralAssetBottleAnalyticsRepository generalAssetBottleAnalyticsRepository)
			: base(unitOfWorkFactory)
		{
			_commonBottleAnalyticsRepository = commonBottleAnalyticsRepository
				?? throw new ArgumentNullException(nameof(commonBottleAnalyticsRepository));
			_generalAssetBottleAnalyticsRepository = generalAssetBottleAnalyticsRepository
				?? throw new ArgumentNullException(nameof(generalAssetBottleAnalyticsRepository));
		}

		public IList<NomenclatureNode> Nomenclatures { get; set; }

		public IList<NomenclatureNode> WaterNomenclatures =>
			Nomenclatures.Where(x => x.Category == NomenclatureCategory.water && !x.IsShabbyBottle).ToList();

		public IList<NomenclatureNode> EmptyNomenclatures =>
			Nomenclatures.Where(x => x.Category == NomenclatureCategory.bottle && !x.IsShabbyBottle).ToList();

		public IList<NomenclatureNode> ShabbyNomenclatures =>
			Nomenclatures.Where(x => x.IsShabbyBottle).OrderByDescending(x => x.Category == NomenclatureCategory.water).ToList();

		public bool TryGetDataTable(DateTime date, out IList<string> errorMessages, out DataTable dataTable)
		{
			using var uow = UnitOfWorkFactory.CreateWithoutRoot();
			date = date.Date.AddDays(1).AddTicks(-1);
			dataTable = null;
			Nomenclatures = _commonBottleAnalyticsRepository.GetBottleAnalyticsNomenclaturesWithShabbyBottlesFuture(uow).ToList();

			if(!ValidateNomenclatureNodes(Nomenclatures, out errorMessages))
			{
				return false;
			}

			var nomenclatureIds = Nomenclatures.Select(x => x.Id).ToArray();

			#region Создание DataTable

			dataTable = new DataTable();
			dataTable.Columns.Add(new DataColumn("Склад", typeof(string)));

			var waterNomenclatures = WaterNomenclatures;
			var emptyNomenclatures = EmptyNomenclatures;
			var shabbyNomenclatures = ShabbyNomenclatures;

			foreach(var nomenclature in waterNomenclatures)
			{
				dataTable.Columns.Add(new DataColumn(nomenclature.VeryShortName, typeof(int)));
			}
			if(waterNomenclatures.Any())
			{
				dataTable.Columns.Add(new DataColumn("Всего полных", typeof(int)));
			}

			foreach(var nomenclature in emptyNomenclatures)
			{
				dataTable.Columns.Add(new DataColumn(nomenclature.VeryShortName, typeof(int)));
			}
			if(emptyNomenclatures.Any())
			{
				dataTable.Columns.Add(new DataColumn("Всего пустых", typeof(int)));
			}

			if(waterNomenclatures.Any() && emptyNomenclatures.Any())
			{
				dataTable.Columns.Add(new DataColumn("Всего", typeof(int)));
			}

			foreach(var nomenclature in shabbyNomenclatures)
			{
				dataTable.Columns.Add(new DataColumn(nomenclature.VeryShortName, typeof(int)));
			}
			if(shabbyNomenclatures.Any())
			{
				dataTable.Columns.Add(new DataColumn("Всего стройки", typeof(int)));
			}

			#endregion

			var driversLateAssetFuture =
				_generalAssetBottleAnalyticsRepository.GetDriversLateAssetFuture(uow, date.AddDays(-1), nomenclatureIds);
			var driversOnDateAssetFuture = _generalAssetBottleAnalyticsRepository.GetDriversOnDateAssetFuture(uow, date, nomenclatureIds);
			var warehouseIncomeAssetFuture =
				_generalAssetBottleAnalyticsRepository.GetWarehouseIncomeAssetFuture(uow, date, nomenclatureIds);
			var warehouseWriteoffAssetFuture =
				_generalAssetBottleAnalyticsRepository.GetWarehouseWriteoffAssetFuture(uow, date, nomenclatureIds);

			#region Заполнение DataTable

			var warehouseNames =
				new List<string> { "Водители просрочка", "Водители сегодня" }
					.Union(warehouseIncomeAssetFuture.Select(x => x.WarehouseName))
					.Union(warehouseWriteoffAssetFuture.Select(x => x.WarehouseName))
					.Distinct()
					.ToList();

			foreach(var warehouseName in warehouseNames)
			{
				var row = dataTable.NewRow();
				row["Склад"] = warehouseName;
				int waterSum = 0, emptySum = 0, shabbySum = 0;

				if(waterNomenclatures.Any())
				{
					foreach(var nomenclatureName in waterNomenclatures.Select(x => x.VeryShortName))
					{
						var value =
							(warehouseIncomeAssetFuture
								.FirstOrDefault(x => x.NomenclatureVeryShortName == nomenclatureName && x.WarehouseName == warehouseName)
								?.Amount ?? 0)
							- (warehouseWriteoffAssetFuture
								.FirstOrDefault(x => x.NomenclatureVeryShortName == nomenclatureName && x.WarehouseName == warehouseName)
								?.Amount ?? 0)
							+ (driversLateAssetFuture
								.FirstOrDefault(x => x.NomenclatureVeryShortName == nomenclatureName && x.WarehouseName == warehouseName)
								?.Amount ?? 0)
							+ (driversOnDateAssetFuture
								.FirstOrDefault(x => x.NomenclatureVeryShortName == nomenclatureName && x.WarehouseName == warehouseName)
								?.Amount ?? 0);

						if(value != 0)
						{
							row[nomenclatureName] = value;
							waterSum += value;
						}
					}
					if(waterSum != 0)
					{
						row["Всего полных"] = waterSum;
					}
				}

				if(emptyNomenclatures.Any())
				{
					foreach(var nomenclatureName in emptyNomenclatures.Select(x => x.VeryShortName))
					{
						var value =
							(warehouseIncomeAssetFuture
								.FirstOrDefault(x => x.NomenclatureVeryShortName == nomenclatureName && x.WarehouseName == warehouseName)
								?.Amount ?? 0)
							- (warehouseWriteoffAssetFuture
								.FirstOrDefault(x => x.NomenclatureVeryShortName == nomenclatureName && x.WarehouseName == warehouseName)
								?.Amount ?? 0)
							+ (driversLateAssetFuture
								.FirstOrDefault(x => x.NomenclatureVeryShortName == nomenclatureName && x.WarehouseName == warehouseName)
								?.Amount ?? 0)
							+ (driversOnDateAssetFuture
								.FirstOrDefault(x => x.NomenclatureVeryShortName == nomenclatureName && x.WarehouseName == warehouseName)
								?.Amount ?? 0);

						if(value != 0)
						{
							row[nomenclatureName] = value;
							emptySum += value;
						}
					}
					if(emptySum != 0)
					{
						row["Всего пустых"] = emptySum;
					}
				}

				if(waterNomenclatures.Any() && emptyNomenclatures.Any())
				{
					if(waterSum + emptySum != 0)
					{
						row["Всего"] = waterSum + emptySum;
					}
				}
				if(shabbyNomenclatures.Any())
				{
					foreach(var nomenclatureName in shabbyNomenclatures.Select(x => x.VeryShortName))
					{
						var value =
							(warehouseIncomeAssetFuture
								.FirstOrDefault(x => x.NomenclatureVeryShortName == nomenclatureName && x.WarehouseName == warehouseName)
								?.Amount ?? 0)
							- (warehouseWriteoffAssetFuture
								.FirstOrDefault(x => x.NomenclatureVeryShortName == nomenclatureName && x.WarehouseName == warehouseName)
								?.Amount ?? 0)
							+ (driversLateAssetFuture
								.FirstOrDefault(x => x.NomenclatureVeryShortName == nomenclatureName && x.WarehouseName == warehouseName)
								?.Amount ?? 0)
							+ (driversOnDateAssetFuture
								.FirstOrDefault(x => x.NomenclatureVeryShortName == nomenclatureName && x.WarehouseName == warehouseName)
								?.Amount ?? 0);

						if(value != 0)
						{
							row[nomenclatureName] = value;
							shabbySum += value;
						}
					}
					if(shabbySum != 0)
					{
						row["Всего стройки"] = shabbySum;
					}
				}

				dataTable.Rows.Add(row);
			}

			return true;

			#endregion
		}

		private bool ValidateNomenclatureNodes(IList<NomenclatureNode> nomenclatureNodes, out IList<string> errorMessages)
		{
			errorMessages = new List<string>();
			if(!nomenclatureNodes.Any())
			{
				errorMessages.Add("Не найдены подходящие номенклатуры");
				return false;
			}

			var noVeryShorNameNomenclatures = nomenclatureNodes.Where(x => String.IsNullOrWhiteSpace(x.VeryShortName)).ToList();
			if(noVeryShorNameNomenclatures.Any())
			{
				var sb = new StringBuilder();
				sb.AppendLine("В следующих номенклатурах не заполнено Очень короткое название:");
				foreach(var nomenclature in noVeryShorNameNomenclatures)
				{
					sb.AppendLine($"\tКод: {nomenclature.Id}, Название: {nomenclature.Name}");
				}
				errorMessages.Add(sb.ToString());
				return false;
			}

			var sameVeryShortNameGrouped = nomenclatureNodes
				.GroupBy(x => x.VeryShortName)
				.Where(x => x.Count() > 1)
				.ToList();
			if(sameVeryShortNameGrouped.Any())
			{
				var sb = new StringBuilder();
				sb.AppendLine("Некоторые номенклатуры имеют одинаковое Очень короткое название:");
				foreach(var groupedNomenclatures in sameVeryShortNameGrouped)
				{
					sb.AppendLine($"{groupedNomenclatures.Key}");
					foreach(var nomenclature in groupedNomenclatures)
					{
						sb.AppendLine($"\tКод: {nomenclature.Id}, Название: {nomenclature.Name}");
					}
				}
				errorMessages.Add(sb.ToString());
				return false;
			}

			return true;
		}
	}
}
