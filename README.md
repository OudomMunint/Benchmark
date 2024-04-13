# Benchmark (.NET 8)

- A C# console program that gathers your system details then lets you run benchmarks on your system.
- You can run a Hashing benchmark with MD5, SHA256 and SHA512.
- Or a single thread and multi thread benchmark.
- Or a encrypt/decrypt benchmark (May need admin privileges)
- Using <a href="https://github.com/dotnet/BenchmarkDotNet"> `BenchmarkDotNet` </a>, `SharpDX` and `NvAPIWrapper`.

# Maintenance

![maintenance-status](https://img.shields.io/badge/maintenance-actively--developed-brightgreen.svg)

# CI/CD & CodeQL

[![CI Build](https://github.com/OudomMunint/Benchmark/actions/workflows/main.yml/badge.svg?branch=master)](https://github.com/OudomMunint/Benchmark/actions/workflows/main.yml) [![CodeQL](https://github.com/OudomMunint/Benchmark/actions/workflows/codeql.yml/badge.svg)](https://github.com/OudomMunint/Benchmark/actions/workflows/codeql.yml)

# Getting Started
- Install `.NET 8 SDK` from <a href="https://dotnet.microsoft.com/download/dotnet/8.0"> `here` </a>
- Open the solution and set as startup project
- Run the benchmark in `Release` mode.
- Check your if system specs is correct
- `Y` to continue
- Use `1`, `2` or `3` to select which benchmarks to run
- Use `4` to run all benchmarks
- For `VSCode` you will need to install the `C#` extention for vscode
- For `VSCode` you also need to create `launch.JSON` and `task.JSON` files if you want to run in different configurations.
- If not you can use the provided JSON files.

# Running on OSX
- Install `.NET 8 SDK` for macOS from <a href="https://dotnet.microsoft.com/download/dotnet/8.0"> `here` </a>
- Open the solution and set as startup project.
- Run the benchmark in `Release` mode.
- If the app is terminated, open Benchmark.sln or csproj in terminal.
- Or `cd` into the `Benchmark` folder and run `dotnet run -c Release`

# Running the EXE

- Open the solution with preferred IDE
- Set the startup project to `Benchmark`
- Build with `dotnet build -c Release`
- Publish with `dotnet publish -c Release`
- Run `Benchmark.exe` in the `C:\Users\<Path to project>\Benchmark\bin\Release\net8.0\publish\` folder.

# Required SDKs

- .NET 8.0.2 from <a href="https://dotnet.microsoft.com/download/dotnet/8.0"> `here` </a>

# Output

<table>
  <tr>
    <td> <h3>Windows 11</h3> </td>
    <td> <h3>MacOS Ventura</h3>  </td>
  </tr>
  <tr>
    <td> <img src="results.png"/> </td>
    <td> <img src="macos.png"/> </td>
  </tr>
</table>

- Scroll down to see results.
- `Runtime` in `seconds(s)` should be the benchmark.

# Compare your results to mine!

## MacBookPro 16" 2021 `macOS 13.6`

```ini
Apple M1 Max 10/32, 1 CPU, 10 logical and 10 physical cores (8P/2E)
```

- Runtime: `98s`

| Method |     Mean |   Error |   StdDev |
| ------ | -------: | ------: | -------: |
| Sha256 | 334.8 ns | 2.10 ns |  9.11 ns |
| Sha512 | 560.0 ns | 2.89 ns | 14.21 ns |
| Md5    | 412.2 ns | 8.53 ns | 10.11 ns |

## MacBookPro 14" 2023 `macOS 13.6`

```ini
Apple M2 Pro 10/16, 1 CPU, 10 logical and 10 physical cores (6P/4E)
```

- Runtime: `92s`

| Method |     Mean |   Error |   StdDev |
| ------ | -------: | ------: | -------: |
| Sha256 | 229.9 ns | 2.91 ns |  8.81 ns |
| Sha512 | 553.0 ns | 1.59 ns | 12.18 ns |
| Md5    | 341.1 ns | 7.42 ns |  9.71 ns |

## MacBookPro 13" 2017 `MacOS 13`

```ini
Intel Core i5-7660U CPU 2.20GHz (Kaby Lake), 1 CPU, 4 logical and 2 physical cores (2P/0E)
```

- Runtime: `573s`

| Method |     Mean |    Error |   StdDev |
| ------ | -------: | -------: | -------: |
| Sha256 | 751.3 us | 12.29 us | 39.97 us |
| Sha512 | 455.8 us |  1.81 us |  1.23 us |
| Md5    | 361.9 us |  3.42 us |  3.02 us |

## MacBookPro 15" 2018 `Windows 10 bootcamp`

```ini
Intel Core i7-8850H CPU 2.60GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores (6P/0E)
```

- Runtime: `216s`

| Method |     Mean |   Error |  StdDev |
| ------ | -------: | ------: | ------: |
| Sha256 | 407.1 μs | 3.20 μs | 3.00 μs |
| Sha512 | 232.6 μs | 4.48 μs | 4.19 μs |
| Md5    | 174.0 μs | 2.08 μs | 1.94 μs |

## MacBookPro 15" 2018 `MacOS 13`

```ini
Intel Core i7-8850H CPU 2.60GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores (6P/0E)
```

- Runtime: `191s`

| Method |     Mean |   Error |   StdDev |
| ------ | -------: | ------: | -------: |
| Sha256 | 240.3 us | 4.79 us | 13.97 us |
| Sha512 | 150.2 us | 0.63 us |  0.59 us |
| Md5    | 161.7 us | 1.21 us |  1.07 us |

## Desktop

```ini
Intel Core i7-8700K CPU 3.70GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores (6P/0E)
```

- Runtime: `151s`

| Method |     Mean |   Error |  StdDev |
| ------ | -------: | ------: | ------: |
| Sha256 | 301.1 μs | 3.50 μs | 3.11 μs |
| Sha512 | 272.1 μs | 4.54 μs | 7.51 μs |
| Md5    | 199.8 μs | 3.18 μs | 1.28 μs |

## Dell latitude 5531

```ini
Intel Core i7-12800H CPU 1.80GHz (Alder Lake), 1 CPU, 20 logical and 14 physical cores (6P/8E)
```

- Runtime: `46s`

| Method |     Mean |   Error |  StdDev |
| ------ | -------: | ------: | ------: |
| Sha256 | 192.9 ns | 3.86 ns | 5.28 ns |
| Sha512 | 449.0 ns | 8.52 ns | 7.97 ns |
| Md5    | 271.1 ns | 5.49 ns | 7.14 ns |

## .NET 7 Ranking:

1. Dell latitude 5531 - i7-12700H @ 55W `46s`
2. MacBook Pro 14" 2023 - M2 Pro 10 Core CPU (6P + 4E) `92s`
3. MacBook Pro 16" 2021 - M1 Max 10 Core CPU (8P + 2E) `98s`
4. Desktop - i7-8700K @ 4.7ghz `151s`
5. MacBook Pro 15" 2018 - i7-8850H @ 45W `191s`
6. MacBook Pro 13" 2017 - i5-7660U @ 15W `573s`

## .NET 8 Ranking:

Testing...

# Minimum system requirements

- .NET 8.0.2
- Dual core CPU
- Windows 10 or MacOS 12
- 4GB RAM
- 1GB Storage
