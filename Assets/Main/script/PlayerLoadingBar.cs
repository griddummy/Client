using UnityEngine;
using UnityEngine.UI;

public class PlayerLoadingBar : MonoBehaviour {
    public Text textPlayerName;
    public Text textLoadingProgress;
    public int percent;
    
    public void SetPlayerName(string playerName)
    {
        textPlayerName.text = playerName;
    }

    public void SetLoadingProgress(int percent)
    {
        this.percent = percent;
        textLoadingProgress.text = percent.ToString() + "%";
    }

}
