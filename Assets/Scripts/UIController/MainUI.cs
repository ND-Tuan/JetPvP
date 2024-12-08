using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainUI : MonoBehaviour
{
    [SerializeField] private GameObject Cam;

    private void Start()
    {
        Cam.GetComponent<RenderFeatureToggler>().ActivateRenderFeatures(0,true);
    }

    private void Update()
    {
        Cam.transform.localPosition += new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f));

    }
    
    public void Play(){
        Cam.GetComponent<RenderFeatureToggler>().ActivateRenderFeatures(0,false);
        SceneManager.LoadScene(1);
    }

    public void Quit(){
        Application.Quit();
    }
}
