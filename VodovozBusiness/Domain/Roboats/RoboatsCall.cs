using QS.DomainModel.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;

namespace Vodovoz.Domain.Roboats
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "реестр звонков Roboats",
		Nominative = "реестр звонков Roboats")]
	public class RoboatsCall : PropertyChangedBase, IDomainObject
	{
		private int _id;
		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		private DateTime _callTime;
		[Display(Name = "Время звонка")]
		public virtual DateTime CallTime
		{
			get => _callTime;
			set => SetField(ref _callTime, value);
		}

		private string _phone;
		[Display(Name = "Телефон")]
		public virtual string Phone
		{
			get => _phone;
			set => SetField(ref _phone, value);
		}

		private RoboatsCallStatus _status;
		[Display(Name = "Статус")]
		public virtual RoboatsCallStatus Status
		{
			get => _status;
			set => SetField(ref _status, value);
		}

		private RoboatsCallResult _result;
		[Display(Name = "Результат звонка")]
		public virtual RoboatsCallResult Result
		{
			get => _result;
			set => SetField(ref _result, value);
		}

		IList<RoboatsCallDetail> _callDetails = new List<RoboatsCallDetail>();
		[Display(Name = "Детали звонка")]
		public virtual IList<RoboatsCallDetail> CallDetails
		{
			get => _callDetails;
			set => SetField(ref _callDetails, value);
		}

		GenericObservableList<RoboatsCallDetail> observableCallDetails;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<RoboatsCallDetail> ObservableCallDetails
		{
			get
			{
				if(observableCallDetails == null)
					observableCallDetails = new GenericObservableList<RoboatsCallDetail>(CallDetails);
				return observableCallDetails;
			}
		}
	}
}
