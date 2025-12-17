using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.TrueMark;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Заявка на отправку счета без отгруки на долги по ЭДО
	/// </summary>
	public class BillForDebtEdoRequest : PrimaryEdoRequest
	{
		private int _orderWithoutShipmentForDebtId;

		/// <summary>
		/// Код счета без отгрузки на долги
		/// </summary>
		[Display(Name = "Код счета без отгрузки на долги")]
		public virtual int OrderWithoutShipmentForDebtId
		{
			get => _orderWithoutShipmentForDebtId;
			set => SetField(ref _orderWithoutShipmentForDebtId, value);
		}

		public override EdoDocumentType DocumentType => EdoDocumentType.Bill;
	}
}
