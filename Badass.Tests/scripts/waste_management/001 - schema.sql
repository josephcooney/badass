CREATE TABLE "user" (
    id serial primary key NOT NULL,
    "name" text NULL,
    is_system bool NOT NULL,
    user_name text NOT NULL,
    created timestamp with time zone not null NOT NULL,
    created_by int NOT NULL references "user"(id),
    CONSTRAINT user_name_unique UNIQUE (user_name)
);

insert into "user" (id, name, is_system, user_name, created, created_by) values (1, 'System', true, 'System', clock_timestamp(), 1);

ALTER SEQUENCE user_id_seq RESTART WITH 2;

COMMENT ON TABLE public.user IS '{"ui":false, "isSecurityPrincipal":true, "createPolicy":false}';

create table government_area (
     id serial primary key not null,
     name text not null,
     created_by int not null references "user"(id),
     created timestamp with time zone not null,
     modified_by int references "user"(id),
     modified timestamp with time zone
);

create table address_file (
     id serial primary key not null,
     government_area_id int not null references government_area(id),
     name text not null,
     mime_type text not null,
     contents bytea not null,
     created_by int references "user" (id) not null,
     created timestamp with time zone not null,
     modified_by int references "user"(id),
     modified timestamp with time zone
);

COMMENT ON TABLE public.address_file IS '{"isAttachment":true}';
COMMENT ON COLUMN public.address_file.mime_type IS '{"isContentType": true}';

create table address_file_column (
    id serial primary key not null,
    address_file_id int not null references address_file(id),
    name text,
    position int  
);

create table file_import_status (
   id serial primary key not null,
   name text not null,
   created_by int not null references "user"(id),
   created timestamp with time zone not null,
   modified_by int references "user"(id),
   modified timestamp with time zone
);

create table file_import (
    id serial primary key not null,
    address_file_id int not null references address_file(id),
    unit_number_col_id int references address_file_column(id),
    street_number_col_id int not null references address_file_column(id),
    street_name_col_id int not null references address_file_column(id),
    suburb_locale_col_id int not null references address_file_column(id),
    post_code_col_id int not null references address_file_column(id),
    file_import_status_id int not null references file_import_status(id),
    completed timestamp,
    created_by int not null references "user"(id),
    created timestamp with time zone not null,
    modified_by int references "user"(id),
    modified timestamp with time zone
);

create table address_validation_status (
    id serial primary key not null,
    name text not null,
    created_by int not null references "user"(id),
    created timestamp with time zone not null,
    modified_by int references "user"(id),
    modified timestamp with time zone        
);

create table address (
     id serial primary key not null,
     government_area_id int not null references government_area(id),
     unit_number text,
     street_number text,
     street_name text not null,
     suburb_locale text not null,
     post_code char(4) not null,
     validation_code text,
     location point,
     validation_status_id int references address_validation_status(id),
     error_message text,
     created_by int not null references "user"(id),
     created timestamp with time zone not null,
     modified_by int references "user"(id),
     modified timestamp with time zone
);



