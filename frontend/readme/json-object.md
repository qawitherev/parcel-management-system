this readme is made for me to dicsuss the things ive just learnt about json 

### 🌟 Understanding JSON Objects and JSON Strings

#### 🧐 What is JSON?
JSON stands for **JavaScript Object Notation**. It's a lightweight data-interchange format that's easy for humans to read and write, and easy for machines to parse and generate.

#### 🔍 JSON Object vs JSON String
1. At first, these two can be confusing:
    - **JSON Object**: `{ name: 'qawi', age: 25 }`
    - **JSON String**: `'{"name": "qawi", "age": 25}'`  
      👉 Notice the quotes in the JSON string!

---

### 🛠️ What is an Object? (You'll use this a lot!)
An **Object** is a tool to interact with JSON objects. Here are some commonly used methods (using the example object above 👆):

#### 🔑 Famous Methods in Objects
1. **`Object.keys(obj)`**  
    Returns an array of keys:  
    `["name", "age"]`  
    👉 Notice it gives the keys of the object.

2. **`Object.values(obj)`**  
    Returns an array of values:  
    `["qawi", 25]`  
    👉 It gives the values corresponding to the keys.

3. **`Object.entries(obj)`**  
    Returns an array of key-value pairs:  
    `[["name", "qawi"], ["age", 25]]`  
    👉 Each key-value pair is represented as an array.

---

### 🤔 What About Nested Objects?
If the object has nested structures, like arrays or objects inside it, only the first layer is converted. For example:

```javascript
const obj = {
  name: 'qawi',
  age: 25,
  favSubject: ['math', 'english'],
  scoreSubject: { math: 90, english: 88 }
};
```

Using `Object.entries(obj)` results in:  
`[['name', 'qawi'], ['age', 25], ['favSubject', ['math', 'english']], ['scoreSubject', { math: 90, english: 88 }]]`  
👉 Notice that deeper layers (arrays or objects) remain unchanged.

---

### 🔄 Converting Entries Back to JSON Object
To convert entries back into a JSON object, use `Object.fromEntries()`. Example:

```javascript
// Convert to entries
const entries = Object.entries(obj);

// Convert back to JSON object
const object = Object.fromEntries(entries);
```

---

### 🛠️ Use Case: `Object.entries()` in Action
#### 📂 Example: `param-builder.ts`
- **What is it?** A generic HTTP params builder.
- **How does it work?**  
  Records go in, HTTP params come out. Plug it into the `GET` method.  
  Inside, it processes the data by filtering out `null` values. For example:  
  Input: `[["name", null], ["age", 25]]`  
  Output: `[["age", 25]]` (only non-null values are included).

---

### 🌐 Building HTTP Params
To create HTTP params, pass the object using `fromObject`. Example:

```javascript
new HttpParams({ fromObject: Object.fromEntries(entries) });
```

👉 After filtering out `null` values, convert the entries back to an object using `Object.fromEntries()` and plug it into the `HttpParams` constructor.

---

🎉 That's it! JSON and objects are powerful tools—mastering them will make your life as a developer much easier!