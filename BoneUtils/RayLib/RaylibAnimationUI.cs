using BoneUtils.Entity.Skeleton.Animation;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoneUtils.RayLib; 
public class RaylibAnimationUI {
	private int ScreenWidth = 800;
	private int ScreenHeight = 600;
	private int MarginX = 20;
	private int MarginY = 30;
	private SkeletonAnimator SkeletonAnimator;

	// hoisted
	float _normalizedTime = 0f;
	float _normalizedPosition = 0f;
	int _xpos = 0;
	public RaylibAnimationUI(SkeletonAnimator animOps) {
		this.SkeletonAnimator = animOps;
	} 
	public void Draw2DTimeline() {
		for(int i = 0; i < SkeletonAnimator.Animations.Count; i++) {
			Raylib.DrawLine(
				MarginX, 
				ScreenHeight-MarginY-(i*MarginY/2), 
				ScreenWidth-MarginX, ScreenHeight-MarginY-(i*MarginY/2),
				Color.DarkGreen
				);

			_normalizedTime = NormalizeAnimationTime(SkeletonAnimator.Runtime, SkeletonAnimator.Animations[i]);
			for(int j = 0; j < SkeletonAnimator.Animations[i].KeyframeCount; j++) {
				TimeToTimelinePos(
					SkeletonAnimator.Animations[i].Animation.Keyframes[j].TimelinePosition, 
					SkeletonAnimator.Animations[i].Animation.TotalDuration, 
					ScreenHeight-MarginY-(i*MarginY/2)
					);
				TimeToTimelinePos(
					_normalizedTime,
					SkeletonAnimator.Animations[i].Animation.TotalDuration, 
					ScreenHeight-MarginY-(i*MarginY/2),
					true
					);
			}
		}
	}
	private float NormalizeAnimationTime(float animatorTime, AnimationInstance inst) {
		return (animatorTime - inst.deltaTimeStarted) % inst.Animation.TotalDuration;
	}
	private void TimeToTimelinePos(float key, float animationTotalDuration, int ypos, bool isCursor = false) {
		_normalizedPosition = key / animationTotalDuration;
		_xpos = (int)(_normalizedPosition*(ScreenWidth-2*MarginX) + MarginX);
		Raylib.DrawLine(_xpos, ypos, _xpos, ypos-(MarginY/3), 
			isCursor ? Color.SkyBlue : Color.DarkGreen
			);
		if(isCursor) {
			Raylib.DrawLine(_xpos, ypos, _xpos+5, ypos-(MarginY/6), Color.SkyBlue);
			Raylib.DrawLine(_xpos, ypos-(MarginY/3), _xpos+5, ypos-(MarginY/6), Color.SkyBlue);
		}
	}
}
