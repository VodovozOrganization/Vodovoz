using System.Collections.Generic;

namespace TrueMark.Contracts.Requests
{
	/// <summary>
	/// Получение информации о коде
	/// </summary>
	public class CodeCheckRequest
	{
		/// <summary>
		/// Заводской номер фискального накопителя
		/// </summary>
		public string FiscalDriveNumber { get; set; }
		/// <summary>
		/// Список кодов маркировки на проверку
		/// </summary>
		IEnumerable<string> Codes { get; set; }
	}
}
