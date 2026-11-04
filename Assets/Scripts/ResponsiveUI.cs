using UnityEngine;

public class ResponsiveUI : MonoBehaviour
{

    [Header("UI Layouts")]
    [SerializeField] private GameObject desktopUI;
    [SerializeField] private GameObject mobileUI;

    private int lastWidth;
    private int lastHeight;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateLayout();
    }

    // Update is called once per frame
    void Update()
    {
        if (Screen.width != lastWidth || Screen.height != lastHeight)
        {
            UpdateLayout();
        }
    }

    private void UpdateLayout()
    {
        lastWidth = Screen.width;
        lastHeight = Screen.height;


        bool isPortrait = Screen.height > Screen.width;

        desktopUI.SetActive(!isPortrait);
        mobileUI.SetActive(isPortrait);
    }
}
