using SimpleGame;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;
[RequireComponent(typeof(SpriteRenderer))]
public class TileView : MonoBehaviour, IPointerClickHandler
{
    public Game GameView { get; set; }

   SpriteRenderer m_Image;
    [SerializeField]


    Game game;
    [SerializeField]
    Sprite selectedSprite;
    [SerializeField]
    Sprite defaultSprite;
    [SerializeField]
    private SpriteRenderer img;
    [SerializeField]
    Text Text;
    public Tile Tile { get; set; }

    public State state;
    private void Start()
    {
        m_Image = GetComponent<SpriteRenderer>();
        Tile.OnControllerChanged += ChangeController;
    }

    private void ChangeController((Player player, int playerTeam) controller)
    {
        var playerColor = game.PlayerViews.FirstOrDefault(p => p.PlayerModel.Team == controller.playerTeam).TeamColor;
        img.color = playerColor;
    }

    private void OnDestroy()
    {
        Tile.OnControllerChanged -= ChangeController;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        game.OnTileClick(this);
    }

    public void UpdateImage()
    {
        PlayerView controller;
        Text.text = Tile.Power.ToString();
        switch (state)
        {
            case State.Default:
                m_Image.sprite = defaultSprite;

                controller = game.PlayerViews.FirstOrDefault(p => p.PlayerModel.Team == Tile.Controller.Team);
                if (controller == null)
                {
                    m_Image.color = Color.white;
                    break;
                }
                m_Image.color = controller.TeamColor;
                break;


            case State.Selected:
                m_Image.sprite = selectedSprite;
                controller = game.PlayerViews.FirstOrDefault(p => p.PlayerModel.Team == Tile.Controller.Team);
                if (controller == null)
                {
                    m_Image.color = Color.white;
                    break;
                }
                m_Image.color = controller.TeamColor;
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

