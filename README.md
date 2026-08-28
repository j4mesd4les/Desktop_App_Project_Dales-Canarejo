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

Actor:                               Student
Use Case:                            Borrow Equipment
Application Service:                 BorrowingServices.cs
Domain Objects Used:                 Student, Equipment, Borrowing, BorrowingStatus
Repository Interfaces Used:          IEquipmentRepository.cs, Exceptions.cs
Infrastructure Implementations Used: EquipmentRepository.cs


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





**IV. Part A – Analyze the System Before Coding**

**A. Actors**
- Students
- Laboratory Staff
- 
**B. Use Cases**

**USE CASE 01**
| Item | Description |
|---|---|
| Use Case | Request Borrow Equipment |
| Primary Actor | Student |
| Preconditions | The student’ requested equipment should exists and available, and if the student has not reached maximum numbers of borrowed equipment |
| Main Action | The student will request to borrow an equipment |
| Expected Result |The request to borrow is approved and the system records the important details for borrowing the equipment |
| Possible Failure | Student is not allowed to borrow based on certain circumstances, equipment does not exist, equipment unavailable |

**USE CASE 02**
| Item | Description |
|---|---|
| Use Case | Check Active Borrowings |
| Primary Actor | Laboratory Staff |
| Preconditions | The student exists in the system database. |
| Main Action | The staff will check in the system of how many a student has currently active borrowings. |
| Expected Result | The system returns the current count of active borrowings to determine if the student can borrow more equipment. |
| Possible Failure | The student does not exist in database and returns an error. |

**USE CASE 03**
| Item | Description |
|---|---|
| Use Case | Return Equipment |
| Primary Actor | Student |
| Preconditions | The student has an active borrowed equipment |
| Main Action | The student will return the equipment |
| Expected Result | The equipment is returned and the equipment is marked as available again in the system |
| Possible Failure | The student has no active borrowed equipment for it to return an equipment |


**C. Identify Domain Concepts**

**STUDENT**
  1. What information must it contain?
  - Important details of the students related to borrowing equipment such as name, id number, date borrowed, student borrowing status, equipment borrowed, and expected return date.
  2.What rules or state belong to it?
  - If the student is able to borrow based on his borrowing status.
  3. What should **not** be the responsibility of that object?
  - Any actions that a laboratory staff should do such as checking item availability, record student details, and managing the overall system a staff member is responsible for.
	
**Equipment**
  1. What information must it contain?
  Equipment type, availability, quantity, name, id, status
  2.What rules or state belong to it?
  Change its state between available and currently borrowed
  3. What should **not** be the responsibility of that object?
  Any responsibility of the student and staff.

**Borrowing**
  1. What information must it contain?
  EquipmentType, Availability, Quantity, Name, Id, Status, ExpectedReturnOn, BorrowedOn
  2.What rules or state belong to it?
  It maintains the current state of the loan transaction (Active vs. Returned) and handles its     own return logic
  3. What should **not** be the responsibility of that object?
  Any responsibilities from the actors like borrowing equipments, system handling, or fetching     and saving data from storage and etc.


          
