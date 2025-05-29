DELETE FROM sales;
DELETE FROM movies;
DELETE FROM theaters;

INSERT INTO theaters (id, name) VALUES 
    (1, 'AMC Downtown'),
    (2, 'Regal Cinemas'),
    (3, 'Cinemark Plaza'),
    (4, 'Alamo Drafthouse'),
    (5, 'IMAX Theater'),
    (6, 'Drive-In Classic'),
    (7, 'Art House Cinema'),
    (8, 'Multiplex 20');

INSERT INTO movies (id, title) VALUES 
    (1, 'The Matrix'),
    (2, 'Inception'),
    (3, 'Parasite'),
    (4, 'Dune'),
    (5, 'Everything Everywhere All at Once'),
    (6, 'Top Gun: Maverick'),
    (7, 'The Batman'),
    (8, 'Avatar: The Way of Water'),
    (9, 'Barbie'),
    (10, 'Oppenheimer');

-- May 2024 
INSERT INTO sales (theater_id, movie_id, sale_date, amount) VALUES 
    (1, 1, '2024-05-09', 1500.00),
    (1, 2, '2024-05-09', 2000.00),
    (2, 1, '2024-05-09', 1200.00),
    (2, 2, '2024-05-09', 800.00);

INSERT INTO sales (theater_id, movie_id, sale_date, amount) VALUES 
    (1, 1, '2024-05-10', 1000.00),
    (1, 2, '2024-05-10', 1500.00),
    (2, 1, '2024-05-10', 2000.00),
    (2, 2, '2024-05-10', 2500.00);

-- January 2024 
INSERT INTO sales (theater_id, movie_id, sale_date, amount) VALUES 
    (3, 3, '2024-01-01', 3500.00),
    (3, 4, '2024-01-01', 4200.00),
    (4, 5, '2024-01-01', 2800.00),
    (4, 6, '2024-01-01', 3100.00);

-- February 2024 
INSERT INTO sales (theater_id, movie_id, sale_date, amount) VALUES 
    (5, 7, '2024-02-14', 5500.00),  
    (5, 8, '2024-02-14', 6200.00),
    (6, 1, '2024-02-15', 1800.00),
    (6, 2, '2024-02-15', 2100.00);

-- March 2024 
INSERT INTO sales (theater_id, movie_id, sale_date, amount) VALUES 
    (7, 3, '2024-03-15', 2400.00),
    (7, 4, '2024-03-15', 2700.00),
    (8, 9, '2024-03-15', 8900.00),  
    (8, 10, '2024-03-15', 9200.00);

-- April 2024 - Mixed performance
INSERT INTO sales (theater_id, movie_id, sale_date, amount) VALUES 
    (1, 5, '2024-04-01', 3300.00),
    (2, 6, '2024-04-01', 3600.00),
    (3, 7, '2024-04-01', 4100.00),
    (4, 8, '2024-04-01', 4400.00);

-- June 2024 - Summer blockbuster season
INSERT INTO sales (theater_id, movie_id, sale_date, amount) VALUES 
    (1, 9, '2024-06-15', 7500.00),
    (1, 10, '2024-06-15', 7800.00),
    (2, 9, '2024-06-15', 8100.00),
    (2, 10, '2024-06-15', 8400.00),
    (5, 9, '2024-06-15', 9500.00),  -- IMAX premium
    (5, 10, '2024-06-15', 9800.00);

-- July 2024 - Peak summer
INSERT INTO sales (theater_id, movie_id, sale_date, amount) VALUES 
    (3, 9, '2024-07-04', 10200.00),  
    (3, 10, '2024-07-04', 10500.00),
    (4, 9, '2024-07-04', 9700.00),
    (4, 10, '2024-07-04', 10000.00),
    (8, 9, '2024-07-04', 11300.00),  
    (8, 10, '2024-07-04', 11600.00);

-- August 2024 - Late summer
INSERT INTO sales (theater_id, movie_id, sale_date, amount) VALUES 
    (6, 3, '2024-08-20', 1500.00),  
    (6, 4, '2024-08-20', 1700.00),
    (7, 5, '2024-08-20', 2900.00),  
    (7, 3, '2024-08-20', 3200.00);

-- September 2024 - Fall season
INSERT INTO sales (theater_id, movie_id, sale_date, amount) VALUES 
    (1, 1, '2024-09-15', 2200.00),
    (2, 2, '2024-09-15', 2500.00),
    (3, 3, '2024-09-15', 2800.00),
    (4, 4, '2024-09-15', 3100.00);

-- October 2024 - Halloween period
INSERT INTO sales (theater_id, movie_id, sale_date, amount) VALUES 
    (5, 7, '2024-10-31', 6600.00),  
    (8, 7, '2024-10-31', 7200.00),
    (1, 7, '2024-10-31', 5400.00),
    (2, 7, '2024-10-31', 5700.00);

-- November 2024 - Thanksgiving weekend
INSERT INTO sales (theater_id, movie_id, sale_date, amount) VALUES 
    (3, 5, '2024-11-28', 8800.00),
    (4, 6, '2024-11-28', 9100.00),
    (5, 8, '2024-11-28', 10400.00),
    (8, 9, '2024-11-28', 11700.00);

-- December 2024 - Holiday season
INSERT INTO sales (theater_id, movie_id, sale_date, amount) VALUES 
    (1, 8, '2024-12-25', 9300.00),  
    (2, 8, '2024-12-25', 9600.00),
    (3, 8, '2024-12-25', 9900.00),
    (4, 8, '2024-12-25', 10200.00),
    (5, 8, '2024-12-25', 11500.00),
    (8, 8, '2024-12-25', 12800.00);

-- Edge cases and special scenarios

-- Zero sales day (theater with no sales)
-- Theater 6 has no sales on May 9, 2024

-- Very low sales amounts
INSERT INTO sales (theater_id, movie_id, sale_date, amount) VALUES 
    (7, 3, '2024-01-02', 100.00),   -- Minimal sales
    (7, 4, '2024-01-02', 150.00);

-- Very high sales amounts
INSERT INTO sales (theater_id, movie_id, sale_date, amount) VALUES 
    (5, 9, '2024-07-05', 25000.00),  -- Record-breaking day
    (5, 10, '2024-07-05', 24500.00);

-- Same movie, different theaters, same day
INSERT INTO sales (theater_id, movie_id, sale_date, amount) VALUES 
    (1, 3, '2024-03-01', 3000.00),
    (2, 3, '2024-03-01', 3500.00),
    (3, 3, '2024-03-01', 4000.00),
    (4, 3, '2024-03-01', 4500.00),
    (5, 3, '2024-03-01', 5000.00);

-- Single theater, all movies, same day
INSERT INTO sales (theater_id, movie_id, sale_date, amount) VALUES 
    (8, 1, '2024-06-01', 2000.00),
    (8, 2, '2024-06-01', 2500.00),
    (8, 3, '2024-06-01', 3000.00),
    (8, 4, '2024-06-01', 3500.00),
    (8, 5, '2024-06-01', 4000.00),
    (8, 6, '2024-06-01', 4500.00),
    (8, 7, '2024-06-01', 5000.00);

-- Fractional amounts
INSERT INTO sales (theater_id, movie_id, sale_date, amount) VALUES 
    (1, 1, '2024-02-01', 1234.56),
    (1, 2, '2024-02-01', 2345.67),
    (2, 1, '2024-02-01', 3456.78),
    (2, 2, '2024-02-01', 4567.89);