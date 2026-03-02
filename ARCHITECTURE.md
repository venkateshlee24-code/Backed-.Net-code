# HRMS/ERP API Architecture

## Layered Design
- `Api` layer: HTTP endpoints, request/response mapping, status code handling.
- `Application` layer: business services and use-case rules (validation, policy decisions).
- `Infrastructure` layer: ADO.NET repositories and SQL Server connection handling.
- `Domain` layer: core business models with no framework dependencies.
- `Database/Scripts` layer: schema scripts and indexes optimized for query paths.

## Request Flow
1. API endpoint receives request.
2. Endpoint calls Application service.
3. Application service enforces business rules.
4. Repository executes async ADO.NET SQL calls.
5. Response is returned from service to API.

## Performance-Focused Decisions
- Async all the way for I/O paths (`OpenAsync`, `ExecuteReaderAsync`, `ExecuteNonQueryAsync`).
- SQL parameterization to avoid SQL injection and support plan reuse.
- Output caching for read-heavy GET endpoints.
- Pagination by default (`OFFSET/FETCH`) to avoid large payload scans.
- Database indexes for HRMS lookup patterns (`EmployeeCode`, `Email`, `DepartmentCode`, `IsActive`).
- Clear separation of read/write responsibilities so high-traffic endpoints can be optimized independently.

## Domain Starting Point
- `Employees` module implemented as template for future HRMS/ERP modules:
  - Payroll
  - Attendance
  - Leave
  - Departments
  - Inventory
  - Procurement

## Database Structure
- Database name: `MyDatabase`
- SQL Server instance: `localhost\SQLEXPRESS`
- Script execution order:
  1. `Database/Scripts/001_CreateEmployees.sql`
  2. `Database/Scripts/002_CreateAuthAndRbac.sql`
  3. `Database/Scripts/003_SeedAdminUser.sql`
  4. `Database/Scripts/004_CreateLedgerAccounts.sql`
  5. `Database/Scripts/005_CreateVouchers.sql`
  6. `Database/Scripts/006_CreateSalesInvoices.sql`
  7. `Database/Scripts/007_CreatePurchaseInvoices.sql`
  8. `Database/Scripts/008_CreatePayments.sql`
  9. `Database/Scripts/009_CreateDocumentSequences.sql`
  10. `Database/Scripts/010_SeedDefaultCoa.sql`

### Tables
- `dbo.Employees`
- `dbo.Users`
- `dbo.Roles`
- `dbo.Modules`
- `dbo.UserRoleAssignments`
- `dbo.RefreshTokens`

### Table Purpose
- `dbo.Employees`: HR employee master data.
- `dbo.Users`: login users for ERP/API authentication.
- `dbo.Roles`: role master (`INITIATOR`, `AUTHORISER`).
- `dbo.Modules`: module master (`AP`, `AR`).
- `dbo.UserRoleAssignments`: maps users to roles and modules.
- `dbo.RefreshTokens`: stores hashed refresh tokens for logout/session revocation.

### Primary Keys
- `Employees.Id` (INT IDENTITY)
- `Users.Id` (INT IDENTITY)
- `Roles.Id` (INT IDENTITY)
- `Modules.Id` (INT IDENTITY)
- `UserRoleAssignments.Id` (BIGINT IDENTITY)
- `RefreshTokens.Id` (BIGINT IDENTITY)

### Foreign Keys
- `UserRoleAssignments.UserId -> Users.Id`
- `UserRoleAssignments.RoleId -> Roles.Id`
- `UserRoleAssignments.ModuleId -> Modules.Id`
- `RefreshTokens.UserId -> Users.Id`

### Unique Indexes
- `UX_Employees_EmployeeCode` on `Employees(EmployeeCode)`
- `UX_Employees_Email` on `Employees(Email)`
- `UX_Users_UserCode` on `Users(UserCode)`
- `UX_Users_Email` on `Users(Email)`
- `UX_Roles_RoleCode` on `Roles(RoleCode)`
- `UX_Modules_ModuleCode` on `Modules(ModuleCode)`
- `UX_UserRoleAssignments_UserRoleModule` on `UserRoleAssignments(UserId, RoleId, ModuleId)`
- `UX_RefreshTokens_TokenHash` on `RefreshTokens(TokenHash)`

### Performance Indexes
- `IX_Employees_DepartmentCode_IsActive` on `Employees(DepartmentCode, IsActive)`
- `IX_RefreshTokens_UserId_RevokedAtUtc_ExpiresAtUtc` on `RefreshTokens(UserId, RevokedAtUtc, ExpiresAtUtc)`

### Seed Data
- Roles seeded: `INITIATOR`, `AUTHORISER`
- Modules seeded: `AP` (Accounts Payable), `AR` (Accounts Receivable)

## API Flow Reference

### Login Flow (JWT)
- Endpoint: `POST /api/v1/auth/login`
- Request body:
```json
{
  "email": "admin@company.com",
  "password": "Admin@123"
}
```
- Processing steps:
  1. `AuthController` receives login request.
  2. `AuthService` validates required fields.
  3. `AuthRepository` loads user from `dbo.Users` by email.
  4. `BcryptPasswordHasher` verifies password with `PasswordHash`.
  5. `AuthRepository` loads active user roles from `dbo.UserRoleAssignments` + `dbo.Roles`.
  6. `JwtTokenService` creates `AccessToken` with claims (`userId`, `email`, `roles`).
  7. Refresh token is generated, hashed (`SHA256`), and stored in `dbo.RefreshTokens`.
  8. API returns token response (`accessToken`, `expiresAtUtc`, `refreshToken`).

- Response sample:
```json
{
  "accessToken": "<jwt-token>",
  "expiresAtUtc": "2026-02-08T07:30:00Z",
  "refreshToken": "<refresh-token>",
  "tokenType": "Bearer"
}
```

### Refresh Flow (Token Rotation)
- Endpoint: `POST /api/v1/auth/refresh`
- Request body:
```json
{
  "refreshToken": "<refresh-token>"
}
```
- Processing steps:
  1. `AuthController` accepts refresh token.
  2. `AuthService` hashes token (`SHA256`) and validates active token from `dbo.RefreshTokens`.
  3. Old refresh token is revoked (`RevokedAtUtc` set).
  4. New access token + refresh token are generated.
  5. New refresh token hash is stored in `dbo.RefreshTokens`.
  6. API returns new token pair.

### Logout Flow
- Endpoint: `POST /api/v1/auth/logout` (requires Bearer token)
- Request body:
```json
{
  "refreshToken": "<refresh-token>"
}
```
- Processing steps:
  1. API reads `userId` from JWT claim.
  2. Refresh token is hashed (`SHA256`).
  3. `AuthRepository` sets `RevokedAtUtc` in `dbo.RefreshTokens`.
  4. API returns `204 No Content` on success.

### Password Hardening Flow
- Change own password:
  - Endpoint: `POST /api/v1/auth/change-password` (requires Bearer token)
  - Request:
```json
{
  "currentPassword": "Admin@123",
  "newPassword": "Admin@12345"
}
```
  - Behavior:
  1. API reads `userId` from JWT.
  2. Current password verified using BCrypt hash from `dbo.Users`.
  3. New password is hashed and updated in `dbo.Users.PasswordHash`.

- Admin reset password:
  - Endpoint: `POST /api/v1/auth/reset-password`
  - Authorization: `CanAuthoriseAp` policy
  - Request:
```json
{
  "userId": 1,
  "newPassword": "Temp@1234"
}
```
  - Behavior:
  1. Requires permission claim `AP:AUTHORISER`.
  2. New password hash is written to `dbo.Users.PasswordHash`.

### Create Employee Flow
- Endpoint: `POST /api/v1/employees`
- Request body:
```json
{
  "employeeCode": "EMP-1001",
  "fullName": "Arun Kumar",
  "email": "arun.kumar@company.com",
  "departmentCode": "FIN",
  "joiningDate": "2026-02-08"
}
```
- Processing steps:
  1. `EmployeesController` receives request.
  2. `EmployeeService` validates required fields and input rules.
  3. `EmployeeService` checks duplicate email via repository.
  4. `EmployeeRepository` inserts row into `dbo.Employees`.
  5. New employee id is returned from SQL (`SCOPE_IDENTITY()`).
  6. API returns `201 Created` with new id.

- Response sample:
```json
{
  "id": 1
}
```

### Operational Sequence (for usage)
1. Login first using `/api/v1/auth/login`.
2. Copy `accessToken` into Swagger `Authorize` (`Bearer <token>`).
3. Call employee APIs (create/list/update/deactivate).

## Authorization Policies (Step 1)
- Permission claim type in JWT: `permission`
- Permission claim values:
  - `AP:INITIATOR`
  - `AP:AUTHORISER`
  - `AR:INITIATOR`
  - `AR:AUTHORISER`
- Policies configured:
  - `CanInitiateAp`
  - `CanAuthoriseAp`
  - `CanInitiateAr`
  - `CanAuthoriseAr`

### Current policy usage
- `POST /api/v1/auth/reset-password` requires `CanAuthoriseAp`.

## ERP Accounting Architecture (Next Phase)

### Scope
- Accounts Receivable (AR): customer invoices, receipt entries.
- Accounts Payable (AP): vendor invoices, payments.
- General Ledger (GL): journal entries, debit/credit balancing, voucher posting.
- Voucher engine: shared posting document for all financial transactions.

### Core Accounting Principles
- Every posted transaction must be double-entry (`TotalDebit == TotalCredit`).
- Draft documents can be edited; posted documents are immutable.
- Reversal is done through reversal vouchers, not direct edits.
- All financial posting uses UTC timestamps and audit fields.
- Posting must be transactional (header + lines + journal entries saved atomically).

### Domain Model (Accounting)
- `LedgerAccount`
  - `AccountId`, `AccountCode`, `AccountName`, `AccountType` (Asset/Liability/Equity/Revenue/Expense), `IsActive`
- `Voucher`
  - `VoucherId`, `VoucherNo`, `VoucherType` (JV/PV/RV/SI/PI), `VoucherDate`, `Status` (Draft/Posted/Cancelled), `Narration`, `CompanyId`
- `VoucherLine`
  - `VoucherLineId`, `VoucherId`, `AccountId`, `DebitAmount`, `CreditAmount`, `LineNarration`, `CostCenter`, `ReferenceType`, `ReferenceId`
- `SalesInvoice`
  - `InvoiceId`, `InvoiceNo`, `InvoiceDate`, `CustomerId`, `SubTotal`, `TaxTotal`, `GrandTotal`, `Status` (Draft/Posted/Paid/Cancelled), `VoucherId`
- `PurchaseInvoice`
  - `InvoiceId`, `InvoiceNo`, `InvoiceDate`, `VendorId`, `SubTotal`, `TaxTotal`, `GrandTotal`, `Status`, `VoucherId`
- `Payment`
  - `PaymentId`, `PaymentNo`, `PaymentDate`, `PartyType` (Customer/Vendor), `PartyId`, `Amount`, `Mode` (Cash/Bank), `Status`, `VoucherId`
- `JournalEntry`
  - logical API concept that creates `Voucher` + `VoucherLine` with type `JV`.

### Posting Rules
- Sales invoice post:
  - Debit: Accounts Receivable
  - Credit: Sales Revenue
  - Credit: Output Tax (if applicable)
- Purchase invoice post:
  - Debit: Expense/Inventory
  - Debit: Input Tax (if applicable)
  - Credit: Accounts Payable
- Customer receipt post:
  - Debit: Cash/Bank
  - Credit: Accounts Receivable
- Vendor payment post:
  - Debit: Accounts Payable
  - Credit: Cash/Bank
- Manual journal post:
  - user-defined debit/credit lines, must balance.

### API Design (v1)
- `POST /api/v1/ar/invoices` create draft sales invoice
- `POST /api/v1/ar/invoices/{id}/post` post sales invoice (creates voucher)
- `POST /api/v1/ap/invoices` create draft purchase invoice
- `POST /api/v1/ap/invoices/{id}/post` post purchase invoice
- `POST /api/v1/payments` create payment/receipt draft
- `POST /api/v1/payments/{id}/post` post payment/receipt (creates voucher)
- `POST /api/v1/journals` create draft journal entry
- `POST /api/v1/journals/{id}/post` post journal voucher
- `GET /api/v1/vouchers/{id}` voucher header + lines
- `GET /api/v1/gl/trial-balance?from=yyyy-MM-dd&to=yyyy-MM-dd`
- `GET /api/v1/gl/ledger/{accountId}?from=yyyy-MM-dd&to=yyyy-MM-dd`

### Application Layer Responsibilities
- Validate mandatory fields and posting dates.
- Validate account configuration and active status.
- Enforce balance check on voucher lines.
- Enforce status transitions:
  - `Draft -> Posted`
  - `Posted -> Cancelled` only via reversal flow.
- Generate document numbers (`InvoiceNo`, `PaymentNo`, `VoucherNo`) using sequence table.

### Infrastructure/Data Responsibilities
- ADO.NET repositories with explicit SQL transactions for post actions.
- Use row-level lock/version check while posting to avoid double posting.
- Maintain indexes on:
  - `Voucher(VoucherDate, CompanyId, Status)`
  - `VoucherLine(VoucherId, AccountId)`
  - `SalesInvoice(InvoiceNo, Status)`
  - `PurchaseInvoice(InvoiceNo, Status)`
  - `Payment(PaymentNo, Status)`

### Authorization (recommended)
- New permission claims:
  - `AP:VOUCHER_INITIATOR`, `AP:VOUCHER_AUTHORISER`
  - `AR:INVOICE_INITIATOR`, `AR:INVOICE_AUTHORISER`
  - `GL:JOURNAL_INITIATOR`, `GL:JOURNAL_AUTHORISER`
- Posting endpoints should require `AUTHORISER` permissions.
- Create/update draft should require `INITIATOR` permissions.

### Suggested Database Script Order (Accounting)
1. `004_CreateLedgerAccounts.sql`
2. `005_CreateVouchers.sql`
3. `006_CreateSalesInvoices.sql`
4. `007_CreatePurchaseInvoices.sql`
5. `008_CreatePayments.sql`
6. `009_CreateDocumentSequences.sql`
7. `010_SeedDefaultCoa.sql`

### Implementation Order (Recommended)
1. Voucher + voucher lines + journal entry posting (foundation).
2. Sales invoice draft + post flow.
3. Purchase invoice draft + post flow.
4. Payments/receipts draft + post flow.
5. Trial balance and account ledger report APIs.

### Non-Functional Requirements
- Idempotent post endpoint behavior (repeat call should not create duplicate vouchers).
- Audit columns on all accounting tables: `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`.
- Full traceability: invoice/payment row stores linked `VoucherId`.
- No hard delete for posted accounting data.

## Accounting API Test Guide (Detailed)

### Tools
- Preferred: `MyWebApi.http` (already added in project root).
- Alternative: Swagger/Postman.

### Prerequisites
1. Execute SQL scripts in this exact order:
   1. `Database/Scripts/001_CreateEmployees.sql`
   2. `Database/Scripts/002_CreateAuthAndRbac.sql`
   3. `Database/Scripts/003_SeedAdminUser.sql`
   4. `Database/Scripts/004_CreateLedgerAccounts.sql`
   5. `Database/Scripts/005_CreateVouchers.sql`
   6. `Database/Scripts/006_CreateSalesInvoices.sql`
   7. `Database/Scripts/007_CreatePurchaseInvoices.sql`
   8. `Database/Scripts/008_CreatePayments.sql`
   9. `Database/Scripts/009_CreateDocumentSequences.sql`
   10. `Database/Scripts/010_SeedDefaultCoa.sql`
2. Run API: `dotnet run`
3. Base URL: `http://localhost:8017`
4. Login with seeded user:
   - email: `admin@company.com`
   - password: `Admin@123`
5. Copy `accessToken` and set `Authorization: Bearer <token>` for protected APIs.

### 1) Login API
- Endpoint: `POST /api/v1/auth/login`
- Purpose: get JWT token for all protected accounting APIs.
- Expected success: `200 OK` with `accessToken`.
- Expected failure:
  - `401 Unauthorized` for wrong credentials.
  - `400 BadRequest` for missing fields.

### 2) GL Accounts API
- Endpoint: `GET /api/v1/gl/accounts`
- Purpose: fetch account ids to use in invoice/payment/journal payloads.
- Expected success: `200 OK`, list of active ledger accounts.
- DB verification:
```sql
SELECT AccountId, AccountCode, AccountName, AccountType, IsActive
FROM dbo.LedgerAccounts
ORDER BY AccountCode;
```

### 3) AR Invoice Draft API
- Endpoint: `POST /api/v1/ar/invoices`
- Purpose: create sales invoice in `Draft`.
- Example payload:
```json
{
  "invoiceDate": "2026-02-15",
  "customerName": "ABC Retail",
  "receivableAccountId": 3,
  "revenueAccountId": 7,
  "taxAccountId": 5,
  "subTotal": 10000,
  "taxTotal": 1800,
  "narration": "Sales invoice draft"
}
```
- Expected success: `201 Created` with `{ "id": <invoiceId> }`.
- Expected failure:
  - `400 BadRequest` if mandatory fields/amounts are invalid.
  - `403 Forbidden` without `AR:INITIATOR` permission.
- DB verification:
```sql
SELECT TOP 1 InvoiceId, InvoiceNo, Status, SubTotal, TaxTotal, GrandTotal, VoucherId
FROM dbo.SalesInvoices
ORDER BY InvoiceId DESC;
```

### 4) AR Invoice Post API
- Endpoint: `POST /api/v1/ar/invoices/{id}/post`
- Purpose: convert sales invoice draft to posted voucher (`SI` type voucher).
- Expected success: `200 OK` with `voucherId`, `voucherNo`, `alreadyPosted`.
- Expected failure:
  - `404 NotFound` if invoice id not present.
  - `403 Forbidden` without `AR:AUTHORISER`.
- Idempotency test:
  - call same post endpoint again.
  - expected: `200 OK`, `alreadyPosted = true`, same `voucherId`.
- DB verification:
```sql
SELECT InvoiceId, Status, VoucherId
FROM dbo.SalesInvoices
WHERE InvoiceId = <invoiceId>;

SELECT v.VoucherId, v.VoucherNo, v.VoucherType, vl.AccountId, vl.DebitAmount, vl.CreditAmount
FROM dbo.Vouchers v
JOIN dbo.VoucherLines vl ON vl.VoucherId = v.VoucherId
WHERE v.VoucherId = <voucherId>;
```

### 5) AP Invoice Draft API
- Endpoint: `POST /api/v1/ap/invoices`
- Purpose: create purchase invoice in `Draft`.
- Expected success: `201 Created` with draft id.
- Expected failure:
  - `400 BadRequest` for invalid payload.
  - `403 Forbidden` without `AP:INITIATOR`.
- DB verification:
```sql
SELECT TOP 1 InvoiceId, InvoiceNo, Status, SubTotal, TaxTotal, GrandTotal, VoucherId
FROM dbo.PurchaseInvoices
ORDER BY InvoiceId DESC;
```

### 6) AP Invoice Post API
- Endpoint: `POST /api/v1/ap/invoices/{id}/post`
- Purpose: post purchase invoice and create voucher.
- Expected success: `200 OK` with `voucherId`.
- Expected failure:
  - `404 NotFound` if invoice id invalid.
  - `403 Forbidden` without `AP:AUTHORISER`.
- Idempotency test:
  - repeat post call and confirm `alreadyPosted = true`.
- DB verification:
```sql
SELECT InvoiceId, Status, VoucherId
FROM dbo.PurchaseInvoices
WHERE InvoiceId = <invoiceId>;
```

### 7) Payment Draft API
- Endpoint: `POST /api/v1/payments`
- Purpose: create payment/receipt draft.
- Example payload (vendor payment):
```json
{
  "paymentDate": "2026-02-15",
  "partyType": "Vendor",
  "partyName": "XYZ Supplies",
  "paymentType": "Payment",
  "offsetAccountId": 4,
  "cashBankAccountId": 2,
  "amount": 1500,
  "narration": "Vendor payment draft"
}
```
- Expected success: `201 Created` with payment id.
- Expected failure:
  - `400 BadRequest` for invalid `partyType`/`paymentType`/amount.
  - `403 Forbidden` without `AP:INITIATOR`.
- DB verification:
```sql
SELECT TOP 1 PaymentId, PaymentNo, PaymentType, Status, Amount, VoucherId
FROM dbo.Payments
ORDER BY PaymentId DESC;
```

### 8) Payment Post API
- Endpoint: `POST /api/v1/payments/{id}/post`
- Purpose: post payment draft and generate voucher (`PV` for payment, `RV` for receipt).
- Expected success: `200 OK`.
- Expected failure:
  - `404 NotFound` for invalid id.
  - `403 Forbidden` without `AP:AUTHORISER`.
- DB verification:
```sql
SELECT PaymentId, Status, VoucherId
FROM dbo.Payments
WHERE PaymentId = <paymentId>;

SELECT VoucherId, VoucherNo, VoucherType
FROM dbo.Vouchers
WHERE VoucherId = <voucherId>;
```

### 9) Journal Draft API
- Endpoint: `POST /api/v1/journals`
- Purpose: save manual journal lines in `Draft`.
- Example payload:
```json
{
  "journalDate": "2026-02-15",
  "narration": "Manual adjustment entry",
  "lines": [
    { "accountId": 8, "debitAmount": 1000, "creditAmount": 0, "lineNarration": "Adjustment expense" },
    { "accountId": 2, "debitAmount": 0, "creditAmount": 1000, "lineNarration": "Adjustment bank" }
  ]
}
```
- Expected success: `201 Created` with journal id.
- Expected failure:
  - `400 BadRequest` if not balanced or less than two lines.
  - `403 Forbidden` without `GL:INITIATOR`.
- DB verification:
```sql
SELECT TOP 1 JournalId, JournalNo, Status, VoucherId
FROM dbo.JournalEntries
ORDER BY JournalId DESC;

SELECT JournalId, AccountId, DebitAmount, CreditAmount
FROM dbo.JournalEntryLines
WHERE JournalId = <journalId>;
```

### 10) Journal Post API
- Endpoint: `POST /api/v1/journals/{id}/post`
- Purpose: validate balanced journal and post to voucher (`JV`).
- Expected success: `200 OK`.
- Expected failure:
  - `404 NotFound` if journal id invalid.
  - `400 BadRequest` if unbalanced at post time.
  - `403 Forbidden` without `GL:AUTHORISER`.
- DB verification:
```sql
SELECT JournalId, Status, VoucherId
FROM dbo.JournalEntries
WHERE JournalId = <journalId>;
```

### 11) Voucher Detail API
- Endpoint: `GET /api/v1/vouchers/{id}`
- Purpose: verify accounting postings line-by-line (debit/credit).
- Expected success: `200 OK` with voucher header and lines.
- Expected failure:
  - `404 NotFound` when voucher does not exist.

### 12) Trial Balance API
- Endpoint: `GET /api/v1/gl/trial-balance?from=yyyy-MM-dd&to=yyyy-MM-dd`
- Purpose: period summary of total debit/credit per account.
- Expected success: `200 OK` with non-zero account balances.
- Expected failure:
  - `400 BadRequest` when `from > to`.
- Validation rule:
  - Sum of `TotalDebit` across rows should equal sum of `TotalCredit`.

### 13) Ledger API
- Endpoint: `GET /api/v1/gl/ledger/{accountId}?from=yyyy-MM-dd&to=yyyy-MM-dd`
- Purpose: account-wise voucher movement statement.
- Expected success: `200 OK` with voucher-wise entries.
- Expected failure:
  - `400 BadRequest` when date range is invalid.

### End-to-End Functional Test (Recommended)
1. Create AR invoice draft and post it.
2. Create AP invoice draft and post it.
3. Create vendor payment draft and post it.
4. Create balanced journal draft and post it.
5. Query posted voucher ids from each post response.
6. Open voucher details and verify line-level debit/credit.
7. Run trial balance for date range and ensure total debit equals total credit.
8. Run ledger for one cash/bank and one payable/receivable account.

### Common Failure Test Cases
- Post same draft twice: should not create duplicate vouchers (`alreadyPosted = true`).
- Create journal with unequal debit/credit: should return `400`.
- Missing/invalid token: should return `401`.
- Valid token but missing permission: should return `403`.
