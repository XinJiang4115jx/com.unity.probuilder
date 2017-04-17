using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;
using ProBuilder2.Interface;
using ProBuilder2.Common;

#if PB_DEBUG
using Parabox.Debug;
#endif

namespace ProBuilder2.EditorCommon
{
	/**
	 * Assign materials to faces and objects.
	 */
	public class pb_Material_Editor : EditorWindow
	{
#if PROTOTYPE
		public static void MenuOpenMaterialEditor()
		{
			Debug.LogWarning("Material Editor is ProBuilder Advanced feature.");
		}
#else
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 1 &1", true, pb_Constant.MENU_MATERIAL_COLORS)]
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 2 &2", true, pb_Constant.MENU_MATERIAL_COLORS)]
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 3 &3", true, pb_Constant.MENU_MATERIAL_COLORS)]
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 4 &4", true, pb_Constant.MENU_MATERIAL_COLORS)]
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 5 &5", true, pb_Constant.MENU_MATERIAL_COLORS)]
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 6 &6", true, pb_Constant.MENU_MATERIAL_COLORS)]
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 7 &7", true, pb_Constant.MENU_MATERIAL_COLORS)]
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 8 &8", true, pb_Constant.MENU_MATERIAL_COLORS)]
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 9 &9", true, pb_Constant.MENU_MATERIAL_COLORS)]
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 10 &0", true, pb_Constant.MENU_MATERIAL_COLORS)]
		public static bool VerifyMaterialAction()
		{
			return pb_Editor.instance != null && pb_Editor.instance.selection.Length > 0;
		}

		private static pb_Editor editor { get { return pb_Editor.instance; } }
		public static pb_Material_Editor instance { get; private set; }
 		private static string m_UserMaterialsPath = "Assets/ProCore/" + pb_Constant.PRODUCT_NAME + "/Data/UserMaterials.asset";

		private static string userMaterialsPath
		{
			get
			{
				if( pb_FileUtil.Exists(m_UserMaterialsPath) )
					return m_UserMaterialsPath;

				m_UserMaterialsPath = pb_FileUtil.FindFile("/Data/UserMaterials.asset");

				return m_UserMaterialsPath;
			}
		}

		public static void MenuOpenMaterialEditor()
		{
			EditorWindow.GetWindow<pb_Material_Editor>(
				pb_Preferences_Internal.GetBool(pb_Constant.pbMaterialEditorFloating),
				"Material Editor",
				true).Show();
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 1 &1", false, pb_Constant.MENU_MATERIAL_COLORS)]
		public static void ApplyMaterial0() {
			Material[] mats;
			if( LoadMaterialPalette(out mats) ) ApplyMaterial(GetSelection(), mats[0]);
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 2 &2", false, pb_Constant.MENU_MATERIAL_COLORS)]
		public static void ApplyMaterial1() {
			Material[] mats;
			if( LoadMaterialPalette(out mats) ) ApplyMaterial(GetSelection(), mats[1]);
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 3 &3", false, pb_Constant.MENU_MATERIAL_COLORS)]
		public static void ApplyMaterial2() {
			Material[] mats;
			if( LoadMaterialPalette(out mats) ) ApplyMaterial(GetSelection(), mats[2]);
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 4 &4", false, pb_Constant.MENU_MATERIAL_COLORS)]
		public static void ApplyMaterial3() {
			Material[] mats;
			if( LoadMaterialPalette(out mats) ) ApplyMaterial(GetSelection(), mats[3]);
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 5 &5", false, pb_Constant.MENU_MATERIAL_COLORS)]
		public static void ApplyMaterial4() {
			Material[] mats;
			if( LoadMaterialPalette(out mats) ) ApplyMaterial(GetSelection(), mats[4]);
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 6 &6", false, pb_Constant.MENU_MATERIAL_COLORS)]
		public static void ApplyMaterial5() {
			Material[] mats;
			if( LoadMaterialPalette(out mats) ) ApplyMaterial(GetSelection(), mats[5]);
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 7 &7", false, pb_Constant.MENU_MATERIAL_COLORS)]
		public static void ApplyMaterial6() {
			Material[] mats;
			if( LoadMaterialPalette(out mats) ) ApplyMaterial(GetSelection(), mats[6]);
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 8 &8", false, pb_Constant.MENU_MATERIAL_COLORS)]
		public static void ApplyMaterial7() {
			Material[] mats;
			if( LoadMaterialPalette(out mats) ) ApplyMaterial(GetSelection(), mats[7]);
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 9 &9", false, pb_Constant.MENU_MATERIAL_COLORS)]
		public static void ApplyMaterial8() {
			Material[] mats;
			if( LoadMaterialPalette(out mats) ) ApplyMaterial(GetSelection(), mats[8]);
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 10 &0", false, pb_Constant.MENU_MATERIAL_COLORS)]
		public static void ApplyMaterial9() {
			Material[] mats;
			if( LoadMaterialPalette(out mats) ) ApplyMaterial(GetSelection(), mats[9]);
		}

		void OnEnable()
		{
			instance = this;
			this.autoRepaintOnSceneChange = true;

			this.minSize = new Vector2(236, 250);

			rowBackgroundStyle = new GUIStyle();
			rowBackgroundStyle.normal.background = EditorGUIUtility.whiteTexture;

			LoadMaterialPalette(out materials);
		}

		void OnDisable()
		{
			instance = null;
		}

		static bool LoadMaterialPalette(out Material[] materials)
		{
			pb_ObjectArray poa = (pb_ObjectArray) AssetDatabase.LoadAssetAtPath(userMaterialsPath, typeof(pb_ObjectArray));

			if(poa != null)
			{
				materials = poa.GetArray<Material>();
				return true;
			}
			else
			{
				materials = new Material[10]
				{
					pb_Constant.DefaultMaterial,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null
				};
				return false;
			}
		}

		void OpenContextMenu()
		{
			GenericMenu menu = new GenericMenu();

			menu.AddItem (new GUIContent("Window/Open as Floating Window", ""), false, () => { SetFloating(true); } );
			menu.AddItem (new GUIContent("Window/Open as Dockable Window", ""), false, () => { SetFloating(false); } );

			menu.ShowAsContext ();
		}

		void SetFloating(bool floating)
		{
			EditorPrefs.SetBool(pb_Constant.pbMaterialEditorFloating, floating);
			this.Close();
			MenuOpenMaterialEditor();
		}

		void SaveUserMaterials()
		{
			pb_ObjectArray poa = (pb_ObjectArray)ScriptableObject.CreateInstance(typeof(pb_ObjectArray));

			poa.array = materials;

			if(!pb_FileUtil.Exists(m_UserMaterialsPath))
			{
				string probuilder_path = pb_FileUtil.GetRootDir();

				if(string.IsNullOrEmpty(probuilder_path))
				{
					Debug.LogWarning("Could not find ProBuilder folder.  Make sure it has not been renamed.");
					probuilder_path = "Assets/ProCore/ProBuilder/";
				}

				System.IO.Directory.CreateDirectory(string.Format("{0}Data", probuilder_path));
				m_UserMaterialsPath = string.Format("{0}Data/UserMaterials.asset", probuilder_path);
			}

			AssetDatabase.CreateAsset(poa, m_UserMaterialsPath);
			AssetDatabase.SaveAssets();
		}

		// Functional vars
		Material queuedMaterial;
		Material[] materials;

		// GUI vars
		GUIStyle rowBackgroundStyle;
		Vector2 scroll = Vector2.zero;

		void OnGUI()
		{
			if(Event.current.type == EventType.ContextClick)
				OpenContextMenu();

			GUILayout.Label("Quick Material", EditorStyles.boldLabel);
			Rect r = GUILayoutUtility.GetLastRect();
			int left = Screen.width - 68;

			GUILayout.BeginHorizontal(GUILayout.MaxWidth(Screen.width-74));
				GUILayout.BeginVertical();

					queuedMaterial = (Material)EditorGUILayout.ObjectField(queuedMaterial, typeof(Material), false);

					GUILayout.Space(2);

					if(GUILayout.Button("Apply (Ctrl+Shift+Click)"))
						ApplyMaterial(GetSelection(), queuedMaterial);

					GUI.enabled = editor != null && editor.selectedFaceCount > 0;
					if(GUILayout.Button("Match Selection"))
					{
						pb_Object tp;
						pb_Face tf;
						if( editor.GetFirstSelectedFace(out tp, out tf) )
							queuedMaterial = tf.material;
					}
					GUI.enabled = true;

				GUILayout.EndVertical();

				GUI.Box( new Rect(left, r.y + r.height + 2, 64, 64), "" );
				if(queuedMaterial != null && queuedMaterial.mainTexture != null)
					EditorGUI.DrawPreviewTexture( new Rect(left+2, r.y + r.height + 4, 60, 60), queuedMaterial.mainTexture, queuedMaterial, ScaleMode.StretchToFill, 0);
				else
				{
					GUI.Box( new Rect(left+2, r.y + r.height + 4, 60, 60), "" );
					GUI.Label( new Rect(left +2, r.height + 28, 120, 32), "None\n(Texture)");
				}

			GUILayout.EndHorizontal();

			GUILayout.Space(4);

			GUI.backgroundColor = pb_Constant.ProBuilderDarkGray;
			pb_GUI_Utility.DrawSeparator(2);
			GUI.backgroundColor = Color.white;

			GUILayout.Label("Material Palette", EditorStyles.boldLabel);

			scroll = GUILayout.BeginScrollView(scroll);

				for(int i = 0; i < materials.Length; i++)
				{
					if(i == 10)
					{
						GUILayout.Space(2);
						GUI.backgroundColor = pb_Constant.ProBuilderLightGray;
						pb_GUI_Utility.DrawSeparator(1);
						GUI.backgroundColor = Color.white;
						GUILayout.Space(2);
					}

					GUILayout.BeginHorizontal();
						if(i < 10)
						{
							if(GUILayout.Button("Alt + " + (i == 9 ? 0 : (i+1)).ToString(), EditorStyles.miniButton, GUILayout.MaxWidth(58)))
								ApplyMaterial(GetSelection(), materials[i]);
						}
						else
						{
							if(GUILayout.Button("Apply", EditorStyles.miniButtonLeft, GUILayout.MaxWidth(44)))
								ApplyMaterial(GetSelection(), materials[i]);

							GUI.backgroundColor = Color.red;
							if(GUILayout.Button("", EditorStyles.miniButtonRight, GUILayout.MaxWidth(14)))
							{
								Material[] temp = new Material[materials.Length-1];
								System.Array.Copy(materials, 0, temp, 0, materials.Length-1);
								materials = temp;
								SaveUserMaterials();
								return;
							}
							GUI.backgroundColor = Color.white;
						}

						EditorGUI.BeginChangeCheck();
							materials[i] = (Material)EditorGUILayout.ObjectField(materials[i], typeof(Material), false);
						if( EditorGUI.EndChangeCheck() )
							SaveUserMaterials();

					GUILayout.EndHorizontal();
				}


				if(GUILayout.Button("Add"))
				{
					Material[] temp = new Material[materials.Length+1];
					System.Array.Copy(materials, 0, temp, 0, materials.Length);
					materials = temp;
					SaveUserMaterials();
				}
			GUILayout.EndScrollView();
		}

		static pb_Object[] GetSelection()
		{
			return Selection.gameObjects.Select(x => x.GetComponent<pb_Object>()).Where(y => y != null).ToArray();
		}

		/**
		 * Applies the currently queued material to the selected face and eats the event.

		 */
		public bool ClickShortcutCheck(EventModifiers em, pb_Object pb, pb_Face quad)
		{
			if(pb_UV_Editor.instance == null)
			{
				if(em == (EventModifiers.Control | EventModifiers.Shift))
				{
					pbUndo.RecordObject(pb, "Quick Apply");
					pb.SetFaceMaterial( new pb_Face[1] { quad }, queuedMaterial);
					OnFaceChanged(pb);
					pb_EditorUtility.ShowNotification("Quick Apply Material");
					return true;
				}
			}

			return false;
		}

		public static void ApplyMaterial(pb_Object[] selection, Material mat)
		{
			if(mat == null) return;

			pbUndo.RecordSelection(selection, "Set Face Materials");

			foreach(pb_Object pb in selection)
			{
				pb_Face[] faces = pb.SelectedFaces;
				pb.SetFaceMaterial(faces == null || faces.Length < 1 ? pb.faces : faces, mat);

				OnFaceChanged(pb);
			}

			if(pb_Editor.instance != null && pb_Editor.instance.selectedFaceCount > 0)
				pb_EditorUtility.ShowNotification("Set Material\n" + mat.name);
		}

		private static void OnFaceChanged( pb_Object pb )
		{
			pb.ToMesh();
			pb.Refresh();
			pb.Optimize();

			// StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags( pb.gameObject );

			// // if nodraw not found, and entity type should be batching static
			// if(pb.GetComponent<pb_Entity>().entityType != EntityType.Mover)
			// {
			// 	flags = flags | StaticEditorFlags.BatchingStatic;
			// 	GameObjectUtility.SetStaticEditorFlags(pb.gameObject, flags);
			// }
		}
#endif
	}
}
