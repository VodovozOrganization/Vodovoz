using QS.DomainModel.Entity;
using System.ComponentModel;

namespace Vodovoz.Domain.Cash
{
	public interface IFinancialCategory : IDomainObject, INotifyPropertyChanged
	{
		string Title { get; }
		FinancialCategoryTypeEnum FinancialCategoryType { get; }
	}
}
