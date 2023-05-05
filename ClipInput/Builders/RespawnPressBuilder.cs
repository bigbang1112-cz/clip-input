using GBX.NET.Engines.Game;
using GBX.NET.Inputs;
using TmEssentials;

namespace ClipInput.Builders;

class RespawnPressBuilder : GenericKeyBuilder<Respawn>
{
    private readonly ClipInputConfig config;

    public RespawnPressBuilder(IReadOnlyCollection<IInput> inputs, ClipInputConfig config) : base(inputs, config)
    {
        this.config = config;
    }

    protected override bool Enable => config.EnableRespawn;

    protected override CGameCtnMediaBlock Initiate(TimeSingle time, bool pressed) => config.Design.InitiateRespawn(time, pressed);
}
