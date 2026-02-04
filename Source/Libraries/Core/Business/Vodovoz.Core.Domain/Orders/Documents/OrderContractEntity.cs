using QS.Print;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Clients;

namespace Vodovoz.Core.Domain.Orders.Documents
{
	public class OrderContractEntity: PrintableOrderDocumentEntity
	{
		private CounterpartyContractEntity _contract;
		int _copiesToPrint = 2;

		#region implemented abstract members of OrderDocument

		public override OrderDocumentType Type => OrderDocumentType.Contract;

		#endregion

		/// <summary>
		/// Договор
		/// </summary>
		[Display(Name = "Договор")]
		public virtual CounterpartyContractEntity Contract
		{
			get => _contract;
			set => SetField(ref _contract, value);
		}

		public override string Name
		{
			get
			{
				if(_contract == null)
				{
					return "Нет договора";
				}
				return $"Договор №{_contract.Number}";
			}
		}

		public override DateTime? DocumentDate => Contract?.IssueDate;
		public override PrinterType PrintType => PrinterType.ODT;

		public override int CopiesToPrint
		{
			get => _copiesToPrint;
			set => _copiesToPrint = value;
		}
	}
}
