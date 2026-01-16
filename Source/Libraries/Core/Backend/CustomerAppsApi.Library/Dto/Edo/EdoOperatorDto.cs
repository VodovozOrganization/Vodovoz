namespace CustomerAppsApi.Library.Dto.Edo
{
	/// <summary>
	/// Данные оператора ЭДО
	/// </summary>
	public class EdoOperatorDto
	{
		private EdoOperatorDto(int id, string code, string name, string brandName)
		{
			ErpId = id;
			Code = code;
			Name = name;
			BrandName = brandName;
		}
		
		/// <summary>
		/// Идентификатор оператора в Erp
		/// </summary>
		public int ErpId { get; }
		/// <summary>
		/// Код
		/// </summary>
		public string Code { get; }
		/// <summary>
		/// Наименование
		/// </summary>
		public string Name { get; }
		/// <summary>
		/// Название брэнда
		/// </summary>
		public string BrandName { get; }
		
		public static EdoOperatorDto Create(int id, string code, string name, string brandName) => new(id, code, name, brandName);
	}
}
