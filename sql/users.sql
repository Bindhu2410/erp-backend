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