*****************

this part of the file will talk about the basics about angular SPA routing 
which will be done later 

*****************


## 🛡️ Guard in Angular Routing

A **Guard** is a special piece of code that runs **before navigation** happens in Angular. It determines whether a route can be accessed or not.

### 🔍 What does a Guard return?
- ✅ **True** – Navigation allowed
- ❌ **False** – Navigation not allowed
- 🌐 **UrlTree** – Redirects the user to a different route

---

### 💡 Common Use Cases
- 🔑 Check if the user is **logged in**
- 🛂 Verify if the user has the **required role** to access a page

---

### ⚙️ How to Create a Guard

1. **Generate the guard service:**
    ```bash
    ng g service <guard-name>
    ```
2. In the generated file, you'll find a function of type `CanActivateFn`:
    ```typescript
    export const myGuard: CanActivateFn = (route, state) => {
      // Guard logic here
      // return true, false, or UrlTree
    };
    ```
3. Implement your guard logic to decide when to allow or block navigation.

---

### 📝 Registering a Guard in Routes

Add your guard to the route definition using the `canActivate` property:

```typescript
{
  path: 'protected',
  component: ProtectedComponent,
  canActivate: [MyGuardService]
}
```

> 📌 **Tip:** You can add multiple guards to the `canActivate` array!

