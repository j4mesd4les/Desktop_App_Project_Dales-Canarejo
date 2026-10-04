-- ============================================================================
-- Laboratory Activity 3 - Part C: Basic SQL against the Equipment Borrowing DB
-- Database: SQLite   |   Tables: Students, Equipment, Borrowings
--
-- Notes
--   * SQLite stores booleans as integers: 1 = true, 0 = false.
--   * Dates are stored as text in ISO format (yyyy-MM-dd HH:mm:ss), so
--     comparing them as text also orders them correctly.
--   * Borrowings.Status is stored as the text 'Active' or 'Returned'.
--   * These statements are for learning/demonstration. The application itself
--     always goes through the repositories and EF Core.
-- ============================================================================


-- ----------------------------------------------------------------------------
-- 1. Basic retrieval: all equipment
-- ----------------------------------------------------------------------------
SELECT *
FROM Equipment;


-- ----------------------------------------------------------------------------
-- 2. Filtering: only equipment that is currently available
-- ----------------------------------------------------------------------------
SELECT Id, Name, Type
FROM Equipment
WHERE IsAvailable = 1
ORDER BY Name;


-- ----------------------------------------------------------------------------
-- 3. Join: active borrowings with the student and equipment information
--    Borrowings only stores StudentId / EquipmentId, so we JOIN to get names.
-- ----------------------------------------------------------------------------
SELECT
    s.Name                    AS Student,
    e.Name                    AS Equipment,
    date(b.BorrowedOn)        AS Borrowed,
    date(b.ExpectedReturnOn)  AS Due
FROM Borrowings AS b
INNER JOIN Students  AS s ON s.Id = b.StudentId
INNER JOIN Equipment AS e ON e.Id = b.EquipmentId
WHERE b.Status = 'Active'
ORDER BY b.ExpectedReturnOn, s.Name;


-- ----------------------------------------------------------------------------
-- 4. Aggregates
-- ----------------------------------------------------------------------------

-- 4a. Number of active borrowings per student.
--     LEFT JOIN keeps students who currently have none (count = 0).
--     This is the same rule the app enforces (maximum 3 active borrowings).
SELECT
    s.Name,
    COUNT(b.Id) AS ActiveBorrowings
FROM Students AS s
LEFT JOIN Borrowings AS b
       ON b.StudentId = s.Id
      AND b.Status = 'Active'
GROUP BY s.Id, s.Name
ORDER BY ActiveBorrowings DESC, s.Name;

-- 4b. Number of equipment records by type, and how many are available.
--     SUM(IsAvailable) works because available = 1 and unavailable = 0.
SELECT
    Type,
    COUNT(*)         AS TotalItems,
    SUM(IsAvailable) AS AvailableItems
FROM Equipment
GROUP BY Type
ORDER BY Type;


-- ----------------------------------------------------------------------------
-- 5. Update: stop a student from borrowing (the "not allowed" scenario)
-- ----------------------------------------------------------------------------
UPDATE Students
SET IsAllowedToBorrow = 0
WHERE StudentNumber = '2023-0003';

-- Check the change:
SELECT StudentNumber, Name, IsAllowedToBorrow
FROM Students
WHERE StudentNumber = '2023-0003';

-- Undo it so the demo data is back to normal:
UPDATE Students
SET IsAllowedToBorrow = 1
WHERE StudentNumber = '2023-0003';
