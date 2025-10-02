using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

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
		private int? _orderItemId;

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

		/// <summary>
		/// Id родительского кода
		/// </summary>
		public virtual int? ParentCodeId { get; set; }

		/// <summary>
		/// Необработанный код
		/// </summary>
		public virtual string RawCode
		{
			get => _rawCode;
			set => SetField(ref _rawCode, value);
		}

		/// <summary>
		/// Код GTIN
		/// </summary>
		[Display(Name = "Код GTIN")]
		public virtual string Gtin
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

		/// <summary>
		/// Id строки заказа
		/// </summary>
		[Display(Name = "Id строки заказа")]
		public virtual int? OrderItemId
		{
			get => _orderItemId;
			set => SetField(ref _orderItemId, value);
		}

		/// <summary>
		/// Идентификационный код
		/// </summary>
		public virtual string IdentificationCode =>
			CodeType == StagingTrueMarkCodeType.Transport
			? RawCode
			: $"01{Gtin}21{SerialNumber}";

		/// <summary>
		/// Вложенные коды
		/// </summary>
		public virtual IObservableList<StagingTrueMarkCode> InnerCodes { get; set; }
			= new ObservableList<StagingTrueMarkCode>();

		/// <summary>
		/// Добавление вложенного кода
		/// </summary>
		/// <param name="innerCode">Вложенный код</param>
		public virtual void AddInnerCode(StagingTrueMarkCode innerCode)
		{
			if(CodeType == StagingTrueMarkCodeType.Group
				&& innerCode.CodeType != StagingTrueMarkCodeType.Group
				&& innerCode.CodeType != StagingTrueMarkCodeType.Identification)
			{
				throw new InvalidOperationException(
					$"Невозможно добавить вложенный код {innerCode.IdentificationCode} типа {innerCode.CodeType} в групповой код {IdentificationCode}");
			}

			if(CodeType == StagingTrueMarkCodeType.Identification)
			{
				throw new InvalidOperationException(
						$"Невозможно добавить вложенный код {innerCode.IdentificationCode} типа {innerCode.CodeType} в код экземпляра {IdentificationCode}");
			}

			innerCode.ParentCodeId = Id;
			InnerCodes.Add(innerCode);
		}

		/// <summary>
		/// Удаление вложенного кода
		/// </summary>
		/// <param name="innerCode">Вложенный код</param>
		/// <exception cref="InvalidOperationException"></exception>
		public virtual void RemoveInnerCode(StagingTrueMarkCode innerCode)
		{
			if(!InnerCodes.Contains(innerCode))
			{
				throw new InvalidOperationException(
					$"Код {innerCode.IdentificationCode} не найден в списке вложенных кодов текущего кода {IdentificationCode}");
			}

			InnerCodes.Remove(innerCode);
		}

		/// <summary>
		/// Является транспортным кодом
		/// </summary>
		public virtual bool IsTransport =>
			CodeType == StagingTrueMarkCodeType.Transport;

		/// <summary>
		/// Является групповым кодом
		/// </summary>
		public virtual bool IsGroup =>
			CodeType == StagingTrueMarkCodeType.Group;

		/// <summary>
		/// Является кодом экземпляра
		/// </summary>
		public virtual bool IsIdentification =>
			CodeType == StagingTrueMarkCodeType.Identification;

		/// <summary>
		/// Получаем все коды, входящие в состав текущего кода, включая сам код и вложенные коды всех уровней
		/// </summary>
		public virtual IList<StagingTrueMarkCode> AllCodes =>
			AllTransportCodes
			.Union(AllGroupCodes)
			.Union(AllIdentificationCodes)
			.ToList();

		/// <summary>
		/// Получаем все транспортные коды, входящие в состав текущего кода, включая сам код (если является транспортным кодом) и вложенные коды всех уровней
		/// </summary>
		public virtual IList<StagingTrueMarkCode> AllTransportCodes =>
			GetAllCodesOfType(this, StagingTrueMarkCodeType.Transport);

		/// <summary>
		/// Получаем все групповые коды, входящие в состав текущего кода, включая сам код (если является групповым кодом) и вложенные коды всех уровней
		/// </summary>
		public virtual IList<StagingTrueMarkCode> AllGroupCodes =>
			GetAllCodesOfType(this, StagingTrueMarkCodeType.Group);

		/// <summary>
		/// Получаем все коды экземпляров, входящие в состав текущего кода, включая сам код (если является кодом экземпляра) и вложенные коды всех уровней
		/// </summary>
		public virtual IList<StagingTrueMarkCode> AllIdentificationCodes =>
			GetAllCodesOfType(this, StagingTrueMarkCodeType.Identification);

		public virtual string GetCodeTypeString()
		{
			switch (CodeType)
			{
				case StagingTrueMarkCodeType.Identification:
					return "Экземплярный";
				case StagingTrueMarkCodeType.Group:
					return "Групповой";
				case StagingTrueMarkCodeType.Transport:
					return "Транспортный";
				default:
					throw new InvalidOperationException("Неизвестный тип кода");
			}
		}

		public virtual string GetCodeSourceString()
		{
			switch(RelatedDocumentType)
			{
				case StagingTrueMarkCodeRelatedDocumentType.CarLoadDocumentItem:
					return "Талон погрузки";
				case StagingTrueMarkCodeRelatedDocumentType.RouteListItem:
					return "Маршрутный лист";
				case StagingTrueMarkCodeRelatedDocumentType.SelfDeliveryDocumentItem:
					return "Отпуск самовывоза";
				default:
					throw new InvalidOperationException("Неизвестный тип документа");
			}
		}

		private IList<StagingTrueMarkCode> GetAllCodesOfType(StagingTrueMarkCode code, StagingTrueMarkCodeType codeType)
		{
			var resultCodes = new List<StagingTrueMarkCode>();

			if(code.CodeType == codeType)
			{
				resultCodes.Add(code);
			}

			foreach(var innerCode in code.InnerCodes)
			{
				resultCodes.AddRange(GetAllCodesOfType(innerCode, codeType));
			}

			return resultCodes;
		}

		private void UpdateChildCodes()
		{
			foreach(var innerCode in InnerCodes)
			{
				innerCode.ParentCodeId = Id;
			}
		}

		public override bool Equals(object obj)
		{
			if(obj is StagingTrueMarkCode code)
			{
				return IdentificationCode == code.IdentificationCode;
			}
			else
			{
				return false;
			}
		}

		public override int GetHashCode()
		{
			return -1155050507 + EqualityComparer<string>.Default.GetHashCode(IdentificationCode);
		}
	}
}
