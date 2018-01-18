using System;

namespace Vodovoz.Domain
{
	public class Wages{
		public static Rates GetDriverRates(DateTime route_date, bool withForwarder=false){
			var r = new Rates();
			r.PhoneServiceCompensationRate = 2;
			r.FullBottleRate = route_date < new DateTime(2017, 9, 18) ? (withForwarder ? 10 : 15) : (withForwarder ? 15 : 20);
			r.SmallBottleRate = route_date < new DateTime(2017, 9, 18) ? (withForwarder ? 10 : 15) : (withForwarder ? 15 : 20);
			r.EmptyBottleRate = 5;
			r.CoolerRate = withForwarder ? 20 : 30;
			r.PaymentPerAddress = 50;
			r.LargeOrderFullBottleRate = withForwarder ? 7 : 9;
			r.LargeOrderEmptyBottleRate = 1;
			r.LargeOrderMinimumBottles = 100;
			r.SmallFullBottleRate = withForwarder ? 2 : 3;
			r.ContractCancelationRate = withForwarder ? 20 : 30;
			r.PaymentWithRast = r.PaymentPerAddress + r.FullBottleRate * 2;
			return r;
		}

		public static Rates GetDriverRatesWithOurCar(DateTime route_date){
			var r = new Rates();
			r.PhoneServiceCompensationRate = 2;
			r.FullBottleRate = route_date < new DateTime(2017, 8, 8) ? 7 : 10;
			r.SmallBottleRate = route_date < new DateTime(2017, 8, 8) ? 7 : 10;
			r.EmptyBottleRate = 5;
			r.CoolerRate = 14;
			r.PaymentPerAddress = 30;
			r.LargeOrderFullBottleRate = 4;
			r.LargeOrderEmptyBottleRate = 1;
			r.LargeOrderMinimumBottles = 100;
			r.SmallFullBottleRate = (decimal)7 / 5;
			r.ContractCancelationRate = 14;
			r.PaymentWithRast = r.PaymentPerAddress + r.FullBottleRate * 2;
			return r;
		}

		public static Rates GetForwarderRates(){
			var r = new Rates();
			r.PhoneServiceCompensationRate = 0;
			r.FullBottleRate = 5;
			r.SmallBottleRate = 0;
			r.EmptyBottleRate = 5;
			r.CoolerRate = 10;
			r.PaymentPerAddress = 0;
			r.LargeOrderFullBottleRate = 4;
			r.LargeOrderEmptyBottleRate = 1;
			r.SmallFullBottleRate = 1;
			r.LargeOrderMinimumBottles = 100;
			r.ContractCancelationRate = 10;
			r.PaymentWithRast = r.FullBottleRate * 2;
			return r;
		}

		public class Rates{
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

