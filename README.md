# Sports Club Management System

An ASP.NET Core MVC coursework project for managing trainers, members, training
sessions, and enrollments.

## Technology

C#, .NET 10, Razor views, Bootstrap 5, and Entity Framework Core with SQLite.

## Features

- Trainer, member, and training session management
- Upcoming training schedule
- Enrollment and cancellation with capacity limits
- Scheduling conflict checks for trainers and members, including session edits;
  back-to-back sessions are allowed
- Home and About pages

No login or registration is required.

## Setup

The application requires the .NET 10 SDK. SQLite uses a local database file;
no database server is needed.

From the repository folder:

```sh
./scripts/dotnet.sh restore
./scripts/dotnet.sh run --project SportsClub -- --seed
./scripts/dotnet.sh run --project SportsClub
```

Open [localhost:5050](http://localhost:5050). The wrapper uses the local SDK if available,
otherwise your installed SDK. The seed command applies migrations and adds demo
data to an empty database. To apply migrations without demo data, use `--migrate`
instead of `--seed`.

The database is `SportsClub/sportsclub.db`. Configure its location in
`SportsClub/appsettings.json`. Session times are displayed in Sofia time.
