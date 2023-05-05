using GBX.NET.Engines.Game;
using GBX.NET.Inputs;
using TmEssentials;

namespace ClipInput.Builders;

class RespawnTapBuilder : GenericKeyTapBuilder<RespawnTM2020>
{
    private readonly ClipInputConfig config;

    public RespawnTapBuilder(IReadOnlyCollection<IInput> inputs, ClipInputConfig config) : base(inputs, config)
    {
        this.config = config;
    }

    protected override bool Enable => config.EnableRespawn;

    protected override CGameCtnMediaBlock Initiate(TimeSingle time, bool pressed) => config.Design.InitiateRespawn(time, pressed);
}
