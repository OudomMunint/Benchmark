# Benchmark (.NET 8)

- A C# console program displays system specs then lets you run benchmarks on your system.
- This program will attempt to get your CPU,RAM & GPU specs.
- You can run a Hashing benchmark with MD5, SHA256 and SHA512.
- Or an intensive CPU benchmark.
- Or an encrypt/decrypt benchmark (May need admin privileges)
- Using <a href="https://github.com/dotnet/BenchmarkDotNet"> `BenchmarkDotNet` </a>, `SharpDX`, `NvAPIWrapper` and `Hardware.info`

# Maintenance

![maintenance-status](https://img.shields.io/badge/maintenance-passively--maintained-yellowgreen.svg)

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

# Debugging
- Set a breakpoint anywhere.
- Run the program in `Debug` mode.
- Use option `5` to start debugging.
- Select the benchmark you want to debug.
- The program will pause at the breakpoint.

# Output

<table>
  <tr>
    <td> <h3>Windows 11</h3> </td>
    <td> <h3>MacOS Ventura</h3>  </td>
  </tr>
  <tr>
    <td> <img src="results.png"/> </td>
    <td> <img src="macos.png" width="700"/> </td>
  </tr>
</table>

- Scroll down to see results.
- `Runtime` in `seconds(s)` should be the benchmark.
- `Global Runtime` in `seconds(s)` can also be the benchmark.
- There might be a 20 seconds delay on first use due to hardware detection by `Hardware.Info`.

# Specs for tested systems.

## MacBookPro 16" 2021

```ini
Apple M1 Max 10/32, 10 Cores 10 Threads (8P/2E)
32GB LPDDR5 6400MHz
macOS 13.6
```

## MacBookPro 14" 2023

```ini
Apple M2 Pro 10/16, 10 Cores 10 Threads (6P/4E)
16GB LPDDR5 6400MHz
macOS 13.6
```

## MacBookPro 13" 2017

```ini
Intel Core i5-7660U CPU 2.20GHz (Kaby Lake), 2 Cores 4 Threads (2P/0E)
8GB DDR3 2133MHz
macOS 12
```

## MacBookPro 15" 2018

```ini
Intel Core i7-8850H CPU 2.60GHz (Coffee Lake), 6 Cores 12 Threads (6P/0E)
16GB DDR4 2400MHz
Windows 10 bootcamp
```

## MacBookPro 15" 2018

```ini
Intel Core i7-8850H CPU 2.60GHz (Coffee Lake), 6 Cores 12 Threads (6P/0E)
16GB DDR4 2400MHz
macOS 13
```

## Desktop PC

```ini
Intel Core i7-8700K CPU 3.70GHz (Coffee Lake), 6 Cores 12 Threads (6P/0E)
16GB DDR4 3600MHz
Windows 11
```

## Workstation

```ini
Ryzen Thread Ripper 1950X CPU 3.9GHz (Zen 1), 16 Cores 32 Threads (16P/0E)
32GB DDR4 3400MHz
Windows 10
```

## Dell latitude 5531

```ini
Intel Core i7-12800H CPU 1.80GHz (Alder Lake), 14 Cores 20 Threads (6P/8E)
```

## .NET 7 Ranking:

1. Dell latitude 5531 - i7-12800H @ 55W `46s`
2. Desktop ThreadRipper - Ryzen Thread Ripper 1950X @ 3.9GHz `49s`
3. MacBook Pro 14" 2023 - M2 Pro 10 Core CPU (6P + 4E) `92s`
4. MacBook Pro 16" 2021 - M1 Max 10 Core CPU (8P + 2E) `98s`
5. Desktop i7 - i7-8700K @ 4.7ghz `151s`
6. MacBook Pro 15" 2018 - i7-8850H @ 45W `191s`
7. MacBook Pro 13" 2017 - i5-7660U @ 15W `573s`

## .NET 8 Ranking:

1. Dell latitude 5531 - i7-12800H @ 55W `32s`
2. MacBook Pro 14" 2023 - M2 Pro 10 Core CPU (6P + 4E) `35s`
3. Desktop ThreadRipper - Ryzen Thread Ripper 1950X @ 3.9GHz `38s`
4. MacBook Pro 16" 2021 - M1 Max 10 Core CPU (8P + 2E) `42s`
5. Desktop i7 - i7-8700K @ 4.7ghz `105s`
6. MacBook Pro 15" 2018 - i7-8850H @ 45W `133s`
7. MacBook Pro 13" 2017 - i5-7660U @ 15W `401s`

# Minimum system requirements

- .NET 8.0.2
- Dual core CPU
- Windows 10 or MacOS 12
- 4GB RAM
- 1GB Storage