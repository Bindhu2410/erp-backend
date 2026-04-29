# Company Setup Module

This module provides complete CRUD operations for managing companies in the ERP system using PostgreSQL stored procedures.

## Features

- **Create Company**: Add new companies with complete validation
- **Update Company**: Modify existing company information
- **Delete Company**: Remove companies (with force delete option for companies with children)
- **Get Company**: Retrieve company details by ID
- **List Companies**: Get all companies
- **Search Companies**: Search companies by various criteria
- **Company Hierarchy**: View parent-child company relationships

## API Endpoints

### Base URL: `/api/CsCompany`

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/` | Create a new company |
| PUT | `/` | Update an existing company |
| DELETE | `/{id}?forceDelete={boolean}` | Delete a company |
| GET | `/{id}` | Get company by ID |
| GET | `/` | Get all companies |
| POST | `/search` | Search companies |
| GET | `/hierarchy` | Get company hierarchy |

## Data Models

### CsCompany (Entity)
- Company ID (Primary Key, Auto-generated)
- Parent Company ID (Foreign Key, Optional)
- Legal Company Name (Required, Unique)
- Registered Address (Line 1 Required, Line 2 Optional)
- City, State, Pincode (Required)
- Contact Information (Phone, Email, Website - Optional)
- Company Logo Path (Optional)
- Base Currency (Default: INR)
- Financial Year Start/End Dates (Required)
- PAN (Required, Unique, 10 characters)
- TAN (Required, Unique, 10 characters)
- GSTIN (Required, Unique, 15 characters)
- Legal Entity Type (Required, Default: Private Limited)
- Legal Name as per PAN/TAN (Required)
- Created/Updated Timestamps

### DTOs Available
- `CsCompanyDto` - Complete company information
- `CreateCsCompanyDto` - For creating new companies
- `UpdateCsCompanyDto` - For updating existing companies
- `CsCompanySearchDto` - For searching companies
- `CsCompanyHierarchyDto` - For hierarchy view

## Validation Rules

### PAN Validation
- Must be exactly 10 characters
- Format: 5 letters + 4 digits + 1 letter (e.g., ABCDE1234F)

### GSTIN Validation
- Must be exactly 15 characters
- Format: 2 digits + 5 letters + 4 digits + 1 letter + 1 alphanumeric + Z + 1 alphanumeric

### Required Fields
- Legal Company Name
- Registered Address Line 1
- City, State, Pincode
- PAN, TAN, GSTIN
- Legal Entity Type
- Legal Name as per PAN/TAN

## Database Stored Procedures

| Stored Procedure | Purpose |
|------------------|---------|
| `sp_create_cs_company` | Create new company |
| `sp_update_cs_company` | Update existing company |
| `sp_delete_cs_company` | Delete company (with/without force) |
| `sp_get_cs_company_by_id` | Get company by ID |
| `sp_get_all_cs_companies` | Get all companies |
| `sp_search_cs_companies` | Search companies |
| `sp_get_cs_company_hierarchy` | Get company hierarchy |

## Example Usage

### Create Company
```json
POST /api/CsCompany
{
  "legalCompanyName": "ABC Private Limited",
  "parentCompanyId": null,
  "registeredAddressLine1": "123 Business Street",
  "registeredAddressLine2": "Near Tech Park",
  "city": "Mumbai",
  "state": "Maharashtra",
  "pincode": "400001",
  "phoneNumber": "+91-22-12345678",
  "emailAddress": "info@abc.com",
  "websiteUrl": "https://www.abc.com",
  "baseCurrency": "INR",
  "financialYearStartDate": "2024-04-01",
  "financialYearEndDate": "2025-03-31",
  "pan": "ABCDE1234F",
  "tan": "MUMX12345A",
  "gstin": "27ABCDE1234F1Z5",
  "legalEntityType": "Private Limited",
  "legalNameAsPerPanTan": "ABC PRIVATE LIMITED"
}
```

### Search Companies
```json
POST /api/CsCompany/search
{
  "searchTerm": "ABC",
  "parentCompanyId": null,
  "legalEntityType": "Private Limited"
}
```

## File Structure

```
CompanySetup/
├── Models/
│   └── CompanySetup/
│       └── CsCompany.cs
├── DTOs/
│   └── CompanySetup/
│       ├── CsCompanyDto.cs
│       ├── CreateCsCompanyDto.cs
│       ├── UpdateCsCompanyDto.cs
│       ├── CsCompanySearchDto.cs
│       └── CsCompanyHierarchyDto.cs
├── Services/
│   └── CompanySetup/
│       ├── ICsCompanyService.cs
│       └── CsCompanyService.cs
└── Controllers/
    └── CompanySetup/
        └── CsCompanyController.cs
```

## Dependencies

- Dapper (for database operations)
- Npgsql (PostgreSQL connector)
- Microsoft.Extensions.Logging (for logging)
- System.ComponentModel.DataAnnotations (for validation)

## Error Handling

The module includes comprehensive error handling for:
- Validation errors (400 Bad Request)
- Not found errors (404 Not Found)
- Business logic errors (400 Bad Request for constraint violations)
- Server errors (500 Internal Server Error)

All errors are logged and return structured JSON responses with appropriate HTTP status codes.
