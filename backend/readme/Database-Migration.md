## 🚀 Database Migrations Made Easy

Database migrations allow you to update your database schema **without** manually using database management tools like Workbench.

### 🛠️ What is a Migration?

A migration is a way to incrementally change your database structure using code. This is especially useful for collaborative projects and version control.

---

### 📚 Example Use Case

Suppose you have a `User` table:

| Id | Username | Email |
|----|----------|-------|

- **Initial migration:** You create the table, but `Username` is **not** unique.
- **Requirement change:** Now you want `Username` to be unique.

---

### 📝 Steps to Add a Unique Constraint

1. **Update the Model:**
    - Modify `ApplicationDbContext.OnModelCreating()` to add a unique constraint to `Username`.

2. **Create a New Migration:**
    - Run the following command inside the `src` directory:
      ```bash
      dotnet ef migrations add "AddUniqueConstraintToUsername" --project "ParcelManagement.Infrastructure" --startup-project "ParcelManagement.Api"
      ```
    - `--project` specifies the project containing your database context and migrations.
    - `--startup-project` specifies the project used to launch the application (where dependencies are resolved).

3. **Apply the Migration:**
    - Run:
      ```bash
      dotnet ef database update
      ```
    - This command applies the migration to your database using the `DbContext` (configured in your `DbContextFactory`).

---

### ✅ Summary

- No need to manually edit the database.
- All changes are tracked and versioned.
- Migrations ensure your database schema stays in sync with your codebase.

> 💡 **Tip:** Always review generated migration files before applying them to production databases!