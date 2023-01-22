using SimpleGame;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;
[RequireComponent(typeof(SpriteRenderer))]
public class TileView : MonoBehaviour
{
    public Game GameView { get; set; }

    [SerializeField]


    Game game;
    [SerializeField]
    Sprite selectedSprite;
    [SerializeField]
    Sprite defaultSprite;
    [SerializeField]
    SpriteRenderer spriteRenderer;

    [SerializeField]
    public Tile TileModel { get; set; }

    public State state;
    private void Start()
    {
        //spriteRenderer = GetComponent<SpriteRenderer>();
    }
    public void Init(Tile tile, Game game)

    {
        this.game = game; TileModel = tile;
        TileModel.OnControllerChanged += ChangeController;
        state = State.Default;
        UpdateImage();
    }

    private void ChangeController((Player player, int playerTeam) controller)
    {
        var playerColor = game.PlayerViews.FirstOrDefault(p => p.PlayerModel.Team == controller.playerTeam).TeamColor;
        spriteRenderer.color = playerColor;
    }

    private void OnDestroy()
    {
        TileModel.OnControllerChanged -= ChangeController;
    }

    public void OnMouseDown()

    {
        game.OnTileClick(this);
        Debug.Log($"Click : {TileModel.Coordinates}");
    }

    public void UpdateImage()
    {
        PlayerView controller;
        gameObject.transform.localScale = new Vector3(1f + (0.1f - TileModel.Power / 10f), 1f + (0.1f - TileModel.Power / 10f), 1);
        switch (state)
        {
            case State.Default:
                spriteRenderer.sprite = defaultSprite;
                if (TileModel.Controller.Player == null)
                {
                    spriteRenderer.color = Color.white;
                    break;
                }
                controller = game.PlayerViews.FirstOrDefault(p => p.PlayerModel.Team == TileModel.Controller.Team);
                if (controller == null)
                {
                    spriteRenderer.color = Color.white;
                    break;
                }
                spriteRenderer.color = controller.TeamColor;
                break;


            case State.Selected:
                spriteRenderer.sprite = selectedSprite;
                if (TileModel.Controller.Player == null)
                {
                    spriteRenderer.color = Color.white;
                    break;
                }
                controller = game.PlayerViews.FirstOrDefault(p => p.PlayerModel.Team == TileModel.Controller.Team);
                if (controller == null)
                {
                    spriteRenderer.color = Color.white;
                    break;
                }
                spriteRenderer.color = controller.TeamColor;
                break;


        }
    }

    public void SetSelectedImage()
    {
        state = State.Selected;
        UpdateImage();
    }

    public enum State
    {
        Selected,
        Default
    }
}

