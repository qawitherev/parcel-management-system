# 📚 Angular for Dummies

In this README, we'll be talking about the **Angular** framework!  
Angular is developed by **Google** 🌐, but it's open source—meaning the community can contribute, though Google has the final say.

---

## 🤔 Why Choose Angular?

Angular is an **opinionated framework** 🏗️, which means it provides a structured way to build your app.  
No need to stress over finding the best architecture or pattern for your frontend—Angular guides you!

---

## 🛠️ How Angular Works Under the Hood
┌─────────────────────┐
│   Your Code (TS +   │
│   Angular Template) │
└─────────┬───────────┘
          │
          ▼
┌─────────────────────────┐
│ 1. TypeScript Compiler  │
│ - TS → JS               │
│ - removes types         │
└─────────┬───────────────┘
          │
          ▼
┌─────────────────────────────┐
│ 2. Angular Compiler (Ivy)   │
│ - HTML → rendering instr.   │ --> remember this js rendering instruction 
│ - {{name}} → ctx.name       │
└─────────┬───────────────────┘
          │
          ▼
┌─────────────────────────┐
│ 3. Bundler (Webpack)    │
│ - merges app + Angular  │
│ - tree-shakes + minify  │
└─────────┬───────────────┘
          │
          ▼
┌───────────────────────────┐
│ 4. Browser Runs main.js   │
│ - Angular bootstraps app  │
│ - Executes rendering instr│
└─────────┬─────────────────┘
          │
          ▼
┌─────────────────────────┐
│ 5. DOM is created        │
│ <h1>Hello World</h1>     │
└─────────────────────────┘
---

## ⚔️ Angular vs React

We’re all familiar with React, but here’s how React works:

### ⚛️ React

- **Virtual DOM** 🪞:
    - Real DOM has its own virtual DOM copy
    - Whenever a value changes, React replaces the virtual DOM with a new one (with updated values)
    - React compares the new virtual DOM and real DOM
    - Changes only what’s different in the real DOM
    - This process is called **diffing** 🔍

### 🅰️ Angular

- **Change Detection** 🔄:
    - Angular doesn’t use a virtual DOM
    - Remembers the rendering instructions from before
    - Every time a value changes, Angular reruns the rendering instructions with the updated value
    - This is called **incremental change detection** ⚡

---

## 🚀 Starting a New Angular Project

The command is:

```bash
ng new <project-name> --routing --style=css --strict
```

- **routing**: Uses `app.routing.module.ts` so you can define routes 🗺️
- **style**: Choose the style you want 🎨
- **strict**: Enables stricter type checking 🔒

---