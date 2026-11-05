using System.ComponentModel.DataAnnotations;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Edo
{
	/// <summary>
	/// Источник документа УПД.
	/// </summary>
	public enum EdoUpdDocumentType
	{
		/// <summary>УПД клиентского заказа.</summary>
		[Display(Name = "Заказ")]
		Order,
		/// <summary>УПД трансфера между организациями.</summary>
		[Display(Name = "Трансфер")]
		Transfer
	}
}
