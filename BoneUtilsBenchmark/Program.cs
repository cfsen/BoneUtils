using BenchmarkDotNet.Running;
using System;

class Program {
	static void Main(string[] args) {
		Console.WriteLine("Pick benchmark to run:");
		Console.WriteLine("1. Quat Multiplication");
		Console.WriteLine("2. Quat Creation");
		Console.WriteLine("3. Run ALL");
		Console.Write("Choice: ");
		var choice = Console.ReadLine();

		string filter = choice switch {
			"1" => "*QuatMultiplication*",
			"2" => "*QuatCreate*",
			"3" => "*",
			_ => "*", // fallback to all
		};

		BenchmarkSwitcher
			.FromAssembly(typeof(Program).Assembly)
			.Run(new[] { "--filter", filter });
	}
}