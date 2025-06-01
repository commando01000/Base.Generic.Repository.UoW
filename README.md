# Base.Generic.Repository.UoW

A base implementation of the **Generic Repository** and **Unit of Work** patterns in .NET. This project provides reusable abstractions and concrete classes to simplify data access logic in your applications, following best practices for maintainability and testability.

## Features

- **Generic Repository Pattern**: Abstracts data access logic, allowing for type-safe CRUD operations on entities.
- **Unit of Work Pattern**: Coordinates the work of multiple repositories by ensuring a single transaction for a business operation.
- **Specification Pattern Support**: Compose complex queries in a reusable and testable way.
- Designed for extensibility and easy integration with Entity Framework or other ORMs.

## Project Structure

- `Base.Repository.sln`  
  Solution file for the project.
- `Base.Repository/`  
  Main library containing:
  - `GenericRepository.cs`  
    Implementation of a generic repository for CRUD operations.
  - `IGenericRepository.cs`  
    Interface defining the contract for the generic repository.
  - `IUnitOfWork.cs`  
    Interface for the Unit of Work pattern.
  - `UnitOfWork.cs`  
    Implementation of the Unit of Work.
  - `Specification/`  
    (Folder for specification pattern support classes.)

## Getting Started

1. **Clone the repository:**
   ```sh
   git clone https://github.com/commando01000/Base.Generic.Repository.UoW.git
   ```

2. **Open in Visual Studio:**
   - Open the `Base.Repository.sln` solution file.

3. **Add a reference to your project:**
   - Add the `Base.Repository` project as a dependency in your solution.

4. **Usage Example:**
   Implement your own repositories or use the generic repository directly:
   ```csharp
   public class MyEntityRepository : GenericRepository<MyEntity>, IMyEntityRepository
   {
       public MyEntityRepository(DbContext context) : base(context) { }
       // Add custom methods here
   }
   ```

## Extending

- Implement additional repository interfaces as needed for your entities.
- Use the Specification pattern classes to compose complex queries.

## Contributing

Pull requests are welcome! For major changes, please open an issue first to discuss what you would like to change.

## License

This project is open-source and available under the [MIT License](LICENSE).

---

**Author:** [commando01000](https://github.com/commando01000)
