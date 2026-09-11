# Campus Equipment Borrowing System
## Laboratory Activity 1: From Requirements to Application Structure

A layered .NET application architecture implementing the domain and application foundation for a university equipment borrowing system, designed according to Clean Architecture and Separation of Concerns principles.

---

# Part A – Analyze the System Before Coding

## A. Actors

### 1. Student
- **Description**: An authorized university learner who requests to borrow laboratory equipment, searches for available equipment, and returns borrowed items.
- **Expectation**: Expects the system to provide accurate availability of equipment, allow borrowing within authorized eligibility and quota limits, and record returns reliably.

### 2. Laboratory Administrator / System
- **Description**: The authority managing the laboratory equipment inventory, setting borrowing quotas, and enforcing institutional policies.
- **Expectation**: Expects the system to strictly validate student eligibility and borrowing limits before approving any request, accurately track equipment status, and prevent unauthorized or unrecorded loans.

---

## B. Major Use Cases

### Use Case 1: Borrow Equipment (Primary Use Case)

| Item | Description |
|---|---|
| **Use Case** | Borrow Equipment |
| **Primary Actor** | Student |
| **Preconditions** | 1. The student is registered in the system.<br>2. The equipment item exists in the laboratory inventory. |
| **Main Action** | 1. The student requests to borrow a specific piece of equipment.<br>2. The system checks if the student exists and is eligible to borrow.<br>3. The system checks if the equipment exists and is currently available.<br>4. The system validates that the student has not exceeded their active borrowing quota.<br>5. The system creates an active borrowing record with a scheduled return date.<br>6. The system marks the equipment as unavailable.<br>7. The system confirms the approved borrowing. |
| **Expected Result** | A new active `Borrowing` record is saved, the equipment `IsAvailable` state becomes `false`, and a success confirmation is returned. |
| **Possible Failure** | - Student does not exist.<br>- Student is not eligible to borrow (account blocked/suspended).<br>- Equipment does not exist.<br>- Equipment is currently unavailable (already borrowed).<br>- Student has already reached their maximum active borrowing limit. |

---

### Use Case 2: Return Equipment

| Item | Description |
|---|---|
| **Use Case** | Return Equipment |
| **Primary Actor** | Student |
| **Preconditions** | 1. The equipment exists.<br>2. An active borrowing record exists for the equipment. |
| **Main Action** | 1. The student returns the borrowed equipment to the laboratory.<br>2. The system locates the active borrowing record for the equipment.<br>3. The system records the return date and marks the borrowing status as `Returned`.<br>4. The system updates the equipment status to available (`IsAvailable = true`).<br>5. The system confirms the return. |
| **Expected Result** | The borrowing record status changes to `Returned` with the return timestamp, and the equipment becomes available again for others to borrow. |
| **Possible Failure** | - Equipment does not exist in inventory.<br>- The equipment is not currently recorded as borrowed (no active borrowing found). |

---

### Use Case 3: Search / View Available Equipment

| Item | Description |
|---|---|
| **Use Case** | Search / View Available Equipment |
| **Primary Actor** | Student |
| **Preconditions** | The equipment inventory is accessible. |
| **Main Action** | 1. The student requests the list of equipment currently available in the laboratory.<br>2. The system filters all inventory records where `IsAvailable == true`.<br>3. The system returns and displays the list of available items. |
| **Expected Result** | The student receives an up-to-date list of equipment ready to be borrowed. |
| **Possible Failure** | No equipment items exist or no items are currently available. |

---

## C. Identify Domain Concepts

### 1. Student
1. **What information must it contain?**
   - `Id`: Unique numeric identifier for the student.
   - `Name`: Full name of the student.
   - `IsEligibleToBorrow`: Boolean flag indicating if the student is in good standing and authorized to borrow.
   - `MaxBorrowLimit`: Maximum number of active equipment borrowings allowed concurrently.
2. **What rules or state belong to it?**
   - Determination of eligibility to borrow (`IsEligibleToBorrow`).
   - Limit evaluation rule: `CanBorrow(int currentActiveBorrowings)` returns `true` only if `IsEligibleToBorrow` is true and `currentActiveBorrowings < MaxBorrowLimit`.
3. **What should NOT be the responsibility of that object?**
   - Querying the database or repository for existing borrowings.
   - Modifying equipment availability state.
   - Formatting output messages for the console or UI.

---

### 2. Equipment
1. **What information must it contain?**
   - `Id`: Unique numeric identifier for the equipment item.
   - `Name`: Descriptive name of the item (e.g., "Epson LCD Projector").
   - `IsAvailable`: Boolean state indicating whether the item is available for loan.
2. **What rules or state belong to it?**
   - Transitioning availability when borrowed: `MarkAsBorrowed()` sets `IsAvailable = false`.
   - Transitioning availability when returned: `MarkAsReturned()` sets `IsAvailable = true`.
3. **What should NOT be the responsibility of that object?**
   - Validating student authorization or borrowing quotas.
   - Managing borrowing transaction histories or timestamps.
   - Connecting directly to persistent storage.

---

### 3. Borrowing
1. **What information must it contain?**
   - `Id`: Unique identifier for the borrowing record.
   - `StudentId`: Foreign key reference to the borrowing student.
   - `EquipmentId`: Foreign key reference to the borrowed equipment.
   - `BorrowDate`: Date and time when the equipment was borrowed.
   - `DueDate`: Date and time by which the equipment is expected to be returned.
   - `ReturnDate`: Nullable date (`DateTime?`) recorded when the item is returned.
   - `Status`: Current state of the borrowing transaction (`BorrowingStatus.Active` or `BorrowingStatus.Returned`).
2. **What rules or state belong to it?**
   - Managing return state: `MarkAsReturned(DateTime returnDate)` sets `Status = BorrowingStatus.Returned` and assigns `ReturnDate`.
   - Initial state validation: Ensuring a new borrowing begins with `BorrowingStatus.Active`.
3. **What should NOT be the responsibility of that object?**
   - Checking whether the student or equipment actually exists in the database.
   - Validating whether the student exceeded their borrow quota.
   - Enforcing inventory persistence.

---

# Part I – Architecture Explanation

## 1. Solution Structure

The solution is divided into distinct projects adhering to Clean Architecture and Separation of Concerns:

```text
EquipmentBorrowing/
│
├── README.md
├── EquipmentBorrowing.sln
│
├── src/
│   ├── EquipmentBorrowing.Domain/
│   │   ├── Student.cs
│   │   ├── Equipment.cs
│   │   ├── Borrowing.cs
│   │   └── BorrowingStatus.cs
│   │
│   ├── EquipmentBorrowing.Application/
│   │   ├── Interfaces/
│   │   │   ├── IStudentRepository.cs
│   │   │   ├── IEquipmentRepository.cs
│   │   │   └── IBorrowingRepository.cs
│   │   ├── Models/
│   │   │   ├── BorrowResult.cs
│   │   │   ├── ReturnResult.cs
│   │   │   └── ActiveBorrowingDto.cs
│   │   └── Services/
│   │       ├── BorrowEquipmentService.cs
│   │       ├── ReturnEquipmentService.cs
│   │       ├── GetAvailableEquipmentService.cs
│   │       ├── GetAllEquipmentService.cs
│   │       ├── GetAllStudentsService.cs
│   │       └── GetActiveBorrowingsService.cs
│   │
│   ├── EquipmentBorrowing.Infrastructure/
│   │   └── Repositories/
│   │       ├── InMemoryStudentRepository.cs
│   │       ├── InMemoryEquipmentRepository.cs
│   │       └── InMemoryBorrowingRepository.cs
│   │
│   └── EquipmentBorrowing.Console/
│       └── Program.cs
│
└── tests/
    └── EquipmentBorrowing.Tests/
        ├── BorrowEquipmentServiceTests.cs
        └── ReturnEquipmentServiceTests.cs
```

### Purpose of Each Project:

- **`EquipmentBorrowing.Domain`**:
  The core of the system. Contains the fundamental domain entities, enums, and business logic (`Student`, `Equipment`, `Borrowing`, `BorrowingStatus`). It has zero external dependencies and does not know about any database, framework, or UI.

- **`EquipmentBorrowing.Application`**:
  Orchestrates business operations and application use cases (e.g., `BorrowEquipmentService`). It defines repository abstractions/interfaces (`IStudentRepository`, `IEquipmentRepository`, `IBorrowingRepository`) and operation result models (`BorrowResult`). It depends solely on the Domain layer.

- **`EquipmentBorrowing.Infrastructure`**:
  Contains technical implementations for data persistence and external mechanisms. In this laboratory activity, it provides in-memory repository implementations (`InMemoryStudentRepository`, `InMemoryEquipmentRepository`, `InMemoryBorrowingRepository`) that store data in C# collections. It depends on `Application` and `Domain`.

- **`EquipmentBorrowing.Tests`**:
  Contains automated unit tests using xUnit. It tests domain rules, service validations, successful operations, and error handling without requiring any database.

- **`EquipmentBorrowing.Console`**:
  The executable composition root for this activity's demonstration (Part H). It is the only project allowed to construct concrete repository classes directly; it wires the in-memory repositories into `BorrowEquipmentService` and prints one successful and one failed borrowing request. It contains no business rules itself — it only assembles and calls the Application layer, which is the same role a future Avalonia UI project would play.

---

## 2. Dependency Direction

Dependencies point strictly **inward** toward the core domain, upholding the **Dependency Inversion Principle (DIP)**:

```text
       Future UI / Executable / Test Runner
                         │
                         ▼
        +──────────────────────────────────+
        │  EquipmentBorrowing.Application  │
        │    (Use Cases & Abstractions)    │
        +──────────────────────────────────+
           │                            ▲
           │ Depends on                 │ Implements
           ▼                            │ Interfaces
+──────────────────────────+   +───────────────────────────────────+
│ EquipmentBorrowing.Domain│   │ EquipmentBorrowing.Infrastructure │
│  (Core Entities & Rules) │   │  (In-Memory Data Repositories)    │
+──────────────────────────+   +───────────────────────────────────+
           ▲                                      │
           │ Depends on                           │
           └──────────────────────────────────────┘
```

- **Domain** depends on nothing.
- **Application** depends only on **Domain**.
- **Infrastructure** implements interfaces defined by **Application** and references **Domain** entities.
- Consumers (such as test suites or future user interfaces) interact with the system through the **Application** layer.

---

## 3. Use Case Mapping

### Implemented Use Case: Borrow Equipment

- **Actor**: Student
- **Use Case**: Borrow Equipment
- **Application Service**: `BorrowEquipmentService`
- **Domain Objects Used**:
  - `Student` (holds eligibility status and `MaxBorrowLimit`)
  - `Equipment` (holds availability status and `MarkAsBorrowed()`)
  - `Borrowing` (records the transaction with borrow date and due date)
  - `BorrowingStatus` (records the `Active` status)
- **Repository Interfaces Used**:
  - `IStudentRepository` (`GetByIdAsync`, `GetAllAsync`)
  - `IEquipmentRepository` (`GetByIdAsync`, `UpdateAsync`, `GetAllAsync`)
  - `IBorrowingRepository` (`GetActiveBorrowingCountAsync`, `AddAsync`)
- **Infrastructure Implementations Used**:
  - `InMemoryStudentRepository`
  - `InMemoryEquipmentRepository`
  - `InMemoryBorrowingRepository`

---

## 4. Reflection

### 1. Why should the application service depend on a repository interface instead of directly depending on a database implementation?
> **Answer**:
> Depending on a repository interface (abstraction) decouples business rules from concrete data access technologies. This adheres to the **Dependency Inversion Principle** (high-level modules should not depend on low-level modules; both should depend on abstractions). Decoupling allows the underlying storage (in-memory collections, SQLite, PostgreSQL, or cloud APIs) to be replaced without modifying business logic. Furthermore, it enables isolated unit testing of application services using in-memory or mock repositories without needing an active database connection.

### 2. Which parts of your current solution could remain unchanged if SQLite were added later?
> **Answer**:
> - **`EquipmentBorrowing.Domain`**: Remains 100% unchanged, as domain entities have no awareness of data storage.
> - **`EquipmentBorrowing.Application`**: Remains 100% unchanged. All use case services (`BorrowEquipmentService`, `ReturnEquipmentService`) and repository interfaces (`IEquipmentRepository`, etc.) remain identical.
> - **`EquipmentBorrowing.Tests`**: Unit tests verifying application business rules and error handling remain valid and unchanged.
> - **Only `EquipmentBorrowing.Infrastructure`** would be updated to add SQLite repository implementations that satisfy the existing interfaces.

### 3. Which project would eventually contain Avalonia Views?
> **Answer**:
> A separate presentation layer project (such as `EquipmentBorrowing.Desktop` or `EquipmentBorrowing.UI`) would contain Avalonia Views and ViewModels. Keeping views in a dedicated presentation project ensures that UI frameworks do not pollute core business or data access layers.

### 4. Should an Avalonia button directly execute database queries? Why or why not?
> **Answer**:
> **No, an Avalonia button should never directly execute database queries.**
> - **Separation of Concerns**: Directly querying a database from UI code couples user interface events with database schemas and SQL statements.
> - **Bypassing Validation**: Business rules (e.g., checking student eligibility, equipment availability, and borrow limits) would be skipped or duplicated across multiple click handlers.
> - **Thread Blocking**: Direct synchronous database calls on the UI thread freeze the application and create an unresponsive user experience.
> Instead, buttons should invoke asynchronous commands that call Application Services.

### 5. What part of your implementation represents the actual business operation requested by the actor?
> **Answer**:
> The `BorrowEquipmentService.ExecuteAsync()` method in the **Application** project represents the actual business operation. It coordinates student validation, equipment availability checks, quota enforcement, `Borrowing` record creation, equipment state mutation, and persistence.

---

## Building and Running Tests

### 1. Build the Solution
```powershell
dotnet build EquipmentBorrowing.sln
```
Expected output: `Build succeeded. 0 Warning(s), 0 Error(s)`

### 2. Run Automated Tests Demonstrating Application Flow
```powershell
dotnet test EquipmentBorrowing.sln
```
All unit tests in `EquipmentBorrowing.Tests` will execute, verifying both the successful borrowing flow and all failure conditions (student does not exist, student not eligible, equipment does not exist, equipment unavailable, and borrow limit reached).

### 3. Run the Required Console Demonstration
```powershell
dotnet run --project src/EquipmentBorrowing.Console
```

`EquipmentBorrowing.Console` is the executable composition root. It creates the
in-memory repositories, injects them into `BorrowEquipmentService`, and prints
one successful borrowing request and one failed request. It contains no business
rules.
