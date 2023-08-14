using System;
using System.Collections.Generic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Services;

namespace Vodovoz.Parameters
{
	public class RouteListParametersProvider : IRouteListParametersProvider
	{
		private readonly IParametersProvider _parametersProvider;
		private string _cargoDailyNormParameterPrefix = "CargoDailyNormFor";

		public RouteListParametersProvider(IParametersProvider parametersProvider)
		{
			_parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
		}

		private string GetCargoDailyNormParameterName(CarTypeOfUse carTypeOfUse) => $"{_cargoDailyNormParameterPrefix}{carTypeOfUse}";

		public int CashSubdivisionSofiiskayaId => _parametersProvider.GetIntValue("cashsubdivision_sofiiskaya_id");
		public int CashSubdivisionParnasId => _parametersProvider.GetIntValue("cashsubdivision_parnas_id");
		public int WarehouseSofiiskayaId => _parametersProvider.GetIntValue("warehouse_sofiiskaya_id");
		public int WarehouseParnasId => _parametersProvider.GetIntValue("warehouse_parnas_id");
		
		//Склад Бугры
		public int WarehouseBugriId => _parametersProvider.GetIntValue("warehouse_bugri_id");
		public int SouthGeographicGroupId => _parametersProvider.GetIntValue("south_geographic_group_id");
		public decimal GetCargoDailyNorm(CarTypeOfUse carTypeOfUse) => _parametersProvider.GetDecimalValue(GetCargoDailyNormParameterName(carTypeOfUse));

		public void SaveCargoDailyNorms(Dictionary<CarTypeOfUse, decimal> cargoDailyNorms)
		{
			foreach(var cargoDailyNorm in cargoDailyNorms)
			{
				_parametersProvider.CreateOrUpdateParameter(GetCargoDailyNormParameterName(cargoDailyNorm.Key), cargoDailyNorm.Value.ToString());
			}
		}
	}
}
