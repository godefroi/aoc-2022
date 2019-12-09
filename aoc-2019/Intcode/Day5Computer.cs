﻿using System;
using System.Collections.Generic;
using System.Text;

namespace aoc_2019.Intcode
{
	internal class Day5Computer : Day2Computer
	{
		[OpCode(3)]
		protected void GetInput(long address)
		{
			Core[address] = Input.Next();
		}

		[OpCode(4)]
		protected void AddOutput(long address)
		{
			Output.AddOutput(Core[address]);
		}

		[OpCode(5)]
		protected void JumpIfTrue(long valueAddress, long jumpAddress, out long? destination)
		{
			destination = Core[valueAddress] != 0 ? Core[jumpAddress] : default(long?);
		}

		[OpCode(6)]
		protected void JumpIfFalse(long valueAddress, long jumpAddress, out long? destination)
		{
			destination = Core[valueAddress] == 0 ? Core[jumpAddress] : default(long?);
		}

		[OpCode(7)]
		protected void LessThan(long value1Address, long value2Address, long outputAddress)
		{
			Core[outputAddress] = Core[value1Address] < Core[value2Address] ? 1 : 0;
		}

		[OpCode(8)]
		protected void Equals(long value1Address, long value2Address, long outputAddress)
		{
			Core[outputAddress] = Core[value1Address] == Core[value2Address] ? 1 : 0;
		}
	}
}
