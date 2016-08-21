using UnityEngine;
using UnityEngine.UI;

public class PlayerLoadingBar : MonoBehaviour {
    public Text textPlayerName;
    public Text textLoadingProgress;

    public void SetPlayerName(string playerName)
    {
        textPlayerName.text = playerName;
    }

    public void SetLoadingProgress(int percent)
    {
        textLoadingProgress.text = percent.ToString() + "%";
    }
}
