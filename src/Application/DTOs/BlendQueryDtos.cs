namespace WineFilesApi.Application.DTOs;

// -------------------- GetBlendVessels --------------------
public sealed class BlendVesselDto
{
    public string Vessel { get; set; } = string.Empty;      // vessel.fcvessel
    public string Batch { get; set; } = string.Empty;       // opmaster.fcbatch
    public string Status { get; set; } = string.Empty;      // opmaster.fcstatus
    public int Volume { get; set; }                         // opmaster.fnvolume
    public int LastCompletedOp { get; set; }                // vessel.fnlastcmop
    public int LastOp { get; set; }                         // vessel.fnlastop
    public string Multi { get; set; } = string.Empty;       // opmaster.fcmulti
}

// -------------------- GetBlendComposition --------------------
public sealed class BlendCompositionRowDto
{
    public int Vintage { get; set; }                        // compostn.fnvintage
    public string Variety { get; set; } = string.Empty;     // compostn.fcvariety
    public string Description { get; set; } = string.Empty; // variety.fcdescript
    public string Block { get; set; } = string.Empty;       // compostn.fcblock
    public string GeoId { get; set; } = string.Empty;       // block.fcgeoid
    public decimal Percent { get; set; }                    // computed
    public decimal Litres { get; set; }                     // sum(compostn.fnpercent * opmaster.fnvolume)
    public string Legend { get; set; } = string.Empty;      // computed legend
}

// -------------------- GetBlendAdditives --------------------
public sealed class BlendAdditiveRowDto
{
    public string OpCode { get; set; } = string.Empty;      // vessaddv.fcopcode
    public string Description { get; set; } = string.Empty; // opcode.fcdescript (stripped of "ADD ")
    public decimal Amount { get; set; }                     // summed
    public string Unit { get; set; } = string.Empty;        // smallest unit
    public int Volume { get; set; }                         // sum(opmaster.fnvolume)
    public string Result { get; set; } = string.Empty;      // rate string
    public string AmountDisplay { get; set; } = string.Empty; // formatted amount string
}

// -------------------- GetVesselComposition --------------------
public sealed class VesselCompositionRowDto
{
    public int Vintage { get; set; }                        // compostn.fnvintage
    public string Variety { get; set; } = string.Empty;     // compostn.fcvariety
    public string Block { get; set; } = string.Empty;       // compostn.fcblock
    public string GeoId { get; set; } = string.Empty;       // block.fcgeoid
    public decimal Percent { get; set; }                    // fnpercent * 100
    public string Description { get; set; } = string.Empty; // variety.fcdescript
    public string Legend { get; set; } = string.Empty;      // computed legend
}