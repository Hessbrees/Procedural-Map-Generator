using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GeneratorSettings : MonoBehaviour
{
    [SerializeField] TMP_Dropdown mapSizeDropdown;
    [SerializeField] TMP_InputField seedInputField;
    [SerializeField] TMP_Dropdown dungeonRoomsDropdown;
    [SerializeField] TMP_Dropdown dungeonSizeDropdown;
    [SerializeField] MapGenerationSettings settings;
    [SerializeField] Button playButton;
    [SerializeField] Transform interfaceTransform;
    [SerializeField] MapGenerator mapGenerator;
    void Start()
    {
        mapSizeDropdown.onValueChanged.AddListener(delegate { SetSize(mapSizeDropdown); });
        seedInputField.onValueChanged.AddListener(delegate { SetSeed(seedInputField); });
        dungeonRoomsDropdown.onValueChanged.AddListener(delegate { SetDungeonRooms(dungeonRoomsDropdown); });
        dungeonSizeDropdown.onValueChanged.AddListener(delegate { SetDungeonSize(dungeonSizeDropdown); });
        playButton.onClick.AddListener(Play);
    }

    public void SetSize(TMP_Dropdown dropdown)
    {
        string[] size = dropdown.captionText.text.Split('x');
        settings.mapWidth = int.Parse(size[0]);
        settings.mapHeight = int.Parse(size[1]);
    }
    
    public void SetSeed(TMP_InputField inputField)
    {
        settings.seed = int.Parse(inputField.text);
    }

    public void SetDungeonRooms(TMP_Dropdown dropdown)
    {
        settings.dungeonMaxRooms = dropdown.value;
    }

    public void SetDungeonSize(TMP_Dropdown dropdown)
    {
        string[] size = dropdown.captionText.text.Split('x');
        settings.dungeonWidth = int.Parse(size[0]);
        settings.dungeonHeight = int.Parse(size[1]);
    }    
    public void Play()
    {
        interfaceTransform.gameObject.SetActive(false);
        mapGenerator.GenerateMap();
    }
}
