# CSVParser Unity3D

The CSVParser package is a simple utility for reading and writing CSV files in Unity3D. It provides a CSVParser class for parsing CSV files and a CSVParserDemo class for demonstrating its usage.

## Installation

1. Download the provided CSVParser package.
2. Import the package into your Unity project.

## CSVParser Class

### Singleton Pattern

The CSVParser class is implemented as a singleton to ensure that only one instance exists throughout the project.

```csharp
namespace CSVToolKit {
    public class CSVParser : MonoBehaviour {
        // ... (class implementation)
    }
}
```

### Properties

- `lineSeperater`: The character used as the line separator (default is '\n').
- `fieldSeperator`: The character used as the field separator (default is ',').

### Methods

- `SetLineSeparator(char seperator)`: Set the line separator character.
- `SetFieldSeparator(char separator)`: Set the field separator character.
- `ReadData(string path, string filename)`: Read CSV data from a file.
- `AddData(string path, string filename, List<string> values)`: Add data to an existing CSV file.

### Usage Example

```csharp
// Reading CSV data
List<List<string>> data = CSVParser.Instance.ReadData("", "Data.csv");

// Adding data to CSV
CSVParser.Instance.AddData("", "Data.csv", new List<string>{"value1", "value2"});
```

## CSVParserDemo Class

The CSVParserDemo class demonstrates the usage of the CSVParser class.

```csharp
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using CSVToolKit;

public class CSVParserDemo : MonoBehaviour {
    // ... (class implementation)
}
```

### Properties

- `fileName`: The name of the CSV file to read and write.
- `rollNoInputField`: Unity UI InputField for roll number input.
- `nameInputField`: Unity UI InputField for name input.
- `contentArea`: Unity UI Text for displaying CSV content.

### Methods

- `Start()`: Unity callback for initialization, calls `ReadCSVFile()`.
- `ReadCSVFile()`: Reads and displays CSV data in the `contentArea`.
- `AddContents()`: Adds new data to the CSV file and updates the display.

### Usage Example

```csharp
// Reading CSV file on Start
void Start() {
    ReadCSVFile();
}

// Adding new data to CSV
public void AddContents() {
    CSVParser.Instance.AddData("", "Data.csv", new List<string>{"rollNo", "name"});
    ReadCSVFile();
}
```

The CSVParser package provides a convenient way to handle CSV files in Unity3D with minimal UI requirements. Use the CSVParser class for parsing and writing CSV files, and refer to the CSVParserDemo class for integration examples.
