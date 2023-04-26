# Bank system 7
This library provides opportunities for using likeness of bank system. You can handle not only users but also other models like banks, cards and etc.

### Updates in version 0.3.9
- Updated logic of creation, update and deletion user. Instead of using external methods to operate with entity we just attach to ef core tracker needed child entity and update with parent entity.
****
# Documentation

## Structure of project

1. Folder **AppContext** is folder where contains context classes for interaction with database without business logic.
2. Folder **Middleware** is folder where contains middleware.
3. Folder **Services** contains 2 sub folders: **Interfaces** and **Repositories**.
4. Folder **Interfaces** contains interfaces which describes structure of inherited classes.
5. Folder **Repositories** is folder with all business logic of project. Only there simple developer has access.

## How to interact with library?
The library provides ways to pass and use own models. For example, You can inherit Your class from base class User and pass it as type to initialized instance of `ServiceConfiguration` or `ServiceConfigurationMiddleware`
and use own model.
Developer can interact with library by following next steps:
1. create instance of class `ServiceConfiguration` and pass as parameters class `ConfigurationOptions` with own settings. (or do the same steps but instead of create instance of `ServiceConfiguration`, use middleware `ServiceConfigurationMiddleware`)
2. interact with repositories throughout public properties of instanced class `ServiceConfiguration`
#### Remember!
If you'll not change connection string to database in class BankServiceOptions or directly in repository classes program may don't work correctly.
You can catch exception like "There isn't database which has been specified." because databases which was used in developing project may doesn't exist on your machine.

## API documentation
### AppContext
There are 2 classes context:

1. **ApplicationContext** - is responsible for all operations in the database.
2. **BankContext** - is responsible for handling operations when use ApplicationContext is impossible.

#### API ApplicationContext
**Methods:**
1. `internal ExceptionModel AvoidDuplication(Bank item)` - implements function for avoiding duplication in table Banks in the database.
2. `private void DatabaseHandle()` - implements handling creating and deleting database.
**Properties:**

1. `public static bool EnsureCreated { get; set; }` - property for handling create database operation
2. `public static bool EnsureDeleted { get; set; }` - property for handling delete database operation
3. `protected internal DbSet<User> Users { get; set; }` - an instance of the table `Users` in database.
4. `protected internal DbSet<Bank> Banks { get; set; }` -an instance of the table `Banks` in database.
5. `protected internal DbSet<BankAccount> BankAccounts { get; set; }` - an instance of the table `BankAccounts` in database.
6. `protected internal DbSet<Credit> Credits { get; set; }` - an instance of the table `Credits` in database.

### API BankContext
**Methods:**
1. `public ExceptionModel CreateOperation(Operation operation, OperationKind operationKind)` - creates transaction operation.
2. `public ExceptionModel DeleteOperation(Operation operation)` - deletes transaction operation
3. `public ExceptionModel BankAccountWithdraw(User user, Bank bank, Operation operation)` - withdraws money from user bank account and accrual to bank's account.
4. `private ExceptionModel BankAccountAccrual(User user, Bank bank, Operation operation)` - accruals money to user bank account from bank's account.
5. `private StatusOperationCode StatusOperation(Operation operation, OperationKind operationKind)` - returns status of operation for next handling of operation.
6. `private void DatabaseHandle()` - implements handling creating and deleting database.
**Properties:**

1. `public  DbSet<User> Users { get; set; }` - an instance of the table `Users` in database.
2. `public DbSet<Bank> Banks { get; set; }` -an instance of the table `Banks` in database.
4. `public DbSet<BankAccount> BankAccounts { get; set; }` - an instance of the table `BankAccounts` in database.
5. `protected internal DbSet<Cards> Cards { get; set; }` - an instance of the table `Cards` in database.

### Services
Services are dividing on 3 sub-folders:

1. **Configuration**
2. **Interfaces**
3. **Repositories**

And some classes that don't belong to any of folders:

1. `BankServiceOptions` - defines options that used by internal services.
2. `Logger` - service for logging some info about repositories operations.
3. `OperationService` - provides connection to mongodb services.

### Configuration
Here located services for configuring library.
1. `ConfigurationOptions` - service provides options for the most full configuring of library.
2. `ModelConfiguration` - service ensures correct relationships between models.
3. `ServiceConifiguration` - service that handles all services that there are in the library.

### Interfaces (and abstract classes)
Here located interfaces which describes behavior of inherited repo-classes.

1. Interface `IRepository<T> : IReaderService<T>, IWriterService<T>, IDisposable where T : class` - interface for implement standard library logic.
   **Methods:**
- `bool FitsConditions(T? item);` - implements logic for checking on conditions true of passed entity.
2. Interface `IExpressionRepository<T> : IExpressionReaderService<T>, IWriterService<T>, IDisposable where T : class` - interface for implement standard library logic with another type of parameters.
   **Methods:**
- `bool FitsConditions(T? item);` - implements logic for checking on conditions true of passed entity.
3. Interface `IReaderService<T> where T : class` - interface for implement reading data from database.
   **Methods:**
- `T Get(Func<T, bool> predicate);` - implements getting an object from database with predicate.
- `bool Exist(Func<T, bool> predicate);` - implements checking exist object with in database predicate.
  **Properties:**
- `IEnumerable<T> All {  get; }` - implements getting a sequence of the entity from database.
4. Interface `IReaderServiceWithTracking<T> where T : class` - interface for implement reading data with tracking from database.
   **Methods:**
- `T Get(Func<T, bool> predicate);` - implements getting an entity from database with predicate.
- `bool Exist(Func<T, bool> predicate);` - implements checking exist object with in database predicate.
  **Properties:**
- `IEnumerable<T> All {  get; }` - implements getting a sequence of the entity from database.
5. Interface `IExpressionReaderService<T> where T : class` - interface for implement reading data from database with another type of parameters.
   **Methods:**
- `T Get(Expression<Func<T, bool>> predicate);` - implements getting an object from database with predicate.
- `bool Exist(Expression<Func<T, bool>> predicate);` - implements checking exist object with in database predicate.
  **Properties:**
- `IEnumerable<T> All {  get; }` - implements getting a sequence of the entity from database.
6. Interface `IWriterService<in T> where T : class` - interface for implement writing, updating and deleting data in database
   **Methods:**
- `ExceptionModel  Create(T item);` - implements adding item in database.
- `ExceptionModel  Update(T item);` - implements updating item in database.
- `ExceptionModel  Delete(T item);` - implements deleting item from database.
7. Interface `ILogger` - interface that provides standard set for logging
   **Methods:**
- `ExceptionModel Log(Report report);` - implements logging report in database.
- `ExceptionModel Log(IEnumerable<Report> reports);` - implements logging collection of reports in database.
  **Properties:**
- `public bool IsReused { get; set; }` - defines possibility use already initialized logger.
- `public LoggerOptions LoggerOptions { get; set; }` - defines options for logger configuration.
8. Abstract class `LoggerExecutor<TOperationType> where TOperationType : Enum` - simple implementation of service for added reports to logger queue
   **Methods:**
- `virtual void Log(ExceptionModel exceptionModel, string methodName, string className, TOperationType operationType, ICollection<GeneralReport<TOperationType>> reports)` - implements standard logic of inserting log data to logger queue. Can be overrided.

### Repositories
Repositories are implementation of various interfaces and working with context classes for interact with database.

1. `BankRepository` - implements interface `IRepository<T>` for handling bank model, contains methods for accrual or withdraw money from bank account and bank.
2. `BankAccountRepository` - implements interface `IRepository<T>` for handling bank account model and Transfer money method.
3. `CardRepository` - implements interface `IRepository<T>` for handling card model and Transfer money method.
4. `UserRepository` - implements interface `IRepository<T>` for handling user model.
5. `CreditRepository` - implements interface `IRepository<T>` for handling credit model and  operations with credit(loan). For example: take, pay credit.
6. `LoggerRepository` - implements interface `IRepository<T>` for handling reports.
7. `OperationRepository` - implements interface `IRepository<T>` for handling operations.

## When cause exception or error?
There are a lot of points when you can catch an exception or error while using api of project but we describe some of them:

1. You use default connection string instead of Your. Can happen so that on Your machine won't database with name which was specified in default connection string.
2. You change content of repositories or context class. If You change some of these You can get an error.
   **Example**:
 ````
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.UseSqlServer(queryConnection);
    }
````
You'll write something like this
 ````
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.UseSqlServer();
    }
````
3. You use methods incorrectly.
   **Example**:
   You want to delete operation therefore You have to use method `DeleteOperation(...)` but You use method `CreateOperation(...)`  and of course You'll get an exception because method `CreateOperation(...)`has return type `ExceptionModel` and it'll returns `ExceptionModel.OperationFailed` because same operation already exist in the database which You are using.

#### **Remember!**
Always change connection string either directly in context class, repository classes or use class BankServiceOptions for configuration.
In any situations when Your program, OS or something else was broken, **only You is responsible for it**. Please, be more intelligent. :>

## Conclusion

Downloading and next using this package is your responsible and only You decide use it or not. All exceptions and crashes of your projects is responsible on You. We was tested our product in many tests and have a conclusion in which says that all methods and logic of project are working correctly. So, we wish You luck.

**Sincerely, hayako.**