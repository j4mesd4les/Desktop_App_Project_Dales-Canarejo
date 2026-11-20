# Lab 4 use cases and verification map

Primary actor: Laboratory Staff / Equipment Custodian. The user selects existing students and physical equipment records. UC-10, UC-11, and UC-17 onward are deferred as stated by the supplied activity.

```mermaid
flowchart LR
  Staff[Laboratory Staff / Equipment Custodian]
  Staff --- UC01((UC-01 View catalog))
  Staff --- UC02((UC-02 Find available))
  Staff --- UC03((UC-03 Borrow one))
  Staff --- UC04((UC-04 View active))
  Staff --- UC05((UC-05 Return))
  Staff --- UC06((UC-06 Checkout multiple))
  Staff --- UC07((UC-07 Student history))
  Staff --- UC08((UC-08 Equipment history))
  Staff --- UC09((UC-09 Search/filter/sort))
  Staff --- UC12((UC-12 Register student))
  Staff --- UC13((UC-13 Update student))
  Staff --- UC14((UC-14 Change eligibility))
  Staff --- UC15((UC-15 Register equipment))
  Staff --- UC16((UC-16 Update equipment))
```

## Required suite summary

The same standard fields apply to every required case below: **Scope** is the named catalog, borrowing, history, search, student, or equipment area; **Level** is User Goal; **Primary Actor** is Laboratory Staff / Equipment Custodian; **Stakeholders** are staff, who need understandable accurate work, and students, whose records and history must stay correct. **Failure Guarantee** for reads is no data changes; for writes a rejection/known failure leaves existing data unchanged, while an unknown commit follows BR-18. **Special Requirements** are PostgreSQL persistence, asynchronous related-data loading where applicable, and the View → ViewModel → Application → repository → Infrastructure separation. **Includes / Extends**: none required. **Open Issues**: none. Each row supplies the trigger, precondition, success guarantee, main scenario, rules and acceptance outcome; the common alternative and exception paths follow the table.

| ID / goal | Actor interaction and main branch | Rules and acceptance outcome |
|---|---|---|
| UC-01 View Equipment Catalog | Open Equipment; display identified items and availability. | BR-05–07; all equipment is visible with its status. Empty catalog is explained. |
| UC-02 Find Available Equipment | Turn on the available-only filter or inspect availability. | BR-10; only currently borrowable items appear in the filtered list. |
| UC-03 Borrow Equipment | Choose one student, one item, due date; confirm. | BR-08–11, 15–16; eligible student and available item become one active record; invalid/stale data is rejected. |
| UC-04 View Active Borrowings | Open active list. | BR-14, 17, 19; active records show student, item and dates; empty state is clear. |
| UC-05 Return Equipment | Select active record and return it. | BR-13–16; retain history, record local return date, make item available; reject repeat return. |
| UC-06 Check Out Multiple Equipment Items | Select student, at least two distinct item IDs and one future due date. | BR-08–12, 15–16; all items issue together or none; stale item, limit, eligibility, duplicate or persistence failure rejects the request. |
| UC-07 View Student Borrowing History | Enter a student ID and load history. | BR-14, 19; active and returned rows remain linked and visible after edits/returns. |
| UC-08 View Equipment Borrowing History | Enter an equipment ID and load history. | BR-10, 14, 19; the item's complete active/returned history is shown. |
| UC-09 Search, Filter, and Sort Borrowings | Search by name/number/item; choose status, date sort and direction. | BR-17, 19; controls combine, order is stable by borrowing ID, and no matches have an explicit message. |
| UC-12 Register Student | Enter number, name and optional contact. | BR-01–02; trimmed unique number, valid required fields, new student eligible. |
| UC-13 Update Student Information | Locate student and edit number/name/contact. | BR-01–03; stable identity and all history remain; blank optional contact clears. |
| UC-14 Change Student Borrowing Eligibility | Permit/restrict existing student. | BR-04; restriction blocks new loans but does not cancel current loans or block return. Repeating choice is harmless. |
| UC-15 Register Equipment | Enter unique equipment number, name and optional description/type. | BR-05–06; new equipment starts available and has no loan. |
| UC-16 Update Equipment Information | Locate item and edit number/name/description. | BR-05–07; stable item/history preserved; availability and dates are not edited here. |

### Alternative and exception flows

| Cases | Alternative flow | Exception flow |
|---|---|---|
| UC-01–02 | Empty catalog or no available items shows an empty result. | PostgreSQL read error shows an error and allows retry. |
| UC-03 | Missing, ineligible, over-limit, unavailable or invalid-date request is rejected with a reason; staff can correct and resubmit. | Stale record/conflict refreshes the view; database error reports known failure or unknown outcome under BR-18. |
| UC-04 | No active borrowings shows an empty state. | Read error is distinct from an empty list and can be retried. |
| UC-05 | A repeated return is rejected without changing the returned record. | Missing/stale borrowing refreshes the list; write failure follows BR-18. |
| UC-06 | Fewer than two items, duplicates, invalid date, ineligible student, limit violation or unavailable item is rejected for correction. | Conflict rejects the whole request; known write failure rolls back all changes; unknown commit is reported without retry. |
| UC-07–08 | No records shows an empty history; selecting another student/item replaces the prior results. | Stale selection or database error is explained and can be retried/reselected. |
| UC-09 | No matches shows a clear empty result; staff can change or clear combined criteria. | Database read error is not presented as an empty search. |
| UC-12, UC-15 | Invalid required fields or duplicate normalized identifier is rejected for correction. | Persistence failure leaves no partially registered record and gives understandable feedback. |
| UC-13, UC-16 | Blank optional contact/description clears that value; unchanged stable IDs and borrowing history remain. | Invalid/duplicate identifier or stale record is rejected; history is preserved. |
| UC-14 | Repeating the current eligibility choice reports the current state without unintended changes. | Missing/stale student is rejected; active loans remain and may still be returned after restriction. |

## Traceability and current evidence

| Use cases | Implementation location | Verification evidence |
|---|---|---|
| UC-01–02 | `EquipmentViewModel`, `EquipmentView` | Build check; manual catalog/available-filter demonstration still required. |
| UC-03, UC-06 | `BorrowEquipmentService`, `EfBorrowingRepository.CheckoutAtomicallyAsync`, `EquipmentView` | Transaction revalidates student, limit, item rows and active loan; clean-database/manual success and forced-failure evidence still required. |
| UC-04–05 | `BorrowingsViewModel`, `ReturnEquipmentService`, `EfBorrowingRepository.ReturnAtomicallyAsync` | Build check; PostgreSQL return/restart demonstration still required. |
| UC-07–09 | `EfBorrowingRepository.GetHistoryAsync` / `SearchAsync`, `BorrowingsViewModel`, `BorrowingsView` | Build check; manual filters/sort/empty-state evidence still required. |
| UC-12–14 | `StudentManagementService`, `StudentManagementViewModel` | Existing validation and update flows; manual integrity/history evidence still required. |
| UC-15–16 | `EquipmentManagementService`, `EquipmentManagementViewModel` | Existing validation and update flows; manual integrity/history evidence still required. |

## Verification record

Observed checks: `dotnet build EquipmentBorrowing.slnx --no-restore -m:1 -p:UseSharedCompilation=false -v minimal` succeeded with 0 warnings and 0 errors. The existing console demonstration exited successfully: one item became unavailable after checkout, available after return, and repeat/missing returns were rejected. EF Core generated a 6,459-byte PostgreSQL migration script containing the three tables, primary/foreign keys, due-date/status checks, normalized identifier uniqueness, the partial unique active-equipment index, and sample seed inserts. `git diff --check` found no whitespace errors.

Not observed: the local PostgreSQL service was not running; the reachable default server requested a password, and no app connection string was configured in this process. An isolated local PostgreSQL cluster did not finish initializing. Therefore the generated SQL was inspected but not executed; migration on a clean DB, real PostgreSQL queries, PostgreSQL constraint rejections, restart persistence, injected rollback, and database recovery still need a run against a reachable disposable database. Do not treat the build, console demo, or generated script as evidence of those database operations.

BR-18 recovery: if a transaction commit response is lost, the application reports an unknown outcome, does not retry automatically, and instructs staff to refresh records before choosing another action. A known pre-commit failure rolls back the transaction. No claim of successful rollback testing is made until the disposable database failure demonstration is recorded.
