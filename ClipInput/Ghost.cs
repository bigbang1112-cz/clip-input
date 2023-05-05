using GBX.NET.Engines.Game;
using GBX.NET.Inputs;

namespace ClipInput;

internal record Ghost(IReadOnlyCollection<IInput> Inputs, CGameCtnGhost? Object = null);
