using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bitrix.DTO;

namespace Bitrix
{
	public interface IBitrixClient
	{
		Task<Company> GetCompany(uint id);
		Task<Contact> GetContact(uint id);
		Task<IList<Deal>> GetDeals(DateTime dateTimeFrom, DateTime dateTimeTo);
		Task<Product> GetProduct(uint id);
		Task<IList<DealProductItem>> GetProductsForDeal(uint dealId);
		Task<bool> SetStatusToDeal(DealStatus status, uint dealId);
	}
}
