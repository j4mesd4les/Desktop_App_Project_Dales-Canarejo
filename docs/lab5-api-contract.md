# Laboratory Activity 5 API Contract

The API uses UTF-8 JSON under `/api/v1`. Stable integer IDs returned by the API identify records. Dates use `YYYY-MM-DD` in the server's local calendar. Lists return arrays, including `[]` when there are no matches. Errors use `{ "code": "...", "message": "..." }`.

| API | Method and route | Request/query | Success | Main failure responses |
|---|---|---|---|---|
| API-01 | GET `/api/v1/equipment` | Optional `available=true\|false` | 200 equipment array | 400 invalid boolean |
| API-02 | GET `/api/v1/students` | Optional `search` | 200 student array | 400 invalid query |
| API-03 | GET `/api/v1/students/{id}` | — | 200 student | 404 missing |
| API-04 | POST `/api/v1/students` | `studentNumber`, `fullName`, optional `contact` | 201 student + `Location` | 400 invalid; 409 duplicate |
| API-05 | PUT `/api/v1/students/{id}` | Complete editable student fields | 200 student | 400 invalid; 404 missing; 409 duplicate |
| API-06 | PATCH `/api/v1/students/{id}/eligibility` | `{ "isEligible": true }` | 200 student | 400 invalid; 404 missing |
| API-07 | GET `/api/v1/equipment/{id}` | — | 200 equipment | 404 missing |
| API-08 | POST `/api/v1/equipment` | `equipmentCode`, `name`, optional `description`, optional `type` | 201 equipment + `Location` | 400 invalid; 409 duplicate |
| API-09 | PUT `/api/v1/equipment/{id}` | Complete editable equipment fields; `type` may be omitted to preserve its current value | 200 equipment | 400 invalid/protected field; 404 missing; 409 duplicate |
| API-10 | POST `/api/v1/borrowings` | `studentId`, `equipmentId`, `expectedReturnDate` | 201 borrowing + `Location` | 400 invalid; 404 missing reference; 409 rule conflict |
| API-11 | POST `/api/v1/checkout-requests` | `studentId`, `equipmentIds` (at least 2), `expectedReturnDate` | 201 `{ "borrowings": [...] }` | 400 invalid; 404 missing reference; 409 rule conflict |
| API-12 | POST `/api/v1/borrowings/{id}/return` | Empty object `{}` | 200 returned borrowing | 400 invalid; 404 missing; 409 already returned |
| API-13 | GET `/api/v1/borrowings/{id}` | — | 200 borrowing | 404 missing |
| API-14 | GET `/api/v1/borrowings` | `search`, `status=All\|Active\|Returned`, `sort=borrowedDate\|expectedReturnDate`, `direction=asc\|desc` | 200 borrowing array | 400 invalid query |
| API-15 | GET `/api/v1/students/{id}/borrowings` | — | 200 student history | 404 missing student |
| API-16 | GET `/api/v1/equipment/{id}/borrowings` | — | 200 equipment history | 404 missing equipment |

Student responses contain `id`, `studentNumber`, `fullName`, `contact`, `isEligible`. Equipment responses contain `id`, `equipmentCode`, `name`, `description`, `isAvailable`, and `type`. The optional `type` request field retains Lab 4's equipment classification; it defaults to `General` when omitted. Borrowing responses contain `id`, `studentId`, `equipmentId`, `borrowedDate`, `expectedReturnDate`, `returnedDate`, `status`, `studentName`, `equipmentName`. API-10/11 never accept client-supplied status or availability. New students are eligible and new equipment is available.

The accompanying API reference lists only equipment code, name, and description in the create/update body. The optional `type` field is an additive continuity extension for Lab 4's UC-15 optional type and existing management form; older clients can omit it.

The complete schemas, methods, routes, and response codes are published in [`openapi.v1.json`](../src/EquipmentBorrowing.Api/openapi.v1.json) and served at `/openapi/v1.json`. An unknown route or ID returns 404; an unsupported HTTP method returns 405. Invalid request syntax/fields return 400, business conflicts return 409, and unexpected failures return a safe 500. A transport failure has no HTTP status and is reported separately by the desktop client.

Multi-item checkout rechecks the student, eligibility, borrowing limit, item existence, and availability inside one PostgreSQL transaction. A known failed checkout is rolled back. If the client loses the response to a change, it refreshes before asking staff to decide whether to retry.
