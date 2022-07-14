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
		private DateTime _callTime;
		private string _phone;
		private RoboatsCallStatus _status;
		private RoboatsCallResult _result;
		IList<RoboatsCallDetail> _callDetails = new List<RoboatsCallDetail>();
		GenericObservableList<RoboatsCallDetail> observableCallDetails;

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		[Display(Name = "Время звонка")]
		public virtual DateTime CallTime
		{
			get => _callTime;
			set => SetField(ref _callTime, value);
		}

		[Display(Name = "Телефон")]
		public virtual string Phone
		{
			get => _phone;
			set => SetField(ref _phone, value);
		}

		[Display(Name = "Статус")]
		public virtual RoboatsCallStatus Status
		{
			get => _status;
			set => SetField(ref _status, value);
		}

		[Display(Name = "Результат звонка")]
		public virtual RoboatsCallResult Result
		{
			get => _result;
			set => SetField(ref _result, value);
		}

		[Display(Name = "Детали звонка")]
		public virtual IList<RoboatsCallDetail> CallDetails
		{
			get => _callDetails;
			set => SetField(ref _callDetails, value);
		}

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
