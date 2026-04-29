CREATE SEQUENCE IF NOT EXISTS public.employees_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

CREATE TABLE public.employees (
    id SERIAL PRIMARY KEY,
    user_created int4 NULL,
	date_created timestamp NULL,
	user_updated int4 NULL,
	date_updated timestamp null,
    -- General Info
	employee_id varchar(100) NOT NULL DEFAULT ('EMP-' || LPAD(nextval('employee_id_seq')::text, 3, '0')),
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100),
    fathers_name VARCHAR(100),
    date_of_birth DATE,
    disability varchar(255),
    identification TEXT,
    blood_group VARCHAR(10),
    height NUMERIC(5,2),
    weight NUMERIC(5,2),
    type_of_employment VARCHAR(255),
    salary_debit_acc VARCHAR(255),
    salary_credit_acc VARCHAR(255),
    image_url varchar(255),
    sales_man VARCHAR(255),

    -- Salary Details
    salary NUMERIC(12,2),
    basic_salary NUMERIC(12,2),
    hra NUMERIC(12,2),
    conveyance NUMERIC(12,2),
    city_compn NUMERIC(12,2),
    esi_app BOOLEAN,
    esi_num VARCHAR(50),
    esi_dt DATE,
    esi_per NUMERIC(8,2),
    pf_app BOOLEAN,
    pf_num VARCHAR(50),
    pf_dt DATE,
    pf_per NUMERIC(8,2),
    tds NUMERIC(12,2),
    eff_from DATE,
    eff_to DATE,
    active BOOLEAN DEFAULT TRUE,

    -- Assignment Details
    date_of_joining DATE,
    department_id int references departments(id),
    designation int references designation(id),
    last_working_date DATE,
    rejoinee_date DATE,
    employee_grade VARCHAR(50),
    report_manager VARCHAR(100),
    report_manager_code VARCHAR(50),
    reporting_head_mail VARCHAR(200),
    notice_period INTEGER,
    cost_center VARCHAR(50),
    id_card_no VARCHAR(50),
    country VARCHAR(100),
    city VARCHAR(100),

    -- Personal Details
    birth_place VARCHAR(100),
    religion VARCHAR(50),
    home_state VARCHAR(100),
    nationality VARCHAR(100),
    country_birth VARCHAR(100),
    is_ex_service BOOLEAN,
    nominee VARCHAR(100),
    nominee_relationship VARCHAR(50),
    recruiter_name VARCHAR(100),
    reference VARCHAR(150),

    -- Language Known
    language_known varchar(100),

    -- Additional Information
    passport_no VARCHAR(50),
    name_as_per_passport VARCHAR(150),
    passport_expiry_date DATE,
    passport_issue_place VARCHAR(100),
    passport_issue_date DATE,
    mothers_maiden_name VARCHAR(150),
    old_passport_no VARCHAR(50),
    insurance_name VARCHAR(100),
    insurance_no VARCHAR(50),
    bank_name VARCHAR(100),
    branch_name VARCHAR(100),
    bank_ac_no VARCHAR(100),
    ifsc_code VARCHAR(50),
    pan_no VARCHAR(50),
    esi_no VARCHAR(50),
    esi_eff_date DATE,
    pf_no VARCHAR(50),
    pf_eff_date DATE,
    voter_id VARCHAR(50),
    driving_license_no VARCHAR(50),
    aadhar_no VARCHAR(20),

    -- Contact Details: Permanent
    perm_address varchar(255),
    perm_city VARCHAR(100),
    perm_state VARCHAR(100),
    perm_telephone VARCHAR(20),
    perm_email VARCHAR(150),
    perm_contact_person VARCHAR(100),
    perm_pincode VARCHAR(10),
    perm_country VARCHAR(100),
    perm_mobile VARCHAR(50),

    -- Communication Contact
    comm_address varchar(255),
    comm_city VARCHAR(100),
    comm_state VARCHAR(100),
    comm_telephone VARCHAR(20),
    comm_email VARCHAR(150),
    comm_contact_person VARCHAR(100),
    comm_pincode VARCHAR(10),
    comm_country VARCHAR(100),
    comm_mobile int,

    -- Family Details
    family_name VARCHAR(100),
    family_age INTEGER,
    family_relationship VARCHAR(50),
    family_occupation VARCHAR(100),
    family_primary_contact VARCHAR(20),
    family_contact int,
    family_email VARCHAR(100),

    -- Education Skills
    edu_course VARCHAR(150),
    edu_board VARCHAR(150),
    edu_institution VARCHAR(150),
    edu_pass_date VARCHAR(20),
    edu_percentage NUMERIC(8,2),

    -- Special achievements
    achievement_what TEXT,
    achievement_when DATE,
    achievement_where VARCHAR(100),
    achievement_remarks TEXT,

    -- Previous Employment
    prev_company_name VARCHAR(150),
    prev_last_designation VARCHAR(150),
    prev_relevant_exp_year INTEGER,
    prev_relevant_exp_month INTEGER,
    prev_ppf_no VARCHAR(50),
    prev_pesi_no VARCHAR(50),
    prev_start_date DATE,
    prev_end_date DATE,

    -- Allowance Details
    allow_eff_from DATE,
    allow_eff_to DATE,
    allowance_type VARCHAR(100),
    allowance_amount NUMERIC(12,2)
);

ALTER TABLE public.employees ADD CONSTRAINT employees_user_created_fkey 
    FOREIGN KEY (user_created) REFERENCES public.users(userid);

ALTER TABLE public.employees ADD CONSTRAINT employees_user_updated_fkey 
    FOREIGN KEY (user_updated) REFERENCES public.users(userid);

   ALTER TABLE departments
ADD CONSTRAINT departments_pkey PRIMARY KEY (id);
ALTER TABLE designation
ADD CONSTRAINT designation_pkey PRIMARY KEY (id);