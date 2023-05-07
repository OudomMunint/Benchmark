``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 10 (10.0.19045.2788/22H2/2022Update)
Intel Core i7-8850H CPU 2.60GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET SDK=7.0.203
  [Host]     : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2 [AttachedDebugger]
  DefaultJob : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2


```
| Method |     Mean |   Error |  StdDev |
|------- |---------:|--------:|--------:|
| Sha256 | 407.1 μs | 3.20 μs | 3.00 μs |
| Sha512 | 232.6 μs | 4.48 μs | 4.19 μs |
|    Md5 | 174.0 μs | 2.08 μs | 1.94 μs |
