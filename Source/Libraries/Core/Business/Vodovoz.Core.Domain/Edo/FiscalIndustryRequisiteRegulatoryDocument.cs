using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Регламентирующий документ отраслевого реквизит товара в фискальном документе
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Masculine,
		Nominative = "Регламентирующий документ отраслевого реквизита",
		NominativePlural = "Регламентирующие документы отраслевых реквизитов"
	)]
	public class FiscalIndustryRequisiteRegulatoryDocument : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private string _foivId;
		private string _docDateTime;
		private string _docNumber;

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Идентификатор Федерального органа исполнительной власти (ФОИВ)
		/// </summary>
		[Display(Name = "Идентификатор Федерального органа исполнительной власти (ФОИВ)")]
		public virtual string FoivId
		{
			get => _foivId;
			set => SetField(ref _foivId, value);
		}

		/// <summary>
		/// Дата регламентирующего документа
		/// </summary>
		[Display(Name = "Дата регламентирующего документа")]
		public virtual string DocDateTime
		{
			get => _docDateTime;
			set => SetField(ref _docDateTime, value);
		}

		/// <summary>
		/// Номер документа, который регламентирует заполнение отраслевых реквизитов.
		/// </summary>
		[Display(Name = "Номер регламентирующего документа")]
		public virtual string DocNumber
		{
			get => _docNumber;
			set => SetField(ref _docNumber, value);
		}
	}
}
