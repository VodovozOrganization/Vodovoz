using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
	public class TrueMarkWaterGroupCode : PropertyChangedBase, IDomainObject
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

		public IObservableList<TrueMarkWaterGroupCode> InnerGroupCodes { get; set; }
			= new ObservableList<TrueMarkWaterGroupCode>();

		public IObservableList<TrueMarkWaterIdentificationCode> InnerWaterCodes { get; set; }
			= new ObservableList<TrueMarkWaterIdentificationCode>();

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
			if(obj is TrueMarkWaterIdentificationCode code)
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
