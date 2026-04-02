using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;
using Framework.IO;

namespace HermesProxy.Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
public class ByteBufferGetDataBenchmarks
{
    private ByteBuffer _smallBuffer = null!;
    private ByteBuffer _mediumBuffer = null!;
    private ByteBuffer _largeBuffer = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Small: 64 bytes (typical small packet)
        _smallBuffer = new ByteBuffer();
        var smallData = new byte[64];
        new Random(42).NextBytes(smallData);
        _smallBuffer.WriteBytes(smallData);

        // Medium: 1KB (typical medium packet)
        _mediumBuffer = new ByteBuffer();
        var mediumData = new byte[1024];
        new Random(42).NextBytes(mediumData);
        _mediumBuffer.WriteBytes(mediumData);

        // Large: 64KB (large packet/update)
        _largeBuffer = new ByteBuffer();
        var largeData = new byte[65536];
        new Random(42).NextBytes(largeData);
        _largeBuffer.WriteBytes(largeData);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _smallBuffer?.Dispose();
        _mediumBuffer?.Dispose();
        _largeBuffer?.Dispose();
    }

    // ========== Small Buffer (64 bytes) ==========

    [Benchmark(Baseline = true)]
    public byte[] Small_Original()
    {
        return _smallBuffer.GetDataOriginal();
    }

    [Benchmark]
    public byte[] Small_Optimized()
    {
        return _smallBuffer.GetData();
    }

    // ========== Medium Buffer (1KB) ==========

    [Benchmark]
    public byte[] Medium_Original()
    {
        return _mediumBuffer.GetDataOriginal();
    }

    [Benchmark]
    public byte[] Medium_Optimized()
    {
        return _mediumBuffer.GetData();
    }

    // ========== Large Buffer (64KB) ==========

    [Benchmark]
    public byte[] Large_Original()
    {
        return _largeBuffer.GetDataOriginal();
    }

    [Benchmark]
    public byte[] Large_Optimized()
    {
        return _largeBuffer.GetData();
    }
}

[MemoryDiagnoser]
[ShortRunJob]
public class ByteBufferReadCStringBenchmarks
{
    private byte[] _shortStringData = null!;
    private byte[] _mediumStringData = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Short string: "Hello" (5 chars)
        _shortStringData = CreateCStringData("Hello");

        // Medium string: 100 chars
        _mediumStringData = CreateCStringData(new string('A', 100));
    }

    private static byte[] CreateCStringData(string str)
    {
        var bytes = Encoding.UTF8.GetBytes(str);
        var data = new byte[bytes.Length + 1];
        bytes.CopyTo(data, 0);
        data[^1] = 0x00;
        return data;
    }

    // ========== Short String (5 chars) ==========

    [Benchmark(Baseline = true)]
    public string Short_Original()
    {
        using var buffer = new ByteBuffer(_shortStringData);
        return buffer.ReadCStringOriginal();
    }

    [Benchmark]
    public string Short_Optimized()
    {
        using var buffer = new ByteBuffer(_shortStringData);
        return buffer.ReadCString();
    }

    // ========== Medium String (100 chars) ==========

    [Benchmark]
    public string Medium_Original()
    {
        using var buffer = new ByteBuffer(_mediumStringData);
        return buffer.ReadCStringOriginal();
    }

    [Benchmark]
    public string Medium_Optimized()
    {
        using var buffer = new ByteBuffer(_mediumStringData);
        return buffer.ReadCString();
    }
}
