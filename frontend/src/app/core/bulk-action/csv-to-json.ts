import { from, Observable } from 'rxjs';

interface CsvParserOption {
  hasHeader: boolean;
  delimiter: string;
  skipEmptyLine: boolean;
  trimContent: boolean;
}

export interface CsvValidationResult {
  valid: boolean;
  message: string;
}

export function parcelToCsvJsonParent<T>(file: File, option: CsvParserOption): Observable<T[]> {
  return from(parcelCsvToJson<T>(file, option));
}

async function parcelCsvToJson<T>(file: File, option: CsvParserOption): Promise<T[]> {
  //validate the file
  const csvText = await getCsvText(file);
  const isCsvValid = await validateCsv(file, csvText);
  if (!isCsvValid.valid) {
    throw Error(isCsvValid.message);
  }

  const csvLines = csvText.split(/\r?\n/);

  // getting the header, ['header1', 'header2', 'header3']
  const headers = option.hasHeader
    ? parseLine(csvLines[0], option)
    : generateGenericHeader(csvLines.length);

  const startIndex = option.hasHeader ? 0 : 1;

  let parsedCsv: T[] = [];

  for (let i = startIndex; i < csvLines.length; i++) {
    const parsedLine = parseLine(csvLines[i], option);
    const obj: any = {};
    headers.forEach((header, index) => {
      obj[header] = parsedLine[i];
    });
    parsedCsv.push(obj as T);
  }

  return parsedCsv;
}

async function getCsvText(file: File): Promise<string> {
  return await file.text();
}

function parseLine(lineText: string, option: CsvParserOption): string[] {
  // for the first iteration, we will just keep it simple
  // no quote escape, no fancy thing
  let result: string[] = [];
  let current = '';
  for (let i = 0; i < lineText.length; i++) {
    let currentChar = lineText[i];
    let nextChar = lineText[i + 1];
    if (nextChar == option.delimiter && result.length != 0) {
      result.push(current);
      current = '';
      i++; // skip the delimiter as current char
    } else {
      current += currentChar;
    }
  }

  result.push(current);

  return result;
}

function generateGenericHeader(count: number): string[] {
  return Array.from({ length: count }, (_, i) => `Column ${i + 1}`);
}

async function validateCsv(file: File, csvText: string): Promise<CsvValidationResult> {
  // check MIME type
  if (file.type !== 'text/csv') {
    return {
      valid: false,
      message: 'File is not a csv file',
    };
  }

  // check if file empty
  if (csvText.length == 0) {
    return {
      valid: false,
      message: 'File is empty',
    };
  }

  // file is valid
  return {
    valid: true,
    message: 'File is valid',
  };
}
