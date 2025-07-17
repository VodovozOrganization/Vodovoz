using System.Collections.Generic;

namespace Vodovoz.ViewModels.ViewModels.Documents.SelfDeliveryCodesScan
{
	public partial class CodesScanViewModel
	{
		/// <summary>
		/// Строка результата сканирования
		/// </summary>
		public class CodeScanRow
		{
			/// <summary>
			/// Номер строки
			/// </summary>
			public int RowNumber { get; set; }
			/// <summary>
			/// Код-агрегат
			/// </summary>
			public CodeScanRow Parent { get; set; }
			/// <summary>
			/// Индивидуальные коды
			/// </summary>
			public List<CodeScanRow> Children { get; set; } = new List<CodeScanRow>();
			/// <summary>
			/// Код
			/// </summary>
			public string RawCode { get; set; }
			/// <summary>
			/// Название номенклатуры
			/// </summary>
			public string NomenclatureName { get; set; }
			/// <summary>
			/// Валиден в ЧЗ
			/// </summary>
			public bool? IsTrueMarkValid { get; set; }
			/// <summary>
			/// Наличие в заказе
			/// </summary>
			public bool? HasInOrder { get; set; }
			/// <summary>
			/// Доп. информация
			/// </summary>
			public string AdditionalInformation { get; set; }
			
			/// <summary>
			/// Текст в поле "Наличие в заказе"
			/// </summary>
			public string HasInOrderString
			{
				get
				{
					if(HasInOrder is null)
					{
						return null;
					}

					return HasInOrder.Value ? "Да" : "Нет";
				}
			}
			
			/// <summary>
			/// Дуюдикат кода
			/// </summary>
			public bool IsDuplicate { get; set; }
		}
	}
}
