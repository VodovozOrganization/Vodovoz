using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Domain.TrueMark
{
	/// <summary>
	/// Транспортный код честного знака
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Masculine,
		Genitive = "транспортного кода честного знака",
		GenitivePlural = "транспортных кодов честного знака",
		Nominative = "транспортный код честного знака",
		NominativePlural = "транспортные коды честного знака",
		Accusative = "транспортный код честного знака",
		AccusativePlural = "транспортные коды честного знака",
		Prepositional = "транспортном коде честного знака",
		PrepositionalPlural = "транспортных кодах честного знака")]
	public class TrueMarkTransportCode : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private string _rawCode;
		private bool _isInvalid;

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

		public virtual IObservableList<TrueMarkTransportCode> InnerTransportCodes { get; set; }
			= new ObservableList<TrueMarkTransportCode>();

		public virtual IObservableList<TrueMarkWaterGroupCode> InnerGroupCodes { get; set; }
			= new ObservableList<TrueMarkWaterGroupCode>();

		public virtual IObservableList<TrueMarkWaterIdentificationCode> InnerWaterCodes { get; set; }
			= new ObservableList<TrueMarkWaterIdentificationCode>();

		public virtual void AddInnerTransportCode(TrueMarkTransportCode innerTransportCode)
		{
			innerTransportCode.ParentTransportCodeId = Id;
			InnerTransportCodes.Add(innerTransportCode);
		}

		public virtual void AddInnerGroupCode(TrueMarkWaterGroupCode innerGroupCode)
		{
			innerGroupCode.ParentTransportCodeId = Id;
			InnerGroupCodes.Add(innerGroupCode);
		}

		public virtual void AddInnerWaterCode(TrueMarkWaterIdentificationCode innerWaterCode)
		{
			innerWaterCode.ParentTransportCodeId = Id;
			InnerWaterCodes.Add(innerWaterCode);
		}


		public virtual void RemoveCode(TrueMarkWaterIdentificationCode waterCode)
		{
			waterCode.ParentTransportCodeId = null;
			InnerWaterCodes.Remove(waterCode);
		}

		public virtual void RemoveCode(TrueMarkWaterGroupCode waterGroupCode)
		{
			waterGroupCode.ParentTransportCodeId = null;
			InnerGroupCodes.Remove(waterGroupCode);
		}

		public virtual void RemoveCode(TrueMarkTransportCode trueMarkTransportCode)
		{
			trueMarkTransportCode.ParentTransportCodeId = null;
			InnerTransportCodes.Remove(trueMarkTransportCode);
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

			var transportCodesToRemove = InnerTransportCodes.ToArray();

			foreach(var transportCode in transportCodesToRemove)
			{
				transportCode.ClearAllCodes();
				RemoveCode(transportCode);
			}
		}

		public virtual IEnumerable<TrueMarkAnyCode> GetAllCodes()
		{
			yield return this;

			foreach(var innerTransportCode in InnerTransportCodes)
			{
				foreach (var code in innerTransportCode.GetAllCodes())
				{
					yield return code;
				}
			}

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
			foreach(var innerTransportCode in InnerTransportCodes)
			{
				innerTransportCode.ParentTransportCodeId = Id;
			}

			foreach(var innerGroupCode in InnerGroupCodes)
			{
				innerGroupCode.ParentTransportCodeId = Id;
			}

			foreach(var innerWaterCode in InnerWaterCodes)
			{
				innerWaterCode.ParentTransportCodeId = Id;
			}
		}

		public override bool Equals(object obj)
		{
			if(obj is TrueMarkTransportCode code)
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
