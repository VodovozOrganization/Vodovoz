using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Roboats
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "детали звонка Roboats",
		Nominative = "детали звонка Roboats")]
	public class RoboatsCallDetail : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private DateTime _operationTime;
		private RoboatsCall _call;
		private RoboatsCallFailType _failType;
		private RoboatsCallOperation _operation;
		private string _description;


		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		[Display(Name = "Время операции")]
		public virtual DateTime OperationTime
		{
			get => _operationTime;
			set => SetField(ref _operationTime, value);
		}

		[Display(Name = "Звонок")]
		public virtual RoboatsCall Call
		{
			get => _call;
			set => SetField(ref _call, value);
		}

		[Display(Name = "Тип проблемы")]
		public virtual RoboatsCallFailType FailType
		{
			get => _failType;
			set => SetField(ref _failType, value);
		}
		
		[Display(Name = "Выполняемое действие")]
		public virtual RoboatsCallOperation Operation
		{
			get => _operation;
			set => SetField(ref _operation, value);
		}

		[Display(Name = "Description")]
		public virtual string Description
		{
			get => _description;
			set => SetField(ref _description, value);
		}
	}
}
