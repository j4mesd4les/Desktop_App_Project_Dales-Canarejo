-- Activity 5 reference dataset. This reset intentionally refuses any database
-- except the dedicated disposable database named below.
DO $$
BEGIN
    IF current_database() <> 'equipment_borrowing_lab5_test' THEN
        RAISE EXCEPTION 'Refusing to reset database %; use equipment_borrowing_lab5_test.', current_database();
    END IF;
END $$;

TRUNCATE TABLE "Borrowings", "Equipment", "Students" RESTART IDENTITY CASCADE;

INSERT INTO "Students" ("Id", "StudentNumber", "StudentNumberNormalized", "Name", "Contact", "IsAllowedToBorrow") VALUES
(1, 'ST-100', 'ST-100', 'Ada Cruz', NULL, TRUE),
(2, 'ST-200', 'ST-200', 'Ben Diaz', NULL, FALSE),
(3, 'ST-300', 'ST-300', 'Cara Lim', NULL, TRUE),
(4, 'ST-400', 'ST-400', 'Dani Ong', NULL, TRUE);

INSERT INTO "Equipment" ("Id", "EquipmentNumber", "EquipmentNumberNormalized", "Name", "Type", "Description", "IsAvailable") VALUES
(1, 'EQ-001', 'EQ-001', 'Camera', 'Photography', NULL, FALSE),
(2, 'EQ-002', 'EQ-002', 'Laptop', 'Computer', NULL, TRUE),
(3, 'EQ-003', 'EQ-003', 'Projector', 'Audio Visual', NULL, TRUE),
(4, 'EQ-004', 'EQ-004', 'Tablet', 'Computer', NULL, FALSE),
(5, 'EQ-005', 'EQ-005', 'Microphone', 'Audio Visual', NULL, FALSE),
(6, 'EQ-006', 'EQ-006', 'Tripod', 'Photography', NULL, TRUE),
(7, 'EQ-007', 'EQ-007', 'Speaker', 'Audio Visual', NULL, TRUE);

-- The date offsets follow the reference: E1 is overdue by one day and the
-- other active/returned examples have a due date two days from today's date.
INSERT INTO "Borrowings" ("Id", "StudentId", "EquipmentId", "BorrowedOn", "ExpectedReturnOn", "Status", "ReturnedOn") VALUES
(1, 1, 1, CURRENT_DATE - 9, CURRENT_DATE - 1, 'Active', NULL),
(2, 1, 3, CURRENT_DATE - 8, CURRENT_DATE + 2, 'Returned', CURRENT_DATE - 2),
(3, 3, 4, CURRENT_DATE - 7, CURRENT_DATE + 2, 'Active', NULL),
(4, 3, 5, CURRENT_DATE - 7, CURRENT_DATE + 2, 'Active', NULL);

SELECT setval(pg_get_serial_sequence('"Students"', 'Id'), (SELECT MAX("Id") FROM "Students"), TRUE);
SELECT setval(pg_get_serial_sequence('"Equipment"', 'Id'), (SELECT MAX("Id") FROM "Equipment"), TRUE);
SELECT setval(pg_get_serial_sequence('"Borrowings"', 'Id'), (SELECT MAX("Id") FROM "Borrowings"), TRUE);

-- Independent confirmation for the Activity 5 evidence record.
SELECT s."StudentNumber", s."Name", s."IsAllowedToBorrow", COUNT(b."Id") FILTER (WHERE b."Status" = 'Active') AS "ActiveCount"
FROM "Students" s LEFT JOIN "Borrowings" b ON b."StudentId" = s."Id"
GROUP BY s."Id" ORDER BY s."Id";

SELECT e."EquipmentNumber", e."Name", e."IsAvailable", b."Status", b."ExpectedReturnOn"
FROM "Equipment" e LEFT JOIN "Borrowings" b ON b."EquipmentId" = e."Id" AND b."Status" = 'Active'
ORDER BY e."Id";
