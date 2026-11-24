# Laboratory Activity 5 Test Execution Record

Environment date: 2026-11-24 (Asia/Manila). Repository: `codex/lab5-api-integration`. Reference dataset date offsets use the API host's local calendar date. The PostgreSQL database credential/configuration was not available in this session, so the lab's reset dataset and database-backed checks could not be executed. No database acceptance case is marked passed based only on code inspection or HTTP status.

## Test summary

| Test | Result | Observation / evidence | Diagnosis or next step |
|---|---|---|---|
| T01 | Partial; database steps not run | API host started without Avalonia; OpenAPI returned parseable JSON with 16 operations; unknown route returned 404 JSON; wrong method returned 405 JSON; malformed JSON, an unknown field, and missing required student fields returned 400 JSON. Valid seeded reads, history content, empty history, and before/after database check need PostgreSQL. | Configure disposable database using README and `docs/lab5-test-data.sql`; then capture read responses and independent SQL before/after. |
| T02 | Not run | Requires seeded PostgreSQL for durable student changes and uniqueness/history checks. | Run the supplied T02 sequence against disposable DB. |
| T03 | Not run | Requires seeded PostgreSQL for equipment persistence and protected availability/history checks. | Run the supplied T03 sequence against disposable DB. |
| T04 | Not run | Requires seeded PostgreSQL for borrow, return, repeat return, and restart persistence. | Run the supplied T04 sequence against disposable DB. |
| T05 | Not run | Requires disposable PostgreSQL. A guarded trigger seam is prepared in `docs/lab5-fault-injection.sql`; it raises an unexpected database error during the S4/E2+E6 checkout so the API should return safe 500 and the transaction should roll back. | Restore the seed, install the test trigger, POST the controlled request, compare fresh SQL state, remove the trigger with `docs/lab5-fault-cleanup.sql`, then record evidence. |
| T06 | Not run | Requires controlled histories to verify relational filtering, sort order, and empty lists. | Run search/filter/sort/history requests and retain response plus DB snapshot. |
| T07 | Partial; end-to-end UI/database steps not run | Static project reference inspection confirms Desktop has no Infrastructure/EF/Npgsql reference. API host and OpenAPI were independently reachable. UI refresh, async interactions, and PostgreSQL persistence were not exercised in this environment. | Run API and desktop together with PostgreSQL and record UI plus readback evidence. |
| T08 | Partial | With API reachable and an empty connection string configured, a DB-backed request returned HTTP 500 with the safe `{code,message}` shape. API unreachable state and recovery/persisted data could not be proved with PostgreSQL here. | Capture a real transport failure, then recover with valid disposable DB; verify no secrets appear in client responses or logs. |

## Build and protocol checks

These checks do not replace T01–T08:

| Check | Observed result |
|---|---|
| `dotnet restore EquipmentBorrowing.slnx --force -p:RestoreUseStaticGraphEvaluation=true -p:NuGetAudit=false -v minimal` | Passed during project setup. |
| `dotnet build EquipmentBorrowing.slnx --no-restore -m:1 -nodeReuse:false` | Passed, 0 warnings and 0 errors. |
| Existing console borrowing demonstration | Passed; borrow made equipment unavailable, return made it available, and repeated/missing returns were rejected. |
| `GET /openapi/v1.json` | Returned parseable OpenAPI 3.1 JSON and all 16 API operation IDs. |
| Unknown route / wrong method | Returned 404 / 405 with safe JSON errors. |
| Malformed JSON on POST | First live run used an unconfigured database and the handler eagerly resolved its database service before reading the body (500). Service resolution now happens after body validation; retest returned 400. |
| Database-backed GET with connection string unset | Returned 500 safe JSON; this is an API response, distinct from an unreachable API. |

No credentials, response traces, or database contents are included in this record. Replace partial/not-run statuses only after preserving the corresponding actual evidence.
