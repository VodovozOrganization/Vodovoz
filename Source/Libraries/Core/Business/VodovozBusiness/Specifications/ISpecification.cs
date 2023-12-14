namespace Vodovoz.Specifications
{
	public interface ISpecification<in T>
	{
		bool IsSatisfiedBy(T entity);
	}
}
