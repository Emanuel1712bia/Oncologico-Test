CREATE TABLE products
(
    id          UUID PRIMARY KEY,
    name        VARCHAR(200)   NOT NULL,
    description VARCHAR(1000)  NOT NULL DEFAULT '',
    indication  VARCHAR(300)   NOT NULL DEFAULT '',
    price       NUMERIC(12, 2) NOT NULL CHECK (price >= 0),
    is_active   BOOLEAN        NOT NULL DEFAULT TRUE
);
