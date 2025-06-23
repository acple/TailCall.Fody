using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace TailCall.Fody
{
    public class TailCall
    {
        public void AddTailPrefix(MethodDefinition method)
        {
            var body = method.Body;
            var targets = body.Instructions.Where(IsTarget).Where(IsTailable).ToArray();

            if (targets.Length == 0)
                return;

            var ilProcessor = body.GetILProcessor();

            foreach (var call in targets)
            {
                var newCall = DuplicateCallInstruction(call);

                OverwriteInstruction(call, OpCodes.Tail, operand: null);

                ilProcessor.InsertAfter(call, newCall);
            }
        }

        private static Instruction DuplicateCallInstruction(Instruction call)
            => call.OpCode.OperandType switch
            {
                OperandType.InlineMethod => Instruction.Create(call.OpCode, (MethodReference)call.Operand),
                OperandType.InlineSig => Instruction.Create(call.OpCode, (CallSite)call.Operand),
                _ => throw new NotSupportedException(),
            };

        private static void OverwriteInstruction(Instruction instruction, OpCode opCode, object? operand)
        {
            instruction.OpCode = opCode;
            instruction.Operand = operand;
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
            => !IsConstrainedGeneric(call) && call.Operand is IMethodSignature method && !IsValueTypeInstanceMethod(method) && !HasByReferenceParameter(method);

        private static bool IsConstrainedGeneric(Instruction call)
            => call.Previous?.OpCode == OpCodes.Constrained;

        private static bool IsValueTypeInstanceMethod(IMethodSignature method)
            => method.HasThis && method is MethodReference { DeclaringType.IsValueType: true };

        private static bool HasByReferenceParameter(IMethodSignature method)
            => method.Parameters.Any(parameter => parameter.ParameterType.IsByReference);
    }
}
