using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Domain.TrueMark
{
	/// <summary>
	/// Групповой код воды честного знака
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Masculine,
		Genitive = "группового кода воды честного знака",
		GenitivePlural = "групповых кодов воды честного знака",
		Nominative = "групповой код воды честного знака",
		NominativePlural = "групповые коды воды честного знака",
		Accusative = "групповой код воды честного знака",
		AccusativePlural = "групповые коды воды честного знака",
		Prepositional = "групповом коде воды честного знака",
		PrepositionalPlural = "групповых кодах воды честного знака")]
	public class TrueMarkWaterGroupCode : PropertyChangedBase, IDomainObject, ITrueMarkCodesProvider
	{
		private int _id;
		private string _rawCode;
		private bool _isInvalid;
		private string _gtin;
		private string _serialNumber;
		private string _checkCode;

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

		public virtual int? ParentWaterGroupCodeId { get; set; }
		public virtual int? ParentTransportCodeId { get; set; }

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
		/// Код не валидный
		/// </summary>
		[Display(Name = "Код не валидный")]
		public virtual bool IsInvalid
		{
			get => _isInvalid;
			set => SetField(ref _isInvalid, value);
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

		public virtual IObservableList<TrueMarkWaterGroupCode> InnerGroupCodes { get; set; }
			= new ObservableList<TrueMarkWaterGroupCode>();

		public virtual IObservableList<TrueMarkWaterIdentificationCode> InnerWaterCodes { get; set; }
			= new ObservableList<TrueMarkWaterIdentificationCode>();

		public virtual string IdentificationCode => $"01{GTIN}21{SerialNumber}";

		public virtual string CashReceiptCode => $"01{GTIN}21{SerialNumber}\u001d93{CheckCode}";

		public virtual string FormatForCheck1260 => $"01{GTIN}21{SerialNumber}\u001d93{CheckCode}";

		public virtual void AddInnerGroupCode(TrueMarkWaterGroupCode innerGroupCode)
		{
			innerGroupCode.ParentWaterGroupCodeId = Id;
			InnerGroupCodes.Add(innerGroupCode);
		}

		public virtual void AddInnerWaterCode(TrueMarkWaterIdentificationCode innerWaterCode)
		{
			innerWaterCode.ParentWaterGroupCodeId = Id;
			InnerWaterCodes.Add(innerWaterCode);
		}

		public virtual void RemoveCode(TrueMarkWaterIdentificationCode waterCode)
		{
			waterCode.ParentWaterGroupCodeId = null;
			InnerWaterCodes.Remove(waterCode);
		}

		public virtual void RemoveCode(TrueMarkWaterGroupCode waterGroupCode)
		{
			waterGroupCode.ParentWaterGroupCodeId = null;
			InnerGroupCodes.Remove(waterGroupCode);
		}

		public virtual void ClearAllCodes()
		{
			var waterCodesToRemove = InnerWaterCodes.ToArray();

			foreach(var waterCode in waterCodesToRemove)
			{
				RemoveCode(waterCode);
			}

			var waterGroupCodesToRemove = InnerGroupCodes.ToArray();

			foreach(var waterGroupCode in waterGroupCodesToRemove)
			{
				waterGroupCode.ClearAllCodes();
				RemoveCode(waterGroupCode);
			}
		}

		public virtual IEnumerable<TrueMarkAnyCode> GetAllCodes()
		{
			yield return this;

			foreach(var innerGroupCode in InnerGroupCodes)
			{
				foreach (var code in innerGroupCode.GetAllCodes())
				{
					yield return code;
				}
			}

			foreach(var innerWaterCode in InnerWaterCodes)
			{
				yield return innerWaterCode;
			}
		}

		public virtual void UpdateChildCodes()
		{
			foreach(var innerGroupCode in InnerGroupCodes)
			{
				innerGroupCode.ParentWaterGroupCodeId = Id;
			}

			foreach(var innerWaterCode in InnerWaterCodes)
			{
				innerWaterCode.ParentWaterGroupCodeId = Id;
			}
		}

		public override bool Equals(object obj)
		{
			if(obj is TrueMarkWaterGroupCode code)
			{
				return RawCode == code.RawCode;
			}
			else
			{
				return false;
			}
		}

		public override int GetHashCode()
		{
			
			return -1155050507 + EqualityComparer<string>.Default.GetHashCode(RawCode);
		}
	}
}
