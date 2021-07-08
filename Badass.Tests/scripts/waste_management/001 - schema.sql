CREATE TABLE "user" (
                        id serial primary key NOT NULL,
                        "name" text NULL,
                        is_system bool NOT NULL,
                        user_name text NOT NULL,
                        created timestamp with time zone not null NOT NULL,
                        created_by int NOT NULL references "user"(id),
                        CONSTRAINT user_name_unique UNIQUE (user_name)
);

insert into "user" (id, name, is_system, user_name, created, created_by) 
values (1, 'System', true, 'System', clock_timestamp(), 1);

ALTER SEQUENCE user_id_seq RESTART WITH 2;

COMMENT ON TABLE public.user IS '{"noAddUI":true, "noEditUI":true, "isSecurityPrincipal":true, "createPolicy":false}';

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

COMMENT ON TABLE public.address_file IS '{"isAttachment":true, "apiHooks":"modify", "apiConstructor":"none"}';
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

insert into file_import_status(id, name, created_by, created)
values (1, 'Created', 1, clock_timestamp());

insert into file_import_status(id, name, created_by, created)
values (2, 'In Progress', 1, clock_timestamp());

insert into file_import_status(id, name, created_by, created)
values (3, 'Completed', 1, clock_timestamp());

insert into file_import_status(id, name, created_by, created)
values (4, 'Error', 1, clock_timestamp());

ALTER SEQUENCE file_import_status_id_seq RESTART WITH 5; -- users probably shouldn't be adding values here anyway, but...

create table file_import (
                             id serial primary key not null,
                             address_file_id int not null references address_file(id),
                             unit_number_col_id int references address_file_column(id),
                             street_number_col_id int not null references address_file_column(id),
                             street_name_col_id int not null references address_file_column(id),
                             suburb_locale_col_id int not null references address_file_column(id),
                             post_code_col_id int not null references address_file_column(id),
                             file_import_status_id int not null references file_import_status(id) default 1, -- default to "created"
                             completed timestamp,
                             created_by int not null references "user"(id),
                             created timestamp with time zone not null,
                             modified_by int references "user"(id),
                             modified timestamp with time zone
);

COMMENT ON COLUMN public.file_import.file_import_status_id IS '{"add": false, "edit":false}';
COMMENT ON COLUMN public.file_import.completed IS '{"add": false, "edit":false}';
COMMENT ON TABLE public.file_import IS '{"apiHooks":"modify", "apiConstructor":"none"}';

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
                         latitude float,
                         longitude float,
                         validation_status_id int references address_validation_status(id),
                         error_message text,
                         search_content tsvector,
                         created_by int not null references "user"(id),
                         created timestamp with time zone not null,
                         modified_by int references "user"(id),
                         modified timestamp with time zone
);

create table waste_type (
                            id serial primary key not null,
                            name text not null,
                            created_by int not null references "user"(id),
                            created timestamp with time zone not null,
                            modified_by int references "user"(id),
                            modified timestamp with time zone
);

create table bin_size (
                          id serial primary key not null,
                          name text not null,
                          created_by int not null references "user"(id),
                          created timestamp with time zone not null,
                          modified_by int references "user"(id),
                          modified timestamp with time zone
);

create table service_type (
                              id serial primary key not null,
                              name text not null,
                              created_by int not null references "user"(id),
                              created timestamp with time zone not null,
                              modified_by int references "user"(id),
                              modified timestamp with time zone
);

create table bin (
                     id serial primary key not null,
                     address_id int references address(id),
                     waste_type_id int not null references waste_type(id),
                     bin_size_id int not null references bin_size(id),
                     service_type_id int not null references service_type(id),
                     created_by int not null references "user"(id),
                     created timestamp with time zone not null,
                     modified_by int references "user"(id),
                     modified timestamp with time zone
);

DO
$$
    BEGIN
        IF NOT EXISTS (
                SELECT
                FROM
                    pg_catalog.pg_roles
                WHERE
                        rolname = 'web_app_role') THEN

            CREATE ROLE web_app_role WITH
                NOLOGIN
                NOSUPERUSER
                NOCREATEDB
                NOCREATEROLE
                INHERIT
                NOREPLICATION
                CONNECTION LIMIT -1;
        END IF;
    END
$$;

GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO web_app_role;

DO
$$
    declare user_pwd text;
    BEGIN
        user_pwd = MD5(random()::text);

        raise notice 'bin_web_user password set to %', user_pwd;

        DROP ROLE if EXISTS bin_web_user;
        EXECUTE format('CREATE USER bin_web_user PASSWORD %L', user_pwd);

    END
$$;

GRANT web_app_role TO bin_web_user;

grant usage, select on user_id_seq to web_app_role;
grant usage, select on government_area_id_seq to web_app_role;
grant usage, select on address_file_id_seq to web_app_role;
grant usage, select on address_file_column_id_seq to web_app_role;
grant usage, select on file_import_status_id_seq to web_app_role;
grant usage, select on file_import_id_seq to web_app_role;
grant usage, select on address_validation_status_id_seq to web_app_role;
grant usage, select on address_id_seq to web_app_role;
grant usage, select on waste_type_id_seq to web_app_role;
grant usage, select on bin_size_id_seq to web_app_role;
grant usage, select on service_type_id_seq to web_app_role;
grant usage, select on bin_id_seq to web_app_role;

SET search_path TO "public";

drop function if exists user_select_current;

CREATE OR REPLACE FUNCTION user_select_current (
    security_user_id_param integer
)
    RETURNS SETOF "user" AS
$$

BEGIN

    RETURN QUERY
        SELECT
            "user".id,
            "user"."name",
            "user".is_system,
            "user".user_name,
            "user".created,
            "user".created_by
        FROM "user"
        WHERE
                "user".id = security_user_id_param ;

END
$$
    LANGUAGE plpgsql VOLATILE SECURITY INVOKER
                     COST 100;

REVOKE ALL ON FUNCTION user_select_current (integer ) FROM public;

GRANT EXECUTE ON FUNCTION user_select_current ( integer ) TO web_app_role;

COMMENT ON FUNCTION user_select_current ( integer )
    IS '{"applicationtype":"user", "generated":false, "fullName":"user_select_current" , "single_result":true }';


SET search_path TO "public";

drop function if exists user_select_by_login;

CREATE OR REPLACE FUNCTION user_select_by_login (
    "name" text,
    user_name text
)
    RETURNS int AS
$$
DECLARE user_id int;

BEGIN

    select id into user_id from "user"
    where "user".user_name = user_select_by_login.user_name;

    IF (user_id is not null) THEN
        return user_id;
    END IF;

    user_id = nextval(pg_get_serial_sequence('user', 'id'));

    insert into "user" (id, "name", is_system, user_name, created, created_by)
    values (user_id, user_select_by_login.name, false, user_select_by_login.user_name, clock_timestamp(), user_id);

    return user_id;

END
$$
    LANGUAGE plpgsql VOLATILE SECURITY INVOKER
                     COST 100;

REVOKE ALL ON FUNCTION user_select_by_login (text, text) FROM public;

GRANT EXECUTE ON FUNCTION user_select_by_login (text, text) TO web_app_role;

COMMENT ON FUNCTION user_select_by_login ( text, text)
    IS '{"applicationtype":"user", "generated":false, "api":false }';

CREATE SCHEMA IF NOT EXISTS hangfire AUTHORIZATION web_app_role;




