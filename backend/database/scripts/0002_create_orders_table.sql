CREATE TABLE orders
(
    id              UUID PRIMARY KEY,
    user_id         VARCHAR(200) NOT NULL,
    status          VARCHAR(20)  NOT NULL,
    created_at_utc  TIMESTAMPTZ  NOT NULL,
    updated_at_utc  TIMESTAMPTZ  NOT NULL
);

CREATE INDEX ix_orders_user_id_created_at_utc ON orders (user_id, created_at_utc DESC);

CREATE INDEX ix_orders_status_created_at_utc ON orders (created_at_utc) WHERE status = 'Pending';
