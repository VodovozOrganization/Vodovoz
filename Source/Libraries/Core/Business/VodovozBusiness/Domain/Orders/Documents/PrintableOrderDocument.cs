using System;
using QS.Print;
using System.Linq;
using fyiReporting.RDL;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Client;

namespace Vodovoz.Domain.Orders.Documents
{
    public abstract class PrintableOrderDocument : OrderDocument, IPrintableDocument
    {
        public virtual PrinterType PrintType => PrinterType.None;
        
        public virtual OutputPresentationType[] RestrictedOutputPresentationTypes { get; set; }
        
        public virtual DocumentOrientation Orientation => DocumentOrientation.Portrait;
        
        int copiesToPrint = -1;
        public virtual int CopiesToPrint {
            get {
                if(copiesToPrint < 0 && typesForVariableQuantity.Contains(Type))
                    return Order.DocumentType.HasValue && Order.DocumentType.Value == DefaultDocumentType.torg12 ? 1 : 2;
                return copiesToPrint;
            }
            set => copiesToPrint = value;
        }
        
        readonly OrderDocumentType[] typesForVariableQuantity = {
            OrderDocumentType.UPD,
            OrderDocumentType.SpecialUPD,
            OrderDocumentType.Torg12,
            OrderDocumentType.ShetFactura
        };
    }
}