# Karnel Travel Guide — Complete Website Audit

## Phase 2 Remediation Update

**Date:** 2026-11-11  
**Application:** ASP.NET Core MVC, .NET 10, EF Core 10, SQL Server LocalDB

This update records the verified remediation work performed after the initial audit. It does not claim browser or external-provider behavior that could not be tested in the current environment.

## 1. Executive Summary

Phase 2 remediation improved the security and public route surface without resetting or deleting existing database data. The committed SMTP username/password/from values were removed from `appsettings.json`; SMTP delivery now fails closed when host, sender, username, or password is missing. Configuration must be provided through User Secrets or environment variables.

The missing `/Experiences` route is now implemented as a public discovery page using the existing shared layout and design system. Its calls to action use the existing Contact inquiry workflow rather than introducing fake experience records or a new database table. `/Search/Advanced` is restored as a compatibility route to the existing advanced search controls at `/Search#discovery`.

Customer ownership protections were verified in source and regression tests. Payment isolation already had a customer-A/customer-B test; a new controller-level ticket ID-tampering test confirms customer A receives `NotFound` for customer B's ticket. The final automated suite passed **68 tests, 0 failed, 0 skipped**.

Browser automation was not available. Therefore visual desktop/tablet/mobile, browser-console, microphone, cursor interaction, and complete authenticated user journeys remain unclaimed and are documented as BLOCKED or PARTIAL.

## 2. Remediation Status

| Priority | Status | Evidence |
|---|---|---|
| SMTP security | PASS for source/config behavior; BLOCKED for real delivery | Secret values removed; incomplete configuration returns false without SMTP attempt; real delivery requires rotated provider credentials. |
| Experiences route | PASS for route/page; PARTIAL for database-backed experience inventory | `/Experiences` returns HTTP 200; page uses inquiry CTAs because no active Experience entity/table exists. |
| Search Advanced route | PASS | `/Search/Advanced` returns HTTP 302 to `/Search#discovery`; existing controls are rendered on Search/Index. |
| Customer ownership | PARTIAL/PASS by tested surfaces | Payment cross-customer test existed and passed; ticket ID tampering test added and passed; authenticated live two-account testing remains unavailable. |
| Browser journeys | BLOCKED | No Playwright/Chrome/Edge/browser automation executable was available. Safe HTTP checks were performed instead. |
| Responsive/visual QA | BLOCKED | Requires browser/device emulation and console capture. |

## 3. Fixed Issues

### SMTP configuration and handling

- Cleared `Email:Smtp:Username`, `Email:Smtp:Password`, and `Email:Smtp:From` in `KarnelTravel/appsettings.json`.
- `AccountEmailService` now requires all of the following before sending: host, sender, username, and password.
- Missing values produce a warning and `false`; no SMTP connection is attempted.
- The password is never written to logs.
- Added `KarnelTravel.Tests/AccountEmailServiceTests.cs` covering incomplete SMTP configuration.

Recommended configuration mechanisms:

```powershell
$env:Email__Smtp__Host = "smtp.gmail.com"
$env:Email__Smtp__Port = "587"
$env:Email__Smtp__Username = "your-sender@example.com"
$env:Email__Smtp__Password = "<rotated-provider-secret>"
$env:Email__Smtp__From = "your-sender@example.com"
$env:Email__Smtp__EnableSsl = "true"
```

Do not commit the values above. In Visual Studio development, use `dotnet user-secrets` from the web project, or configure equivalent deployment secret variables. The previously committed Gmail app password must be revoked/rotated at the provider. Removing it from the current file does not erase it from Git history.

### Experiences

- Added `Controllers/ExperiencesController.cs` with anonymous access.
- Added `Views/Experiences/Index.cshtml` using the shared public layout.
- Added nature/mountain, culture/heritage, and food/local-life discovery cards.
- Each card routes to the existing validated Contact inquiry action with `Experience Planning` category and service preselection.
- No fake database records or new database schema were introduced.

### Search Advanced

- Added `[HttpGet("/Search/Advanced")]` to `SearchController`.
- The action redirects to the existing Search page's `#discovery` section.
- Updated home and footer navigation to use the restored compatibility route.

### Customer ownership verification

- Existing `DemoPaymentWorkflowTests.Customer_cannot_pay_for_another_customers_booking` verifies payment isolation.
- Added a controller-level ticket test proving customer A cannot open customer B's ticket by changing the ticket ID.
- Source inspection confirms customer booking details use `OwnedBookings()`, payment uses `OwnedBookingQuery()`, tickets compare the booking's customer ID/email to claims, and profile updates use the authenticated claim rather than a URL ID.

## 4. Verification Evidence

### Build and tests

- `dotnet test KarnelTravel.Tests/KarnelTravel.Tests.csproj --no-restore --verbosity minimal`
- Result: **68 total, 68 passed, 0 failed, 0 skipped**.
- Solution build succeeded as part of the test run.

### HTTP route checks

Performed against the local application on `http://127.0.0.1:5275` without submitting booking/payment forms or changing business data:

| Route | Result |
|---|---|
| `/Experiences` | 200 |
| `/Search/Advanced` | 302 to `/Search#discovery` |
| `/Search` | 200 |
| `/Contact?category=Experience%20Planning&service=Culture%20and%20heritage` | 200 |
| `/css/site.css` | 200 |
| `/js/site.js` | 200 |
| `/Bookings/Create` | 302 to login |
| `/Customer/Dashboard` | 302 to login |
| `/Admin/Dashboard` | 302 to login |

The application connected to the existing LocalDB during public page checks. No records were inserted, updated, deleted, or reset by these checks.

## 5. Security Findings

| Severity | Finding | Current status |
|---|---|---|
| Critical | SMTP credential had previously been committed to source control. | Current value removed; provider-side rotation and Git-history cleanup remain required. |
| High | Full authenticated two-customer live IDOR test was not possible without browser/session tooling and controlled credentials. | Controller/source and isolated regression coverage improved; live verification remains open. |
| Medium | HTTP forced-port test logged HTTPS redirection warning because no HTTPS port was configured for that test process. | Not a production HTTPS result; verify normal HTTPS launch profile before deployment. |
| Low | External image/font/video URLs remain dependencies. | Not changed in this remediation. |

Positive controls verified: antiforgery attributes on state-changing actions, cookie authentication, hashed passwords/reset codes, server-side booking/payment ownership, server-calculated payment amounts, and no full card/CVV/expiry properties in the payment transaction model.

## 6. Git History Cleanup Guidance

The exposed provider credential must be revoked first. Then coordinate history cleanup with all repository users. For a repository where rewriting history is approved, use a current secret-scanning/history-rewrite procedure such as `git filter-repo` with a replacement file, force-push the rewritten branches/tags, and require collaborators to reclone or hard-reset. Do not paste the old secret into commands, reports, commits, or issue comments. If the repository is public or the secret reached a remote, assume compromise even after rewriting history.

## 7. Remaining Blockers

- Rotate/revoke the exposed SMTP provider secret and configure a replacement outside source control.
- Test real SMTP delivery after rotation; no provider delivery was claimed.
- Perform browser-based registration/login, search, experience inquiry submission, booking, payment success/failure/cancellation, confirmation, dashboard, and admin journeys.
- Perform two real customer-session IDOR checks for booking details, payment, tickets, profile, and notifications.
- Perform desktop/tablet/mobile layout checks, browser-console checks, image-failure checks, cursor/animation checks, reduced-motion checks, and horizontal-overflow checks.
- Consider adding a real database-backed Experiences catalog if the business requires managed experience records; the current page is intentionally inquiry-led because the active model/table was removed.
- Validate HTTPS deployment configuration and production logging/secret storage.

## 8. Final Readiness Assessment

| Area | Score | Evidence-based assessment |
|---|---:|---|
| Frontend | 7/10 | Experiences and Advanced navigation repaired; visual/browser testing blocked. |
| Backend | 8/10 | Build/tests pass and public/protected routing is verified. |
| Database | 8/10 | Existing SQL-backed architecture preserved; no destructive test performed. |
| Booking system | 6/10 | Server-side implementation exists; authenticated lifecycle not browser-tested. |
| Payment simulation | 8/10 | Server-side amount, ownership, validation, duplicate protection, and tests exist. |
| Admin panel | 6/10 | Policy/CRUD implementation exists; full browser CRUD matrix remains. |
| Customer panel | 7/10 | Ownership filters and new ticket regression coverage; live two-account test remains. |
| Karnel AI | 7/10 | Rule-based database-backed implementation; browser/API journey remains. |
| Security | 7/10 | Secret removed from current config and missing credentials fail closed; rotation/history cleanup still required. |
| Overall readiness | 7/10 | Suitable for controlled review; not fully pre-launch until secrets, browser QA, and authenticated journeys are completed. |

## 9. Original Audit Scope Not Repeated

The prior audit's full architecture, database, route, AI, frontend, security, and payment findings remain the baseline. This document section records only Phase 2 remediation changes and newly verified evidence rather than restating the full audit.

## 10. Phase 3 End-to-End Browser QA and Release Readiness

**Verification date:** 2026-11-11

This phase was limited by the available environment. No Playwright, Selenium, npm/npx, or .NET browser-automation package was installed. Microsoft Edge was installed, so limited headless public-page renderings were captured, but this is not equivalent to interactive browser automation and no authenticated journey is marked PASS.

### Browser and responsive evidence

| Check | Result | Evidence / limitation |
|---|---|---|
| Browser automation framework | BLOCKED | No supported automation framework or driver was installed; no new dependency or browser binary was added. |
| Public page headless rendering | LIMITED | `/Experiences` rendered in Edge at 1920x1080, 1366x768, 768x1024, and 390x844. PNGs were written to the OS temporary directory, not the repository. |
| Screenshot review of actual defects | NOT TESTED | The available tooling did not provide an interactive visual review workflow; no defect is claimed or fixed. |
| Browser console/network capture | BLOCKED | No automation framework was available. |
| Cursor, animation, reduced-motion, overflow, image and form interaction checks | BLOCKED | No interactive browser session was available. |

### Customer journey

The following were **not executed**: registration, login, destination search interaction, experience inquiry submission, booking submission, payment-page interaction, successful/failed/cancelled demo payment, confirmation, booking history, profile update, and logout. Public HTTP checks confirmed protected routes redirect to `/Account/Login`, but this is not a customer journey result.

### Multi-customer security

No live SQL Server two-customer accounts were created and no authenticated ID/URL/request-parameter tampering session was run. Existing source and isolated regression evidence remains positive: customer-owned booking queries, payment ownership checks, profile lookup by authenticated identity, and ticket ownership checks are present; payment isolation and ticket ID-tampering tests passed. Live verification of bookings, payments, tickets, profile information, and notifications remains BLOCKED.

### Admin journey

Admin authentication, dashboard, listing CRUD, booking management, inquiry management, customer management, payment management, and logout were not executed in a browser. The admin controllers are protected by the `AdminOnly` policy and existing authorization/antiforgery tests passed, but implementation and source inspection must not be reported as end-to-end CRUD PASS. No admin records were created or deleted.

### Persistence, routes, and regression

- `dotnet build` succeeded.
- All **68 automated tests passed** with 0 failures.
- `dotnet ef migrations list --no-build` connected to SQL Server LocalDB and showed all committed migrations applied through `20261111093340_AddAdminPasswordResetCode`; no migration was applied by this audit.
- Read-only local HTTP checks returned 200 for `/`, `/Experiences`, `/Search`, and the Contact inquiry page. `/Search/Advanced` returned its expected 302 to `/Search#discovery`. `/Bookings/Create`, `/Customer/Dashboard`, `/Admin/Dashboard`, and `/DemoPayment/Checkout/1` redirected to login.
- Application logs contained no unhandled exception during the read-only checks. The HTTP-only test launch emitted an expected HTTPS redirection warning; EF also reported a non-fatal query-order warning for an existing `FirstOrDefault` query.
- The working tree contained pre-existing Phase 2 changes and untracked Phase 2 files before this report update. No existing customer data was changed by this audit.

### SMTP final status

Current `appsettings.json` contains no SMTP username, password, or sender value. Missing SMTP configuration is handled safely: the account email service logs a warning, returns `false`, and does not attempt delivery; the corresponding automated test passed. No credentials were invented or entered, and no real email was delivered. Real SMTP delivery is **BLOCKED** until a replacement provider credential is configured through User Secrets or deployment environment variables and tested successfully. Previously exposed provider credentials still require revocation/rotation and repository-history remediation.

### Experiences architecture recommendation

Keep Experiences inquiry-led for the current release. The active Experiences entity/table was removed by the `RemoveExperiences` migration, and the current public page routes customers to the existing Contact inquiry workflow. If the business later requires inventory, pricing, availability, ownership, reporting, or admin CRUD, design a database-backed catalog as a separate approved feature: define the domain model and lifecycle, migration, admin permissions, customer-facing availability rules, inquiry/booking relationship, seed strategy, and migration/backout plan before reintroducing an entity. Do not restore the removed entity solely to make the page appear database-backed.

### Final launch recommendation

**NOT READY for a controlled public launch.** Build, automated regression, migration inspection, read-only route checks, source-level ownership controls, and missing-SMTP handling are positive. However, browser automation, all authenticated customer and admin journeys, live two-customer authorization testing, responsive/console QA, and real SMTP delivery remain unverified. Complete those checks with isolated test accounts, a disposable or explicitly approved test database, rotated provider credentials, and a supported browser automation framework before assigning a public-launch PASS.
