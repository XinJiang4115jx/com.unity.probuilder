using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
	/// <inheritdoc />
	/// <summary>
	/// Options menu window container. Do not instantiate this yourself, the toolbar will handle opening option windows.
	/// </summary>
	public sealed class MenuOption : EditorWindow
	{
        System.Action onSettingsGUI = null;
        System.Action onSettingsDisable = null;

		internal static MenuOption Show(System.Action onSettingsGUI, System.Action onSettingsEnable, System.Action onSettingsDisable)
		{
			MenuOption win = EditorWindow.GetWindow<MenuOption>(true, "Options", true);
			win.hideFlags = HideFlags.HideAndDontSave;

			if(win.onSettingsDisable != null)
				win.onSettingsDisable();

			if(onSettingsEnable != null)
				onSettingsEnable();

			win.onSettingsDisable = onSettingsDisable;

			win.onSettingsGUI = onSettingsGUI;

			// don't let window hang around after a script reload nukes the pb_MenuAction instances
			object parent = ReflectionUtility.GetValue(win, typeof(EditorWindow), "m_Parent");
			object window = ReflectionUtility.GetValue(parent, typeof(EditorWindow), "window");
			ReflectionUtility.SetValue(parent, "mouseRayInvisible", true);
			ReflectionUtility.SetValue(window, "m_DontSaveToLayout", true);

			win.Show();

			return win;
		}

		/// <summary>
		/// Close any currently open option windows.
		/// </summary>
		public static void CloseAll()
		{
			foreach(MenuOption win in Resources.FindObjectsOfTypeAll<MenuOption>())
				win.Close();
		}

		void OnEnable()
		{
			autoRepaintOnSceneChange = true;
		}

		void OnDisable()
		{
			if(onSettingsDisable != null)
				onSettingsDisable();
		}

		void OnSelectionChange()
		{
			Repaint();
		}

		void OnHierarchyChange()
		{
			Repaint();
		}

		void OnGUI()
		{
			if(onSettingsGUI != null)
			{
				onSettingsGUI();
			}
			else if(Event.current.type == EventType.Repaint)
			{
				EditorApplication.delayCall += CloseAll;
				GUIUtility.ExitGUI();
			}
		}
	}
}
