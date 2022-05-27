using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;

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

		private RoboatsCallFailType _failType;
		[Display(Name = "Тип проблемы")]
		public virtual RoboatsCallFailType FailType
		{
			get => _failType;
			set => SetField(ref _failType, value);
		}

		private RoboatsCallResult _result;
		[Display(Name = "Результат звонка")]
		public virtual RoboatsCallResult Result
		{
			get => _result;
			set => SetField(ref _result, value);
		}

		private RoboatsCallOperation _operation;
		[Display(Name = "Выполняемое действие")]
		public virtual RoboatsCallOperation Operation
		{
			get => _operation;
			set => SetField(ref _operation, value);
		}

		private string _description;
		[Display(Name = "Description")]
		public virtual string Description
		{
			get => _description;
			set => SetField(ref _description, value);
		}
	}
}
