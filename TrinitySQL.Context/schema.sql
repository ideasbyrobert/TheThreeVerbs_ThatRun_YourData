CREATE TABLE IF NOT EXISTS theaters (
    id INTEGER PRIMARY KEY,
    name TEXT NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS movies (
    id INTEGER PRIMARY KEY,
    title TEXT NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS sales (
    id INTEGER PRIMARY KEY,
    theater_id INTEGER NOT NULL,
    movie_id INTEGER NOT NULL,
    sale_date TEXT NOT NULL, 
    amount DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (theater_id) REFERENCES theaters(id),
    FOREIGN KEY (movie_id) REFERENCES movies(id),
    UNIQUE(theater_id, movie_id, sale_date)
);

CREATE INDEX IF NOT EXISTS idx_sales_date ON sales(sale_date);
CREATE INDEX IF NOT EXISTS idx_sales_theater_date ON sales(theater_id, sale_date);