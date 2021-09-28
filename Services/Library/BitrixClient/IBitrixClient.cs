using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bitrix.DTO;

namespace Bitrix
{
    public interface IBitrixClient
    {
		Company GetCompany(uint id);
		Contact GetContact(uint id);
		IList<Deal> GetDeals(DateTime dateTimeFrom, DateTime dateTimeTo);
		Product GetProduct(uint id);
		IList<DealProductItem> GetProductsForDeal(uint dealId);

		bool SetStatusToDeal(DealStatus status, uint dealId);
	}
}