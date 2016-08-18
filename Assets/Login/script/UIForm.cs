using UnityEngine;
using System.Collections.Generic;

public abstract class UIForm : MonoBehaviour {

    private static Dictionary<string, UIForm> listForm = new Dictionary<string, UIForm>(); // 생성되는 모든 폼을 저장하는 해시테이블
    private static UIForm s_curForm = null; // 현재 출력되는 폼
    public static UIForm curForm
    {
        get
        {
            return s_curForm;
        }
    }
    protected virtual void Awake() // 폼 생성시 해쉬테이블에 추가한다.
    {
        Debug.Log("생성 - " + GetType().Name);
        listForm.Add(GetType().Name, this); // 클래스 이름이 Key가 된다.
    }

    protected void OnDestroy() // 폼 파괴시 해시테이블에서 삭제한다.
    {
        listForm.Remove(ToString());
        if (s_curForm == this)
            s_curForm = null;
    }

    public static bool ChangeForm(string formName) // 현재 출력되는 폼을 변경한다. 
    {
        Debug.Log("체인지 :"+formName);
        UIForm form;
        if (listForm.TryGetValue(formName, out form))
        {
            if (s_curForm != null)
            {
                s_curForm.OnPause();
                s_curForm.gameObject.SetActive(false);                
            }
            s_curForm = form;
            s_curForm.gameObject.SetActive(true);
            s_curForm.OnResume();
            return true;
        }
        else
        {
            return false;
        }            
    }
    protected virtual void OnResume() // 출력되는폼(ChangeForm())으로 변경되면 호출된다.
    {

    }
    protected virtual void OnPause()
    {

    }
}
