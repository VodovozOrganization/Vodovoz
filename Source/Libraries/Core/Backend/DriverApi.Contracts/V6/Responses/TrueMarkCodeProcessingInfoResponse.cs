namespace DriverApi.Contracts.V6.Responses
{
	/// <summary>
	/// Результат выполнения запроса по обработке кода ЧЗ
	/// </summary>
	public class TrueMarkCodeProcessingResultResponse
	{
		/// <summary>
		/// Результат выполнения операции
		/// </summary>
		public RequestProcessingResultTypeDto Result { get; set; }

		/// <summary>
		/// Описание ошибки
		/// </summary>
		public string Error { get; set; }

		/// <summary>
		/// Данные по номенклатуре и кодам ЧЗ
		/// </summary>
		public NomenclatureTrueMarkCodesDto Nomenclature { get; set; }
	}
}
