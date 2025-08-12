Upon writing the repository implementation that is located inside the infrastructure project, 
I think the need of a reusable class that can construct the filter without congesting our repository 
implementation. 

For example, we need to fetch Parcel based on: 
- parcel tracking number 
- parcel status 
- entry date
- ...
one way we could do it is manually implementing all this inside our repo, congesting our repo 
this is nasssty

so the solution is we use specification pattern 
- me make an interface that represent this filter. 
- the implentation for this interface is the filter

/code block
public interface ISpecification {
    Expression<Func<T, bool>> ToExpression();
}
/code block



