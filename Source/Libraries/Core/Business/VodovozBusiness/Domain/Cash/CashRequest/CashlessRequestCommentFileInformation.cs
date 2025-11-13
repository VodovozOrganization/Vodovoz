using QS.DomainModel.Entity;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Common;

namespace VodovozBusiness.Domain.Cash.CashRequest
{
	/// <summary>
	/// Информация о прикрепленном файле к комментарияю заявки на оплату по безналу
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "информация о прикрепляемых файлах к комментариям заявки на оплату по безналу",
		Nominative = "информация о прикрепленном файле к комментарияю заявки на оплату по безналу")]
	[HistoryTrace]
	public class CashlessRequestCommentFileInformation : FileInformation
	{
		private int _cashlessRequestCommentId;

		/// <summary>
		/// Идентификатор комментария заявки на оплату по безналу
		/// </summary>
		[Display(Name = "Идентификатор комментария заявки на оплату по безналу")]
		[HistoryIdentifier(TargetType = typeof(CashlessRequestComment))]
		public virtual int CashlessRequestCommentId
		{
			get => _cashlessRequestCommentId;
			set => SetField(ref _cashlessRequestCommentId, value);
		}
	}
}
