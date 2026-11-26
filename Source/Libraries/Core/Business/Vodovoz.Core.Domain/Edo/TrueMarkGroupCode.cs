using TrueMark.Contracts;
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
	public class TrueMarkGroupCode : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private string _rawCode;
		private bool _isInvalid;
		private string _gtin;
		private string _serialNumber;
		private string _checkCode;
		private TrueMarkProductGroup _productGroup;

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

		public virtual int? ParentGroupCodeId { get; set; }
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

		/// <summary>
		/// Товарная группа маркировки (вода / корма для животных)
		/// </summary>
		[Display(Name = "Товарная группа маркировки")]
		public virtual TrueMarkProductGroup ProductGroup
		{
			get => _productGroup;
			set => SetField(ref _productGroup, value);
		}

		public virtual IObservableList<TrueMarkGroupCode> InnerGroupCodes { get; set; }
			= new ObservableList<TrueMarkGroupCode>();

		public virtual IObservableList<TrueMarkIdentificationCode> InnerIdentificationCodes { get; set; }
			= new ObservableList<TrueMarkIdentificationCode>();

		public virtual string IdentificationCode => $"01{GTIN}21{SerialNumber}";

		public virtual string CashReceiptCode => TrueMarkCodeHelper.FormatCashReceiptCode(ProductGroup, IdentificationCode, CheckCode);

		public virtual void AddInnerGroupCode(TrueMarkGroupCode innerGroupCode)
		{
			innerGroupCode.ParentGroupCodeId = Id;
			InnerGroupCodes.Add(innerGroupCode);
		}

		public virtual void AddInnerIdentificationCode(TrueMarkIdentificationCode innerIdentificationCode)
		{
			innerIdentificationCode.ParentGroupCodeId = Id;
			InnerIdentificationCodes.Add(innerIdentificationCode);
		}

		public virtual void RemoveCode(TrueMarkIdentificationCode identificationCode)
		{
			identificationCode.ParentGroupCodeId = null;
			InnerIdentificationCodes.Remove(identificationCode);
		}

		public virtual void RemoveCode(TrueMarkGroupCode groupCode)
		{
			groupCode.ParentGroupCodeId = null;
			InnerGroupCodes.Remove(groupCode);
		}

		public virtual void ClearAllCodes()
		{
			var identificationCodesToRemove = InnerIdentificationCodes.ToArray();

			foreach(var identificationCode in identificationCodesToRemove)
			{
				RemoveCode(identificationCode);
			}

			var groupCodesToRemove = InnerGroupCodes.ToArray();

			foreach(var groupCode in groupCodesToRemove)
			{
				groupCode.ClearAllCodes();
				RemoveCode(groupCode);
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

			foreach(var innerIdentificationCode in InnerIdentificationCodes)
			{
				yield return innerIdentificationCode;
			}
		}

		public virtual void UpdateChildCodes()
		{
			foreach(var innerGroupCode in InnerGroupCodes)
			{
				innerGroupCode.ParentGroupCodeId = Id;
			}

			foreach(var innerIdentificationCode in InnerIdentificationCodes)
			{
				innerIdentificationCode.ParentGroupCodeId = Id;
			}
		}

		public override bool Equals(object obj)
		{
			if(obj is TrueMarkGroupCode code)
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
