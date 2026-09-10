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

---

# ACT#2 – Desktop Application and Updated Architecture

## 6. Desktop Project

The project now has an **Avalonia Desktop project** called `EquipmentBorrowing.Desktop`.

This project contains the main user interface of the system. It has two main pages:

* **Available Equipment** – where the user can select a student, equipment, and return date before borrowing.
* **Active Borrowings** – where the user can see active borrowings and return an equipment.

The Desktop project also uses **MVVM** through CommunityToolkit.Mvvm. The Views handle the UI, while the ViewModels handle the user actions, selected items, messages, and calls to the Application layer.

---

## 7. Updated Architecture

The updated flow of the system is:

```text
View
  ↓
ViewModel
  ↓
Application Service
  ↓
Repository Interface
  ↓
Infrastructure
```

Each part has its own job:

* **Domain** – contains the main objects like `Student`, `Equipment`, and `Borrowing`.
* **Application** – contains the main operations and rules, such as borrowing and returning equipment.
* **Infrastructure** – handles the actual data storage. Currently, it uses in-memory repositories.
* **Desktop** – contains the Avalonia UI, ViewModels, navigation, and dependency injection setup.
* **Tests** – used to test and demonstrate that the system works.

The View does not directly access the repositories. Instead, the ViewModel connects the UI to the Application Service, which then handles the actual operation.

---

## 8. Borrow Equipment Flow

The borrowing process works like this:

```text
User selects student, equipment, and return date
                    ↓
             Equipment View
                    ↓
          Equipment ViewModel
                    ↓
        BorrowEquipmentService
                    ↓
          Repository Interfaces
                    ↓
         In-Memory Repositories
                    ↓
       Equipment becomes unavailable
       and borrowing is recorded
```

First, the user selects a student, an available equipment, and an expected return date.

When the user clicks **Borrow Selected Equipment**, the `EquipmentViewModel` sends the request to `BorrowEquipmentService`.

The service then checks the important rules, such as:

* Does the student exist?
* Is the student allowed to borrow?
* Is the equipment available?
* Has the student reached the borrowing limit?
* Is the return date valid?

If everything is okay, the equipment is marked as unavailable and a new borrowing is added.

The ViewModel then refreshes the equipment list and shows a success message.

---

## 9. Return Equipment Flow

The return process works like this:

```text
User selects an active borrowing
                    ↓
            Borrowings View
                    ↓
         Borrowings ViewModel
                    ↓
        ReturnEquipmentService
                    ↓
          Repository Interfaces
                    ↓
         In-Memory Repositories
                    ↓
      Borrowing becomes returned
      and equipment becomes available
```

The user first selects an active borrowing from the list.

After clicking **Return Selected Borrowing**, the `BorrowingsViewModel` calls the `ReturnEquipmentService`.

The service checks if the borrowing exists and if it has not already been returned.

If everything is okay, the borrowing is marked as returned and the equipment becomes available again.

The ViewModel then reloads the active borrowings list so the returned item is removed from the list.

---

## 10. Architectural Reflection

### 1. Why should the View not call the repository directly?

The View should mainly handle the UI. If it directly calls the repository, it could skip some of the rules in the Application layer. Using the Application Service keeps the process organized.

### 2. Why should business rules not be placed in the ViewModel?

The ViewModel is mainly for handling the UI and user actions. Things like checking equipment availability or the borrowing limit should be handled by the Application Service instead.

### 3. What is the responsibility of the ViewModel?

The ViewModel connects the UI to the Application layer. It handles things like selected items, buttons, lists, messages, and calling the correct service.

### 4. Why can the Application layer work without knowing about Avalonia?

The Application layer does not use Avalonia. It only works with the Domain and repository interfaces. Because of this, the same Application code could still be used even if the UI was changed to something else.

### 5. What is the advantage of having one DI composition point?

The Desktop project has one main place where all the dependencies are registered. This makes it easier to see which repositories, services, and ViewModels are being used.

The repositories are also registered as singletons, so the same in-memory data is shared throughout the application. This is why an equipment that was borrowed becomes unavailable and stays that way when moving between pages.

### 6. Which interface parts stay unchanged if SQLite replaces the in-memory repository?

The repository interfaces can mostly stay the same. The main change would be in the Infrastructure project, where the in-memory repositories would be replaced with SQLite repositories.

The Application Services would still use the same interfaces, so the borrowing and returning logic would not need to be rewritten just because the storage changed.



          
