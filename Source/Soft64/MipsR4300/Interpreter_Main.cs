﻿/*
Soft64 - C# N64 Emulator
Copyright (C) Soft64 Project @ Codeplex
Copyright (C) 2013 - 2014 Bryan Perris

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>
*/

namespace Soft64.MipsR4300
{
    public partial class Interpreter
    {
        [OpcodeHook("CACHE")]
        private void Inst_Cache(MipsInstruction inst)
        {
            logger.Debug("Cache instruction ignored");
        }

        [OpcodeHook("BREAK")]
        private void Inst_Break(MipsInstruction inst)
        {
            MipsState.CP0Regs.CauseReg.ExceptionType = ExceptionCode.Breakpoint;
        }

        [OpcodeHook("MFHI")]
        private void Inst_Mfhi(MipsInstruction inst)
        {
            MipsState.WriteGPRUnsigned(inst.Rd, MipsState.Hi);
        }

        [OpcodeHook("MFLO")]
        private void Inst_Mflo(MipsInstruction inst)
        {
            MipsState.WriteGPRUnsigned(inst.Rd, MipsState.Lo);
        }

        [OpcodeHook("MTHI")]
        private void Inst_Mthi(MipsInstruction inst)
        {
            MipsState.Hi = MipsState.ReadGPRUnsigned(inst.Rs);
        }

        [OpcodeHook("MTLO")]
        private void Inst_Mtlo(MipsInstruction inst)
        {
            MipsState.Lo = MipsState.ReadGPRUnsigned(inst.Rs);
        }
    }
}