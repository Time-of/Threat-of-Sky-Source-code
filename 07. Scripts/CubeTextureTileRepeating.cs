using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/**
 * 작성자: 20181220 이성수
 * 사이즈를 늘려도 텍스쳐를 타일링할 수 있도록 하기 위해 만든 클래스입니다.
 * 큐브에만 적용됩니다. Plane 에 적용 시 텍스쳐가 깨집니다.
 * https://github.com/Dsphar/Cube_Texture_Auto_Repeat_Unity/blob/master/ReCalcCubeTexture.cs
 * 위 소스를 프로젝트에 맞게 수정한 버전입니다.
 */
public class CubeTextureTileRepeating : MonoBehaviour
{
	private Renderer RenderComponent;

	//[SerializeField]
	//private float TileCountX = 1.0f;

	//[SerializeField]
	//private float TileCountY = 1.0f;



	void Awake()
	{
		RenderComponent = GetComponent<Renderer>();

		TileTexture();
	}



	void TileTexture()
	{
		//RenderComponent.material.mainTextureScale = new Vector2(TileCountX, TileCountY);

		

#if UNITY_EDITOR
		Mesh StaticMesh = Instantiate(GetComponent<MeshFilter>().sharedMesh);
		GetComponent<MeshFilter>().mesh = StaticMesh;

		StaticMesh.SetUVs(0, SetupUV(StaticMesh));
#else
		Mesh StaticMesh = GetComponent<MeshFilter>().mesh;

		//StaticMesh.uv = SetupUV(StaticMesh);
		StaticMesh.SetUVs(0, SetupUV(StaticMesh));
#endif
	
		RenderComponent.sharedMaterial.mainTexture.wrapMode = TextureWrapMode.Repeat;
	}



	Vector2[] SetupUV(Mesh StaticMesh)
	{
		Vector2[] MeshUVs = StaticMesh.uv;

		float Width = transform.localScale.x;
		float Height = transform.localScale.y;
		float Depth = transform.localScale.z;

		// Front
		MeshUVs[2] = new Vector2(0, Height);
		MeshUVs[3] = new Vector2(Width, Height);
		MeshUVs[0] = new Vector2(0, 0);
		MeshUVs[1] = new Vector2(Width, 0);

		// Back
		MeshUVs[7] = new Vector2(0, 0);
		MeshUVs[6] = new Vector2(Width, 0);
		MeshUVs[11] = new Vector2(0, Height);
		MeshUVs[10] = new Vector2(Width, Height);

		// Left
		MeshUVs[19] = new Vector2(Depth, 0);
		MeshUVs[17] = new Vector2(0, Height);
		MeshUVs[16] = new Vector2(0, 0);
		MeshUVs[18] = new Vector2(Depth, Height);

		// Right
		MeshUVs[23] = new Vector2(Depth, 0);
		MeshUVs[21] = new Vector2(0, Height);
		MeshUVs[20] = new Vector2(0, 0);
		MeshUVs[22] = new Vector2(Depth, Height);

		// Top
		MeshUVs[4] = new Vector2(Width, 0);
		MeshUVs[5] = new Vector2(0, 0);
		MeshUVs[8] = new Vector2(Width, Depth);
		MeshUVs[9] = new Vector2(0, Depth);

		// Bottom
		MeshUVs[13] = new Vector2(Width, 0);
		MeshUVs[14] = new Vector2(0, 0);
		MeshUVs[12] = new Vector2(Width, Depth);
		MeshUVs[15] = new Vector2(0, Depth);

		return MeshUVs;
	}
}
