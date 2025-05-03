using Raylib_cs;
using static Raylib_cs.Raylib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Numerics;
using System.Diagnostics;
using System.Reflection;

namespace BoneUtils.RayLib;

/*
Adapted from: 
https://github.com/raylib-cs/raylib-cs/blob/master/Examples/Shaders/BasicLighting.cs
https://github.com/raylib-cs/raylib-cs/blob/master/Examples/Shared/Rlights.cs

Using shaders collected from Raylib-cs: lightning.vs, lighting.fs

*/
public class RaylibRenderer {
	private int SceneLightCount = 0;
	private Light[] SceneLights = new Light[3];
	private Shader Shader;
	private Model Sphere;

	public Camera3D Camera;

	public RaylibRenderer(Camera3D cam) {
		Camera = cam;
	}

	public unsafe void Init() {
		Mesh m = GenMeshSphere(0.3f, 16, 16);
		Sphere = LoadModelFromMesh(m);

		string fs = ReadEmbeddedText("BoneUtils.RayLib.Resources.Shaders.lighting.fs");
		string vs = ReadEmbeddedText("BoneUtils.RayLib.Resources.Shaders.lighting.vs");
		Shader = LoadShaderFromMemory(vs, fs);

        Shader.Locs[(int)ShaderLocationIndex.VectorView] = GetShaderLocation(Shader, "viewPos");

        int ambientLoc = GetShaderLocation(Shader, "ambient");
        float[] ambient = [0.1f, 0.1f, 0.1f, 1.0f];
        SetShaderValue(Shader, ambientLoc, ambient, ShaderUniformDataType.Vec4);

        Sphere.Materials[0].Shader = Shader;

		for(int i = 0; i <3; i++)
			SceneLights[i] = CreateLight(Shader, i);
 	}

	public void Unload() {
		// TODO
	}

	public void DrawSphere(Vector3 pos, float scale = 1.0f) {
		DrawModel(Sphere, pos, scale, Color.White);
	}

	public unsafe void UpdateLights() {
		for(int i = 0; i < 3; i++) {
			SetShaderValue(Shader, SceneLights[i].EnabledLoc, 1, ShaderUniformDataType.Int);
			SetShaderValue(Shader, SceneLights[i].TypeLoc, SceneLights[i].Type, ShaderUniformDataType.Int);
			SetShaderValue(Shader, SceneLights[i].PosLoc, SceneLights[i].Position, ShaderUniformDataType.Vec3);
			SetShaderValue(Shader, SceneLights[i].TargetLoc, SceneLights[i].Target, ShaderUniformDataType.Vec3);
			SetShaderValue(Shader, SceneLights[i].ColorLoc, SceneLights[i].Color, ShaderUniformDataType.Vec4);
		}
		SetShaderValue(
			Shader, 
			Shader.Locs[(int)ShaderLocationIndex.VectorView], 
			Camera.Position, 
			ShaderUniformDataType.Vec3);
	}

	public void DrawLightDebug() {
		for(int i = 0; i < 3; i++) {
			DrawSphere(SceneLights[i].Position, 0.5f);
		}
	}

	// Helpers

	private Light CreateLight(Shader shader, int i) {
		Light l = new() {
			Type	 = LightType.Point,
			Position = SceneLightCount switch {
				0 => new Vector3(6.0f, 3.0f, 3.0f),
				1 => new Vector3(-4.0f, 1.0f, -6.0f),
				_ => new Vector3(-8.0f, 6.0f, 0.0f)
			},
			Target	 = Vector3.Zero,
			Color	 = i switch {
				0 => new Vector4(1, 1, 1, 0.4f),
				1 => new Vector4(0.6f, 0.8f, 0.8f, 0.4f),
				_ => new Vector4(0.3f, 0.3f, 0.3f, 0.3f)
			},
			
			EnabledLoc	 = GetShaderLocation(shader, $"lights[{SceneLightCount}].enabled"),
			TypeLoc		 = GetShaderLocation(shader, $"lights[{SceneLightCount}].type"),
			PosLoc		 = GetShaderLocation(shader, $"lights[{SceneLightCount}].position"),
			TargetLoc	 = GetShaderLocation(shader, $"lights[{SceneLightCount}].target"),
			ColorLoc	 = GetShaderLocation(shader, $"lights[{SceneLightCount}].color"),
		};

		SceneLightCount++;
		return l;
	}

	private string ReadEmbeddedText(string resourcePath) {
		using Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath)
			?? throw new Exception($"Embedded resource '{resourcePath}' not found.");
		using StreamReader reader = new(stream);
		return reader.ReadToEnd();
	}

	// Structures for lights

	private enum LightType {
		Directorional,
		Point
	}
	private struct Light {
		public LightType Type;
		public Vector3 Position;
		public Vector3 Target;
		public Vector4 Color;

		public int EnabledLoc;
		public int TypeLoc;
		public int PosLoc;
		public int TargetLoc;
		public int ColorLoc;
	}
}
