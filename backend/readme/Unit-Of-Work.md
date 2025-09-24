this readme will discuss about the UnitOfWork pattern implemented in this project 

first, we need to know the background and the problem statement that makes the author(me) to implement UnitOfWork pattern.
this stems from the dirty db write that occurs when creating a row inside a table

an example: 
we have an endpoint that creates new Locker row, in repo scope, everything works perfectly, data is written and committed, 
up until the controller part, somehow the dto mapping is wrong and return NullReferenceException. 
our ApiMiddleware will catch this error and shortcut the request returning 500
on client side, it looks like the endpoint returning error and we failed to create the new Locker, when in fact, the new locker has been 
recorded inside the database. this is not good 

so, the author wants somewhere inside the controller execution, whenever a POST, PATCH, UPDATE, DELETE happen, use UNIT-OF-WORK which basically means 
the transaction into the database can be committed and rolled-back, in this case rollback when exception occur. 

how can we achive this?
Well first we have to make our controller some how to be wrapped inside a try catch block. Fortunately we have something is .NET that is called 
IAsyncActionFilter /// A filter that asynchronously surrounds execution of the action, after model binding is complete. (this is from the documentation)
we can imagine it like a gate to our controller 
we can check for exception happen in the next (a delegate where our controller action is) if-else and also exception through try catch block
ps: note that our controller here means everything from controller, to the deep down until our repo code, by looking at 
var result = await next(); --> remember this is a delegate of our controller 
check for exception
if (result.Exception!=null) --> check if there exception happens down the line 

now that we have access whether our controller has exception or not, next we have to construct the begin, commit and rollback transaction. 
while we can do it directly inside our Filter class (the concrete class for IAsyncActionFilter), that would break our architecture design, where we would use dbcontext inside api layer. (controller is located inside api layer)

come the unitOfWork pattern 
- our transaction is presented by a class thats implementing the rollback, commit
- this transaction is wrapped inside another class. why 
- interface 1, lets call it IDisposableTransaction, implements IDisposable, IAsyncDisposable, inside have methods, CommitAsync(), RollbackAsync() 
- interface 2, lets call it IUnitOfWork, inside have method, BeginTransaction()
- the logic, we need the transaction to only life not the whole http request, hence the IDisposable (we will use using, so that it is automatically disposed)
- UnitOfWork only responsible to start the engine (to begin the transaction)
- separationOfConcern things 
now lets properly see the code

interface 1: the transaction 
public interface IDisposableTransaction: IDisposable, IAsyncDisposable{
    Task CommitAsync(); 
    Task RollbackAsync(); 
}

interface 2: the UnitOfwork
public interface IUnitOfWork {
    Task<IDisposableTransaction> BeginTransaction();
}

note that these interfaces in inside the core (so that infra and api can see it)

implementation, 
public class DisposableTransaction: IDisposableTransaction {
    private readonly IDbContextTransaction _transaction; 
    public DisposableTransaction(
        IDbContextTransaction transaction // receives transaction to implement rollback and commit
    ) {
        // implementation here 
    }
}

public class UnitOfWork: IUnitOfWork {
    private readonly ApplicationDbContext _dbContext; 

    public UnitOfWork(
        ApplicationDbContext dbContext // need db context to start the transaction
    ){
        // implement beginTransaction
        public async Task<IDisposableTransaction> BeginTransaction() {
            var transaction = await _dbContext.Database.BeginTransaction();
            return new DisposableTransaction(transaction);
        }
    }
}

DisposableTransaction can be a private class, because we dont use it anywhere, it is tied with UnitOfWork class only and dont forget to register UnitOfWork and IUnitOfWork inside DI container 

now, we can use the rollback, and commit inside filter, remember to use it through using, so that it automatically disposed (by calling dispose() from the DisposableTransaction class)

await using var transaction = await _unitOfWork.BeginTransaction(); 
try {
    doSomething()
    if next have exception
        transaction.rollback()
    else 
        transaction.commit()
} catch {
    transaction.rollback()
} 
// transaction will be automatically rolled-back here 


now, with this unitOfWork and action filter, we can prevent dirty read on our endpoints. 
asekkkk!