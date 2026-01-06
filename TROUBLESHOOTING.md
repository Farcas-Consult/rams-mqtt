# Troubleshooting Guide

## Gate Creation Error

If you're getting "An error occurred while creating the gate", here are steps to debug:

### 1. Check the Error Details

The error message should now include more details. If you're still getting a generic error, check:

**Via Swagger UI:**
- Look at the response body - it should contain the actual error message
- Check the HTTP status code (400 = bad request, 500 = server error)

**Via Application Logs:**
- Check the console output when running `dotnet run`
- Look for detailed error messages in the logs

### 2. Common Issues and Solutions

#### Issue: "Name is required"
**Solution:** Make sure you're sending a `name` field in the request body:
```json
{
  "name": "Gate 1",  // Required field
  "description": "Optional description",
  "isActive": true
}
```

#### Issue: "Gate with name 'X' already exists"
**Solution:** Gate names must be unique. Use a different name or delete the existing gate first.

#### Issue: Database Connection Error
**Solution:** 
- Ensure SQL Server is running
- Check connection string in `appsettings.json`
- Verify database migrations have run successfully

#### Issue: LocationId Validation
**Solution:** If providing `locationId`, ensure the location exists in the database:
```json
{
  "name": "Gate 1",
  "locationId": 1  // Must exist in StorageUnits table
}
```
Or omit `locationId` (it's optional):
```json
{
  "name": "Gate 1"
}
```

### 3. Test with Minimal Request

Try creating a gate with the minimum required fields:

```bash
curl -X POST "http://localhost:5001/api/v1/gates" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Test Gate"
  }'
```

### 4. Check Database Constraints

If the error persists, check if there are database constraint violations:
- Unique constraint on Gate.Name
- Foreign key constraint on LocationId (if provided)

### 5. View Application Logs

When running the API, check the console output for detailed error messages. The improved error handling should now show the actual exception message.

