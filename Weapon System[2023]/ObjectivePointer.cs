using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectivePointer : MonoBehaviour
{
    public GameObject arrowPrefab;

    private Character _character;
    private FloorManager _floorManager;
    private RogueManager _rougeManager;
    private HashSet<Transform> _enemiesWithArrow = new HashSet<Transform>();
    private HashSet<Transform> _exitsWithArrow = new HashSet<Transform>();
    void Start()
    {
        _character = GetComponentInParent<Character>();
        _rougeManager = RogueManager.instance;
        _floorManager = _rougeManager.floorManager;
    }

    void Update()
    {
        if (_floorManager.endFloor)
        {
            for (int i = 0; i < _floorManager.exits.Count; i++)
            {
                foreach (Interactable exits in _floorManager.exits)
                {
                    if (_exitsWithArrow.Contains(exits.transform))
                    {
                        continue; // Skip enemies that already have an arrow
                    }
                    GameObject targetarrow = Instantiate(arrowPrefab, _character.transform);
                    targetarrow.GetComponent<Pointer>().pointer = exits.gameObject;
                    _exitsWithArrow.Add(exits.transform);
                }
            }
        }
        else
        {
            if(_rougeManager.allCurrentEncounter.Count <= 2)
            {
                foreach (GameObject enemy in _rougeManager.allCurrentEncounter)
                {
                    if (_enemiesWithArrow.Contains(enemy.transform))
                    {
                        continue; // Skip enemies that already have an arrow
                    }
                    GameObject targetarrow = Instantiate(arrowPrefab, _character.transform);
                    targetarrow.GetComponent<Pointer>().pointer = enemy;
                    _enemiesWithArrow.Add(enemy.transform);
                }
            }
        }
    }
}
