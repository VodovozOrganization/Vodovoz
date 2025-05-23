using System;

namespace VodovozBusiness.Errors.Clients.Counterparties
{
	public class CounterpartyNotFoundException : Exception
	{
		public CounterpartyNotFoundException(int id)
		{
			Id = id;
		}

		public int Id { get; }

		public override string Message => $"Контрагент с идентификатором {Id} не найден.";
	}
}
