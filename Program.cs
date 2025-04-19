using BoneUtils.Tests;
using BoneUtils.Entity.Skeleton;
using Raylib_cs;
using System.Numerics;
using System.Diagnostics;

//SkeletonEntityTests bt = new();
//bt.Entity_TranslateSpine();
//bt.Entity_RotateSpine();
//bt.Entity_WorldTranslateVectors();
//return;

RayLibVisualTest rlvt = new();

Raylib.InitWindow(800, 600, "BoneUtils RayLib Demo");
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
	rlvt.DrawDemo();
    Raylib.EndMode3D();

	Raylib.DrawFPS(10, 10);
	rlvt.DrawHelp();
	rlvt.DrawHelpOverlay();

	rlvt.HandleInput();
	rlvt.ActiveDemo.HandleDemoInput();

    Raylib.EndDrawing();
}

Raylib.CloseWindow();
