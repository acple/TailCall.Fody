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
            var tailCallInstructions = body.Instructions.Where(IsTarget).ToArray();

            foreach (var call in tailCallInstructions)
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
    }
}
