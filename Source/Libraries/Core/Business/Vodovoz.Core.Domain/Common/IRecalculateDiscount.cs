namespace Vodovoz.Core.Domain.Common
{
	public interface IRecalculateDiscount
	{
		//void RecalculateDiscount(IDataContext context);
	}
	
	public interface IRecalculateDiscount<in T> : IRecalculateDiscount
		where T : IDataContext<T>
	{
		void RecalculateDiscount(T context);
	}
}
