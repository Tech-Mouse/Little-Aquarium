using UnityEngine;
using UnityEngine.UIElements;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private UIDocument menu;
    private VisualElement menuElem;
    private Button button;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clip;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private void OnEnable()
    {
        menuElem = menu.rootVisualElement.Q<VisualElement>("VisualElement");
        button = menuElem.Q<Button>("Close");

        button.clicked += () => {
            audioSource.clip = clip;
            audioSource.Play();
            menuElem.style.display = DisplayStyle.None;
        };
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
