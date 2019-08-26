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

            foreach (var call in targets)
            {
                var il = body.GetILProcessor();

                // require clone original instruction to reset jump target references
                var clone = Instruction.Create(call.OpCode, call.Operand as MethodReference);
                var tail = Instruction.Create(OpCodes.Tail);
                il.Replace(call, tail);
                il.InsertAfter(tail, clone);
            }
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
