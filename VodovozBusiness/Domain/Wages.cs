using System;

namespace Vodovoz.Domain
{
	public class Wages{
		public static Rates GetDriverRates(bool withForwarder=false){
			return new Rates
			{
				PhoneServiceCompensationRate = 2,
				FullBottleRate = withForwarder ? 10 : 15,
				EmptyBottleRate = 5,
				CoolerRate = withForwarder ? 20 : 30,
				PaymentPerAddress = 50,
				LargeOrderFullBottleRate = withForwarder ? 7 : 9,
				LargeOrderEmptyBottleRate = 1,
				LargeOrderMinimumBottles = 100,
				SmallFullBottleRate = withForwarder ? 2 : 3,
				ContractCancelationRate = withForwarder ? 20 : 30,
			};
		}

		public static Rates GetDriverRatesWithOurCar(){
			return new Rates
			{
				PhoneServiceCompensationRate = 2,
				FullBottleRate = 7,
				EmptyBottleRate = 5,
				CoolerRate = 14,
				PaymentPerAddress = 30,
				LargeOrderFullBottleRate = 4,
				LargeOrderEmptyBottleRate = 1,
				LargeOrderMinimumBottles = 100,
				SmallFullBottleRate = (decimal)7 / 5,
				ContractCancelationRate = 14,
			};
		}

		public static Rates GetForwarderRates(){
			return new Rates
			{
				PhoneServiceCompensationRate=0,
				FullBottleRate = 5,
				EmptyBottleRate = 5,
				CoolerRate = 10,
				LargeOrderFullBottleRate = 4,
				LargeOrderEmptyBottleRate = 1,
				SmallFullBottleRate = 1,
				LargeOrderMinimumBottles = 100,
				ContractCancelationRate = 10
			};
		}

		public class Rates{
			public decimal PhoneServiceCompensationRate;
			public decimal FullBottleRate;
			public decimal EmptyBottleRate;
			public decimal CoolerRate;
			public decimal PaymentPerAddress;
			public decimal LargeOrderFullBottleRate;
			public decimal LargeOrderEmptyBottleRate;
			public int LargeOrderMinimumBottles;
			public decimal SmallFullBottleRate;
			public decimal ContractCancelationRate;
		}
	}		
}

