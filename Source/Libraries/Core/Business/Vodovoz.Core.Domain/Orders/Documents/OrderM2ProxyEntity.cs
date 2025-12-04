using QS.Print;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Employees;

namespace Vodovoz.Core.Domain.Orders.Documents
{
	public class OrderM2ProxyEntity : PrintableOrderDocumentEntity
	{
		private int _copiesToPrint = 1;
		private M2ProxyDocumentEntity _m2Proxy;

		#region implemented abstract members of OrderDocument

		public override OrderDocumentType Type => OrderDocumentType.M2Proxy;

		#endregion

		/// <summary>
		/// Доверенность М-2
		/// </summary>
		[Display(Name = "Доверенность М-2")]
		public virtual M2ProxyDocumentEntity M2Proxy
		{
			get => _m2Proxy;
			set => SetField(ref _m2Proxy, value);
		}

		public override string Name => $"Доверенность М-2 №{M2Proxy.Id}";

		public override DateTime? DocumentDate => M2Proxy?.Date;

		public override PrinterType PrintType => PrinterType.ODT;

		public override int CopiesToPrint
		{
			get => _copiesToPrint;
			set => _copiesToPrint = value;
		}
	}
}
