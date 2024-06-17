using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class PlanetVisualizator : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private TMP_Text textComponent;
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private GameObject smile;
    [SerializeField] private GameObject angry;
    [SerializeField] private GameObject mark;

    [Header("Type 0 = neytral, 1 = player, 2 or more = enemies")]
    public int Type;
    public int Value;

    public static UnityEvent OnPlanetTypeChanged = new UnityEvent();
    public static List<PlanetVisualizator> selected = new List<PlanetVisualizator>();

    public void Start()
    {
        UpdateView();
    }

    public void UpdateInvoke()
    {
        if (Type == 0) return;

        Value++;
        UpdateView();
    }

    public void TouchBubble(BubbleVisualizator _bubble)
    {
        if(_bubble.Type == Type)
        {
            Value += _bubble.Value;
        }
        else
        {
            Value -= _bubble.Value;
            if(Value <= 0)
            {
                Value = -Value;
                Type = _bubble.Type;
                OnPlanetTypeChanged?.Invoke();
            }
        }

        UpdateView();
        Destroy(_bubble.gameObject);
    }

    public void UpdateView()
    {
        spriteRenderer.sprite = sprites[Type];
        
        textComponent.text = Value.ToString();
        textComponent.color = Type > 0? Color.white : Color.black;

        smile.SetActive(Type == 1);
        angry.SetActive(Type > 1);

        transform.localScale = Vector3.one * (0.3f + Value * 0.002f);
        mark.SetActive(selected.Contains(this));
    }

    public void SendPoints(PlanetVisualizator _target, BubbleVisualizator _bbl)
    {
        while (Value > 1)
        {
            var bbl = Instantiate(
                _bbl,
                Vector2.MoveTowards(transform.position, _target.transform.position, transform.localScale.x),
                Quaternion.identity,
                transform.parent);
            bbl.Init(Type, 1, _target);
            Value -= 1;
        }

        UpdateView();
    }

    private void OnMouseEnter()
    {
        Mark();
    }

    private void OnMouseDown()
    {
        Mark();
    }

    private void Mark()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButton(0))
#else
        if (Input.touchCount > 0)
#endif
        {
            Debug.Log(gameObject.name);
            if (Type == 1)
            {
                if (!selected.Contains(this))
                {
                    selected.Add(this);
                }
            }
            else
            {
                for (var i = 0; i < selected.Count; i++)
                    if (selected[i].Type != 1)
                    {
                        var temp = selected[i];
                        selected.RemoveAt(i);
                        temp.UpdateView();
                    }

                selected.Add(this);
            }

            UpdateView();
        }
    }
}
