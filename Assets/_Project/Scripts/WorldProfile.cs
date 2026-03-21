using UnityEngine;

[CreateAssetMenu(fileName = "New World", menuName = "Space League/World Profile")]
public class WorldProfile : ScriptableObject
{
    [Header("Environment Identity")]
    public string worldName = "Unknown World";
    
    [Header("Court Dimensions")]
    [Tooltip("Standard width is 15.24")]
    public float courtWidth = 15.24f;
    [Tooltip("Standard length is 28.65")]
    public float courtLength = 28.65f;

    [Header("Court Appearance (Visuals - .mat)")]
    [Tooltip("The visual color/texture of the floor")]
    public Material courtFloorAppearance;
    [Tooltip("The visual color/texture of the force field boundary")]
    public Material boundaryAppearance;
    
    [Header("Standard Hoop Settings (Dimensions & Visuals)")]
    [Tooltip("The visual material for the backboard")]
    public Material backboardAppearance;
    [Tooltip("X is Width, Y is Height. Standard is 1.83 x 1.07")]
    public Vector2 backboardSize = new Vector2(1.83f, 1.07f);
    [Tooltip("Standard radius is 0.228")]
    public float rimRadius = 0.228f;
    public float rimTubeRadius = 0.016f; // <-- Renamed here!

    [Header("Alien Hoop Overrides")]
    [Tooltip("Drop a custom prefab here for elliptical backboards or square rims. If left empty, the game uses the Standard Hoop Settings above.")]
    public GameObject customHoopPrefab;

    [Header("World Physics (.physicMaterial)")]
    public float ballGravity = 9.81f; 
    [Tooltip("How bouncy/slippery the floor is")]
    public PhysicsMaterial courtFloorPhysics;
    [Tooltip("How the ball bounces off the backboard")]
    public PhysicsMaterial backboardPhysics;
    [Tooltip("How the ball bounces off the rim")]
    public PhysicsMaterial rimPhysics;
}