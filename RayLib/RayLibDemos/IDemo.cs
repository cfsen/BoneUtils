namespace BoneUtils.RayLib.RayLibDemos;
public interface IDemo {
	public void Draw();
	public void HandleDemoInput();
	public void DrawHelpOverlay();
	public void Update(float deltaTime);
}
