# Laboratory Activity 5 Traceability and Baseline

## Baseline and architecture

The authentic Lab 4 submission visible in the repository history is `fd09490e5cd7b2e62bcb79e78609cb3d6fc4dc9b` (`fix: preserve in-memory borrow and return behavior`). It is present in local Git history; no retroactive Lab 4 tag was added.

Lab 4 desktop directly composed Application services and Infrastructure persistence. In this revision, the API composes Application, repository, EF Core, and PostgreSQL services. The Avalonia app references only Domain, Application display/read models, Contracts, and Client; it has no Infrastructure, Npgsql, or DbContext project reference. `EquipmentBorrowingApiClient` performs asynchronous HTTP requests; desktop ViewModels own loading and user feedback. The API keeps database writes behind application services and repositories. Multi-item checkout and return use database transactions and revalidation. This is an architectural boundary change; existing domain and Activity 4 workflows are retained.

Reproduction instructions and the disposable reference dataset are in the Activity 5 section of the root README and [`lab5-test-data.sql`](lab5-test-data.sql). The database schema uses the committed EF migration; this activity adds no schema migration.

## Use-case traceability

| Lab 4 use case | Rules | API | Implementation reference | Acceptance evidence |
|---|---|---|---|---|
| UC-01 Browse equipment | BR-05–07, 10, 18–19 | API-01 | `Api/Program.cs` `GET /equipment`; `EquipmentBorrowingApiClient.GetEquipmentAsync` | T01/T07; T01 partial, database portion not run |
| UC-02 Select available item | BR-10, 18–19 | API-01 | `Program.cs` availability query; `EquipmentViewModel` filter | T01/T07; T01 partial, database portion not run |
| UC-03 Borrow item | BR-08–11, 14–16, 18–19 | API-10, 13 | `Program.cs` `POST /borrowings`; `BorrowEquipmentService`; `EfBorrowingRepository.CheckoutAtomicallyAsync` | T04/T07; not run against PostgreSQL |
| UC-04 View active borrowings | BR-14, 18–19 | API-14 | `Program.cs` `GET /borrowings`; `BorrowingsViewModel` | T01/T06/T07; database portion not run |
| UC-05 Return item | BR-10, 13–16, 18–19 | API-12, 13 | `Program.cs` return route; `EfBorrowingRepository.ReturnAtomicallyAsync` | T04/T07; not run against PostgreSQL |
| UC-06 Multi-item checkout | BR-08–12, 14–16, 18–19 | API-11 | `Program.cs` checkout route; `BorrowEquipmentService.BorrowMultipleAsync`; `EfBorrowingRepository.CheckoutAtomicallyAsync` | T05/T07; not run against PostgreSQL |
| UC-07 Student history | BR-14, 18–19 | API-02, 03, 15 | student and borrowing routes in `Program.cs`; `GetStudentHistoryAsync` | T01/T06/T07; database portion not run |
| UC-08 Equipment history | BR-14, 18–19 | API-01, 07, 16 | equipment and history routes in `Program.cs`; `GetEquipmentHistoryAsync` | T01/T06/T07; database portion not run |
| UC-09 Search/filter/sort borrowings | BR-17–19 | API-14 | `Program.cs` query validation; `EfBorrowingRepository.SearchAsync` | T01/T06/T07; database portion not run |
| UC-12 Register student | BR-01–02, 16, 18–19 | API-04 | `Program.cs` `POST /students`; `StudentManagementService` | T02/T07; not run against PostgreSQL |
| UC-13 Edit student | BR-01–03, 14, 16, 18–19 | API-02, 03, 05 | `Program.cs` student routes; `StudentManagementService` | T02/T07; not run against PostgreSQL |
| UC-14 Change eligibility | BR-04, 08, 13–14, 18–19 | API-02, 03, 06 | `Program.cs` eligibility route; `StudentManagementService.SetEligibilityAsync` | T02/T07; not run against PostgreSQL |
| UC-15 Register equipment | BR-05–06, 10, 16, 18–19 | API-08 | `Program.cs` `POST /equipment`; `EquipmentManagementService`; optional `type` retained for Lab 4 continuity | T03/T07; not run against PostgreSQL |
| UC-16 Edit equipment | BR-05–07, 10, 14, 16, 18–19 | API-01, 07, 09 | `Program.cs` equipment routes; `EquipmentManagementService`; API-09 preserves omitted type and protected availability/history | T03/T07; not run against PostgreSQL |

## Submission identity

The laboratory sheet asks an individual submission to identify the student by full name and student ID, or a pair by both members' names and IDs. These details were not present in the repository context available for this work. Add the correct identity in the requested private submission comment before submitting; no identity was guessed or committed here.
