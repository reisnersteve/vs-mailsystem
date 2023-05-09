using Vintagestory.API.Server;

namespace mailsystem.src.Handlers
{
    public abstract class Handler
    {
        public abstract void Initialize(ICoreServerAPI api);
    }
}
