using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using UnityEngine;

public class Science4all : PartModule
{
	public static Color white = new Color(1f, 1f, 1f);
	public static Color red = new Color(1f, 0f, 0f);
	public static Color green = new Color(0f, 1f, 0f);

	private Rect windowPos;

	private string activePlanet = "Kerbin";

	private Science4AllSciences allScience = new Science4AllSciences();

	private List<ConfigNode> allTechs = new List<ConfigNode>();

	public override void OnStart(StartState state){
		string stringstate = Enum.GetName (typeof(StartState), state);
		Debug.Log(string.Format("Science4All - OnStart({0})", stringstate)); 
		if (stringstate == "Editor") {
			Debug.Log ("Science4All - Editor Mode");
		} else {
			allScience.getTech ();
			RenderingManager.AddToPostDrawQueue (3, new Callback (drawGUI));//start the GUI
			if ((windowPos.x == 0) && (windowPos.y == 0)) {//windowPos is used to position the GUI window, lets set it in the center of the screen
				windowPos = new Rect (Screen.width / 2, Screen.height / 2, 10, 10);
			}
		}
	}

	private void WindowGUI(int windowID) {
		GUIStyle mySty = new GUIStyle(GUI.skin.button);
		mySty.normal.textColor = mySty.focused.textColor = Color.white;
		mySty.hover.textColor = mySty.active.textColor = Color.yellow;
		mySty.onNormal.textColor = mySty.onFocused.textColor = mySty.onHover.textColor = mySty.onActive.textColor = Color.green;
		mySty.padding = new RectOffset(5, 5, 5, 5);

		GUILayout.BeginHorizontal ();
		foreach(DominantBody tempPlanet in allScience.AllDBs){
			string i = tempPlanet.getName ();
			if( GUILayout.Button(i) ){
				// Debug.Log("Sci4all db button pressed: " + i);
				activePlanet = i;
			}
		}
		if( GUILayout.Button("OFF") ){
			// Debug.Log("Sci4all OFF button pressed!");
			activePlanet = "";
			windowPos.height = 72;
		}
		GUILayout.EndHorizontal ();
		GUI.DragWindow(new Rect(0, 0, 10000, 800));
		fillScienceModuleBox ();
	}

	private void drawGUI() {
		GUI.skin = HighLogic.Skin;
		var winName = "Science4All";
		windowPos = GUILayout.Window(1, windowPos, WindowGUI, winName, GUILayout.MinWidth(1200), GUILayout.ExpandHeight(true));
	}

	private void fillScienceModuleBox() {
		if (activePlanet != "") {
			// KeyNotFoundException
			try
			{  
				DominantBody thisPlanet = allScience.getPlanet (activePlanet);

				int biomeId = thisPlanet.getBiomeId ("");
				int techId = thisPlanet.BiomeList [biomeId].getScienceId ("recovery");
				List<ScienceWhere> myRecovery = thisPlanet.BiomeList[biomeId].ScienceList[techId].ScienceWhereList;
				if (myRecovery.Count > 0) {
					string recoveryText = "You already ";
					foreach (ScienceWhere sciW in myRecovery) {
						Color procentColor = procentToColor (sciW.sci / sciW.cap * 100F);
						recoveryText += colored (procentColor, sciW.getName ()) + " ";
					}
					recoveryText += "at " + colored (white, activePlanet);
					GUILayout.BeginHorizontal ();
					GUILayout.Label (recoveryText);
					GUILayout.EndHorizontal ();
				}

				// LEGEND
				GUILayout.BeginHorizontal ();
				GUILayout.Box ("FL = Flying Low");
				GUILayout.Box ("FH = Flying High");
				GUILayout.Box ("SL = In Space Low");
				GUILayout.Box ("SH = In Space High");
				GUILayout.Box ("SP = Surface Splashed");
				GUILayout.Box ("LN = Surface Landed");
				GUILayout.Box (colored (green, "done or minimal sci"));
				GUILayout.Box (colored(white, "more science possible"));
				GUILayout.Box (colored(red, "needed"));
				GUILayout.EndHorizontal ();
				// END LEGEND 

				GUILayout.BeginVertical ();
		// vert ->

				List<string> biomeList = new List<string> ();
				foreach (Biome temp in thisPlanet.BiomeList.Values.ToList()) {
					biomeList.Add (temp.getName());	
				}
				biomeList.Sort ();
		// hori ->
				GUILayout.BeginHorizontal ();
				// GUILayout.Label (colored(white,PlanetName),GUILayout.Width(150));
				GUILayout.Label (colored(white,thisPlanet.getName()),GUILayout.Width(150));

				foreach (string biomeName in biomeList) {
					if (biomeName == "" || biomeName == null) {
						GUILayout.Label ("Planet", GUILayout.Width (150));
					} else {
						GUILayout.Label (biomeName, GUILayout.Width (150));
					}
				}
				GUILayout.EndHorizontal ();
		// hori <-
				bool found = false;
				foreach (string scienceModuleName in allScience.scienceModuleList) {
					if (scienceModuleName == "recovery") {
						continue;
					}
					GUILayout.BeginHorizontal ();
		// hori ->
					GUILayout.Label (scienceModuleName,GUILayout.Width(150));
					foreach (string sortedBiomes in biomeList) {
						found = false;
						string FL = colored (red, "FL");
						string FH = colored (red, "FH");
						string SL = colored (red, "SL");
						string SH = colored (red, "SH");
						string SP = colored (red, "SP");
						string LN = colored (red, "LN");
						foreach (Biome thisBiome in thisPlanet.BiomeList.Values) {
							if (!found) {
								if (sortedBiomes == thisBiome.getName ()) {
									foreach (var sci in thisBiome.ScienceList) {
										if (!found) {
											if (sci.Value.getName () == scienceModuleName) {
												foreach (ScienceWhere sciW in sci.Value.ScienceWhereList) {

													Color procentColor = procentToColor (sciW.sci / sciW.cap * 100F);
													string sciText = sciW.getName ();
													if (sciText == "FlyingLow") {
														FL = colored (procentColor, "FL");
													} else if (sciText == "FlyingHigh") {
														FH = colored (procentColor, "FH");
													} else if (sciText == "InSpaceLow") {
														SL = colored (procentColor, "SL");
													} else if (sciText == "InSpaceHigh") {
														SH = colored (procentColor, "SH");
													} else if (sciText == "SrfSplashed") {
														SP = colored (procentColor, "SP");
													} else if (sciText == "SrfLanded") {
														LN = colored (procentColor, "LN");
													}
													found = true;											
												}
											}
										}
									}
								}
							}
						}
						GUILayout.Label (FL + " " + FH + " " + SL + " " + SH + " " + SP + " " + LN, GUILayout.Width (150));
					}
					GUILayout.EndHorizontal ();
		// hori <-
				}

				GUILayout.EndVertical ();
		// vert <-
			}

			catch (KeyNotFoundException e){
				// Debug.Log (e.Message);
			}

		}
	}

	private Color procentToColor(float procent){
		if (procent > 99) {
			return green;
		} else if (procent >= 75) {
			return white;
		} else {
			return red;
		}
	}

	public static string colorHex(Color32 c) {
		return "#" + c.r.ToString("x2") + c.g.ToString("x2") + c.b.ToString("x2");
	}

	public static string colored(Color c, string text) {
		text = "<color=\"" + colorHex(c) + "\">" + text + "</color>";
		return text;
	}
		
		
	private void onVesselChange(Vessel vessel) {
		// reset achievement state here
		// RenderingManager.RemoveFromPostDrawQueue(3, new Callback(drawGUI));
		// this.drawGUI ();
	}

	private void onDominantBodyChange() {
		RenderingManager.RemoveFromPostDrawQueue(3, new Callback(drawGUI));
		this.drawGUI ();
	}


}

public class Science4AllSciences {
	public List<DominantBody> AllDBs = new List<DominantBody> ();

	// just game sci
	public List<string> scienceModuleList = new List<string> () { 
		"recovery",
		"mysteryGoo",
		"mobileMaterialsLab",
		"temperatureScan",
		"barometerScan",
		"seismicScan",
		"gravityScan",
		"atmosphereAnalysis",
		"crewReport",
		"evaReport",
		"surfaceSample"
	};

	public ConfigNode[] sciences;

	public Science4AllSciences() {
		
	}

	public DominantBody getPlanet(string PlanetName){
		int dbCounter = -1;


		for (int i = 0; i < AllDBs.Count; i++) {
			if (AllDBs [i].PlanetName == PlanetName) {
				dbCounter = i;
			}
		}

		if (dbCounter == -1) {
			dbCounter = AllDBs.Count ();
			DominantBody Planet = new DominantBody(PlanetName);
			AllDBs.Add (Planet);
			return Planet;
		}
		return AllDBs[dbCounter];
	}

	public void getTech(){
		string scienceName = "";
		string biomeName = "";
		string scienceWhereName = "";
		int biomeId = -1;
		int scienceId = -1;
		int scienceWhereId = -1;

		string persistentfile = KSPUtil.ApplicationRootPath + "saves/" + HighLogic.SaveFolder + "/persistent.sfs";
		ConfigNode config = ConfigNode.Load (persistentfile);
		ConfigNode gameconf = config.GetNode ("GAME");
		ConfigNode[] scenarios = gameconf.GetNodes ("SCENARIO");
		foreach (ConfigNode scenario in scenarios) {
			if (scenario.GetValue ("name") == "ResearchAndDevelopment") {
				sciences = scenario.GetNodes("Science");

				// recovery@KerbinSubOrbited
				// gravityScan@MinmusInSpaceHighLowlands
				// gravityScan@KerbinInSpaceLowHighlands
				// crewReport@KerbinSrfLandedLaunchPad
				// evaReport@KerbinSrfLandedRunway

				// evaReport@KerbinFlyingLowShores
				// evaReport 	@ 		Kerbin 		FlyingLow 	Shores

				//      |       		|       	|      		|

				//		scienceName		planetName	sciWhere	biomeName

				foreach (ConfigNode sience in sciences) {
					scienceName = biomeName = scienceWhereName = "";
					biomeId = scienceId = scienceWhereId = -1;
					string[] tempString = sience.GetValue ("id").Split ('@'); // evaReport @ KerbinFlyingLowShores
					scienceName = tempString [0]; // evaReport

					// no mods
					if (!scienceModuleList.Contains (scienceName)) {
						continue;
					}
						
					// agro string foo
					string rest = tempString [1]; // KerbinFlyingLowShores
					var r = new Regex(@"
                		(?<=[A-Z])(?=[A-Z][a-z]) |
                 		(?<=[^A-Z])(?=[A-Z]) |
                 		(?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);

					string[] spacedString = r.Replace (rest, " ").Split(' '); // Kerbin Flying Low Shores

					DominantBody thisPlanet = getPlanet (spacedString [0]); // hier rein kommt diese science

					if (scienceName == "recovery") {
						switch (spacedString [1]) { // Flying
						case "Flew":
							if (spacedString.Count () > 2) {
								// FlewBy
								scienceWhereName = "Flew By";
							} else {
								scienceWhereName = "Flew";
							}
							break;
						case "Sub":
							scienceWhereName = "Sub Orbited";
							break;
						case "Orbited":
							scienceWhereName = "Orbited";
							break;
						case "Surfaced":
							scienceWhereName = "Surfaced";
							break;
						default:
							continue;
						}
					} else {
						// biome kann auch aus 2 parts bestehen zb. Ice Caps
						switch (spacedString [1]) { // Flying
						case "In":
							// In Space High 
							// In Space Low
							// In Space Low Water   !! mal mit mal ohne biome...
							if (spacedString [3] == "High") {
								scienceWhereName = "InSpaceHigh";
							} else {
								scienceWhereName = "InSpaceLow";
							}
							if (spacedString.Count () > 4) { // biome ?
								if (spacedString.Count () > 5) { // biome laenger als ein wort ?
									biomeName = spacedString [4] + spacedString [5];	
								} else {
									biomeName = spacedString [4];
								}
							}
							break;

						case "Srf":
							// Srf Landed Grasslands
							// Srf Splashed Grasslands
							if (spacedString [2] == "Landed") {
								scienceWhereName = "SrfLanded";	
							} else {
								scienceWhereName = "SrfSplashed";
							}
							if (spacedString.Count () > 4) { // biome laenger als ein wort ?
								biomeName = spacedString [3] + spacedString [4];	
							} else {
								biomeName = spacedString [3];
							}
							break;
						case "Flying":
							// Flying Low Grasslands
							// Flying Low
							// Flying High
							if (spacedString [2] == "Low") {
								scienceWhereName = "FlyingLow";	
							} else {
								scienceWhereName = "FlyingHigh";
							}
							if (spacedString.Count () > 3) { // biome ?
								if (spacedString.Count () > 4) { // biome laenger als ein wort ?
									biomeName = spacedString [3] + spacedString [4];	
								} else {
									biomeName = spacedString [3];
								}
							}
							break;
						default:
							continue;
						}
					}
					ScienceWhere sciWhere = new ScienceWhere (scienceWhereName);
					sciWhere.cap = float.Parse(sience.GetValue ("cap"));
					sciWhere.sci = float.Parse(sience.GetValue ("sci"));

					biomeId = thisPlanet.getBiomeId (biomeName);
					if (biomeId == -1) {
						biomeId = thisPlanet.addBiome (biomeName);
					}

					scienceId = thisPlanet.getBiome(biomeId).getScienceId(scienceName);
					if (scienceId == -1) {
						scienceId = thisPlanet.getBiome(biomeId).addScience(scienceName);
					} 

					scienceWhereId = thisPlanet.getBiome(biomeId).getScienceId(scienceWhereName);
					if (scienceId == -1) {
						scienceId = thisPlanet.getBiome(biomeId).addScience(scienceWhereName);
					} 

					thisPlanet.BiomeList [biomeId].ScienceList [scienceId].ScienceWhereList.Add(sciWhere);
				}
			}
		}
	}
}

public class DominantBody {
	public string PlanetName = "";
	// Texture iconGFX;
	public Dictionary<int, Biome> BiomeList = new Dictionary<int, Biome> ();


	public DominantBody(string Name){
		PlanetName = Name;
	}

	public string getName(){
		return PlanetName;
	}

	public int addBiome(string biomeName){
		int index = BiomeList.Count();
		BiomeList.Add(index, new Biome(biomeName));
		return index;
	}

	public int getBiomeId(string biomeName){
		foreach (var pair in BiomeList) {
			Biome temp = pair.Value;
			if (temp.getName() == biomeName) {
				return pair.Key;
			}
		}
		return -1;
	}

	public string getBiomeName(int biomeId){
		if (BiomeList.ContainsKey (biomeId)) {
			return BiomeList [biomeId].getName();
		}
		return "";
	}
	public Biome getBiome(int biomeId){
		if (BiomeList.ContainsKey (biomeId)) {
			return BiomeList [biomeId];
		}
		return null;
	}
}
public class Biome {
	string BiomeName;
	public Dictionary<int, Science> ScienceList = new Dictionary<int, Science> ();

	public Biome(string biomeName){
		BiomeName = biomeName;
	}

	public string getName(){
		return BiomeName;
	}

	public int addScience(string scienceName){
		int index = ScienceList.Count();
		ScienceList.Add(index, new Science(scienceName));
		return index;
	}

	public int getScienceId(string scienceName){
		foreach (var pair in ScienceList) {
			Science temp = pair.Value;
			if (temp.getName() == scienceName) {
				return pair.Key;
			}
		}
		return -1;
	}

	public string getScienceName(int scienceId){
		if (ScienceList.ContainsKey (scienceId)) {
			return ScienceList [scienceId].getName();
		}
		return "";
	}
	public Science getScience(int scienceId){
		if (ScienceList.ContainsKey (scienceId)) {
			return ScienceList [scienceId];
		}
		return null;
	}
}



public class Science {
	string ScienceName = "";
	public List<ScienceWhere> ScienceWhereList = new List<ScienceWhere> ();

	public Science(string scienceName){
		ScienceName = scienceName;
	}

	public string getName(){
		return ScienceName;
	}

	public int addScienceWhere(string scienceWhereName){
		int index = ScienceWhereList.Count();
		ScienceWhereList.Add(new ScienceWhere(scienceWhereName));
		return index;
	}

	public int getScienceWhereId(string scienceWhereName){
		for(int i = 0; i <= ScienceWhereList.Count(); i++){
			if (ScienceWhereList[i].getName() == scienceWhereName) {
				return i;
			}
		}
		return -1;
	}

	public string getScienceWhereName(int scienceWhereId){
		return ScienceWhereList [scienceWhereId].getName();
	}
	public ScienceWhere getScienceWhere(int scienceWhereId){
		return ScienceWhereList [scienceWhereId];
	}
}

public class ScienceWhere {
	public string ScienceWhereName = "";
	public float sci;
	public float cap;

	public ScienceWhere(string sWhere) {
		ScienceWhereName = sWhere;
	}

	public string getName(){
		return ScienceWhereName;
	}
}