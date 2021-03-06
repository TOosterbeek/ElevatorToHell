using Godot;

public class SceneManager : Node2D
{
	[Signal]
	public delegate void LevelCompleted(LevelResult result);

	[Signal]
	public delegate void StartNextLevel();

	Node currentScene;

	LevelResult previousLevelResult;

	AudioStreamPlayer2D musicPlayer;

	public override void _Ready()
	{
		Start();
	}

	public void Start(){
		previousLevelResult = new LevelResult();
		previousLevelResult.waveNum = 0;
		previousLevelResult.altitude = 120;

		musicPlayer = GetNode<AudioStreamPlayer2D>("MainMusicPlayer");
		musicPlayer.Play();

		PackedScene scene = GD.Load<PackedScene>("res://levels/MainMenu.tscn");
		currentScene = scene.Instance();

		this.Connect("LevelCompleted", this, "OnLevelCompleted");
		currentScene.Connect("StartNextLevel", this, "OnStartNextLevel");

		AddChild(currentScene);
	}

	public void Restart(){
		if(currentScene != null){
			RemoveChild(currentScene);
			currentScene.QueueFree();
		}

		Start();
	}

	//testing

	public void OnStartNextLevel()
	{
		PackedScene scene = GD.Load<PackedScene>("res://levels/Game.tscn");
		RemoveChild(currentScene);

		currentScene = scene.Instance();
		currentScene.Connect("ready", this, "OnLevelReady");
		currentScene.Connect("LevelCompleted", this, "OnLevelCompleted");
		currentScene.Connect("GameOver", this, "OnGameOver");

		musicPlayer.Stop();
		AddChild(currentScene);
	}

	public void OnLevelReady()
	{
		int waveNum = 1;
		int altitude = 100;
		if (previousLevelResult != null)
		{
			waveNum = previousLevelResult.waveNum + 1;
			altitude = previousLevelResult.altitude;
		}

		GameManager gameManager = (GameManager)FindNode("Game", true, false);
		gameManager.StartLevel(waveNum, altitude, previousLevelResult);
	}


	public void OnLevelContinue()
	{
		PackedScene scene = GD.Load<PackedScene>("res://levels/SceneTransition.tscn");

		currentScene.Disconnect("ready", this, "PlayFadeInAnimation");
		RemoveChild(currentScene);
		// currentScene.Dispose();

		currentScene = scene.Instance();
		currentScene.Connect("ready", this, "PlayFadeInAnimation");

		AddChild(currentScene);
	}

	public void PlayFadeInAnimation()
	{
		AnimationPlayer animationPlayer = (AnimationPlayer)FindNode("AnimationPlayer", true, false);
		Label delvingDeeperLabel = (Label)FindNode("DelvingDeeperLabel", true, false);
		delvingDeeperLabel.Visible = true;

		ColorRect colorRect = (ColorRect)FindNode("SceneTransitionRect", true, false);

		animationPlayer.Connect("animation_finished", colorRect, "FaderAnimationEnd");
		animationPlayer.Connect("animation_finished", this, "FadeInFinished");
		animationPlayer.Connect("animation_started", colorRect, "FaderAnimationStart");

		animationPlayer.Play("FadeOut");
	}

	public void FadeInFinished(string name)
	{
		OnStartNextLevel();
	}

	private void OnLevelCompleted(LevelResult result)
	{
		previousLevelResult = result;
		PackedScene scene = GD.Load<PackedScene>("res://levels/SceneTransition.tscn");
		RemoveChild(currentScene);

		currentScene = scene.Instance();
		currentScene.Connect("ready", this, "PlayFadeOutAnimation");

		AddChild(currentScene);
	}

	public void PlayFadeOutAnimation()
	{
		AnimationPlayer animationPlayer = (AnimationPlayer)FindNode("AnimationPlayer", true, false);
		ColorRect colorRect = (ColorRect)FindNode("SceneTransitionRect", true, false);

		animationPlayer.Connect("animation_finished", colorRect, "FaderAnimationEnd");
		animationPlayer.Connect("animation_finished", this, "FadeOutFinished");
		animationPlayer.Connect("animation_started", colorRect, "FaderAnimationStart");

		animationPlayer.Play("FadeIn");
	}

	public void FadeOutFinished(string name)
	{
		PackedScene scene = GD.Load<PackedScene>("res://levels/MovingDown.tscn");
		RemoveChild(currentScene);
		currentScene.Dispose();

		currentScene = scene.Instance();
		currentScene.Connect("ready", this, "OnMovingDownMenuReady");

		AddChild(currentScene);
	}

	public void OnMovingDownMenuReady()
	{
		musicPlayer.Play();
		if (previousLevelResult == null)
		{
			previousLevelResult = new LevelResult();
			previousLevelResult.waveNum = 1;
		}

		MovingDown movingDown = (MovingDown)FindNode("MovingDown", true, false);
		movingDown.OnLevelCompleted(previousLevelResult, this);
	}


	private void OnGameOver(LevelResult result)
	{
		previousLevelResult = result;
		PackedScene scene = GD.Load<PackedScene>("res://levels/SceneTransition.tscn");
		RemoveChild(currentScene);

		currentScene = scene.Instance();
		currentScene.Connect("ready", this, "PlayFadeOutGameOverAnimation");

		AddChild(currentScene);
	}

	public void PlayFadeOutGameOverAnimation()
	{
		AnimationPlayer animationPlayer = (AnimationPlayer)FindNode("AnimationPlayer", true, false);
		ColorRect colorRect = (ColorRect)FindNode("SceneTransitionRect", true, false);

		animationPlayer.Connect("animation_finished", colorRect, "FaderAnimationEnd");
		animationPlayer.Connect("animation_finished", this, "FadeOutGameOverFinished");
		animationPlayer.Connect("animation_started", colorRect, "FaderAnimationStart");

		animationPlayer.Play("FadeIn");
	}

	public void FadeOutGameOverFinished(string name)
	{
		PackedScene scene = GD.Load<PackedScene>("res://levels/GameOver.tscn");
		RemoveChild(currentScene);

		currentScene = scene.Instance();
		currentScene.Connect("ready", this, "OnGameOverMenuReady");

		AddChild(currentScene);
	}

	public void OnGameOverMenuReady()
	{
		musicPlayer.Play();
		if (previousLevelResult == null)
		{
			previousLevelResult = new LevelResult();
			previousLevelResult.waveNum = 1;
		}

		GameOver gameOver = (GameOver)FindNode("GameOver", true, false);
		gameOver.OnLevelCompleted(previousLevelResult, this);
	}
}