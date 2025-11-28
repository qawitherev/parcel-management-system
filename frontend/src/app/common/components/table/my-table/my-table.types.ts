/**
 * Table component type definitions
 * This file contains all type-safe interfaces for the MyTable component
 */

/**
 * Type-safe table column configuration
 * @template T - The type of data rows in the table
 * 
 * @example
 * ```typescript
 * interface User {
 *   id: string;
 *   name: string;
 *   email: string;
 * }
 * 
 * const columns: TableColumn<User>[] = [
 *   { key: 'id', label: 'ID', isActionColumn: false },
 *   { key: 'name', label: 'Name', isActionColumn: false },
 *   { key: 'email', label: 'Email', isActionColumn: false },
 * ];
 * ```
 */
export interface TableColumn<T = any> {
    /**
     * The key of the property to display in this column.
     * Can be a direct property key or a dot-notation path for nested properties.
     * For action columns, this can be any string.
     */
    key: keyof T | string;

    /**
     * The display label for the column header
     */
    label: string;

    /**
     * Whether this column contains an action button instead of data
     */
    isActionColumn: boolean;
}

/**
 * Type for table data rows
 * @template T - The type of data in each row
 */
export type TableData<T> = T[];

/**
 * Event emitted when an action button is clicked
 * @template T - The type of the row data
 */
export interface TableActionEvent<T> {
    /**
     * The data of the row where the action was triggered
     */
    data: T;
}
