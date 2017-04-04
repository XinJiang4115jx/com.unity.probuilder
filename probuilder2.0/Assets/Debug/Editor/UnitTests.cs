#if PB_DEBUG

using UnityEngine;
using UnityEditor;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProBuilder2.Common;
using ProBuilder2.MeshOperations;
using Parabox.Debug;

/**
 *	Tests
 */
public class UnitTests : Editor
{
	static pb_Profiler profiler = new pb_Profiler("ProBuilder Performance Tests");

	static System.Func<bool>[] tests = new System.Func<bool>[]
	{
		VerifyAppendFace,
		VerifySubdividePipe,
		VerifyGetUniversalEdges,
		VerifyAppendVertices
	};

	static string[] friendly_testNames = new string[]
	{
		"VerifyAppendFace",
		"VerifySubdividePipe",
		"VerifyGetUniversalEdges",
		"VerifyAppendVertices"
	};

	[MenuItem("Tools/ProBuilder/Debug/Run Unit Tests")]
	public static void MenuRunUnitTests()
	{
		profiler.Reset();

		StringBuilder sb = new StringBuilder();

		for(int i = 0; i < tests.Length; i++)
		{
			EditorUtility.DisplayProgressBar("Unit Tests", friendly_testNames[i].ToString(), (1f/tests.Length) );
			sb.AppendLine(friendly_testNames[i].ToString() + (tests[i]() ? " Passed" : "  Failed"));
		}

		EditorUtility.ClearProgressBar();

		Debug.Log( profiler.ToString() );

		Debug.Log(sb.ToString());
	}

	[MenuItem("Tools/ProBuilder/Unit Tests/Find NearestIndexPriorToValue")]
	public static bool VerifyFindNearestIndex()
	{
		List<int> list = new List<int>() {
		//	0, 1, 2, 3, 4, 5, 6
			0, 1, 3, 4, 6, 7, 12
		};

		if( pbUtil.NearestIndexPriorToValue(list, 5) != 3 )
		{
			Debug.LogError("NearestIndexPriorToValue failed a test!");
			return false;
		}

		if( pbUtil.NearestIndexPriorToValue(list, -1) != -1 )
		{
			Debug.LogError("NearestIndexPriorToValue failed a test!");
			return false;
		}

		if( pbUtil.NearestIndexPriorToValue(list, 14) != 6 )
		{
			Debug.LogError("NearestIndexPriorToValue failed a test!");
			return false;
		}

		if( pbUtil.NearestIndexPriorToValue(list, 12) != 5 )
		{
			Debug.LogError("NearestIndexPriorToValue failed a test!  "  + pbUtil.NearestIndexPriorToValue(list, 12));
			return false;
		}

		if( pbUtil.NearestIndexPriorToValue(list, 0) != 0 )
		{
			Debug.LogError("NearestIndexPriorToValue failed a test!");
			return false;
		}

		list = new List<int>() { 0, 1, 2 };

		if( pbUtil.NearestIndexPriorToValue(list, 1) != 1 )
		{
			Debug.LogError("NearestIndexPriorToValue failed a test!");
			return false;
		}

		if( pbUtil.NearestIndexPriorToValue(list, 2) != 1 )
		{
			Debug.LogError("NearestIndexPriorToValue failed a test!" + pbUtil.NearestIndexPriorToValue(list, 2));
			return false;
		}

		if( pbUtil.NearestIndexPriorToValue(list, 0) != 0 )
		{
			Debug.LogError("NearestIndexPriorToValue failed a test!");
			return false;
		}

		if( pbUtil.NearestIndexPriorToValue(list, 3) != 2 )
		{
			Debug.LogError("NearestIndexPriorToValue failed a test!");
			return false;
		}

		if( pbUtil.NearestIndexPriorToValue(list, -192) != -1 )
		{
			Debug.LogError("NearestIndexPriorToValue failed a test!");
			return false;
		}

		Debug.Log("NearestIndexPriorToValue passed all tests!");
		return true;

	}

	private static bool VerifyAppendFace()
	{
		pb_Object pb = pb_ShapeGenerator.CubeGenerator(Vector3.one);
		pb_Face face = new pb_Face( new int[6] {0, 1, 2, 1, 3, 2} );
		Vector3[] vertices = new Vector3[4] {
			new Vector3(0f, 0f, 0f),
			new Vector3(1f, 0f, 0f),
			new Vector3(0f, 1f, 0f),
			new Vector3(1f, 1f, 0f)
		};

		Color[] colors = pbUtil.FilledArray(Color.white, 4);

		pb_Face result = pb.AppendFace(vertices, colors, new Vector2[vertices.Length], face);
		if(result == null) return false;

		if(pb.vertices[pb.vertices.Length-1] != new Vector3(1f, 1f, 0f))
		{
			DestroyImmediate(pb.gameObject);
			return false;
		}

		DestroyImmediate(pb.gameObject);
		return true;
	}

	private static bool VerifySubdividePipe()
	{
		#if PB_DEBUG && !PROTOTYPE
			pb_Object pb = pb_ShapeGenerator.CylinderGenerator(12, 5f, 5f, 6);

			profiler.BeginSample("Subdivide Pipe");
			bool success = pb.Subdivide();
			profiler.EndSample();

			// @todo
			float executionTime = .1f; //profiler.AverageTime("Subdivide Pipe");

			GameObject.DestroyImmediate(pb.gameObject);

			return success && executionTime < 1f;
		#else

			Debug.Log("TURN ON PROFILE TIMES WHEN RUNNING UNIT TESTS");
			return false;
		#endif
	}

	private static bool VerifyGetUniversalEdges()
	{
		#if PB_DEBUG && !PROTOTYPE

		// float radius, float height, float thickness, int subdivAxis, int subdivHeight)
		pb_Object pb = pb_ShapeGenerator.PipeGenerator(50f, 50f, 10f, 32, 32);

		profiler.BeginSample("GetUniversalEdges");
		pb_Edge.GetUniversalEdges(pb_Edge.AllEdges(pb.faces), pb.sharedIndices).Distinct().ToArray();
		profiler.EndSample();

		GameObject.DestroyImmediate(pb.gameObject);

		return true;
		#else
		Debug.Log("TURN ON PROFILE TIMES WHEN RUNNING UNIT TESTS");
		return false;
		#endif
	}

	private static bool VerifyAppendVertices()
	{
		pb_Object pb = pb_ShapeGenerator.CubeGenerator(Vector3.one);
		Vector3[] v = pb.vertices;

		pb_Face face = pb.faces[0];

		Vector3[] points = new Vector3[]
		{
			(v[face.edges[0].x] + v[face.edges[0].y])/2f,
			(v[face.edges[1].x] + v[face.edges[1].y])/2f
		};

		Color[] colors = new Color[]
		{
			(pb.colors[face.edges[0].x] + pb.colors[face.edges[0].y]) / 2f,
			(pb.colors[face.edges[1].x] + pb.colors[face.edges[1].y]) / 2f
		};

		pb_Face outFace = null;

		face = pb.faces[1];

		try
		{
			if( !pb.AppendVerticesToFace(face, points, colors, out outFace) )
			{
				GameObject.DestroyImmediate(pb.gameObject);
				return false;
			}
		}
		catch (System.Exception e)
		{
			Debug.LogError("AppendVerticesToFace: " + e.ToString());
			GameObject.DestroyImmediate(pb.gameObject);
			return false;
		}

		GameObject.DestroyImmediate(pb.gameObject);

		return true;
	}
}
#endif
