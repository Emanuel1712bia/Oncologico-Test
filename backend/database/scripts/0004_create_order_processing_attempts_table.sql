CREATE TABLE order_processing_attempts
(
    id              UUID PRIMARY KEY,
    order_id        UUID         NOT NULL REFERENCES orders (id),
    attempt_number  INTEGER      NOT NULL CHECK (attempt_number > 0),
    started_at_utc  TIMESTAMPTZ  NOT NULL,
    finished_at_utc TIMESTAMPTZ  NOT NULL,
    success         BOOLEAN      NOT NULL,
    error_message   VARCHAR(2000)
);

CREATE INDEX ix_order_processing_attempts_order_id ON order_processing_attempts (order_id);

CREATE UNIQUE INDEX ux_order_processing_attempts_order_id_attempt_number
    ON order_processing_attempts (order_id, attempt_number);
