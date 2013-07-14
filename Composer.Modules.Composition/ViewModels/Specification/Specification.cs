namespace Composer.Specification
{
    public abstract class Specification<T>
    {
        public abstract bool IsSatisfiedBy(T obj);

        public AndSpecification<T> And(Specification<T> specification)
        {
            return new AndSpecification<T>(this, specification);
        }

        public OrSpecification<T> Or(Specification<T> specification)
        {
            return new OrSpecification<T>(this, specification);
        }

        public NotSpecification<T> Not(Specification<T> specification)
        {
            return new NotSpecification<T>(this, specification);
        }
    }
}