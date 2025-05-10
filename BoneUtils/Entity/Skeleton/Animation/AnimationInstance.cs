namespace BoneUtils.Entity.Skeleton.Animation;
public class AnimationInstance {
	// Animation data
	public AnimationContainer Animation; 

	// Animation state
	public bool IsRunning = false;
	public bool Loop = true;
	public float deltaTimeStarted = 0.0f;

	// Blending delegate for animatinon
	public delegate TransformSnapshot XfmBlender(TransformSnapshot xfmOrigin, TransformSnapshot xfmTarget);

	// KeyframeFinder lookup optimizers
	public int KeyframeCount = 0;
	public float LastLookupTime = 0;
	public int LastLookupKeyframe = 1;
	public int LastOrigin = -1;
	public int LastTarget = -1;
	public AnimationInstance(AnimationContainer animationContainer) {
		Animation = animationContainer;
		KeyframeCount = Animation.Keyframes.Count;
		if(KeyframeCount < 2)
			throw new Exception("Animations must have at least two keyframes."); // TODO find appropriate built in exception
	}
	public override string ToString() {
		return $"""
			--- BGN: AnimationInstance
			Animation: 
				Total Duration: {Animation.TotalDuration} 
				Type: {Animation.Type} 
			IsRunning: {IsRunning} 
			Loop: {Loop} 
			deltaTimeStarted: {deltaTimeStarted} 
			KeyframeCount: {KeyframeCount} 
			LastLookupTime: {LastLookupTime} 
			LastLookupKeyframe: {LastLookupKeyframe} 
			LastOrigin: {LastOrigin} 
			LastTarget: {LastTarget} 
			--- END: AnimationInstance
			""";
	}
}
