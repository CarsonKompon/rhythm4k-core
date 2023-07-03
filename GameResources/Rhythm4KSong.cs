using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Rhythm4K;

[GameResource("Rhythm4K Song", "rhythm", "Describes a Rhythm4K Song", Icon = "piano")]
public partial class Rhythm4KSong : GameResource
{
    public string ChartPath { get; set; }
    public string SoundName { get; set; }
	public string OsuId = "";
	
    [ResourceType("png")]
    public string CoverArt { get; set; }
    public string[] LoadingTips { get; set;}
    
    [HideInEditor] public Song Song { get; set; }


	public static List<Rhythm4KSong> All => AllOriginal.Concat( AllExtras ).ToList();
	public static List<Rhythm4KSong> AllOriginal => ResourceLibrary.GetAll<Rhythm4KSong>().ToList();
	public static List<Rhythm4KSong> AllExtras { get; set; } = new List<Rhythm4KSong>();

	protected override void PostLoad()
	{
		base.PostLoad();

        Init();
	}

    public void Init()
    {
        Song = SongBuilder.Load(ChartPath);
    }

}
