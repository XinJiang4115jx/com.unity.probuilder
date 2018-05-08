using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class TriangulateObject : MenuAction
	{
		public override ToolbarGroup group { get { return ToolbarGroup.Object; } }
		public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/Object_Triangulate", IconSkin.Pro); } }
		public override TooltipContent tooltip { get { return _tooltip; } }
		public override string menuTitle { get { return "Triangulate"; } }

		static readonly TooltipContent _tooltip = new TooltipContent
		(
			"Triangulate Objects",
			@"Removes all quads and n-gons on the mesh and inserts triangles instead.  Use this and a hard smoothing group to achieve a low-poly facetized look."
		);

		public override bool IsEnabled()
		{
			return 	ProBuilderEditor.instance != null && MeshSelection.Top().Length > 0;
		}

		public override ActionResult DoAction()
		{
			return MenuCommands.MenuFacetizeObject(MeshSelection.Top());
		}
	}
}
