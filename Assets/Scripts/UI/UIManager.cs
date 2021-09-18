using DevionGames.UIWidgets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private UIWidget characterCreatorPanel;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.C))
        {
            if(characterCreatorPanel.gameObject.activeInHierarchy)
                characterCreatorPanel.Close();
            else
            {
                characterCreatorPanel.gameObject.SetActive(true);
                characterCreatorPanel.Show();
            }
        }
    }
}
