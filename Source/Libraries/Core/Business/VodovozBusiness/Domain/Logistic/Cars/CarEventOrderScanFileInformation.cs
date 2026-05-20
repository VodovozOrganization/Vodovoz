using QS.DomainModel.Entity;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Common;

namespace Vodovoz.Domain.Logistic
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "информация о сканах заказ-нарядов событий ТС",
		Nominative = "информация о скане заказ-наряда события ТС")]
	[HistoryTrace]
	public class CarEventOrderScanFileInformation : FileInformation
	{
		private int _carEventId;

		[Display(Name = "Идентификатор события ТС")]
		[HistoryIdentifier(TargetType = typeof(CarEvent))]
		public virtual int CarEventId
		{
			get => _carEventId;
			set => SetField(ref _carEventId, value);
		}
	}
}
