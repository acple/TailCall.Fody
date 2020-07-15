using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace TailCall.Fody
{
    public class TailCall
    {
        private static readonly IEnumerable<OpCode> Jumps = new[]
        {
            OpCodes.Beq, OpCodes.Beq_S,
            OpCodes.Bge, OpCodes.Bge_S, OpCodes.Bge_Un, OpCodes.Bge_Un_S,
            OpCodes.Bgt, OpCodes.Bgt_S, OpCodes.Bgt_Un, OpCodes.Bgt_Un_S,
            OpCodes.Ble, OpCodes.Ble_S, OpCodes.Ble_Un, OpCodes.Ble_Un_S,
            OpCodes.Blt, OpCodes.Blt_S, OpCodes.Blt_Un, OpCodes.Blt_Un_S,
            OpCodes.Bne_Un, OpCodes.Bne_Un_S,
            OpCodes.Br, OpCodes.Br_S,
            OpCodes.Brfalse, OpCodes.Brfalse_S,
            OpCodes.Brtrue, OpCodes.Brtrue_S,
        };

        public void AddTailPrefix(MethodDefinition method)
        {
            var body = method.Body;
            var targets = body.Instructions.Where(IsTarget).Where(IsTailable).ToArray();

            foreach (var call in targets)
            {
                var jumps = body.Instructions.Where(x => Jumps.Contains(x.OpCode) && x.Operand.Equals(call)).ToArray();
                var tail = Instruction.Create(OpCodes.Tail);

                body.GetILProcessor().InsertBefore(call, tail);

                foreach (var jump in jumps)
                    jump.Operand = tail;

                MoveSequencePoint(method.DebugInformation, call, tail);
            }
        }

        private static void MoveSequencePoint(MethodDebugInformation debugInformation, Instruction from, Instruction to)
        {
            if (debugInformation.GetSequencePoint(from) is not { } current)
                return;

            var moved = new SequencePoint(to, current.Document)
            {
                StartLine = current.StartLine,
                StartColumn = current.StartColumn,
                EndLine = current.EndLine,
                EndColumn = current.EndColumn,
            };

            var sequencePoints = debugInformation.SequencePoints;

            sequencePoints.Remove(current);
            sequencePoints.Add(moved);
        }

        private static bool IsTarget(Instruction instruction)
            => IsCall(instruction.OpCode) && IsRet(instruction.Next.OpCode) && !IsTailed(instruction);

        private static bool IsCall(OpCode opCode)
            => opCode == OpCodes.Call || opCode == OpCodes.Callvirt || opCode == OpCodes.Calli;

        private static bool IsRet(OpCode opCode)
            => opCode == OpCodes.Ret;

        private static bool IsTailed(Instruction call)
            => call.Previous?.OpCode == OpCodes.Tail;

        private static bool IsTailable(Instruction call)
            => !IsConstrainedGeneric(call) && call.Operand is MethodReference method && !IsValueTypeInstanceMethod(method) && !HasByReferenceParameter(method);

        private static bool IsConstrainedGeneric(Instruction call)
            => call.Previous?.OpCode == OpCodes.Constrained;

        private static bool IsValueTypeInstanceMethod(MethodReference method)
            => method.HasThis && method.DeclaringType.IsValueType;

        private static bool HasByReferenceParameter(MethodReference method)
            => method.Parameters.Any(parameter => parameter.ParameterType.IsByReference);
    }
}
