-- Initialize database with DDL
-- Create database schema for Loccar application

-- public.customer definição
CREATE TABLE public.customer (
    id_customer serial4 NOT NULL,
    "name" varchar(100) NOT NULL,
    email varchar(255) NULL,
    phone varchar(50) NULL,
    driver_license varchar(11) NULL,
    created timestamp NULL,
    CONSTRAINT customer_pkey PRIMARY KEY (id_customer)
);

-- public.vehicle definição
CREATE TABLE public.vehicle (
    id_vehicle serial4 NOT NULL,
    brand varchar(50) NULL,
    model varchar(50) NULL,
    manufacturing_year int4 NULL,
    model_year int4 NULL,
    vin varchar(11) NULL,
    fuel_tank_capacity int4 NULL,
    daily_rate numeric(10, 2) NULL,
    reduced_daily_rate numeric(10, 2) NULL,
    monthly_rate numeric(10, 2) NULL,
    company_daily_rate numeric(10, 2) NULL,
    reserved bool NULL,
    CONSTRAINT vehicle_pkey PRIMARY KEY (id_vehicle),
    CONSTRAINT vehicle_vin_key UNIQUE (vin)
);

-- public.cargo_vehicle definição
CREATE TABLE public.cargo_vehicle (
    id_vehicle int4 NOT NULL,
    cargo_capacity numeric(10, 2) NULL,
    cargo_type varchar(50) NULL,
    tare_weight numeric(10, 2) NULL,
    cargo_compartment_size varchar(100) NULL,
    CONSTRAINT cargo_vehicle_pkey PRIMARY KEY (id_vehicle),
    CONSTRAINT cargo_vehicle_id_vehicle_fkey FOREIGN KEY (id_vehicle) REFERENCES public.vehicle(id_vehicle)
);

-- public.leisure_vehicle definição
CREATE TABLE public.leisure_vehicle (
    id_vehicle int4 NOT NULL,
    "automatic" bool NULL,
    power_steering bool NULL,
    air_conditioning bool NULL,
    category varchar(50) NULL,
    CONSTRAINT leisure_vehicle_pkey PRIMARY KEY (id_vehicle),
    CONSTRAINT leisure_vehicle_id_vehicle_fkey FOREIGN KEY (id_vehicle) REFERENCES public.vehicle(id_vehicle)
);

-- public.motorcycle definição
CREATE TABLE public.motorcycle (
    id_vehicle int4 NOT NULL,
    traction_control bool NULL,
    abs_brakes bool NULL,
    cruise_control bool NULL,
    CONSTRAINT motorcycle_pkey PRIMARY KEY (id_vehicle),
    CONSTRAINT motorcycle_id_vehicle_fkey FOREIGN KEY (id_vehicle) REFERENCES public.vehicle(id_vehicle)
);

-- public.passenger_vehicle definição
CREATE TABLE public.passenger_vehicle (
    id_vehicle int4 NOT NULL,
    passenger_capacity int4 NULL,
    tv bool NULL,
    air_conditioning bool NULL,
    power_steering bool NULL,
    CONSTRAINT passenger_vehicle_pkey PRIMARY KEY (id_vehicle),
    CONSTRAINT passenger_vehicle_id_vehicle_fkey FOREIGN KEY (id_vehicle) REFERENCES public.vehicle(id_vehicle)
);

-- public.reservation definição
CREATE TABLE public.reservation (
    reservationnumber serial4 NOT NULL,
    id_customer int4 NOT NULL,
    id_vehicle int4 NOT NULL,
    rental_date timestamp NOT NULL,
    return_date timestamp NOT NULL,
    rental_days int4 NULL,
    daily_rate numeric(10, 2) NULL,
    rate_type varchar(20) NULL,
    insurance_vehicle numeric(10, 2) NULL,
    insurance_third_party numeric(10, 2) NULL,
    tax_amount numeric(10, 2) NULL,
    CONSTRAINT reservation_pkey PRIMARY KEY (reservationnumber),
    CONSTRAINT reservation_id_customer_fkey FOREIGN KEY (id_customer) REFERENCES public.customer(id_customer),
    CONSTRAINT reservation_id_vehicle_fkey FOREIGN KEY (id_vehicle) REFERENCES public.vehicle(id_vehicle)
);

-- Create indexes
CREATE INDEX ix_reservation_id_customer ON public.reservation USING btree (id_customer);
CREATE INDEX ix_reservation_id_vehicle ON public.reservation USING btree (id_vehicle);

CREATE TABLE public.roles (
	id serial4 NOT NULL,
	"name" varchar(50) NOT NULL,
	description text NULL,
	CONSTRAINT roles_name_key UNIQUE (name),
	CONSTRAINT roles_pkey PRIMARY KEY (id)
);


-- public.users definição

-- Drop table

-- DROP TABLE public.users;

CREATE TABLE public.users (
	id serial4 NOT NULL,
	username varchar(100) NOT NULL,
	email varchar(255) NOT NULL,
	password_hash text NOT NULL,
	is_active bool DEFAULT true NULL,
	created_at timestamp DEFAULT now() NULL,
	updated_at timestamp DEFAULT now() NULL,
	CONSTRAINT users_email_key UNIQUE (email),
	CONSTRAINT users_pkey PRIMARY KEY (id)
);


-- public.user_roles definição

-- Drop table

-- DROP TABLE public.user_roles;

CREATE TABLE public.user_roles (
	user_id int4 NOT NULL,
	role_id int4 NOT NULL,
	CONSTRAINT user_roles_pkey PRIMARY KEY (user_id, role_id),
	CONSTRAINT user_roles_role_id_fkey FOREIGN KEY (role_id) REFERENCES public.roles(id) ON DELETE CASCADE,
	CONSTRAINT user_roles_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE
);