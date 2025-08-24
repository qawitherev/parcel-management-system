Upon writing the repository implementation that is located inside the infrastructure project, 
I think we need a reusable class to construct filters without congesting our repository implementation.

For example, suppose we need to fetch `Parcel` based on:
- Parcel tracking number
- Parcel status
- Entry date
- ...and so on

One way is to manually implement all this inside our repo, but that would congest our repo—this is nasssty.

**The solution:** Use the Specification Pattern!

- We make an interface that represents the filter.
- The implementation for this interface *is* the filter.

```csharp
public interface ISpecification<T> {
    Expression<Func<T, bool>> ToExpression();
}
```

---

## Specification Pattern Enhanced

The initial specification only supports the `where` syntax in LINQ, e.g.,

> Get parcel  
> where `parcel.ResidentUnitId == residentUnitId`

But what if we want to join multiple tables?

**Consider this scenario:**
- `Parcel` -(residentUnitId)-> `ResidentUnit` -(residentUnitId)-> `UserResidentUnit` <-(userId)- `User`

Suppose we want to get parcels associated with a user.  
Normally, with SQL, we’d join the tables and use `WHERE UserResidentUnit.userId == userId`.

With LINQ, it looks like:
```csharp
dbContext.Parcel
    .Include(p => p.ResidentUnit)
    .ThenInclude(ru => ru.UserResidentUnit)
    .Where(p => p.ResidentUnit.UserResidentUnit.Any(uru => uru.UserId == userId))
```

But we don’t want to write this stuff for every single query we want, sooo...

### We modify our specification to support joins!

#### Steps:
1. **Add a new member to our interface called `IncludeExpressions` (a list):**  
   This holds a list of parent `Include` and `ThenInclude` children.
   ```csharp
   List<IncludeExpression<T>> IncludeExpressions { get; }
   ```
2. **`IncludeExpression` is a class that holds a single include and its ThenInclude children.**
3. **`Include` is of type `Expression<Func<T, object>>`** (relate to how `Include` is used above).
4. **`ThenInclude` is of type `Expression<Func<object, object>>`** (relate to how `ThenInclude` is used above).

#### Using the specification

Basically the same as before, but now we add the `Include` and `ThenInclude` inside the constructor, since `IncludeExpressions` is a property, not a method.

Just like LINQ, but with a twist:

```csharp
IncludeExpressions = new List<IncludeExpression<Parcel>> {
    new IncludeExpression<Parcel>(p => p.ResidentUnit)
        .ThenInclude(ru => ((ResidentUnit)ru).UserResidentUnit)
}
```
Notice the casting here to `ru`—we cast it into `ResidentUnit`.

#### Applying it to the repository

The concept:  
From the specification, pass it to a query of `IQueryable<T>`.  
Then, extract the `IncludeExpressions` and apply the `where` from `ToExpression()`.

But doing this in every single repository is "mendoksai" (Japanese for troublesome), so we created a `BaseRepository` class that implements the extracting and applying filter logic.  
Repositories inherit this base repo so they can use its methods.

---

#### Possible additions to the specification interface

- **Projection (the select statement):**  
  We decided *not* to implement this because of limitations—when using projection, we can only select from the root entity. If we want to select from nested joined tables, we can’t do that.

- **Pagination:**  
  We’ll do this one some other time.