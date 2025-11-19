Application Features

HR Super User has full system access. They can manage all users, change roles, edit details, activate or deactivate accounts, set hourly rates, reset passwords, and export payroll reports.

Programme Coordinator and Academic Manager each have their own review dashboards. These dashboards include automated claim validation checks and live claim status updates through SignalR.

Lecturers can submit their claims with automatically filled profile information. The system calculates payouts live as they type, and they can upload supporting documents.

HR Dashboard allows HR to handle account creation, editing, role assignments, activation controls, hourly rate management, and password resets.

Reporting tools allow exporting all approved claims to CSV or PDF for payroll processing.

Navigation and page visibility automatically adjust based on the logged-in user’s role.

How To Run The Application

Run the following commands in order:

dotnet restore

dotnet build

dotnet ef database update

dotnet run

After the application starts, browse to the URL shown in the console, for example:

https://localhost:xxxx

Login Information

Use one of the seeded accounts after running the application. For example:

HR Super User
Email: hr@cmcs.local

Password: Hr@12345
