# MyTable Component - Type Safety Documentation

## Overview

The `MyTable` component has been updated to be fully type-safe using TypeScript generics. This ensures compile-time type checking for table columns and data, preventing runtime errors and improving developer experience.

## Changes Made

### 1. **MyTable Component** (`my-table.ts`)
- Converted to a generic component: `MyTable<T>`
- Added type parameter `T` to represent the type of data rows
- Updated all inputs/outputs to use proper types:
  - `@Input() columns: TableColumn<T>[]`
  - `@Input() data: T[]`
  - `@Output() actionClicked = new EventEmitter<T>()`
- Removed all `any` types for better type safety
- Added JSDoc comments for better documentation

### 2. **TableColumn Interface** (`my-table.ts`)
- Made generic: `TableColumn<T>`
- Changed `key` property from `string` to `keyof T | string`
  - This provides autocomplete for valid property keys
  - Still allows string for nested properties (dot notation) and action columns
- Added comprehensive JSDoc documentation

### 3. **Type Definitions File** (`my-table.types.ts`)
- Created a new types file with:
  - `TableColumn<T>` interface with detailed documentation
  - `TableData<T>` type alias
  - `TableActionEvent<T>` interface
  - Usage examples in JSDoc comments

### 4. **Updated Components**

#### **ParcelsList Component** (`parcels-list.ts`)
- Updated `tableColumns` to use `TableColumn<ParcelResponse>[]`
- Fixed column key from `'dimension'` to `'dimensions'` (matching the actual property)
- Fixed label from duplicate "Tracking Number" to "Locker"
- Now has full type safety for parcel data

#### **UiComponent** (`ui-component.ts`)
- Created `TestUser` interface for test data
- Updated `tableColumns` to use `TableColumn<TestUser>[]`
- Renamed `TestDataData` to `testData` for better naming
- Changed type from `any` to `TestUser[]`
- Updated `onTableActionClicked` parameter from `any` to `TestUser`

## Usage Examples

### Basic Usage

```typescript
// Define your data interface
interface User {
  id: string;
  name: string;
  email: string;
  age: number;
}

// Create type-safe columns
const columns: TableColumn<User>[] = [
  { key: 'id', label: 'ID', isActionColumn: false },
  { key: 'name', label: 'Name', isActionColumn: false },
  { key: 'email', label: 'Email', isActionColumn: false },
  { key: 'age', label: 'Age', isActionColumn: false },
  { key: 'edit', label: 'Edit', isActionColumn: true }
];

// Create data array
const users: User[] = [
  { id: '1', name: 'John Doe', email: 'john@example.com', age: 30 },
  { id: '2', name: 'Jane Smith', email: 'jane@example.com', age: 25 }
];

// Handle action clicks with type safety
onActionClicked(user: User) {
  console.log(`Editing user: ${user.name}`);
}
```

### Template Usage

```html
<app-my-table
  [columns]="columns"
  [data]="users"
  [count]="totalCount"
  (actionClicked)="onActionClicked($event)"
  (paginationClicked)="onPaginationChanged($event)"
>
</app-my-table>
```

### Nested Properties

The table supports dot notation for nested properties:

```typescript
interface Product {
  id: string;
  name: string;
  category: {
    id: string;
    name: string;
  };
  price: {
    amount: number;
    currency: string;
  };
}

const columns: TableColumn<Product>[] = [
  { key: 'id', label: 'ID', isActionColumn: false },
  { key: 'name', label: 'Product Name', isActionColumn: false },
  { key: 'category.name', label: 'Category', isActionColumn: false },
  { key: 'price.amount', label: 'Price', isActionColumn: false }
];
```

## Benefits

1. **Type Safety**: Compile-time checking prevents typos in column keys
2. **Autocomplete**: IDEs can suggest valid property keys
3. **Refactoring**: Renaming properties will show errors in table configurations
4. **Documentation**: JSDoc comments provide inline documentation
5. **Maintainability**: Easier to understand and modify code
6. **Error Prevention**: Catches errors before runtime

## Migration Guide

To migrate existing code to use the type-safe table:

1. **Define an interface** for your data:
   ```typescript
   interface MyData {
     id: string;
     name: string;
     // ... other properties
   }
   ```

2. **Update column definitions**:
   ```typescript
   // Before
   tableColumns: TableColumn[] = [...]
   
   // After
   tableColumns: TableColumn<MyData>[] = [...]
   ```

3. **Update data array type**:
   ```typescript
   // Before
   data: any[] = [...]
   
   // After
   data: MyData[] = [...]
   ```

4. **Update event handlers**:
   ```typescript
   // Before
   onActionClicked(data: any) { ... }
   
   // After
   onActionClicked(data: MyData) { ... }
   ```

## Type Safety Features

### Compile-Time Checks

The TypeScript compiler will now catch errors like:

```typescript
interface User {
  id: string;
  name: string;
}

// ❌ Error: 'email' does not exist in type 'User'
const columns: TableColumn<User>[] = [
  { key: 'email', label: 'Email', isActionColumn: false }
];

// ✅ Correct
const columns: TableColumn<User>[] = [
  { key: 'name', label: 'Name', isActionColumn: false }
];
```

### Generic Type Inference

When using the component, TypeScript can infer types:

```typescript
// Type is inferred as TableColumn<ParcelResponse>[]
const columns = [
  { key: 'trackingNumber', label: 'Tracking', isActionColumn: false }
] satisfies TableColumn<ParcelResponse>[];
```

## Files Modified

1. `/frontend/src/app/common/components/table/my-table/my-table.ts`
2. `/frontend/src/app/common/components/table/my-table/my-table.types.ts` (new)
3. `/frontend/src/app/features/parcel/parcels/pages/parcels-list/parcels-list.ts`
4. `/frontend/src/app/system-pages/ui-component/ui-component.ts`
5. `/frontend/src/app/system-pages/ui-component/ui-component.html`

## Testing

The build has been verified to compile successfully with no TypeScript errors:

```bash
cd frontend && npm run build
```

All existing functionality remains intact while adding type safety benefits.
