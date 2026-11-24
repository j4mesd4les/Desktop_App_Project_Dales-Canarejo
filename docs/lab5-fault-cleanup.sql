-- Run after T05, and only against the disposable acceptance database.
DO $$
BEGIN
    IF current_database() <> 'equipment_borrowing_lab5_test' THEN
        RAISE EXCEPTION 'Refusing to remove a test trigger from database %.', current_database();
    END IF;
END $$;

DROP TRIGGER IF EXISTS lab5_fail_checkout_test_trigger ON "Borrowings";
DROP FUNCTION IF EXISTS lab5_fail_checkout_test();
