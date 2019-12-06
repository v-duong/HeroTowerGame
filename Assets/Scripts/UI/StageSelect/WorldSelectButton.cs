using UnityEngine;

public class WorldSelectButton : MonoBehaviour
{
    public int worldNum;

    public void ButtonOnClick()
    {
        UIManager.Instance.StageSelectPanel.SetSelectedAct(worldNum);


    }
}