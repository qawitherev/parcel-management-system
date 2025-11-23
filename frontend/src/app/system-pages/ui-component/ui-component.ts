import { Component } from '@angular/core';
import { MyButton } from "../../common/components/buttons/my-button/my-button";
import { AppConsole } from '../../utils/app-console';
import { TableColumn, MyTable } from '../../common/components/table/my-table/my-table';
import { PaginationEmitData } from '../../common/components/pagination/pagination';
import { MySearchbar } from "../../common/components/searchbar/my-searchbar/my-searchbar";

/**
 * Interface for test user data
 */
interface TestUser {
  id: string;
  name: string;
  age: number;
}

@Component({
  selector: 'app-ui-component',
  imports: [MyButton, MyTable, MySearchbar],
  templateUrl: './ui-component.html',
  styleUrl: './ui-component.css'
})
export class UiComponent {

  tableColumns: TableColumn<TestUser>[] = [
    { key: 'id', label: 'ID', isActionColumn: false },
    { key: 'name', label: 'Name', isActionColumn: false },
    { key: 'age', label: 'Age', isActionColumn: false },
    { key: 'edit', label: 'Edit', isActionColumn: true }
  ];

  testData: TestUser[] = [
    { id: '550e8400-e29b-41d4-a716-446655440000', name: 'Alice', age: 25 },
    { id: '550e8400-e29b-41d4-a716-446655440001', name: 'Bob', age: 30 },
    { id: '550e8400-e29b-41d4-a716-446655440002', name: 'Charlie', age: 22 },
    { id: '550e8400-e29b-41d4-a716-446655440003', name: 'David', age: 28 },
    { id: '550e8400-e29b-41d4-a716-446655440004', name: 'Eve', age: 35 },
    { id: '550e8400-e29b-41d4-a716-446655440005', name: 'Frank', age: 27 },
    { id: '550e8400-e29b-41d4-a716-446655440006', name: 'Grace', age: 24 },
    { id: '550e8400-e29b-41d4-a716-446655440007', name: 'Heidi', age: 32 },
    { id: '550e8400-e29b-41d4-a716-446655440008', name: 'Ivan', age: 29 },
    { id: '550e8400-e29b-41d4-a716-446655440009', name: 'Judy', age: 26 }
  ];

  onClick(event: MouseEvent) {
    AppConsole.log("TEST: hello")
  }

  onTableActionClicked(data: TestUser) {
    AppConsole.log(`DEBUG DATA: \n
      data: ${JSON.stringify(data)}`)
  }

  onPaginationClicked(data: PaginationEmitData) {
    AppConsole.log(`Pagination clicked with data: \n
      ${JSON.stringify(data)}`)
  }

  onSearchBarChanged(data: string) {
    AppConsole.log(`From searchbar: ${data}`)
  }
}
