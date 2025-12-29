using UnityEngine;
using UnityEngine.UI;

public class CookingManager : MonoBehaviour
{
    [Header("Cooking Equipment")]
    public GameObject panObject;
    public GameObject potObject; // If you have other cooking tools
    public GameObject cuttingBoardObject;

    [Header("Food Objects")]
    public GameObject food1Object; // Food that appears with pan
    public GameObject food2Object; // Food that appears with pot
    public GameObject food3Object; // Food that appears with cutting board

    [Header("Food Buttons")]
    public Button food1Button; // Foods that need pan
    public Button food2Button; // Foods that need pot
    public Button food3Button; // Foods that need cutting board

    void Start()
    {
        // Hide all cooking equipment and food at start
        HideAllEquipment();
        HideAllFood();
    }

    void HideAllEquipment()
    {
        if (panObject != null) panObject.SetActive(false);
        if (potObject != null) potObject.SetActive(false);
        if (cuttingBoardObject != null) cuttingBoardObject.SetActive(false);
    }

    void HideAllFood()
    {
        if (food1Object != null) food1Object.SetActive(false);
        if (food2Object != null) food2Object.SetActive(false);
        if (food3Object != null) food3Object.SetActive(false);
    }

    public void OnFood1Clicked()
    {
        HideAllEquipment();
        HideAllFood();
        
        if (panObject != null)
        {
            panObject.SetActive(true);
        }
        
        if (food1Object != null)
        {
            food1Object.SetActive(true);
        }
        
        Debug.Log("Pan and Food1 appeared for cooking!");
    }

    public void OnFood2Clicked()
    {
        HideAllEquipment();
        HideAllFood();
        
        if (potObject != null)
        {
            potObject.SetActive(true);
        }
        
        if (food2Object != null)
        {
            food2Object.SetActive(true);
        }
        
        Debug.Log("Pot and Food2 appeared for cooking!");
    }

    public void OnFood3Clicked()
    {
        HideAllEquipment();
        HideAllFood();
        
        if (cuttingBoardObject != null)
        {
            cuttingBoardObject.SetActive(true);
        }
        
        if (food3Object != null)
        {
            food3Object.SetActive(true);
        }
        
        Debug.Log("Cutting board and Food3 appeared for preparation!");
    }

    // Public method to hide pan specifically
    public void HidePan()
    {
        if (panObject != null)
            panObject.SetActive(false);
    }

    // Add this method to your CookingManager
    public void ShowPanForCooking()
    {
        HideAllEquipment();
        HideAllFood();
        
        if (panObject != null)
        {
            panObject.SetActive(true);
        }
        
        if (food1Object != null)
        {
            food1Object.SetActive(true);
        }
        
        Debug.Log("Pan and Food1 are now visible for cooking!");
    }

    // Method to hide everything
    public void HideEverything()
    {
        HideAllEquipment();
        HideAllFood();
    }
}