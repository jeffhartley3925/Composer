namespace Composer.Specification
{
    public abstract class CompositeSpecification<T> : Specification<T>
    {
        protected readonly Specification<T> _leftSide;
        protected readonly Specification<T> _rightSide;

        public CompositeSpecification(
            Specification<T> leftSide, Specification<T> rightSide)
        {
            _leftSide = leftSide;
            _rightSide = rightSide;
        }
    }
}