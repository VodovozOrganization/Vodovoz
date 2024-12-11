using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace Vodovoz.Core.Domain.Edo
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		Nominative = "заявка на отправку документов ЭДО",
		NominativePlural = "заявки на отправку документов ЭДО"
	)]
	public abstract class CustomerEdoRequest : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private DateTime _time;
		private CustomerEdoRequestType _type;
		private CustomerEdoRequestSource _source;
		private IObservableList<TrueMarkProductCode> _productCodes = new ObservableList<TrueMarkProductCode>();
		private EdoDocumentType _documentType;

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		[Display(Name = "Время")]
		public virtual DateTime Time
		{
			get => _time;
			set => SetField(ref _time, value);
		}

		[Display(Name = "Тип")]
		public virtual CustomerEdoRequestType Type
		{
			get => _type;
			set => SetField(ref _type, value);
		}

		[Display(Name = "Источник")]
		public virtual CustomerEdoRequestSource Source
		{
			get => _source;
			set => SetField(ref _source, value);
		}

		[Display(Name = "Коды маркировки")]
		public virtual IObservableList<TrueMarkProductCode> ProductCodes
		{
			get => _productCodes;
			set => SetField(ref _productCodes, value);
		}

		public virtual EdoDocumentType DocumentType
		{
			get => _documentType;
			set => SetField(ref _documentType, value);
		}
	}
}
