-- public.roles definition

-- Drop table

-- DROP TABLE public.roles;

CREATE TABLE public.roles (
	roleid serial4 NOT NULL,
	rolename varchar(50) NOT NULL,
	description text NULL,
	issystemrole bool DEFAULT false NOT NULL,
	datecreated timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL,
	createdby int4 NULL,
	isactive bool DEFAULT true NOT NULL,
	CONSTRAINT roles_pkey PRIMARY KEY (roleid),
	CONSTRAINT roles_rolename_key UNIQUE (rolename)
);


-- public.roles foreign keys

ALTER TABLE public.roles ADD CONSTRAINT roles_createdby_fkey FOREIGN KEY (createdby) REFERENCES public.users(userid);

-- public.users definition

-- Drop table

-- DROP TABLE public.users;

CREATE TABLE public.users (
	userid serial4 NOT NULL,
	username varchar(50) NOT NULL,
	email varchar(100) NOT NULL,
	firstname varchar(50) NOT NULL,
	lastname varchar(50) NOT NULL,
	passwordhash varchar(255) NOT NULL,
	passwordsalt varchar(50) NOT NULL,
	phonenumber varchar(20) NULL,
	profileimageurl varchar(255) NULL,
	datecreated timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL,
	lastlogindate timestamp NULL,
	isactive bool DEFAULT true NOT NULL,
	islocked bool DEFAULT false NOT NULL,
	failedloginattempts int4 DEFAULT 0 NOT NULL,
	resetpasswordtoken varchar(100) NULL,
	resetpasswordexpiry timestamp NULL,
	preferredlanguage varchar(10) DEFAULT 'en-US'::character varying NULL,
	timezone varchar(50) DEFAULT 'UTC'::character varying NULL,
	twofactorenabled bool DEFAULT false NULL,
	twofactorkey varchar(100) NULL,
	lastpasswordchangedate timestamp NULL,
	requirepasswordchange bool DEFAULT false NULL,
	notes text NULL,
	CONSTRAINT users_email_key UNIQUE (email),
	CONSTRAINT users_pkey PRIMARY KEY (userid),
	CONSTRAINT users_username_key UNIQUE (username)
);

-- public.userroles definition

-- Drop table

-- DROP TABLE public.userroles;

CREATE TABLE public.userroles (
	id serial4 NOT NULL,
	userid int4 NOT NULL,
	roleid int4 NOT NULL,
	dateassigned timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL,
	assignedby int4 NULL,
	CONSTRAINT userroles_pkey PRIMARY KEY (id)
);


-- public.userroles foreign keys

ALTER TABLE public.userroles ADD CONSTRAINT userroles_assignedby_fkey FOREIGN KEY (assignedby) REFERENCES public.users(userid);
ALTER TABLE public.userroles ADD CONSTRAINT userroles_roleid_fkey FOREIGN KEY (roleid) REFERENCES public.roles(roleid);
ALTER TABLE public.userroles ADD CONSTRAINT userroles_userid_fkey FOREIGN KEY (userid) REFERENCES public.users(userid);

-- public.teamhierarchy definition

-- Drop table

-- DROP TABLE public.teamhierarchy;

CREATE TABLE public.teamhierarchy (
	hierarchyid serial4 NOT NULL,
	userid int4 NOT NULL,
	parent_userid int4 NULL,
	roleid int4 NOT NULL,
	region varchar(100) NULL,
	assignedby int4 NULL,
	assigned_date timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL,
	CONSTRAINT teamhierarchy_pkey PRIMARY KEY (hierarchyid)
);


-- public.teamhierarchy foreign keys

ALTER TABLE public.teamhierarchy ADD CONSTRAINT fk_th_assignedby FOREIGN KEY (assignedby) REFERENCES public.users(userid);
ALTER TABLE public.teamhierarchy ADD CONSTRAINT fk_th_manager FOREIGN KEY (parent_userid) REFERENCES public.users(userid);
ALTER TABLE public.teamhierarchy ADD CONSTRAINT fk_th_role FOREIGN KEY (roleid) REFERENCES public.roles(roleid);
ALTER TABLE public.teamhierarchy ADD CONSTRAINT fk_th_user FOREIGN KEY (userid) REFERENCES public.users(userid);

-- public.permissions definition

-- Drop table

-- DROP TABLE public.permissions;

CREATE TABLE public.permissions (
	permissionid serial4 NOT NULL,
	permissionname varchar(100) NOT NULL,
	description text NULL,
	category varchar(50) NULL,
	isactive bool DEFAULT true NOT NULL,
	CONSTRAINT permissions_permissionname_key UNIQUE (permissionname),
	CONSTRAINT permissions_pkey PRIMARY KEY (permissionid)
);
-- public.rolepermissions definition

-- Drop table

-- DROP TABLE public.rolepermissions;

CREATE TABLE public.rolepermissions (
	roleid int4 NOT NULL,
	permissionid int4 NOT NULL,
	dateassigned timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL,
	assignedby int4 NULL,
	CONSTRAINT rolepermissions_pkey PRIMARY KEY (roleid, permissionid)
);


-- public.rolepermissions foreign keys

ALTER TABLE public.rolepermissions ADD CONSTRAINT rolepermissions_assignedby_fkey FOREIGN KEY (assignedby) REFERENCES public.users(userid);
ALTER TABLE public.rolepermissions ADD CONSTRAINT rolepermissions_permissionid_fkey FOREIGN KEY (permissionid) REFERENCES public.permissions(permissionid);
ALTER TABLE public.rolepermissions ADD CONSTRAINT rolepermissions_roleid_fkey FOREIGN KEY (roleid) REFERENCES public.roles(roleid);



