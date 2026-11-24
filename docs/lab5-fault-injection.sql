-- Test-only failure seam for T05. Apply only after restoring the disposable
-- reference dataset. Use POST checkout-requests with S4 and E2/E6 (IDs 4, 2, 6).
DO $$
BEGIN
    IF current_database() <> 'equipment_borrowing_lab5_test' THEN
        RAISE EXCEPTION 'Refusing to add a test trigger to database %.', current_database();
    END IF;
END $$;

CREATE OR REPLACE FUNCTION lab5_fail_checkout_test()
RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
    IF NEW."StudentId" = 4 AND NEW."EquipmentId" = 6 THEN
        RAISE EXCEPTION 'Activity 5 controlled transaction failure' USING ERRCODE = 'P0001';
    END IF;
    RETURN NEW;
END;
$$;

CREATE TRIGGER lab5_fail_checkout_test_trigger
BEFORE INSERT ON "Borrowings"
FOR EACH ROW EXECUTE FUNCTION lab5_fail_checkout_test();
