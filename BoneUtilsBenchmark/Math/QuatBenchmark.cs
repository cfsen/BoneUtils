using BenchmarkDotNet.Attributes;
using System.Numerics;

using BoneUtils.Math;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Diagnostics.Windows.Configs;

namespace BoneUtilsBenchmark.Math;

public class QuatBenchmark {
	private Quat q0, q1;
	private Quaternion n0, n1;

	[GlobalSetup]
	public void Setup() {
		q0 = new(){ W=0, X=1, Y=0, Z=0 };
		q1 = new(){ W=0, X=0, Y=1, Z=0 };
		n0 = new(){ W=0, X=1, Y=0, Z=0 };
		n1 = new(){ W=0, X=0, Y=1, Z=0 };
	}
	[BenchmarkCategory("QuatMultiplicationBench"), Benchmark]
	public Quat QuatMultiplication_Quat() {
		return q0*q1;
	}
	[BenchmarkCategory("QuatMultiplicationBench"), Benchmark]
	public Quaternion QuatMultiplication_Native() {
		return n0*n1;
	}
	[BenchmarkCategory("QuatCreate"), Benchmark]
	public Quat QuatCreate_Quat() {
		return new Quat();
	}
	[BenchmarkCategory("QuatCreate"), Benchmark]
	public Quaternion QuatCreate_Native() {
		return new Quaternion();
	}
}
