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
| Use Case | System Recording |
| Primary Actor | Laboratory Staff |
| Preconditions | The student's request to borrow an equipment is approved |
| Main Action | The staff will record important details of the student that borrowed an item |
| Expected Result | Successfully recorded student details and store it in a information datatase |
| Possible Failure | The system fails to save the student's borrowing details due to missing, invalid, or incomplete information. |

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
  Equipment type, availability, quantity, name, id, status
  2.What rules or state belong to it?
  Change its state between available and currently borrowed
  3. What should **not** be the responsibility of that object?
  Any responsibility of the student and staff.
          
