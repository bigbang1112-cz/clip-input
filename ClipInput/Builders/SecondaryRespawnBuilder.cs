using GBX.NET.Engines.Game;
using GBX.NET.Inputs;
using TmEssentials;

namespace ClipInput.Builders;

class SecondaryRespawnBuilder : GenericKeyTapBuilder<SecondaryRespawn>
{
    private readonly ClipInputConfig config;

    public SecondaryRespawnBuilder(IReadOnlyCollection<IInput> inputs, ClipInputConfig config) : base(inputs, config)
    {
        this.config = config;
    }

    protected override bool Enable => config.EnableSecondaryRespawn;

    protected override CGameCtnMediaBlock Initiate(TimeSingle time, bool pressed) => config.Design.InitiateSecondaryRespawn(time, pressed);
}
