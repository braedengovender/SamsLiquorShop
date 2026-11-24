# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Sam's Liquor Shop is an ASP.NET **Web Forms** app (.NET Framework 4.7.2, C#, old-style non-SDK `.csproj`). It has an online shop for customers and back-office pages for cashiers and managers. It talks to SQL Server through raw ADO.NET (`SqlConnection`/`SqlCommand`). Entity Framework and ASP.NET Identity are referenced only because they came with the Visual Studio template.

## Build / run

- Open `SamsLiquorShop.sln` in Visual Studio. It runs under IIS Express at `https://localhost:44371/`.
- Command-line build (needs Visual Studio Build Tools; the `dotnet` CLI cannot build this project):
  `nuget restore SamsLiquorShop.sln` then `msbuild SamsLiquorShop.sln /p:Configuration=Debug`
- There is no test project and no linter.
- **Adding a file:** because the project is non-SDK style, every new `.cs`, `.aspx` or `.ascx` file must also be listed in `SamsLiquorShop.csproj` as `<Compile Include>` or `<Content Include>`, with `DependentUpon` for code-behind and designer files. Otherwise it won't build or deploy. When you add a server control to markup, update the matching `.designer.cs` field too.

## Database

- There is one connection string, `GroupWst15ConnectionString` in `Web.config`, pointing at a shared remote SQL Server. Pages and helpers read it with `ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"]`. There is no local database and no migrations.
- Tables use `Table_Column` naming (`Customer_ID`, `Employee_Status`, `Product_ID`, …). Status columns hold strings such as `'Active'`.
- `Database/*.sql` holds hand-run, idempotent schema scripts (`IF OBJECT_ID… IS NULL`). Schema changes are added as new scripts here, not applied automatically.
- `Helpers/TableSchema.cs` reads `INFORMATION_SCHEMA` (cached for 2 minutes) so pages such as Suppliers and Categories adapt to whichever columns actually exist (`FindAny("Supplier_Phone", "Supplier_PhoneNumber")`). Use it when a page has to work before and after a schema script has been run.

## Auth and roles (custom, not Identity)

- `Web.config` sets `<authentication mode="None">`. The `Account/` folder, `Startup.Auth.cs`, `IdentityConfig.cs` and `Models/IdentityModels.cs` are unused template leftovers. The real login is the root `Login.aspx`, plus `Register.aspx`, `ForgotPassword.aspx` and `VerifyOtp.aspx`.
- Login checks the `Customer` table first, then `Employee` (`Employee_Role` is `Manager` or `Cashier`). Passwords are compared as plain text in SQL. After the password check, a 6-digit email OTP is stored in `Session["Otp*"]`/`Session["Pending*"]` and confirmed on `VerifyOtp.aspx`.
- Once signed in, the session holds `UserRole` plus `CustomerID` or `EmployeeID`.
- Each protected page starts `Page_Load` with `if (!AuthHelper.RequireRole(this, "Customer"|"Cashier"|"Manager")) return;`. This also checks against the database that the account is still Active and still has that role.
- Folders map to roles: `Customer/`, `Employee/` (cashier) and `Manager/`. Each has its own nav user control (`CustomerNav.ascx`, `CashierNav.ascx`, `ManagerNav.ascx`). Shared back-office grids are in `Controls/` (`SalesBrowser.ascx`, `OnlineOrdersBrowser.ascx`).

## Business logic lives in `Helpers/`

Code-behind files handle the UI. Shared rules live in static helpers:

- `OrderHelper`: validates online orders (including SA ID number age check and card checks), builds quotes, `PlaceOrder` (transactional) and the online-order status machine (`ChangeStatus`, `GetNextStatus`, `CanCancel`, `ExpireUncollected`). Customer, cashier and manager online-order pages all go through it.
- `CartHelper`: cart, saved-for-later and wishlist state kept in **Session**. `WishlistHelper` persists wishlists to the `Wishlist` table.
- `PricingHelper` / `PromoHelper`: current prices and discounts, promo codes (including student-email rules).
- `SlotHelper`: delivery and collection slots, store hours and public holidays. Always use `SlotHelper.Now()` for "now" (South Africa time, since the server may be on UTC).
- `PaymentGateway`: a **simulated** card gateway. The result depends on the card's last four digits (e.g. `0002` = declined).
- `ReceiptHelper` / `OnlineReceiptHelper` / `ExportHelper`: receipts and report exports. `StockLogHelper` writes stock-movement audit entries.
- `OtpHelper` / `OrderEmailHelper`: email through the Mailjet HTTP API.

## Secrets, email and session tracking

- `OtpSecrets.config` (gitignored, in the project root) holds `MailjetApiKey`, `MailjetSecretKey` and `OtpDemoMode`. If it's missing or demo mode is on, `OtpHelper.DemoMode` is true and the OTP is shown rather than emailed, so login still works without keys.
- reCAPTCHA keys are in `Web.config` `appSettings`. If they're blank, Google's test keys are used.
- Session is InProc. `Helpers/SessionWatch` tracks logged-in customers, and `SessionBeacon.ashx` (page-unload beacon) plus `Global.asax` `Session_End` use it to email logout, closed-tab or expired notices.

## Other conventions

- Generic handlers (`.ashx`) cover AJAX-style endpoints: `ProductImage`, `SearchSuggestions`, `ChatbotReply`, `NewsletterSignup` and `SessionBeacon`.
- FriendlyUrls is on, so links can leave off `.aspx`. `customErrors` sends errors to `Error.aspx` and 404s to `NotFound.aspx`.
- Charts on dashboards use `Scripts/chart.umd.min.js`. Styling is Bootstrap 5 plus `Content/Site.css`, with `Site.Master` as the master page.
- Always use parameterized SQL (`cmd.Parameters.AddWithValue`). Dynamic column names must come from `TableSchema` and be wrapped with `TableSchema.Q()`.
