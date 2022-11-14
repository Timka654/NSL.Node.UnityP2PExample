using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BackButton : MonoBehaviour
{
    [SerializeField] private GameObject CurrentScreen;
    [SerializeField] private GameObject PrevScreen;

    // Start is called before the first frame update
    void Start()
    {
        var btn = GetComponent<Button>();

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() =>
        {
            CurrentScreen.SetActive(false);
            PrevScreen.SetActive(true);
        });
    }
}
