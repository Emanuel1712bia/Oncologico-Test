CREATE TABLE order_items
(
    id           UUID PRIMARY KEY,
    order_id     UUID           NOT NULL REFERENCES orders (id),
    product_id   UUID           NOT NULL,
    product_name VARCHAR(200)   NOT NULL,
    unit_price   NUMERIC(12, 2) NOT NULL CHECK (unit_price >= 0),
    quantity     INTEGER        NOT NULL CHECK (quantity > 0)
);

CREATE INDEX ix_order_items_order_id ON order_items (order_id);
