using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Код честного знака для промежуточного хранения
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Masculine,
		Nominative = "код ЧЗ для промежуточного хранения",
		NominativePlural = "коды ЧЗ для промежуточного хранения",
		Prepositional = "коде ЧЗ для промежуточного хранения",
		PrepositionalPlural = "кодах ЧЗ для промежуточного хранения",
		Accusative = "код ЧЗ для промежуточного хранения",
		AccusativePlural = "коды ЧЗ для промежуточного хранения",
		Genitive = "коде ЧЗ для промежуточного хранения",
		GenitivePlural = "коды ЧЗ для промежуточного хранения")]
	[HistoryTrace]
	public class StagingTrueMarkCode : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private string _rawCode;
		private string _gtin;
		private string _serialNumber;
		private string _checkCode;
		private StagingTrueMarkCodeType _codeType;
		private StagingTrueMarkCodeRelatedDocumentType _relatedDocumentType;
		private int _relatedDocumentId;

		/// <summary>
		/// Идентификатор
		/// </summary>
		public virtual int Id
		{
			get => _id;
			set
			{
				if(SetField(ref _id, value))
				{
					UpdateChildCodes();
				}
			}
		}

		public virtual int? ParentCodeId { get; set; }

		/// <summary>
		/// Необработанный код
		/// </summary>
		[Display(Name = "Необработанный код")]
		public virtual string RawCode
		{
			get => _rawCode;
			set => SetField(ref _rawCode, value);
		}

		/// <summary>
		/// Код GTIN
		/// </summary>
		[Display(Name = "Код GTIN")]
		public virtual string GTIN
		{
			get => _gtin;
			set => SetField(ref _gtin, value);
		}

		/// <summary>
		/// Серийный номер экземпляра
		/// </summary>
		[Display(Name = "Серийный номер экземпляра")]
		public virtual string SerialNumber
		{
			get => _serialNumber;
			set => SetField(ref _serialNumber, value);
		}

		/// <summary>
		/// Код проверки
		/// </summary>
		[Display(Name = "Код проверки")]
		public virtual string CheckCode
		{
			get => _checkCode;
			set => SetField(ref _checkCode, value);
		}

		/// <summary>
		/// Тип кода
		/// </summary>
		[Display(Name = "Тип кода")]
		public virtual StagingTrueMarkCodeType CodeType
		{
			get => _codeType;
			set => SetField(ref _codeType, value);
		}

		/// <summary>
		/// Тип связанного документа
		/// </summary>
		[Display(Name = "Тип связанного документа")]
		public virtual StagingTrueMarkCodeRelatedDocumentType RelatedDocumentType
		{
			get => _relatedDocumentType;
			set => SetField(ref _relatedDocumentType, value);
		}

		/// <summary>
		/// Id связанного документа
		/// </summary>
		[Display(Name = "Id связанного документа")]
		public virtual int RelatedDocumentId
		{
			get => _relatedDocumentId;
			set => SetField(ref _relatedDocumentId, value);
		}

		public virtual string IdentificationCode =>
			CodeType == StagingTrueMarkCodeType.Transport
			? RawCode
			: $"01{GTIN}21{SerialNumber}";

		public virtual IObservableList<StagingTrueMarkCode> InnerCodes { get; set; }
			= new ObservableList<StagingTrueMarkCode>();

		public virtual void AddInnerCode(StagingTrueMarkCode innerCode)
		{
			innerCode.ParentCodeId = Id;
			InnerCodes.Add(innerCode);
		}

		public virtual void UpdateChildCodes()
		{
			foreach(var innerCode in InnerCodes)
			{
				innerCode.ParentCodeId = Id;
			}
		}
	}

	/// <summary>
	/// Тип кода ЧЗ для промежуточного хранения
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Masculine,
		Nominative = "тип кода ЧЗ для промежуточного хранения",
		NominativePlural = "типы кодов ЧЗ для промежуточного хранения",
		Prepositional = "типе кода ЧЗ для промежуточного хранения",
		PrepositionalPlural = "типах кодов ЧЗ для промежуточного хранения",
		Accusative = "тип кода ЧЗ для промежуточного хранения",
		AccusativePlural = "типы кодов ЧЗ для промежуточного хранения",
		Genitive = "типа кода ЧЗ для промежуточного хранения",
		GenitivePlural = "типы кодов ЧЗ для промежуточного хранения")]
	public enum StagingTrueMarkCodeType
	{
		/// <summary>
		/// Код экземпляра
		/// </summary>
		[Display(Name = "Код экземпляра")]
		Identification,
		/// <summary>
		/// Групповой код
		/// </summary>
		[Display(Name = "Групповой код")]
		Group,
		/// <summary>
		/// Транспортный код
		/// </summary>
		[Display(Name = "Транспортный код")]
		Transport
	}

	/// <summary>
	/// Тип связанного документа кода ЧЗ для промежуточного хранения
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Masculine,
		Nominative = "тип связанного документа кода ЧЗ для промежуточного хранения",
		NominativePlural = "типы связанных документов кодов ЧЗ для промежуточного хранения",
		Prepositional = "типе связанного документа кода ЧЗ для промежуточного хранения",
		PrepositionalPlural = "типах связанных документов кодов ЧЗ для промежуточного хранения",
		Accusative = "тип связанного документа кода ЧЗ для промежуточного хранения",
		AccusativePlural = "типы связанных документов кодов ЧЗ для промежуточного хранения",
		Genitive = "типе связанного документа кода ЧЗ для промежуточного хранения",
		GenitivePlural = "типах связанных документов кодов ЧЗ для промежуточного хранения")]
	public enum StagingTrueMarkCodeRelatedDocumentType
	{
		/// <summary>
		/// Строка талона погрузки
		/// </summary>
		[Display(Name = "Строка талона погрузки")]
		CarLoadDocumentItem,
		/// <summary>
		/// Строка маршрутного листа
		/// </summary>
		[Display(Name = "Строка маршрутного листа")]
		RouteListItem,
		/// <summary>
		/// Строка документа самовывоза
		/// </summary>
		[Display(Name = "Строка документа самовывоза")]
		SelfDeliveryDocumentItem
	}
}
