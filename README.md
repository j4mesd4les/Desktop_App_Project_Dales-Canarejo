# equipment_borrowing (Python version of the lab's C# structure)

Same layering the lab asks for — Domain / Application / Infrastructure / Tests —
just written in Python instead of C#. Dataclasses stand in for C# records/classes,
`ABC` stands in for C# interfaces, `async def` stands in for `Task<T>`.

## 1. Solution structure

- **domain/** — `Student`, `Equipment`, `Borrowing`, `BorrowingStatus`. Plain data +
  the one behavior that belongs to the concept itself (`Borrowing.mark_returned`).
  No I/O, no framework code.
- **application/** — `BorrowEquipmentService` (the one use case implemented),
  the repository interfaces it depends on (`IStudentRepository`, etc.), and the
  business-rule exceptions it can raise.
- **infrastructure/** — `InMemory*Repository` classes: the only place that knows
  data is sitting in a `dict`. Swapping these for real DB-backed repositories
  later requires zero changes to `application/` or `domain/`.
- **tests/** — `unittest.IsolatedAsyncioTestCase` tests for the service (success
  + each failure path).
- **demo.py** — runnable script showing one success case and two failure cases.

## 2. Dependency direction

```
demo.py / tests
      |
      v
 application  ----depends on---->  domain
      ^
      | implements the interfaces
      |
 infrastructure
```

`application` only knows about interfaces it defines itself; `infrastructure`
depends *inward* on those interfaces, never the other way around. `domain` has
zero outward dependencies.

## 3. Use case mapping (Borrow Equipment)

```
Actor:                Student
Use Case:              Borrow Equipment
Application Service:   BorrowEquipmentService.borrow()
Domain Objects Used:   Student, Equipment, Borrowing, BorrowingStatus
Repository Interfaces: IStudentRepository, IEquipmentRepository, IBorrowingRepository
Infrastructure Impls:  InMemoryStudentRepository, InMemoryEquipmentRepository,
                        InMemoryBorrowingRepository
```

## 4. Reflection

1. **Why depend on an interface instead of a concrete repository?** So
   `BorrowEquipmentService` can be tested and reasoned about without a real
   database, and so the storage technology can change without touching the
   business rule code.
2. **What stays unchanged if a real DB is added?** All of `domain/` and
   `application/` — only `infrastructure/` gets new classes
   (e.g. `SqliteEquipmentRepository`) that implement the same interfaces.
3. **Where would a GUI go?** A new top-level layer (e.g. a `ui/` package or a
   separate app) that calls `BorrowEquipmentService`, the same way `demo.py`
   does — it never touches the repositories directly.
4. **Should a UI button run a DB query directly?** No — it should call the
   application service, which enforces the business rules first. Otherwise
   validation gets duplicated (or skipped) in every place that touches data.
5. **What actually represents the business operation?**
   `BorrowEquipmentService.borrow()` — everything else (domain models,
   repositories) exists to support that one operation.

## Run it

```
python3 demo.py
python3 -m unittest discover -s tests -v
```
