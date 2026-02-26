-- TourSearch Database Initialization Script
-- This script runs automatically when the PostgreSQL container starts for the first time

-- ============================================================
-- 1. CREATE TABLES
-- ============================================================

-- 1. Destinations table
CREATE TABLE IF NOT EXISTS destinations (
    id SERIAL PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    country VARCHAR(100) NOT NULL,
    description TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- 2. Travel styles table
CREATE TABLE IF NOT EXISTS travel_styles (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- 3. Tours table (main table)
CREATE TABLE IF NOT EXISTS tours (
    id SERIAL PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    duration_days INTEGER NOT NULL CHECK (duration_days > 0),
    base_price DECIMAL(10, 2) NOT NULL CHECK (base_price >= 0),
    start_date TIMESTAMP,
    destination_id INTEGER NOT NULL REFERENCES destinations(id),
    travel_style_id INTEGER NOT NULL REFERENCES travel_styles(id),
    description TEXT,
    itinerary TEXT,
    whats_included TEXT,
    image_url VARCHAR(500),
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- 4. Tour photos table
CREATE TABLE IF NOT EXISTS tour_photos (
    id SERIAL PRIMARY KEY,
    tour_id INTEGER NOT NULL REFERENCES tours(id) ON DELETE CASCADE,
    url VARCHAR(500) NOT NULL,
    is_main BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- 5. Users table
CREATE TABLE IF NOT EXISTS users (
    id SERIAL PRIMARY KEY,
    email VARCHAR(150) NOT NULL UNIQUE,
    password_hash VARCHAR(100) NOT NULL,
    salt VARCHAR(50) NOT NULL,
    role VARCHAR(20) NOT NULL DEFAULT 'user',
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT check_role CHECK (role IN ('user', 'admin'))
);

-- 6. Bookings table
CREATE TABLE IF NOT EXISTS bookings (
    id SERIAL PRIMARY KEY,
    tour_id INTEGER NOT NULL REFERENCES tours(id),
    user_id INTEGER REFERENCES users(id),
    customer_name VARCHAR(200) NOT NULL,
    customer_email VARCHAR(150) NOT NULL,
    persons_count INTEGER NOT NULL CHECK (persons_count > 0),
    status VARCHAR(20) NOT NULL DEFAULT 'new',
    total_price DECIMAL(10, 2),
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT check_status CHECK (status IN ('new', 'confirmed', 'cancelled', 'completed'))
);

-- 7. Password reset tokens table
CREATE TABLE IF NOT EXISTS password_reset_tokens (
    id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    token VARCHAR(500) NOT NULL UNIQUE,
    expires_at TIMESTAMP NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Indexes for faster search
CREATE INDEX IF NOT EXISTS idx_tours_destination ON tours(destination_id);
CREATE INDEX IF NOT EXISTS idx_tours_travel_style ON tours(travel_style_id);
CREATE INDEX IF NOT EXISTS idx_tours_start_date ON tours(start_date);
CREATE INDEX IF NOT EXISTS idx_bookings_tour ON bookings(tour_id);
CREATE INDEX IF NOT EXISTS idx_bookings_user ON bookings(user_id);
CREATE INDEX IF NOT EXISTS idx_tour_photos_tour ON tour_photos(tour_id);
CREATE INDEX IF NOT EXISTS idx_password_reset_tokens_token ON password_reset_tokens(token);
CREATE UNIQUE INDEX IF NOT EXISTS idx_password_reset_tokens_user_id ON password_reset_tokens(user_id);

-- ============================================================
-- 2. SEED DATA
-- ============================================================

-- === DESTINATIONS ===
INSERT INTO destinations (name, country, description) VALUES
('Bali', 'Indonesia', 'Beautiful island with temples, rice terraces and beaches'),
('Thailand', 'Thailand', 'Land of smiles with rich culture and cuisine'),
('Vietnam', 'Vietnam', 'Ancient temples, emerald bays and delicious food'),
('Japan', 'Japan', 'Unique blend of ancient traditions and modern technology'),
('Iceland', 'Iceland', 'Land of fire and ice with geysers and northern lights'),
('Peru', 'Peru', 'Home of Machu Picchu and ancient Incan civilization'),
('Morocco', 'Morocco', 'Exotic bazaars, desert adventures and imperial cities'),
('Costa Rica', 'Costa Rica', 'Biodiversity hotspot with rainforests and wildlife'),
('Greece', 'Greece', 'Ancient ruins, stunning islands and Mediterranean cuisine'),
('South Africa', 'South Africa', 'Safari adventures and diverse landscapes');

-- === TRAVEL STYLES ===
INSERT INTO travel_styles (name, description) VALUES
('Classic', 'Traditional sightseeing tours with comfortable accommodations'),
('18-to-Thirtysomethings', 'Adventures designed for young travelers'),
('Adventure', 'Active trips with hiking, rafting and outdoor activities'),
('Luxury', 'Premium experiences with top-tier accommodations'),
('Family', 'Kid-friendly tours with activities for all ages'),
('Active', 'Sports and fitness focused adventures');

-- === USERS ===
-- Password for both: Password123!
-- Hash: HwuYWVti+SGyMGupNcSJMLWUp5w+VaYstWCf6QTUdzQ= (SHA256 with salt)
-- Salt: YWJjZGVmZ2hpamtsbW5vcA== (base64 of "abcdefghijklmnop")
INSERT INTO users (email, password_hash, salt, role) VALUES
('admin@toursearch.com', 'HwuYWVti+SGyMGupNcSJMLWUp5w+VaYstWCf6QTUdzQ=', 'YWJjZGVmZ2hpamtsbW5vcA==', 'admin'),
('user@example.com', 'HwuYWVti+SGyMGupNcSJMLWUp5w+VaYstWCf6QTUdzQ=', 'YWJjZGVmZ2hpamtsbW5vcA==', 'user');

-- === TOURS ===
INSERT INTO tours (name, duration_days, base_price, start_date, destination_id, travel_style_id, description, itinerary, whats_included) VALUES
('Bali: Beaches & Boat Rides', 9, 356.00, '2026-06-29', 1, 1, 'Explore Bali beaches, visit ancient temples and enjoy boat rides',
 '[{"day":1,"title":"Arrival in Denpasar","description":"Welcome to Bali! Transfer to your hotel in Seminyak. Evening welcome dinner."},{"day":2,"title":"Ubud & Rice Terraces","description":"Visit the famous Tegallalang rice terraces and explore Ubud art market."},{"day":3,"title":"Temple Day","description":"Explore sacred temples including Tanah Lot and Uluwatu at sunset."}]',
 '["Accommodation (8 nights)","Daily breakfast","Airport transfers","Experienced local guide","Temple entrance fees"]'),

('Classic Bali', 8, 548.00, '2026-01-02', 1, 1, 'Classic Bali tour visiting Ubud, rice terraces and beaches',
 '[{"day":1,"title":"Welcome to Bali","description":"Arrive in Denpasar, transfer to Ubud. Welcome meeting at 6 PM."},{"day":2,"title":"Ubud Exploration","description":"Morning yoga, visit Monkey Forest and local art galleries."}]',
 '["Accommodation (7 nights)","Daily breakfast","All transportation","Guide services","Activity fees"]'),

('Northern Thailand: Hilltribes & Villages', 8, 488.00, '2026-01-24', 2, 2, 'Visit hill villages and experience local tribal culture',
 '[{"day":1,"title":"Bangkok Arrival","description":"Arrive in Bangkok, explore Khao San Road area."},{"day":2,"title":"Journey North","description":"Fly to Chiang Mai, visit night bazaar."}]',
 '["Accommodation (7 nights)","Daily breakfast","Domestic flight","Hill tribe village visits","Cooking class"]'),

('Vietnam: Historic Cities & Halong Bay', 12, 561.00, '2026-04-04', 3, 1, 'Historic cities of Vietnam and Halong Bay cruise',
 '[{"day":1,"title":"Hanoi Arrival","description":"Welcome to Vietnam! Transfer to hotel in Old Quarter."},{"day":2,"title":"Hanoi City Tour","description":"Visit Ho Chi Minh Mausoleum, Temple of Literature, and enjoy street food tour."}]',
 '["Accommodation (11 nights)","Halong Bay overnight cruise","All transportation","Local guides","Entrance fees"]'),

('Iceland: Fire & Ice', 8, 1350.00, '2026-06-01', 5, 3, 'Glaciers, volcanoes, geysers and northern lights',
 '[{"day":1,"title":"Reykjavik","description":"Arrive in Reykjavik, explore the colorful city center."},{"day":2,"title":"Golden Circle","description":"Visit Thingvellir National Park, Geysir hot springs, and Gullfoss waterfall."}]',
 '["Accommodation (7 nights)","Super Jeep tours","Glacier hiking equipment","Northern lights hunt","Expert guides"]'),

('Peru: Machu Picchu Adventure', 10, 899.00, '2026-03-15', 6, 3, 'Trek to Machu Picchu through the Sacred Valley',
 '[{"day":1,"title":"Lima Arrival","description":"Welcome to Peru! Transfer to hotel in Miraflores."},{"day":2,"title":"Fly to Cusco","description":"Morning flight to Cusco, acclimatization day."}]',
 '["Accommodation (9 nights)","Inca Trail permits","Professional trekking guides","Camping equipment","All meals on trek"]'),

('Morocco: Imperial Cities', 9, 675.00, '2026-02-20', 7, 1, 'Explore Marrakech, Fes and the Sahara Desert',
 '[{"day":1,"title":"Marrakech","description":"Arrive in Marrakech, explore Jemaa el-Fnaa square."},{"day":2,"title":"Medina Discovery","description":"Full day guided tour of Marrakech medina and souks."}]',
 '["Accommodation (8 nights)","Desert camp experience","All transportation","Guided medina tours","Welcome dinner"]'),

('Costa Rica Wildlife', 8, 720.00, '2026-05-10', 8, 5, 'Rainforest adventures and wildlife encounters',
 '[{"day":1,"title":"San Jose","description":"Arrive in San Jose, transfer to cloud forest lodge."},{"day":2,"title":"Monteverde","description":"Canopy zip-line tour and night wildlife walk."}]',
 '["Accommodation (7 nights)","All activities","Transportation","Naturalist guides","National park fees"]'),

('Greek Islands Hopping', 12, 1150.00, '2026-07-01', 9, 4, 'Sail through stunning Greek islands',
 '[{"day":1,"title":"Athens","description":"Arrive in Athens, welcome dinner with Acropolis views."},{"day":2,"title":"Ferry to Mykonos","description":"Morning ferry to Mykonos, explore the windmills."}]',
 '["Luxury accommodation (11 nights)","Ferry tickets","Private transfers","Gourmet meals","Wine tasting"]'),

('South Africa Safari', 10, 1450.00, '2026-08-15', 10, 3, 'Ultimate safari experience with Big Five',
 '[{"day":1,"title":"Johannesburg","description":"Arrive in Johannesburg, transfer to safari lodge."},{"day":2,"title":"First Safari","description":"Morning and evening game drives in Kruger National Park."}]',
 '["Safari lodge accommodation (9 nights)","All game drives","Expert safari guides","Park fees","Most meals"]');

-- === TOUR PHOTOS ===
INSERT INTO tour_photos (tour_id, url, is_main) VALUES
(1, 'https://images.unsplash.com/photo-1537996194471-e657df975ab4?w=800', true),
(2, 'https://images.unsplash.com/photo-1552465011-b4e21bf6e79a?w=800', true),
(3, 'https://images.unsplash.com/photo-1519451241324-20b4ea2c4220?w=800', true),
(4, 'https://images.unsplash.com/photo-1528127269322-539801943592?w=800', true),
(5, 'https://images.unsplash.com/photo-1504893524553-b855bce32c67?w=800', true),
(6, 'https://images.unsplash.com/photo-1587595431973-160d0d94add1?w=800', true),
(7, 'https://images.unsplash.com/photo-1489749798305-4fea3ae63d43?w=800', true),
(8, 'https://images.unsplash.com/photo-1518259102261-b40117eabbc9?w=800', true),
(9, 'https://images.unsplash.com/photo-1533105079780-92b9be482077?w=800', true),
(10, 'https://images.unsplash.com/photo-1516426122078-c23e76319801?w=800', true);

-- Done!
SELECT 'Database initialized successfully!' as status;
