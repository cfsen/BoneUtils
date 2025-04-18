using BoneUtils.Tests;
using BoneUtils.Entity.Skeleton;
using Raylib_cs;
using System.Numerics;

//SkeletonEntityTests bt = new();
//bt.Entity_TranslateSpine();
//bt.Entity_RotateSpine();
//bt.Entity_WorldTranslateVectors();
//return;

RayLibVisualTest rlvt = new();
SkeletonEntity spine = rlvt.SimpleSpine();

Quaternion q = Quaternion.CreateFromYawPitchRoll(0.0f, MathF.PI/2, 0.0f);

Raylib.InitWindow(800, 600, "3D Test");
Raylib.SetTargetFPS(60);

Camera3D camera = new Camera3D {
    Position = new Vector3(6.0f, 2.0f, 0.0f),
    Target = new Vector3(0.0f, 2.0f, 0.0f),
    Up = new Vector3(0.0f, 1.0f, 0.0f),
    FovY = 60.0f,
    Projection = CameraProjection.Perspective
};

while (!Raylib.WindowShouldClose()) {
    Raylib.BeginDrawing();
    Raylib.ClearBackground(Color.Black);
    Raylib.BeginMode3D(camera);

	foreach(var bone in spine.Bones.Values) {
		Raylib.DrawSphere(spine.BoneWorldPosition(bone), 0.25f, Color.Red);
	}

    Raylib.EndMode3D();
	Raylib.DrawFPS(10, 10);

	rlvt.HandleInput(spine, q);

    Raylib.EndDrawing();
}

Raylib.CloseWindow();

