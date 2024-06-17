using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleVisualizator : MonoBehaviour
{
    public int Type { get; private set; } //0 - player
    public int Value { get; private set; }

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color[] colors;
    private PlanetVisualizator planetVisualizator; //target

    Rigidbody2D rigidbody;
    public static float timeScale = 1f;

    public void Init(int _type, int _value, PlanetVisualizator _planet)
    {
        Type = _type;
        Value = _value;
        planetVisualizator = _planet;

        spriteRenderer.color = colors[Type - 1];
        rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        rigidbody.MovePosition(Vector2.MoveTowards(
            transform.position, 
            planetVisualizator.transform.position, 
            Time.deltaTime * 4f * timeScale));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent<PlanetVisualizator>(out var planet))
        {
            if(planet == planetVisualizator)
            {
                planet.TouchBubble(this);
                SoundManager.Instance.PlayBlup();
            }
        }
    }
}
