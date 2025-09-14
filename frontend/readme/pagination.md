# 🚀 Pagination: Building from Scratch

While I could just use an npm module for pagination, I refuse because I want to learn how pagination is achieved. So, in this section, we'll be discussing how I implemented pagination from scratch. 💡

## 🎯 Setting the Goal

To be able to write pagination from scratch, I must first have a clear goal of the type of pagination I want to achieve. The pagination characteristics I aim to implement are as follows:

1. ✅ Users can navigate to the **previous** and **next** pages.
2. ✅ Users can choose a **page size** from a predefined set of page sizes (hardcoded, no need for dynamic updates).
3. ✅ Users can see the **total number of pages**.

### 🖼️ Pagination Navigation Display

Based on these characteristics, the pagination navigation display will look like this:

```
[previous] [1][2][3][...][n] [next]
[previous] [1][..][4][5][6][..][n] [next]
[previous] [1][..][7][8][n] [next]
```

- The **first** and **last** pages are always visible (last page is only visible when applicable).
- The **window size** is 3.

---

## 🧠 Algorithm Overview

To achieve this, we use an algorithm (see: `pagination.ts.getPageNavDisplay()`):

1. The method returns the exact items to display, e.g., `[1, 2, 3, ..., 8]`.
2. **How it works:**
    - Determine the `left` and `right` numbers based on the current page:
      ```typescript
      const left = Math.max(2, currentPage - 1);
      const right = Math.min(totalPages - 1, currentPage + 1);
      ```
    - If `currentPage > 2`, push `'..'` into the array.
    - From `left` to `right`, push the numbers into the array. For example, if `left = 5` and `right = 7`, push `[5, 6, 7]`.
    - Check if `right < totalPages - 1`, then push `'..'` to ensure proper display.
    - Finally, push the last page.

    This results in something like `[1][2][..][n]` or similar.

---

## 🛠️ Parent-Child Communication

### 🔄 Data Flow

- Every time pagination is performed, an API call is made, and pagination is re-rendered.
- The **parent component** must persist the following data:
  - `pageSize`
  - `currentPage`
  - (No need to persist `totalPages` since it can be derived from the above two.)

### 📥 Passing Data to the Child Component

To pass data from the parent to the child component, we use the `@Input()` decorator:

1. Declare the input in the child component:
    ```typescript
    @Input() inCurrentPage: number;
    ```
2. Pass the data in the parent template:
    ```html
    <app-pagination [inCurrentPage]="paginationCurrentPage"></app-pagination>
    ```

### 📤 Emitting Data to the Parent Component

To notify the parent about pagination state changes (e.g., `currentPage` or `pageSize`), we use the `@Output()` decorator to emit data from the child component:

1. Declare an emitter object in the child component:
    ```typescript
    @Output() dataEmitter = new EventEmitter<T>();
    ```
2. Listen to the emitter in the parent template:
    ```html
    <app-pagination
         (dataEmitter)="functionThatCatchTheData($event)"
    ></app-pagination>
    ```
3. Handle the emitted data in the parent component:
    ```typescript
    functionThatCatchTheData(data: any) {
         // Process the emitted data
    }
    ```

---

🎉 That's it! With this approach, we can build a fully functional pagination system from scratch while learning the underlying concepts. 🚀
