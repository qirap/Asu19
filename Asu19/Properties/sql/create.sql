create table users
(
    id SERIAL PRIMARY KEY,
    login varchar(32),
    password varchar(32),
    first_name varchar(32),
    last_name varchar(32),
    address varchar(32),
    role varchar(32)
);

create table cars
(
    id SERIAL PRIMARY KEY,
    brand varchar(30),
    model varchar(32)
);

create table user_car
(
    id SERIAL PRIMARY KEY,
    user_id int references users (id),
    car_id int references cars (id)
);

create table employees
(
    id SERIAL PRIMARY KEY,
    first_name varchar(32),
    last_name varchar(32),
    address varchar(32),
    year date
);

create table services
(
    id SERIAL PRIMARY KEY,
    name varchar(32),
    price decimal
);

create table employee_service
(
    id SERIAL PRIMARY KEY,
    employee_id int references employees (id),
    service_id int references services (id)
);

create table requests
(
    id SERIAL PRIMARY KEY,
    status varchar(32),
    user_id int references users (id),
    car_id int references cars (id),
    employee_id int references employees (id),
    service_id int references services (id),
    startTime timestamp,
    endTime timestamp
);