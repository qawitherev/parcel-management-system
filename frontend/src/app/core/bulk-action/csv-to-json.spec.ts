import { parcelToCsvJsonParent } from './csv-to-json';
import { take } from 'rxjs/operators';

describe('parcelToCsvJsonParent', () => {
  function createCsvFile(content: string, type: string = 'text/csv'): File {
    return new File([content], 'test.csv', { type });
  }

  const option = {
    hasHeader: true,
    delimiter: ',',
    skipEmptyLine: false,
    trimContent: false,
  };

  it('should parse a valid CSV with header', async () => {
    const csv = 'name,age\nJohn,30\nJane,25';
    const file = createCsvFile(csv);
    const result$ = parcelToCsvJsonParent<{ name: string; age: string }>(file, option);
    const result = await result$.pipe(take(1)).toPromise();
    expect(result).toEqual([
      { name: 'John', age: '30' },
      { name: 'Jane', age: '25' },
    ]);
  });

  it('should throw error for non-csv file type', async () => {
    const file = createCsvFile('name,age\nJohn,30', 'text/plain');
    const result$ = parcelToCsvJsonParent<{ name: string; age: string }>(file, option);
    await expectAsync(result$.pipe(take(1)).toPromise()).toBeRejectedWithError('File is not a csv file');
  });

  it('should throw error for empty file', async () => {
    const file = createCsvFile('');
    const result$ = parcelToCsvJsonParent<{ name: string; age: string }>(file, option);
    await expectAsync(result$.pipe(take(1)).toPromise()).toBeRejectedWithError('File is empty');
  });

  it('should parse CSV without header', async () => {
    const csv = 'John,30\nJane,25';
    const file = createCsvFile(csv);
    const optionNoHeader = { ...option, hasHeader: false };
    const result$ = parcelToCsvJsonParent<{ Column1: string; Column2: string }>(file, optionNoHeader);
    const result = await result$.pipe(take(1)).toPromise();
    expect(result).toEqual([
      { Column1: 'John', Column2: '30' },
      { Column1: 'Jane', Column2: '25' },
    ]);
  });
});
