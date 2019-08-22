using System.Collections.Generic;
using System.Linq;
using Fody;

namespace TailCall.Fody
{
    public class ModuleWeaver : BaseModuleWeaver
    {
        public override void Execute()
        {
            var methods = this.ModuleDefinition.GetTypes()
                .Where(type => !type.IsEnum)
                .SelectMany(type => type.Methods)
                .Where(method => !method.IsConstructor && method.HasBody);

            var tail = new TailCall();

            foreach (var method in methods)
                tail.AddTailPrefix(method);
        }

        public override IEnumerable<string> GetAssembliesForScanning()
            => Enumerable.Empty<string>();

    }
}
