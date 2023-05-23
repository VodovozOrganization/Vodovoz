using System;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Goods;
using Vodovoz.Services;

namespace Vodovoz.Parameters
{
	public class NomenclatureParametersProvider : INomenclatureParametersProvider
	{
		private readonly IParametersProvider _parametersProvider;

		public NomenclatureParametersProvider(IParametersProvider parametersProvider)
		{
			_parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
		}
		
		#region INomenclatureParametersProvider implementation

		public int Folder1cForOnlineStoreNomenclatures => _parametersProvider.GetIntValue("folder_1c_for_online_store_nomenclatures");

		public int PaidDeliveryNomenclatureId => _parametersProvider.GetIntValue("paid_delivery_nomenclature_id");

		public int MeasurementUnitForOnlineStoreNomenclatures => _parametersProvider.GetIntValue("measurement_unit_for_online_store_nomenclatures");

		public int RootProductGroupForOnlineStoreNomenclatures => _parametersProvider.GetIntValue("root_product_group_for_online_store_nomenclatures");

		public int CurrentOnlineStoreId => _parametersProvider.GetIntValue("current_online_store_id");

		public decimal GetWaterPriceIncrement => _parametersProvider.GetDecimalValue("water_price_increment");

		public string OnlineStoreExportFileUrl => _parametersProvider.GetStringValue("online_store_export_file_url");

		public int FastDeliveryNomenclatureId => _parametersProvider.GetIntValue("fast_delivery_nomenclature_id");

		public int PromotionalNomenclatureGroupId => _parametersProvider.GetIntValue("promotional_nomenclature_group_id");

		public int DailyCoolerRentNomenclatureId => _parametersProvider.GetIntValue(nameof(DailyCoolerRentNomenclatureId));

		public int[] PaidDeliveriesNomenclaturesIds() => new[]
		{
			PaidDeliveryNomenclatureId,
			FastDeliveryNomenclatureId
		};

		#region Получение номенклатур воды

		public Nomenclature GetWaterSemiozerie(IUnitOfWork uow)
		{
			int id = _parametersProvider.GetIntValue("nomenclature_semiozerie_id");
			return uow.GetById<Nomenclature>(id);
		}

		public Nomenclature GetWaterKislorodnaya(IUnitOfWork uow)
		{
			int id = _parametersProvider.GetIntValue("nomenclature_kislorodnaya_id");
			return uow.GetById<Nomenclature>(id);
		}

		public Nomenclature GetWaterSnyatogorskaya(IUnitOfWork uow)
		{
			int id = _parametersProvider.GetIntValue("nomenclature_snyatogorskaya_id");
			return uow.GetById<Nomenclature>(id);
		}

		public Nomenclature GetWaterKislorodnayaDeluxe(IUnitOfWork uow)
		{
			int id = _parametersProvider.GetIntValue("nomenclature_kislorodnaya_deluxe_id");
			return uow.GetById<Nomenclature>(id);
		}

		public Nomenclature GetWaterStroika(IUnitOfWork uow)
		{
			int id = _parametersProvider.GetIntValue("nomenclature_stroika_id");
			return uow.GetById<Nomenclature>(id);
		}

		public Nomenclature GetWaterRuchki(IUnitOfWork uow)
		{
			int id = _parametersProvider.GetIntValue("nomenclature_ruchki_id");
			return uow.GetById<Nomenclature>(id);
		}
		
		#endregion
		
		public Nomenclature GetDefaultBottleNomenclature(IUnitOfWork uow)
		{
			var id = _parametersProvider.GetIntValue("default_bottle_nomenclature");
			return uow.GetById<Nomenclature>(id);
		}
		
		public Nomenclature GetNomenclatureToAddWithMaster(IUnitOfWork uow)
		{
			var id = _parametersProvider.GetIntValue("номенклатура_для_выезда_с_мастером");
			return uow.GetById<Nomenclature>(id);
		}
		
		public int[] GetSanitisationNomenclature(IUnitOfWork uow)
		{
			var ids = _parametersProvider.GetStringValue("выезд_мастера_для_сан_обр");
			
			var splitedIds = ids
				.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
				.Select(x => int.Parse(x))
				.ToArray();
			
			return splitedIds;
		}
		
		public Nomenclature GetForfeitNomenclature(IUnitOfWork uow)
		{
			var id = _parametersProvider.GetIntValue("forfeit_nomenclature_id");
			return uow.GetById<Nomenclature>(id);
		}
		
		public int GetIdentifierOfOnlineShopGroup()
		{
			string parameterName = "код_группы_товаров_для_интерент-магазина";
			
			if(!_parametersProvider.ContainsParameter(parameterName)
			   || !int.TryParse(_parametersProvider.GetParameterValue(parameterName), out int res))
			{
				return 0;
			}

			return res;
		}

		public Nomenclature GetFastDeliveryNomenclature(IUnitOfWork uow)
		{
			return uow.GetById<Nomenclature>(FastDeliveryNomenclatureId);
		}

		#endregion INomenclatureParametersProvider implementation
	}
}
