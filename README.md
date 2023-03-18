# This documentation has been outdated. As soon as possible it'll be rewrote for oldest version of project.
# Documentation of bank system
This package was developed for common using and learning definite technology.
It also let everybody to use bank system in its projects and change logic for your needs.

## Structure of project

 1. Folder **AppContext** is folder where contain context classes for interaction with database without buisness logic.
 5. Folder **Services** contains 2 sub folders: **Interfaces** and **Repositories**.
 6. Folder **Interfaces** contains interfaces which describes structure of inherited classes.
 7. Folder **Repositories** is folder with all buisness logic of project. Only there simple developer has access.

## How to interact with database?
Developer can interact with database using variables queryConnection in repo-classes. These variables propose connection to database. Dev also can use public methods of repo-classes.
#### Remember!
If you'll not change connection string to database in repo-classes program may don't work correctly.
You can catch exception like "There isn't database which has been specified." because databases which was used in developing project may doesn't exist on your machine.

## API documentation
### AppContext
There are 2 classes context:

 1. **BankContext** - is responsible for handling of user's resources.
 2. **BankAccountContext** - is responsible for handling of order's resources.


#### API BankContext

**Methods:**
 1. `public ExceptionModel CreateOperation(OperationModel operationModel,`     						     `OperationKind operationKind)` - creates transaction operation.
 2. `public ExceptionModel DeleteOperation(OperationModel operationModel)` - delete transaction operation
 3. `public ExceptionModel TakeCredit(BankAccountModel bankAccountModel, CreditModel creditModel)` - gives money to user which creditionals specified in `CreditModel`.
 4. `public ExceptionModel RepayCredit(BankAccountModel bankAccountModel, CreditModel creditModel)` - handles repaying credit.
 5. `private ExceptionModel BankAccountWithdraw(BankModel bankModel, OperationModel operationModel)` - withdraw money from bank's account.
 6. `public ExceptionModel BankAccountWithdraw(BankAccountModel bankAccount, BankModel bank, OperationModel operation)` - withdraw money from user bank account and accrual to bank's account.
 7. `private ExceptionModel BankAccountAccrual(BankAccountModel bankAccount, BankModel bank, OperationModel operation)` - accrual money to user bank account from bank's account.
 8. `private ExceptionModel AddCredit(CreditModel creditModel)` - adds a new credit field to database and binds it with user.
 9. `private ExceptionModel RemoveCredit(CreditModel creditModel)` - removes definite credit field from database.
 10. `private StatusOperationCode StatusOperation(OperationModel operationModel, OperationKind operationKind)` - returns status of operation for next handling of operation.

**Properties:**

 1. `public  DbSet<UserModel> Users { get; set; }` - an instance of the table `Users` in database.
 2. `public DbSet<BankModel> Banks { get; set; }` -an instance of the table `Banks` in database.
 3. `public DbSet<OperationModel> Operations { get; set; }` - an instance of the table `Operations` in database.
 4. `public DbSet<BankAccountModel> BankAccounts { get; set; }` - an instance of the table `BankAccounts` in database.
 5. `public DbSet<CreditModel> Credits { get; set; }` - an instance of the table `Credits` in database.
 
#### API BankAccountContext
**Properties:**

 1. `public  DbSet<OrderModel> Orders { get; set; }` - an instance of the table `Orders` in database.
 2. `public DbSet<BankAccountModel> BankAccounts { get; set; }` - an instance of the table `BankAccounts` in database.

### Services
Services are dividing on 2 sub-folders:

 1. **Interfaces**
 2. **Repositories**

### Interfaces
Here located interfaces which describes behavior of inherited repo-classes.
 1. Interface `IRepository<T>` - parent interface from which will inherite other interfaces and repo-classes. Describes main logic and structure of the project.<br>**Methods**:  <br>- `ExceptionModel  Create(T item);` - implements creating item and adding it in database. <br>- `ExceptionModel  Update(T item);` -implements updating item in database.<br>- `ExceptionModel  Delete(T item);` - implements deleting item from database.<br>- `IEnumerable<T> Get();` - implements getting a sequence of the objects from database.<br>- `T Get(Guid id);` - implements getting an object from database with definite ID.<br> - `T  Get(Expression<Func<T, bool>> predicate);` - implements getting an object from database with func-condition.<br>- `bool  Exist(Guid id);` - implements checking exist object with definite ID in database or not.<br>- `bool  Exist(Expression<Func<T, bool>> predicate);` - implements checking exist object with func-condition in database.<br><br>
 2. Interface `IBankAccountRepository` - describes implementations for handling   resources of bank's accounts.<br>**Methods:**<br>- `ExceptionModel Accrual(BankAccountModel item, decimal amountAccrual);` - implements accrualing money on account.<br>- `ExceptionModel Withdraw(BankAccountModel item, decimal amountAccrual);` - implements withdrawing money from account.<br>- `ExceptionModel Update(BankAccountModel item, UserModel user);` - implements updating account of definite user.<br><br>
 3. Interface `IBankRepository` - describes implementatins for handling resources of banks.<br>**Methods:**<br>- `ExceptionModel BankAccountAccrual(BankAccountModel bankAccount, BankModel bank, OperationModel operation);` - implements accrualing money to bank.<br>- `ExceptionModel BankAccountWithdraw(BankAccountModel bankAccount, BankModel bank, OperationModel operation);` - implements withdrawing money from bank.<br>- `ExceptionModel TakeCredit(BankAccountModel bankAccount, CreditModel credit);` - implements giving credit to definite user.<br>- `ExceptionModel RepayCredit(BankAccountModel bankAccount, CreditModel credit);` - implements repaying credit of definite user.


### Repositories
Repositories are implementation of various interfaces and working with context classes for interracting with database. 

 1. `BankRepository` - implements interfaces `IBankRepository<T>` & `IBankAccountRepository` and also parent-interface `IRepository<T>`
 2. `BankAccountRepository` - implements interfaces <br>`IBankAccountRepository` & `IRepository<T>`
 
<br><br>

## When cause exception or error?
There are some points when you can catch an exception or error while using api of project:

 1. You use default connection string instead of your. Can happen so that on Your machine won't database with name which was specified in default connection string.
 2. You change content of repositories or context class. If You change some of these You can get an error. <br> **Example**: <br> 
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
 3. You use methods incorrectly. <br> **Example**:<br> You wanna delete operation therefore You have to use method `DeleteOperation(...)` but You use method `CreateOperation(...)`  and of course You'll get an exception because method `CreateOperation(...)`has return type `ExceptionModel` and it'll returns `ExceptionModel.OperationFailed` because same operation already exist in the database which You are using.

#### **Remember!**
Always change connection string either directly in context class (**You are responsible for it!**) or use constructors of repo-classes.

## Conclusion

Dowloading and next using this package is your respoinsible and only you decide use it or not. All exceptions and crashes of your projects is responsible on you. We was tested our product in many tests and have a conclusion in which says that all methods and logic of project are working correctly. So, we wish you luck.<br>
**Sincerely, hayako.**
