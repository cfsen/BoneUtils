using BoneUtils.RayLib;
using Raylib_cs;

RaylibDemoRunner rldr = new();

Raylib.InitWindow(800, 600, "BoneUtils demo");
Raylib.SetTargetFPS(60);

while (!Raylib.WindowShouldClose()) {
	rldr.HandleInput();
	rldr.Update(Raylib.GetFrameTime());

    Raylib.BeginDrawing();
		Raylib.ClearBackground(Color.Black);

		Raylib.BeginMode3D(rldr.Camera);
			rldr.DrawDemo3D();
		Raylib.EndMode3D();

		rldr.DrawRunner2D();
		rldr.DrawDemo2D();
    Raylib.EndDrawing();
}

Raylib.CloseWindow();
