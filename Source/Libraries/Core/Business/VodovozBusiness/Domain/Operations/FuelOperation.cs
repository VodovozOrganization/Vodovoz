using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Operations;

namespace Vodovoz
{
	/// <summary>
	/// Операция по топливу
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "операции с топливом",
		Nominative = "операция с топливом")]
	[HistoryTrace]
	public class FuelOperation : OperationBase
	{
		public const decimal LitersOutlayedLimit = 9_999.99m;
		public const string DialogMessage = nameof(DialogMessage);

		private FuelType _fuel;
		private Employee _driver;
		private Car _car;
		private decimal _litersGived;
		private decimal _payedLiters;
		private decimal _litersOutlayed;
		private bool _isFine;

		/// <summary>
		/// Тип топлива
		/// </summary>
		[Display(Name = "Тип топлива")]
		public virtual FuelType Fuel
		{
			get => _fuel;
			set => SetField(ref _fuel, value);
		}

		/// <summary>
		/// Водитель
		/// </summary>
		[Display(Name = "Водитель")]
		public virtual Employee Driver
		{
			get => _driver;
			set => SetField(ref _driver, value);
		}

		/// <summary>
		/// Автомобиль
		/// </summary>
		[Display(Name = "Транспортное средство")]
		public virtual Car Car
		{
			get => _car;
			set => SetField(ref _car, value);
		}

		/// <summary>
		/// Выдано литров
		/// </summary>
		[Display(Name = "Выдано литров топлива")]
		public virtual decimal LitersGived
		{
			get => _litersGived;
			set => SetField(ref _litersGived, value);
		}

		/// <summary>
		/// Выдано литров деньгами
		/// </summary>
		[Display(Name = "Выдано литров топлива деньгами")]
		public virtual decimal PayedLiters
		{
			get => _payedLiters;
			set => SetField(ref _payedLiters, value);
		}

		/// <summary>
		/// Израсходовано литров
		/// </summary>
		[Display(Name = "Потрачено литров топлива")]
		public virtual decimal LitersOutlayed
		{
			get => _litersOutlayed;
			set => SetField(ref _litersOutlayed, value);
		}

		/// <summary>
		/// Со штрафом
		/// </summary>
		[Display(Name = "Операция со штрафом")]
		public virtual bool IsFine
		{
			get => _isFine;
			set => SetField(ref _isFine, value);
		}

		public FuelOperation() { }

		public virtual string Title => $"{GetType().GetSubjectName()} №{Id}";
		
		public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(LitersOutlayed > LitersOutlayedLimit)
			{
				var sb = new StringBuilder();
				var propertyName = this.GetPropertyInfo(x => x.LitersOutlayed).GetCustomAttribute<DisplayAttribute>(true).Name;
				validationContext.Items.TryGetValue(DialogMessage, out var message);

				sb.Append(
					$"Поле {propertyName} в операции топлива с значением {LitersOutlayed} не должно превышать лимит {LitersOutlayedLimit}");

				if(message != null)
				{
					sb.Insert(0, $"{message}\n");
				}
				
				yield return new ValidationResult(sb.ToString(), new []{ nameof(LitersOutlayed) });
			}
		}
	}
}

