# AddressBook (ASP.NET Core, EF Core, SQLite)

A minimal address book built with .NET 8 Razor Pages, Entity Framework Core, and SQLite. Manage customers and their addresses with inline edit, sorting, and validation.

## Features
- Customers grid with sorting (Customer Number, First Name, Last Name) and row selection.
- Inline edit with Save/Cancel; delete with confirmation.
- Address management for the selected customer.
- Exactly one address per type (Home, Work) per customer; add row hides when both exist.
- Validation: CustomerNumber numeric and non-negative; State is two letters (e.g., CA); ZIP is `12345` or `12345-6789`.
- Clear run-time error messages; responsive layout; hover row actions.

## Prerequisites
- .NET 8 SDK  
  macOS: `brew install --cask dotnet-sdk`  
  Windows: `winget install Microsoft.DotNet.SDK.8`  
  Linux: install `dotnet-sdk-8.0` via your distribution’s Microsoft feed
- (Optional) SQLite tools: `sqlite3` or “DB Browser for SQLite”.
- Trust local HTTPS cert (recommended): `dotnet dev-certs https --trust`

## Quick Start
```bash

cd <repo>/AddressBookApp

# Restore packages
dotnet restore

# EF Core tools (repo-local preferred; global fallback)
dotnet tool restore || dotnet tool update -g dotnet-ef

# Create/update the local SQLite database
dotnet ef database update

# Run (hot reload)
dotnet watch
# or
# dotnet run
