using QS.Print;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Domain.Client;

namespace Vodovoz.Core.Domain.Orders.Documents
{
	public class PrintableOrderDocumentEntity : OrderDocumentEntity, IPrintableDocument
	{
		private int _copiesToPrint = -1;
		private readonly OrderDocumentType[] _typesForVariableQuantity = {
			OrderDocumentType.UPD,
			OrderDocumentType.SpecialUPD,
			OrderDocumentType.Torg12,
			OrderDocumentType.ShetFactura
		};

        /// <summary>
        /// Тип формата для печати документа
        /// </summary>
        [Display(Name = "Тип формата для печати документа")]
        public virtual PrinterType PrintType => PrinterType.None;

        /// <summary>
        /// Ориентация страницы для печати документа
        /// </summary>
        [Display(Name = "Ориентация страницы для печати документа")]
        public virtual DocumentOrientation Orientation => DocumentOrientation.Portrait;

        /// <summary>
        /// Количество копий для печати
        /// </summary>
        [Display(Name = "Количество копий для печати")]
        public virtual int CopiesToPrint
		{
			get
			{
				return _copiesToPrint < 0 && _typesForVariableQuantity.Contains(Type)
					? Order.DocumentType.HasValue && Order.DocumentType.Value == DefaultDocumentType.torg12 ? 1 : 2
					: _copiesToPrint;
			}
			set => _copiesToPrint = value;
		}

	}
}
