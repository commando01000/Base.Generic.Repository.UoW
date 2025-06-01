# Base.Generic.Repository.UoW

A lightweight, generic repository implementation with Unit of Work pattern for .NET applications. This library aims to simplify data access logic by providing reusable, extensible, and testable repository and unit of work abstractions.

## Features

- Generic repository pattern for CRUD operations
- Unit of Work pattern to manage database transactions
- Easily extensible for custom repository logic
- Supports dependency injection
- Designed for testability

## Getting Started

### Installation

Add the project to your solution, or reference the compiled DLL in your .NET project.

### Usage

#### 1. Configure Your DbContext

```csharp
public class AppDbContext : DbContext
{
    public DbSet<Customer> Customers { get; set; }
    // Add other entities...
}
```

#### 2. Register Services (using Microsoft.Extensions.DependencyInjection)

```csharp
services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

// Register UnitOfWork with AppDbContext
services.AddScoped(typeof(IUnitOfWork<AppDbContext>), typeof(UnitOfWork<AppDbContext>));
```

#### 3. Inject and Use in Your Services

```csharp
public class CustomerService
{
    private readonly IUnitOfWork<AppDbContext> _unitOfWork;

    public CustomerService(IUnitOfWork<AppDbContext> unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task AddCustomerAsync(Customer customer)
    {
        await _unitOfWork.Repository<Customer>().AddAsync(customer);
        await _unitOfWork.CompleteAsync();
    }
}
```

## Project Structure

- **GenericRepository/**: Implements the generic repository pattern.
- **UnitOfWork/**: Implements Unit of Work pattern.
- **Interfaces/**: Contains repository and unit of work interfaces.
- **Entities/**: Your domain entities go here.

## Example

```csharp
// Adding a new entity
var customer = new Customer { Name = "John Doe" };
await _unitOfWork.Repository<Customer>().AddAsync(customer);
await _unitOfWork.CompleteAsync();

// Retrieving entities
var customers = await _unitOfWork.Repository<Customer>().GetAllAsync();
```

## Contributing

Contributions are welcome! Please fork the repository and submit a pull request.

## License

This project is licensed under the MIT License.

## Contact

For issues, please use the [GitHub Issues](https://github.com/commando01000/Base.Generic.Repository.UoW/issues) page.
