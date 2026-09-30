using System;
using Vodovoz.Core.Domain.Interfaces.Sale;

namespace Vodovoz.Core.Domain.Sale
{
	public class DiscountAmount : IDiscountAmount
	{
		public DiscountAmount(int id)
		{
			Id = id;
		}
		
		private DiscountAmount(int id, string name, decimal amount)
		{
			Id = id;
			Name = name;
			Amount = Math.Round(amount, 2);
		}
		
		public int Id { get; }
		public string Name { get; private set; }
		public decimal Amount { get; private set; }

		public void Update(string name, decimal amount)
		{
			Name = name;
			Amount = amount;
		}

		public static IDiscountAmount Create(int id) => new DiscountAmount(id);
		public static IDiscountAmount Create(int id, string name, decimal amount) => new DiscountAmount(id, name, amount);
	}
}
