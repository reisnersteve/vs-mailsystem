using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Server;

namespace mailsystem.src.Handlers
{
    public abstract class Handler
    {
        public abstract void Initialize(ICoreServerAPI api);
    }
}
