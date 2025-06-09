# User Login
**Setup and Database Migration Instructions**

**Step 1**: Open Project
Open the project in Visual Studio.

**Step 2**: Install Required Packages
Open the terminal (Bash or Command Prompt) and run the following commands one by one:

  dotnet add package Pomelo.EntityFrameworkCore.MySql
  
  dotnet add package Microsoft.EntityFrameworkCore.Tools

**Step 3**: Create Initial Migration
Run the command to create the initial migration:

  dotnet ef migrations add InitialCreate

**Step 4**: Update Database
Apply the migration to update the database schema:

  dotnet ef database update

**Optional Step (If EF CLI Not Found)**
If running step 3 gives an error like dotnet ef command not found, install the EF Core CLI tool globally:

  dotnet tool install --global dotnet-ef

Then re-run steps 3 and 4.

