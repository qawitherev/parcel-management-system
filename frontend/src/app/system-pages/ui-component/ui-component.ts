import { Component } from '@angular/core';
import { MyButton } from "../../common/components/buttons/my-button/my-button";
import { AppConsole } from '../../utils/app-console';
import { TableColumn, MyTable } from '../../common/components/table/my-table/my-table';

interface TestData {
  id: string, 
  name: string, 
  age: number
}

@Component({
  selector: 'app-ui-component',
  imports: [MyButton, MyTable],
  templateUrl: './ui-component.html',
  styleUrl: './ui-component.css'
})
export class UiComponent {

  tableColumns: TableColumn<TestData>[] = [
    { key: 'id', label: 'ID'}, 
    { key: 'name', label: 'Name'}, 
    { key: 'age', label: 'Age'}
  ];

  TestDataData: TestData[] = [
    { id: '1', name: 'Alice', age: 25 },
    { id: '2', name: 'Bob', age: 30 },
    { id: '3', name: 'Charlie', age: 22 },
    { id: '4', name: 'David', age: 28 },
    { id: '5', name: 'Eve', age: 35 },
    { id: '6', name: 'Frank', age: 27 },
    { id: '7', name: 'Grace', age: 24 },
    { id: '8', name: 'Heidi', age: 32 },
    { id: '9', name: 'Ivan', age: 29 },
    { id: '10', name: 'Judy', age: 26 }
  ];

  onClick(event: MouseEvent) {
    AppConsole.log("TEST: hello")
  }
}
