namespace Composer.Specification
{
    public class OrSpecification<T> : CompositeSpecification<T>
    {
        public OrSpecification(Specification<T> leftSide, Specification<T> rightSide)
            : base(leftSide, rightSide)
        {
        }

        public override bool IsSatisfiedBy(T obj)
        {
            return _leftSide.IsSatisfiedBy(obj) || _rightSide.IsSatisfiedBy(obj);
        }
    }
}