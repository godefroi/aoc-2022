﻿using aoc_2019.Intcode.Infrastructure;

using System.Reflection;
using System.Text.RegularExpressions;

namespace aoc_2019.Intcode;

internal partial class IntcodeComputer
{
	private static readonly List<int> _instructions;

	private readonly SparseArray<long> _memory;

	private long _instructionPointer = 0;

	static IntcodeComputer()
	{
		_instructions = new List<int>();

		var instructionMethods = typeof(IntcodeComputer)
			.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
			.Select(mi => (method: mi, match: InstructionRegex().Match(mi.Name)))
			.Where(p => p.match.Success)
			.Select(p => (p.method, opcode: long.Parse(p.match.Groups["opcode"].ValueSpan), name: p.match.Groups["name"].Value));

		foreach (var (method, opcode, name) in instructionMethods) {
			Console.WriteLine($"{name} ({opcode}) -> {method.Name} ({method.GetParameters().Length} parameters)");
		}
	}

	public IntcodeComputer(IEnumerable<long> program)
	{
		_memory = new SparseArray<long>(program);
	}

	public IntcodeComputer(IEnumerable<string> program) : this(program.Select(long.Parse)) { }

	public IntcodeComputer(string program) : this(program.Split(',')) { }

	public bool Terminated { get; private set; }

	public long? GetMemoryValue(long index) => _memory[index];

	public InterruptType Resume()
	{
		if (_memory == null) {
			throw new InvalidOperationException("Memory has not been initialized.");
		}

		if (Terminated) {
			throw new InvalidOperationException("The program has already terminated.");
		}

		while (true) {
			// make sure we didn't blow past the end of the program somehow
			if (_instructionPointer > _memory.Length) {
				throw new InvalidOperationException("Executed past end of program");
			}

			var currentOpCode = _memory[_instructionPointer];

			// opcode 99 is program end
			if (currentOpCode == 99) {
				Terminated = true;
				return InterruptType.Terminated;
			}

			// decode the opcode into an instruction
			var op = Decode(currentOpCode);

			// execute the instruction
			op(_memory[_instructionPointer + 1], _memory[_instructionPointer + 2], _memory[_instructionPointer + 3]);

			// advance the instruction pointer
			_instructionPointer += 4;

			//// get the instruction and build the array of parameters
			//var (inst, modes) = DecodeInstruction(_memory[_instructionPointer]);
			//var prms = new long[inst.ParameterCount];

			//// set the paramters based on the mode for each
			//for (var i = 0; i < prms.Length; i++) {
			//	prms[i] = modes[i] switch {
			//		ParameterMode.Immediate => _instructionPointer + 1 + i,
			//		ParameterMode.Position => _memory[_instructionPointer + 1 + i],
			//		ParameterMode.Relative => RelativeBase + _memory[_instructionPointer + 1 + i],
			//		_ => throw new InvalidOperationException($"Unknown parameter mode {modes[i]}")
			//	};
			//}

			//var jmp = default(long?);
			//var outp = false;

			//// execute the instruction
			//try {
			//	jmp = inst.Execute(prms);
			//} catch (TargetInvocationException ex) {
			//	switch (ex.InnerException) {
			//		case InputNeededException _:
			//			return InterruptType.Input;
			//		case OutputReadyException _:
			//			outp = true;
			//			break;
			//		default:
			//			throw ex.InnerException;
			//	}
			//}

			//// either execute the jump, or advance past the opcode plus parameters
			//if (jmp.HasValue)
			//	_instructionPointer = jmp.Value;
			//else
			//	_instructionPointer += inst.ParameterCount + 1;

			//// currently this won't happen, but we can make it happen if we need it
			//if (outp)
			//	return InterruptType.Output;
		}
	}

	public override string ToString() => _memory.ToString();

	private Action<long, long, long> Decode(long opCode)
	{
		switch (opCode) {
			case 1: return Op01Add;
			case 2: return Op02Multiply;
			default : throw new NotImplementedException();
		}
	}

	internal readonly record struct Instruction(long opCode);
	//private (Instruction Instruction, List<ParameterMode> Modes) DecodeInstruction(long opCode)
	//{
	//	var ocstr   = opCode.ToString().PadLeft(2, '0');
	//	var inst_id = Convert.ToInt32(ocstr.Substring(ocstr.Length - 2));
	//	var modes   = ocstr.Reverse().Skip(2).Select(c => (ParameterMode)Convert.ToInt32(c.ToString())).ToList();

	//	// make sure we understand the opcode
	//	if (!Instructions.ContainsKey(inst_id))
	//		throw new InvalidOperationException($"The opcode {inst_id} is not valid for this implementation.");

	//	// get the instruction
	//	var instr = Instructions[inst_id];

	//	// add enough default modes to cover all the parameters
	//	while (modes.Count < instr.ParameterCount)
	//		modes.Add(ParameterMode.Position);

	//	// address parameters we treat as immediate
	//	//foreach( var ap in instr.AddressParameters )
	//	//	modes[ap] = ParameterMode.Immediate;

	//	return (instr, modes);
	//}


	public enum InterruptType
	{
		Input,
		Output,
		Terminated
	}

	[GeneratedRegex("Op(?<opcode>\\d+)(?<name>.*)")]
	private static partial Regex InstructionRegex();
}
