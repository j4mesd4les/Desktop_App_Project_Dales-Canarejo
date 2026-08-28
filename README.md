# Campus Equipment Borrowing System

**1. Solution Structure**

* **Domain**: Holds and manage the core models such as Student, Equipment, Borrowinng, BorrowingStatus. There is no logic outside of basic object properties.
* **Application**: This holds the rules and workflow of the system and defines repository interfaces.
* **Infrastructure**: It Holds the actual data of storage logic.
* **Tests**: Can be utilized for demo purposes and any testing process.

# Campus Equipment Borrowing System

## 1. Solution Structure

* **Domain**: Holds your core models (`Student`, `Equipment`, `Borrowing`). No logic outside of basic object properties.
* **Application**: Holds the rules and workflow (`BorrowEquipmentService`) and defines repository interfaces.
* **Infrastructure**: Holds actual data storage logic (currently `InMemory` repositories).
* **Tests**: The runnable entry point that acts as a demo to execute test cases.

---

## 2. Dependency Direction

```text
EquipmentBorrowing.Tests
      │
      ├───► EquipmentBorrowing.Infrastructure
      │             │
      │             ▼ (implements interfaces)
      ▼             │
EquipmentBorrowing.Application
      │
      ▼
EquipmentBorrowing.Domain
```

**3. Use Case Mapping**

```text
Actor:                               Student
Use Case:                            Borrow Equipment
Application Service:                 BorrowingServices.cs
Domain Objects Used:                 Student, Equipment, Borrowing, BorrowingStatus
Repository Interfaces Used:          IEquipmentRepository.cs, Exceptions.cs
Infrastructure Implementations Used: EquipmentRepository.cs
```

**4. Reflection**

1. Why depend on repository interfaces instead of databases directly?
- Because it keeps code flexible and can test easily in-memory and swap databases without changing application rules.
2. Which parts remain unchanged if SQLite is added?
- Domain and Application would remain unchanged because they only depend on abstract rules and repository interfaces rather than actual database details.
3. Which project would contain Avalonia Views?
- It would be for new UI projects.
4. Should an Avalonia button execute DB queries directly?
- No, because it should call the Application Service so rules like availability or borrowing limits are properly checked first before executing DB queries directly.
5. What part represents the actual business operation?
- The BorrowingServices.cs method because it coordinates all the steps and imposed the borrowing rules requested by the actor. It also ensures the student is allowed to borrow, checks item availability, and updates system records all in one place before completing the request.




          
