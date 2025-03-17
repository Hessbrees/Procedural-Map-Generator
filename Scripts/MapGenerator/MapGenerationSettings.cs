using UnityEngine;

[CreateAssetMenu(fileName = "MapGenerationSettings", menuName = "Map/Generation Settings")]
public class MapGenerationSettings : ScriptableObject
{
    public int seed = 0;
    [Header("Map Dimensions")]
    [Min(1)]
    public int mapWidth = 1;
    [Min(1)]
    public int mapHeight =1;

    [Header("Dungeon Settings")]
    [Min(1)]
    public int dungeonWidth = 1;
    [Min(1)]
    public int dungeonHeight = 1;
    public int dungeonMaxRooms = 2;

    [Header("Perlin Noise Settings")]
    public int permutationSize = 256;
    public float persistence = 0.5f;
    public int octaves = 6;

    [Header("Ocean Settings")]
    public float oceanStrength = 4f;

    [Header("Structure Settings")]
    public int bspIterations = 2;
    public int townMinimumSize = 35;
    public int townMaximumSize = 40;

    [Header("Path Settings")]
    public int pathLength = 15;

    [Header("Environment Settings")]
    public string lSystemAxiom = "F";
    public string lSystemRules = "F+F-F-F+F";
    public int lSystemIterations = 6;
    public float probabilityOfEnvironment_Grass = 0.1f;
    public float probabilityOfEnvironment_Stone = 0.1f;
    public float probabilityOfEnvironment_Sand = 0.1f;
    public float probabilityOfEnvironment_Dirt = 0.1f;
    public float probabilityOfEnvironment_Water = 0.1f;
    public float probabilityOfEnvironment_Ocean = 0.1f;
    public int numberOfStartPositions_Grass = 25;
    public int numberOfStartPositions_Stone = 25;
    public int numberOfStartPositions_Sand = 25;
    public int numberOfStartPositions_Dirt = 25;
    public int numberOfStartPositions_Water = 25;
    public int numberOfStartPositions_Ocean = 25;
} 