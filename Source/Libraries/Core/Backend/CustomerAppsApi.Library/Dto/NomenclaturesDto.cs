﻿using System.Collections.Generic;
using Vodovoz.Nodes;

namespace CustomerAppsApi.Library.Dto
{
	/// <summary>
	/// Номенклатуры
	/// </summary>
	public class NomenclaturesDto
	{
		/// <summary>
		/// Сообщение об ошибке
		/// </summary>
		public string ErrorMessage { get; set; }
		/// <summary>
		/// Список номенклатур
		/// </summary>
		public IList<OnlineNomenclatureDto> OnlineNomenclatures { get; set; }
	}
}
