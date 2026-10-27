# Reporting integration tests

Uses the Sales/Inventory convention: xUnit, real Application/Persistence/Shared registration, MediatR and validators, fresh service scope per operation, EF InMemory by default and optional SQL Server.

## Run in memory (PowerShell)

```powershell
Set-Location 'D:\Asp.net projects\Pos-ReportingService'
Remove-Item Env:REPORTING_TEST_SQL_CONNECTION -ErrorAction SilentlyContinue
dotnet test .\Pos.ReportingService.IntegrationTests\Pos.ReportingService.IntegrationTests.csproj -m:1 --logger 'console;verbosity=normal'
```

SQL-only tests are reported as skipped. They do not pretend to evaluate AT TIME ZONE in memory. The timezone helper itself is tested in either mode.

## Run against SQL Server

Use an available development SQL Server with a login permitted to CREATE and DROP databases:

```powershell
$env:REPORTING_TEST_SQL_CONNECTION = 'Server=localhost;Database=master;Integrated Security=True;TrustServerCertificate=True'
dotnet test .\Pos.ReportingService.IntegrationTests\Pos.ReportingService.IntegrationTests.csproj -m:1 --logger 'console;verbosity=normal'
```

For SQL Express use Server=.\SQLEXPRESS; for LocalDB use Server=(localdb)\MSSQLLocalDB. Use your actual server name. TrustServerCertificate is for this development setup.

When the variable is set ALL repository tests use SQL Server, not just those tagged SqlServer. The two daily-report tests must now execute rather than be skipped. To run only SQL-specific tests add `--filter Category=SqlServer`.

Every fixture replaces the supplied database name with ReportingIntegrationTests_<random GUID>, clears AttachDBFilename and disables pooling. It creates the database and deletes it on disposal. It never migrates, seeds or deletes the supplied database. Interrupted runs may leave a database with this prefix for manual cleanup.

SQL Server needs current timezone data and support for AT TIME ZONE. Verify:

```sql
SELECT name FROM sys.time_zone_info WHERE name = 'Egypt Standard Time';
```

The test model changes read-only keyless projections to keyed tables only in the disposable test database, matching the external schema/table names. EnsureCreated creates that test schema; production migrations are not applied. Thus these tests verify report execution on SQL, not compatibility with every source service migration. Production models are unchanged.

## Coverage and limits

- Tenant isolation and missing tenant; optional filters and validation pipeline.
- Dashboard status/currency/date rules, return-only product/branch totals, cashier grouping.
- Completed collections minus change; refund paying branch and later refunds.
- Open/closed shift reconciliation and empty shifts.
- Inventory pagination and availability; inclusive low threshold, active alerts and variant matching.
- Export handlers and registrations; over-limit rejection, more than one page, Arabic and CSV escaping, invariant numbers and cancellation.
- Real Reports.View authorization policy (authorization service; no token issuance/server startup).
- Cairo day boundaries across 2026, including 23/25-hour days.
- SQL-only daily grouping, exclusive date end, CSV parity and return-only export row limits.

InMemory does not validate SQL translation, query plans, source indexes or production isolation behavior. No test replaces AT TIME ZONE with a custom memory evaluator. Run the SQL suite before release. These are application/repository integration tests, not full HTTP/browser end-to-end tests.

Cashier names use a fake HTTP transport in tests (no local Identity user table). Tests cover token forwarding, 500-ID batching, missing users and Identity outages. Configure IdentityApi:BaseUrl for deployments; development uses https://localhost:7083/. Live Identity authentication and networking are not exercised by these tests.
