using BoneUtils.Mockups;
using BoneUtils.Entity.Skeleton;
using Raylib_cs;
using System.Numerics;
using System.Diagnostics;
using BoneUtils.Helpers;
using BoneUtils.RayLib;

RayLibVisualTest rlvt = new();

Raylib.InitWindow(800, 600, "BoneUtils RayLib Demo");
Raylib.SetTargetFPS(60);

Camera3D camera = new Camera3D {
    Position = new Vector3(6.0f, 2.0f, 0.0f),
    Target = new Vector3(0.0f, 2.0f, 0.0f),
    Up = new Vector3(0.0f, 1.0f, 0.0f),
    FovY = 45.0f,
    Projection = CameraProjection.Perspective
};

float deltaTime = 0.0f;
float deltaTimeHigh = 0.0f;

while (!Raylib.WindowShouldClose()) {
	rlvt.HandleInput();
	rlvt.ActiveDemo.HandleDemoInput();

	deltaTime = Raylib.GetFrameTime();
	rlvt.ActiveDemo.Update(deltaTime);

	if(deltaTime > deltaTimeHigh) deltaTimeHigh = deltaTime;

    Raylib.BeginDrawing();
    Raylib.ClearBackground(Color.Black);

    Raylib.BeginMode3D(camera);
	rlvt.DrawDemo();
    Raylib.EndMode3D();

	Raylib.DrawFPS(10, 10);
	Raylib.DrawText($"dt: {deltaTime}", 100, 10, 16, Color.Green);
	Raylib.DrawText($"high: {deltaTimeHigh}", 100, 26, 16, Color.Green);
	rlvt.DrawHelp();
	rlvt.DrawHelpOverlay();

    Raylib.EndDrawing();

}

Raylib.CloseWindow();
