# 🎓 Student Management System — ASP.NET Core MVC & Raw ADO.NET

A high-performance, full-stack ASP.NET Core MVC web application built with **raw ADO.NET (`Microsoft.Data.SqlClient`)** using the **Disconnected Data Architecture (`SqlDataAdapter` & `DataTable`)**. 

This project intentionally bypasses high-level ORMs like Entity Framework Core to demonstrate a low-level understanding of relational database connectivity, disconnected memory tables, custom SQL parameter mapping, and ASP.NET Core MVC mechanics.

---

## 📌 Project Overview

* **Entity:** `Student` (Properties: `StudentId`, `StudentName`, `RegistrationNumber`, `Department`, `AttendancePercentage`).
* **Architecture:** Model-View-Controller (MVC) with a decoupled **Repository Pattern**.
* **Database Engine:** Microsoft SQL Server using disconnected relational mapping.
* **Key Focus:** Explicit parameter binding, custom data adapters, tag helper mechanics, and async thread pool execution.

---

## 🏛 Architecture & Disconnected Data Model

The application isolates database operations inside a dedicated `StudentRepository` class. Rather than maintaining open database connections during UI interactions, it operates on a **Disconnected Data Model**:

1. **Fetch & Populate:** `SqlDataAdapter.Fill()` retrieves data from SQL Server into an in-memory `DataTable` and immediately frees the database connection.
2. **In-Memory Manipulation:** Rows are created (`NewRow()`), updated (`DataRow["Col"] = val`), or marked for deletion (`DataRow.Delete()`) locally inside the `DataTable`.
3. **Batch Synchronization:** `SqlDataAdapter.Update()` inspects the row states (`Added`, `Modified`, `Deleted`) inside the `DataTable` and dispatches the corresponding `SqlCommand` execution back to SQL Server in a single atomic pass.

### 📐 Disconnected Model Data Flow


```

┌─────────────────────────────────────────────────────────────────────────┐
│                           ASP.NET Core MVC                              │
│  [Views (.cshtml)]  ◄───►  [StudentController]  ◄───►  [Student Model] │
└────────────────────────────────────┬────────────────────────────────────┘
│
(Calls Repository Methods)
▼
┌─────────────────────────────────────────────────────────────────────────┐
│                            REPOSITORY LAYER                             │
│                           StudentRepository                             │
│                                                                         │
│   1. Open Short-Lived Connection                                        │
│   2. Fill In-Memory DataTable ────────────────────────┐                 │
│   3. Close Connection                              │                 │
│                                                    ▼                 │
│                                          ┌───────────────────┐       │
│                                          │     DataTable     │       │
│                                          │ (In-Memory Copy)  │       │
│                                          └─────────┬─────────┘       │
│                                                    │                 │
│   4. Apply Edits / Add / Delete Locally            │                 │
│   5. Re-open Connection & Run Adapter.Update() ◄───┘                 │
└────────────────────────────────────┬────────────────────────────────────┘
│
(Executes Batch SQL)
▼
┌─────────────────────────────────────────────────────────────────────────┐
│                            SQL SERVER DB                                │
│                         [students Table]                                │
└─────────────────────────────────────────────────────────────────────────┘

```

---

## 🛠 Tools & Tech Stack

* **Language:** C# 13 / .NET
* **Framework:** ASP.NET Core MVC
* **Data Access:** Raw ADO.NET (`Microsoft.Data.SqlClient`)
* **Core ADO.NET Classes:** `SqlConnection`, `SqlCommand`, `SqlDataAdapter`, `DataTable`, `DataRow`, `SqlParameter`
* **Database:** Microsoft SQL Server
* **Frontend:** Razor Views (`.cshtml`), HTML5, CSS3, Tag Helpers

---

## 🚨 Problems Faced & How They Were Resolved

### 1. Form Submissions Saving `Null` / Empty Data
* **Problem:** Submitting the "Create Student" form resulted in empty strings and `0.0` attendance records inserted into SQL Server.
* **Root Cause:** The Razor engine was missing global Tag Helper registrations. Without `_ViewImports.cshtml`, elements like `<input asp-for="StudentName" />` compiled as plain HTML `<input />` without `name` attributes, breaking ASP.NET Core MVC's Model Binder.
* **Resolution:** Created `Views/_ViewImports.cshtml` containing `@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers`.

---

### 2. Parameter Mapping Failures in Disconnected `SqlDataAdapter`
* **Problem:** Executing `adapter.Update(studentTable)` caused updates to fail or push nulls because parameter values were assigned statically instead of reading from `DataTable` source columns.
* **Root Cause:** In disconnected mode, parameters must map dynamically to the schema columns of the modified `DataRow`.
* **Resolution:** Explicitly configured `SourceColumn` bindings on commands:
  ```csharp
  updateStudent.Parameters.Add("@name", SqlDbType.VarChar, 100, "StudentName");
  
  SqlParameter attendancePercent = new SqlParameter("@AttPer", SqlDbType.Decimal) 
  { 
      Precision = 5, 
      Scale = 2, 
      SourceColumn = "AttendancePercentage" 
  };
  updateStudent.Parameters.Add(attendancePercent);

```

---

### 3. `FillSchema` vs. `Fill` Runtime Exceptions During Edits

* **Problem:** Attempting to update or delete a student caused an `IndexOutOfRangeException` when accessing `studentTable.Rows[0]`.
* **Root Cause:** `FillSchema` only fetches table metadata (column names and types) without populating any actual data rows into the `DataTable`.
* **Resolution:** Switched to `studentAdapter.Fill(studentTable)` with `MissingSchemaAction.AddWithKey` for edit and delete operations, ensuring the existing record is loaded into memory before applying edits.

---

### 4. Controller Inverted Logic & Invalid HTML Navigation

* **Problem:** Clicking "Delete" returned a `404 Not Found` page on successful operations, and action buttons in views were non-functional.
* **Root Cause:** The `Delete` controller action checked `if (!success)` incorrectly, and Razor views nested `<a>` links directly inside `<button>` tags, breaking standard browser click event propagation.
* **Resolution:** Fixed controller conditionals to return `RedirectToAction("Index")` on success, and styled standalone `<a asp-action="...">` elements cleanly without wrapping `<button>` tags.

---

## 🚀 How to Run the Project Locally

### Prerequisites

* [.NET SDK](https://dotnet.microsoft.com/) installed
* [Microsoft SQL Server](https://www.microsoft.com/en-us/sql-server/) running locally

### 1. Database Setup

Execute the following script in SQL Server Management Studio (SSMS) or Azure Data Studio:

```sql
CREATE DATABASE UniversityDb;
GO

USE UniversityDb;
GO

CREATE TABLE students (
    StudentId INT IDENTITY(1,1) PRIMARY KEY,
    StudentName NVARCHAR(100) NOT NULL,
    RegistrationNumber NVARCHAR(30) NOT NULL,
    Department NVARCHAR(50) NOT NULL,
    AttendancePercentage DECIMAL(5,2) NOT NULL
);

```

### 2. Configure Connection String

Configure your local connection string using .NET User Secrets:

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=UniversityDb;Trusted_Connection=True;TrustServerCertificate=True;"

```

### 3. Run the Application

```bash
dotnet restore
dotnet run

```

Navigate to `https://localhost:5001/Student` in your web browser.

```

```