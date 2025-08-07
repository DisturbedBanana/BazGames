using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;
using TMPro;
using RedBlueGames.Tools.TextTyper;

public class GameHandler : MonoBehaviour
{
    public enum GameState
    {
        WaitingForStart,
        Game,
        Pledge
    }
    
    public static GameHandler Instance;
    
    public UnityEvent onGameStart;
    public UnityEvent onGameEnd;
    public UnityEvent onPledgeStart;
    
    string currentGame;
    Pledge currentPledge;
    public GameState currentState = GameState.WaitingForStart;
    
    public string CurrentGame => currentGame;
    public Pledge CurrentPledge => currentPledge;
    
    [Header("Game Variables")]
    [SerializeField] Image backgroundImage;
    [SerializeField] TextTyper mainText;
    [SerializeField] Transform nextButtonTransform;
    [SerializeField] Transform backButtonTransform;
    
    [Space(10)]
    [SerializeField] List<Transform> objectsToShrink = new List<Transform>();

    private void OnEnable()
    {
        onGameStart.AddListener(() => mainText.TypeText(CurrentGame));
        onGameStart.AddListener(SetBackgroundColor);
        onGameStart.AddListener(() => SlideButtons(false));
        onGameStart.AddListener(() => currentGame = GoogleDocParser.Instance.GetRandomGame());
        onGameStart.AddListener(() => currentPledge = GoogleDocParser.Instance.GetRandomPledge());
        
        onPledgeStart.AddListener(() => mainText.TypeText(CurrentPledge.text));
        
        onGameEnd.AddListener(() => SlideButtons(true));
        onGameEnd.AddListener(UnShrinkObjects);
        onGameEnd.AddListener(() => mainText.TypeText("")); // Clear text when game ends
        onGameEnd.AddListener(() => currentState = GameState.WaitingForStart);
    }
    
    private void OnDisable()
    {
        onGameStart.RemoveListener(() => mainText.TypeText(CurrentGame));
        onGameStart.RemoveListener(SetBackgroundColor);
        onGameStart.RemoveListener(() => SlideButtons(false));
        onGameStart.RemoveListener(() => currentGame = GoogleDocParser.Instance.GetRandomGame());
        onGameStart.RemoveListener(() => currentPledge = GoogleDocParser.Instance.GetRandomPledge());
        
        onPledgeStart.RemoveListener(() => mainText.TypeText(CurrentPledge.text));
        
        onGameEnd.RemoveListener(() => SlideButtons(true));
        onGameEnd.RemoveListener(UnShrinkObjects);
    }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void StartButtonPressed()
    {
        currentPledge = GoogleDocParser.Instance.GetRandomPledge();
        currentGame = GoogleDocParser.Instance.GetRandomGame();

        currentState = GameState.Game;
        
        if (objectsToShrink.Count == 0) return;
        foreach (var obj in objectsToShrink)
        {
            obj.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InCubic).OnComplete(onGameStart.Invoke);
        }
        
    }
    
    public void NextButtonPressed()
    {
        switch (currentState)
        {
            case GameState.Game:
                currentState = GameState.Pledge;
                onPledgeStart.Invoke();
                break;
            case GameState.Pledge:
                currentState = GameState.Game;
                onGameStart.Invoke();
                break;
        }
    }
    
    public void BackButtonPressed()
    {
        onGameEnd.Invoke();
    }
    
    private void UnShrinkObjects()
    {
        if (objectsToShrink.Count == 0) return;
        foreach (var obj in objectsToShrink)
        {
            obj.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutCubic);
        }
    }
    
    private void SetBackgroundColor()
    {
        if (backgroundImage == null) return;
        
        Color targetColor = Color.black;
            
        switch (CurrentPledge.severity)
        {
            case PledgeSeverity.Easy:
                targetColor = Color.green;
                break;
            case PledgeSeverity.Medium:
                targetColor = Color.yellow;
                break;
            case PledgeSeverity.Hard:
                targetColor = Color.red;
                break;
            default:
                backgroundImage.color = Color.magenta;
                break;
        }
            
        backgroundImage.DOColor(targetColor, 1f).SetEase(Ease.InOutQuad) ;
    }
    
    private void TypeGameText()
    {
        mainText.TypeText(CurrentGame);
    }

    private void SlideButtons(bool reversed = false)
    {
        if(currentState == GameState.WaitingForStart) return;
        
        int movementValue = reversed ? 850 : 340;

        if (reversed)
        {
            Debug.Log("Reversed");
        }
        
        var nextRect = nextButtonTransform.GetComponent<RectTransform>();
        var backRect = backButtonTransform.GetComponent<RectTransform>();

        nextRect.DOAnchorPosX(movementValue, 1.1f).SetEase(Ease.OutBack);
        backRect.DOAnchorPosX(-movementValue, 1.1f).SetEase(Ease.OutBack);
    }
}
