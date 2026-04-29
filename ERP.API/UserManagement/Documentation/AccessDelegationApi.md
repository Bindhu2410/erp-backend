# Access Delegation API Documentation

## Overview
The Access Delegation API provides comprehensive functionality for managing user access delegations in the ERP system. This allows users to temporarily delegate their access rights to other users, enabling smooth business operations during absences, vacations, or role transitions.

## Table of Contents
1. [API Endpoints](#api-endpoints)
2. [Data Models](#data-models)
3. [Business Rules](#business-rules)
4. [Use Cases](#use-cases)
5. [Error Handling](#error-handling)
6. [Best Practices](#best-practices)

## API Endpoints

### Base URL
```
http://localhost:5104/api/accessdelegation
```

### Authentication
All endpoints require JWT authentication. Include the token in the Authorization header:
```
Authorization: Bearer <your-jwt-token>
```

### Endpoints Summary

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/` | Create new access delegation |
| GET | `/{id}` | Get delegation by ID |
| PUT | `/{id}` | Update existing delegation |
| DELETE | `/{id}` | Delete delegation |
| GET | `/` | Get all delegations (paginated) |
| GET | `/active` | Get current active delegations |
| GET | `/from-user/{userId}` | Get delegations by delegator |
| GET | `/to-user/{userId}` | Get delegations by delegate |
| GET | `/from-user/{userId}/paged` | Get delegations by delegator (paginated) |
| GET | `/to-user/{userId}/paged` | Get delegations by delegate (paginated) |
| POST | `/search` | Search delegations with filters |
| POST | `/date-range` | Get delegations by date range |
| GET | `/user/{userId}/history` | Get user's delegation history |
| POST | `/extend` | Extend delegation end date |
| GET | `/check-active/{fromUserId}/{toUserId}` | Check if delegation exists |
| POST | `/statistics` | Get delegation statistics |
| POST | `/most-active-delegators` | Get most active delegators |
| POST | `/most-popular-delegates` | Get most popular delegates |
| POST | `/deactivate-expired` | Deactivate expired delegations |

## Data Models

### AccessDelegationDto
```csharp
public class AccessDelegationDto
{
    public int DelegationId { get; set; }
    public int FromUserId { get; set; }
    public int ToUserId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Reason { get; set; }
    public bool IsActive { get; set; }
    public int CreatedBy { get; set; }
    public DateTime DateCreated { get; set; }
}
```

### CreateAccessDelegationDto
```csharp
public class CreateAccessDelegationDto
{
    [Required]
    public int FromUserId { get; set; }
    
    [Required]
    public int ToUserId { get; set; }
    
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
    
    [StringLength(1000)]
    public string? Reason { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    [Required]
    public int CreatedBy { get; set; }
}
```

### UpdateAccessDelegationDto
```csharp
public class UpdateAccessDelegationDto
{
    [Required]
    public int DelegationId { get; set; }
    
    public int? FromUserId { get; set; }
    public int? ToUserId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    [StringLength(1000)]
    public string? Reason { get; set; }
    
    public bool? IsActive { get; set; }
}
```

### DelegationSearchDto
```csharp
public class DelegationSearchDto
{
    public int? FromUserId { get; set; }
    public int? ToUserId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? IsActive { get; set; }
    public string? SearchText { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
```

### AccessDelegationPagedDto
```csharp
public class AccessDelegationPagedDto
{
    public List<AccessDelegationDto> Delegations { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public long TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
}
```

### DelegationStatisticsDto
```csharp
public class DelegationStatisticsDto
{
    public long TotalDelegations { get; set; }
    public long ActiveDelegations { get; set; }
    public long ExpiredDelegations { get; set; }
    public long UpcomingDelegations { get; set; }
    public decimal AverageDurationDays { get; set; }
}
```

## Business Rules

### Validation Rules
1. **Date Validation**: Start date must be before end date
2. **User Validation**: Users cannot delegate to themselves
3. **Duration Limits**: Delegations cannot exceed 365 days
4. **Overlap Prevention**: Users cannot have overlapping active delegations to the same delegate
5. **Future Dates**: Delegations can be scheduled for future dates
6. **Extension Rules**: Only active, non-expired delegations can be extended

### Access Control Rules
1. **Creation Rights**: Users can create delegations for themselves or if they have admin privileges
2. **Modification Rights**: Only the delegator or admin can modify delegations
3. **Deletion Rights**: Only the delegator or admin can delete delegations
4. **View Rights**: Users can view delegations where they are either delegator or delegate

### Automatic Processing
1. **Expiration Handling**: Expired delegations are automatically deactivated
2. **Scheduled Activation**: Future delegations become active at start date
3. **Audit Trail**: All delegation activities are logged for compliance

## Use Cases

### Use Case 1: Vacation Coverage
**Scenario**: John is going on vacation and needs Sarah to handle his customer service responsibilities.

**Steps**:
1. Create delegation from John to Sarah
2. Set start/end dates for vacation period
3. Specify reason: "Vacation coverage - customer service"
4. Sarah gains access to John's customer service functions during the period
5. Delegation automatically expires after vacation

### Use Case 2: Temporary Role Assignment
**Scenario**: Manager needs to temporarily assign additional responsibilities to team member.

**Steps**:
1. Create delegation for specific role/permissions
2. Set appropriate duration
3. Monitor delegation usage
4. Extend if needed
5. Deactivate when no longer required

### Use Case 3: Emergency Coverage
**Scenario**: Employee is unexpectedly unavailable and urgent coverage is needed.

**Steps**:
1. Admin creates immediate delegation
2. Set current date as start date
3. Specify emergency reason
4. Delegate gains immediate access
5. Adjust end date as situation develops

### Use Case 4: Training and Mentoring
**Scenario**: Senior employee mentoring junior staff member.

**Steps**:
1. Create delegation for learning purposes
2. Set extended duration for training period
3. Monitor delegation usage for training progress
4. Gradually reduce scope as competency increases

## Error Handling

### Common Error Scenarios

#### 1. Validation Errors (400 Bad Request)
```json
{
  "success": false,
  "message": "Validation failed",
  "errors": [
    "Start date must be before end date",
    "Cannot delegate to yourself"
  ]
}
```

#### 2. Not Found Errors (404 Not Found)
```json
{
  "success": false,
  "message": "Access delegation not found",
  "errors": [
    "Delegation with ID 999 not found"
  ]
}
```

#### 3. Authorization Errors (401 Unauthorized)
```json
{
  "success": false,
  "message": "Unauthorized access",
  "errors": [
    "You don't have permission to access this delegation"
  ]
}
```

#### 4. Server Errors (500 Internal Server Error)
```json
{
  "success": false,
  "message": "An error occurred while processing the request",
  "errors": [
    "Database connection failed"
  ]
}
```

### Error Recovery Strategies
1. **Retry Logic**: Implement exponential backoff for transient errors
2. **Circuit Breaker**: Prevent cascading failures
3. **Fallback Mechanisms**: Provide alternative paths when primary fails
4. **Monitoring**: Track error rates and patterns
5. **Alerting**: Notify administrators of critical errors

## Best Practices

### API Usage Best Practices

#### 1. Pagination
- Always use pagination for list operations
- Limit page size to reasonable values (max 100)
- Use cursor-based pagination for large datasets

#### 2. Filtering and Searching
- Use specific filters to reduce data transfer
- Implement server-side filtering for better performance
- Use search parameters effectively

#### 3. Date Handling
- Always use UTC dates for API communication
- Include timezone information when relevant
- Validate date ranges on both client and server

#### 4. Error Handling
- Implement proper error handling on client side
- Log errors appropriately for debugging
- Provide meaningful error messages to users

#### 5. Performance Optimization
- Use appropriate HTTP methods (GET, POST, PUT, DELETE)
- Implement caching where appropriate
- Minimize unnecessary API calls

### Security Best Practices

#### 1. Authentication
- Always validate JWT tokens
- Implement token refresh mechanisms
- Use secure token storage

#### 2. Authorization
- Implement proper role-based access control
- Validate permissions for each operation
- Log authorization attempts

#### 3. Data Protection
- Encrypt sensitive data in transit and at rest
- Implement proper audit logging
- Follow data retention policies

#### 4. Input Validation
- Validate all input parameters
- Sanitize user input
- Implement rate limiting

### Database Best Practices

#### 1. Stored Procedures
- Use parameterized queries to prevent SQL injection
- Implement proper error handling in stored procedures
- Optimize query performance

#### 2. Indexing
- Create appropriate indexes for frequently queried columns
- Monitor query performance
- Regular index maintenance

#### 3. Backup and Recovery
- Implement regular backup procedures
- Test recovery procedures
- Monitor database health

### Monitoring and Logging

#### 1. Application Logging
- Log all API requests and responses
- Include correlation IDs for tracing
- Log errors with sufficient context

#### 2. Performance Monitoring
- Track API response times
- Monitor resource usage
- Set up alerts for performance degradation

#### 3. Business Metrics
- Track delegation creation rates
- Monitor usage patterns
- Analyze delegation effectiveness

### Testing Strategies

#### 1. Unit Testing
- Test individual components in isolation
- Mock external dependencies
- Achieve high code coverage

#### 2. Integration Testing
- Test API endpoints end-to-end
- Validate database interactions
- Test error scenarios

#### 3. Load Testing
- Test system under expected load
- Identify performance bottlenecks
- Validate scalability

#### 4. Security Testing
- Test authentication and authorization
- Validate input sanitization
- Test for common security vulnerabilities

## Example Workflows

### Workflow 1: Complete Delegation Lifecycle
```bash
# 1. Create delegation
curl -X POST "http://localhost:5104/api/accessdelegation" \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer $TOKEN" \
     -d '{
       "fromUserId": 123,
       "toUserId": 456,
       "startDate": "2024-02-01T08:00:00Z",
       "endDate": "2024-02-14T17:00:00Z",
       "reason": "Annual vacation coverage",
       "isActive": true,
       "createdBy": 123
     }'

# 2. Check delegation status
curl -X GET "http://localhost:5104/api/accessdelegation/check-active/123/456" \
     -H "Authorization: Bearer $TOKEN"

# 3. Extend delegation if needed
curl -X POST "http://localhost:5104/api/accessdelegation/extend" \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer $TOKEN" \
     -d '{
       "delegationId": 789,
       "newEndDate": "2024-02-21T17:00:00Z"
     }'

# 4. View delegation history
curl -X GET "http://localhost:5104/api/accessdelegation/user/123/history" \
     -H "Authorization: Bearer $TOKEN"
```

### Workflow 2: Delegation Management Dashboard
```bash
# 1. Get current active delegations
curl -X GET "http://localhost:5104/api/accessdelegation/active?pageNumber=1&pageSize=20" \
     -H "Authorization: Bearer $TOKEN"

# 2. Get delegation statistics
curl -X POST "http://localhost:5104/api/accessdelegation/statistics" \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer $TOKEN" \
     -d '{
       "startDate": "2024-01-01T00:00:00Z",
       "endDate": "2024-12-31T23:59:59Z"
     }'

# 3. Get most active delegators
curl -X POST "http://localhost:5104/api/accessdelegation/most-active-delegators" \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer $TOKEN" \
     -d '{
       "limit": 10,
       "startDate": "2024-01-01T00:00:00Z",
       "endDate": "2024-12-31T23:59:59Z"
     }'

# 4. Cleanup expired delegations
curl -X POST "http://localhost:5104/api/accessdelegation/deactivate-expired" \
     -H "Authorization: Bearer $TOKEN"
```

## Conclusion

The Access Delegation API provides a comprehensive solution for managing user access delegations in the ERP system. By following the documented endpoints, data models, and best practices, developers can integrate delegation functionality into their applications effectively.

For additional support or questions, please refer to the main API documentation or contact the development team.
