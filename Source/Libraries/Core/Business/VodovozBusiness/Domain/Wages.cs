using System;

namespace Vodovoz.Domain
{
	public class Wages
	{
		public static Rates GetDriverRates(DateTime route_date, bool withForwarder = false)
		{
			var r = new Rates {
				PhoneServiceCompensationRate = route_date < new DateTime(2019, 5, 14) ? 2 : 0,
				FullBottleRate = route_date < new DateTime(2017, 9, 18) ? (withForwarder ? 10 : 15) : (withForwarder ? 15 : 20),
				SmallBottleRate = route_date < new DateTime(2017, 9, 18) ? (withForwarder ? 10 : 15) : (withForwarder ? 15 : 20),
				EmptyBottleRate = 5,
				CoolerRate = withForwarder ? 20 : 30,
				PaymentPerAddress = route_date < new DateTime(2019, 5, 14) ? 50 : 52,
				LargeOrderFullBottleRate = withForwarder ? 7 : 9,
				LargeOrderEmptyBottleRate = 1,
				LargeOrderMinimumBottles = 100,
				SmallFullBottleRate = withForwarder ? 2 : 3,
				ContractCancelationRate = withForwarder ? 20 : 30
			};
			r.PaymentWithRast = r.PaymentPerAddress + r.FullBottleRate * 2;
			return r;
		}

		public static Rates GetDriverRatesWithOurCar(DateTime route_date)
		{
			var r = new Rates {
				PhoneServiceCompensationRate = route_date < new DateTime(2019, 5, 14) ? 2 : 0,
				FullBottleRate = route_date < new DateTime(2017, 8, 8) ? 7 : 10,
				SmallBottleRate = route_date < new DateTime(2017, 8, 8) ? 7 : 10,
				EmptyBottleRate = 5,
				CoolerRate = 14,
				PaymentPerAddress = route_date < new DateTime(2019, 5, 14) ? 30 : 32,
				LargeOrderFullBottleRate = 4,
				LargeOrderEmptyBottleRate = 1,
				LargeOrderMinimumBottles = 100,
				SmallFullBottleRate = (decimal)7 / 5,
				ContractCancelationRate = 14
			};
			r.PaymentWithRast = r.PaymentPerAddress + r.FullBottleRate * 2;
			return r;
		}

		public static Rates GetForwarderRates()
		{
			var r = new Rates {
				PhoneServiceCompensationRate = 0,
				FullBottleRate = 5,
				SmallBottleRate = 5,
				EmptyBottleRate = 5,
				CoolerRate = 10,
				PaymentPerAddress = 0,
				LargeOrderFullBottleRate = 4,
				LargeOrderEmptyBottleRate = 1,
				SmallFullBottleRate = 1,
				LargeOrderMinimumBottles = 100,
				ContractCancelationRate = 10
			};
			r.PaymentWithRast = r.FullBottleRate * 2;
			return r;
		}

		public class Rates
		{
			public decimal PhoneServiceCompensationRate;
			public decimal FullBottleRate;
			public decimal SmallBottleRate;
			public decimal EmptyBottleRate;
			public decimal CoolerRate;
			public decimal PaymentPerAddress;
			public decimal LargeOrderFullBottleRate;
			public decimal LargeOrderEmptyBottleRate;
			public int LargeOrderMinimumBottles;
			public decimal SmallFullBottleRate;
			public decimal ContractCancelationRate;
			/// <summary>
			/// При расторжении
			/// </summary>
			public decimal PaymentWithRast;
		}
	}
}

