Features:
HR super user can access every part of the app, manage users, and export payroll reports.
Programme Coordinator and Academic Manager have separate review dashboards with automated claim checks and live status updates via SignalR.
Lecturers submit claims with auto-filled profile info, live payout calculation, and supporting document uploads.
HR dashboard lets HR create/edit accounts, set roles, hourly rates, activate/deactivate users, and reset passwords.
Reporting tools export approved claims to CSV or PDF for payroll.
Navigation and page access change automatically based on the logged-in role.
How to run:
dotnet restore
dotnet build
dotnet ef database update
dotnet run (then browse to the shown URL, e.g., https://localhost:xxxx)
Log in with one of the seeded accounts, e.g., HR super user hr@cmcs.local / Hr@12345
