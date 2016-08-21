using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Dialog : MonoBehaviour {

    public delegate void OnClosedEvent(bool bPositive, object obj);
    public event OnClosedEvent OnClosed;

    public Button btnPositive;
    public Button btnNegative;

    private Coroutine corClosing;
    private object objResult;
    public void SetResultObject(object obj)
    {
        objResult = obj;
    }    
    void Awake()
    {
        if (btnPositive != null)
        {
            btnPositive.onClick.AddListener(() => OnClickPositive());
        }
        if (btnNegative != null)
        {
            btnNegative.onClick.AddListener(() => OnClickNegative());
        }
        
    }
    protected virtual void OnClickPositive()
    {
        Close(true);
    }
    protected virtual void OnClickNegative()
    {
        Close(false);
    }     
    public virtual void Init()
    {
        
    }
    public void Open()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            Init();
        }        
    }

    public void Close(bool bPositive = true)
    {
       
        if (gameObject.activeSelf)
        {            
            gameObject.SetActive(false);
            if (OnClosed != null)
            {
                OnClosed(bPositive, objResult);
            }
                
        }
    }
    public void Close(bool bPositive, float delaySec)
    {
        if (corClosing != null)
            StopCoroutine(corClosing);

        corClosing = StartCoroutine(DelayClosing(bPositive, delaySec));
    }    
    private IEnumerator DelayClosing(bool bPositive, float time)
    {
        yield return new WaitForSeconds(time);
        Close(bPositive);
    }
}
