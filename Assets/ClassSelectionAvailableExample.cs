using Gtec.UnityInterface;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Gtec.UnityInterface.BCIManager;

public class ClassSelectionAvailableExample : MonoBehaviour
{
    private uint _selectedClass = 0;
    private bool _update = false;
    public CVEPFlashController2D _flashController;
    private Dictionary<int, SpriteRenderer> _selectedObjects;
    public static string msg = "";
    void Start()
    {
        //attach to class selection available event
        BCIManager.Instance.ClassSelectionAvailable += OnClassSelectionAvailable;

        
        _selectedObjects = new Dictionary<int, SpriteRenderer>();
        List<CVEPFlashObject2D> applicationObjects = _flashController.ApplicationObjects;
        foreach(CVEPFlashObject2D applicationObject in applicationObjects)
        {
            SpriteRenderer[] spriteRenderers = applicationObject.GameObject.GetComponentsInChildren<SpriteRenderer>();
            foreach(SpriteRenderer spriteRenderer in spriteRenderers)
            {
                if (spriteRenderer.name.Contains("Selected"))
                {
                    _selectedObjects.Add(applicationObject.ClassId, spriteRenderer);
                }
            }
        }

        foreach(KeyValuePair<int, SpriteRenderer> kvp in _selectedObjects)
        {
            kvp.Value.gameObject.SetActive(false);
        }
    }

    void OnApplicationQuit()
    {
        //detach from class selection available event
        BCIManager.Instance.ClassSelectionAvailable -= OnClassSelectionAvailable;
    }

    void Update()
    {
        //TODO ADD YOUR CODE HERE
        if(_update)
        {
            foreach (KeyValuePair<int, SpriteRenderer> kvp in _selectedObjects)
            {
                kvp.Value.gameObject.SetActive(false);
            }

            if(_selectedClass > 0)
            {
                _selectedObjects[(int)_selectedClass].gameObject.SetActive(true);
            }

            _update = false;
        } 
    }

    /// <summary>
    /// This event is called whenever a new class selection is available. Th
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnClassSelectionAvailable(object sender, EventArgs e)
    {
        ClassSelectionAvailableEventArgs ea = (ClassSelectionAvailableEventArgs)e;
       _selectedClass = ea.Class;
        _update = true;
        Debug.Log(string.Format("Selected class: {0}", ea.Class));
    }
}
