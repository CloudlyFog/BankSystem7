
# Bank system 7
This package was developed for common using and learning definite technology.
It also let everybody to use bank system in its projects and change logic for your needs.

**It's a beta version of library. Some exceptions can be weren't found.**

### Updates in version 0.3.4-beta
- Added clearing tracker in method Create in BankRepository.cs for avoid exceptions with tracking the same entities.
- Added class LoggerOptions. Added property of LoggerOptions to ConfigurationOptions.cs.
- Added Logging. Implemented interface ILogger.cs in class Logger.cs.
- Added repository LoggerRepository.cs for handling logs.
- Added repository OperationRepository for handling operation entities in the database.
- Added logger in the UserRepository.
- Added new model for logging - Report.cs.
- Added interaction with logger services via properties in ServiceConfiguration.cs.
- Updated logic of user creation to avoid exceptions with needlessness creating of dependent entities.
- Updated accrual and withdraw operations. Changes consists in updating card model of user during accrual/withdraw operation.
- Removed field IssueCreditDate from parameters of instance maker of class Credit.
- Added fields in ConfigurationOptions for setting options for logger and operation service.
- Removed ability of set any repository value outside creation instance of ServiceConfiguration.
****
# Documentation

## Structure of project

1.  Folder **AppContext** is folder where contain context classes for interaction with database without business logic.
2.  Folder **Services** contains 2 sub folders: **Interfaces** and **Repositories**.
3.  Folder **Interfaces** contains interfaces which describes structure of inherited classes.
4.  Folder **Repositories** is folder with all business logic of project. Only there simple developer has access.

## How to interact with database?
Developer can interact with database using class BankServiceOptions that provides properties for a little configuration. Set connection string you can by set value to property Connection. Ensure create or delete database by setting value to properties EnsureCreated and EnsureDeleted.
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
1. `public static bool EnsureDeleted { get; set; }` - property for handling delete database operation
2. `protected internal DbSet<User> Users { get; set; }` - an instance of the table `Users` in database.
3. `protected internal DbSet<Bank> Banks { get; set; }` -an instance of the table `Banks` in database.
4. `protected internal DbSet<Operation> Operations { get; set; }` - an instance of the table `Operations` in database.
5. `protected internal DbSet<BankAccount> BankAccounts { get; set; }` - an instance of the table `BankAccounts` in database.
6. `protected internal DbSet<Credit> Credits { get; set; }` - an instance of the table `Credits` in database.


#### API BankContext

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
3. `public DbSet<Operation> Operations { get; set; }` - an instance of the table `Operations` in database.
4. `public DbSet<BankAccount> BankAccounts { get; set; }` - an instance of the table `BankAccounts` in database.


### Services
Services are dividing on 2 sub-folders:

1. **Interfaces**
2. **Repositories**

### Interfaces
Here located interfaces which describes behavior of inherited repo-classes.
1. Interface `IRepository<T>` - parent interface from which will inherit other interfaces and repo-classes. Describes main logic and structure of the project.
   **Methods**:
- `ExceptionModel  Create(T item);` - implements creating item and adding it in database.
- `ExceptionModel  Update(T item);` -implements updating item in database.
- `ExceptionModel  Delete(T item);` - implements deleting item from database.
- `IEnumerable<T> All {  get; }` - implements getting a sequence of the objects from database.
-  `T  Get(Expression<Func<T, bool>> predicate);` - implements getting an object from database with func-condition.
-  `bool  Exist(Expression<Func<T, bool>> predicate);` - implements checking exist object with in database func-condition.

### Repositories
Repositories are implementation of various interfaces and working with context classes for interact with database.

1. `BankRepository` - implements interface `IRepository<T>` for handling bank model, contains methods for accrual or withdraw money from bank account and bank.
2. `BankAccountRepository` - implements interface `IRepository<T>` for handling bank account model and Transfer money method.
3. `CardRepository` - implements interface `IRepository<T>` for handling card model and Transfer money method.
4. `UserRepository` - implements interface `IRepository<T>` for handling user model.
5. `CreditRepository` - implements interface `IRepository<T>` for handling credit model and  operations with credit(loan). For example: take, pay credit.

## When cause exception or error?
There are some points when you can catch an exception or error while using api of project:

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