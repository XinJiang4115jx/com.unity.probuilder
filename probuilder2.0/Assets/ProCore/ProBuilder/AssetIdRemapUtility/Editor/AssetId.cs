﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UObject = UnityEngine.Object;

namespace ProBuilder.AssetUtility
{
	[Serializable]
	class AssetIdentifierTuple
	{
		public AssetId source;
		public AssetId destination;

		public AssetIdentifierTuple()
		{
			source = null;
			destination = null;
		}

		public AssetIdentifierTuple(AssetId src, AssetId dest)
		{
			source = src ?? new AssetId();
			destination = dest ?? new AssetId();
		}

		public bool AssetEquals(AssetIdentifierTuple other)
		{
			return AssetId.IsValid(source) == AssetId.IsValid(other.source) &&
			       source.AssetEquals(other.source) &&
			       AssetId.IsValid(destination) == AssetId.IsValid(other.destination) &&
			       destination.AssetEquals(other.destination);
		}
	}

	[Serializable]
	class StringTuple
	{
		public string key;
		public string value;

		public StringTuple(string k, string v)
		{
			key = k;
			value = v;
		}
	}

	[Serializable]
	class NamespaceRemapObject : ISerializationCallbackReceiver
	{
		[NonSerialized]
		public Dictionary<string, string> map = new Dictionary<string, string>();

		public bool TryGetValue(string key, out string value)
		{
			return map.TryGetValue(key, out value);
		}

		// serialize as key value pair to make json easier to read
		[SerializeField]
		StringTuple[] m_Map;

		public void OnBeforeSerialize()
		{
			m_Map = map.Select(x => new StringTuple(x.Key, x.Value)).ToArray();
		}

		public void OnAfterDeserialize()
		{
			for (int i = 0, c = m_Map.Length; i < c; i++)
				map.Add(m_Map[i].key, m_Map[i].value);
		}
	}

	enum Origin
	{
		Source,
		Destination
	}

	[Serializable]
	class AssetIdRemapObject
	{
		public List<string> sourceDirectory = new List<string>();
		public string destinationDirectory = null;
		public NamespaceRemapObject namespaceMap = null;
		public List<AssetIdentifierTuple> map = new List<AssetIdentifierTuple>();

		public AssetIdentifierTuple this[int i]
		{
			get { return map[i]; }
			set { map[i] = value; }
		}

		public void Clear(Origin origin)
		{
			switch (origin)
			{
				case Origin.Source:
					sourceDirectory.Clear();
					for (int i = 0, c = map.Count; i < c; i++)
						map[i].source.Clear();
					break;

				case Origin.Destination:
					destinationDirectory = "";
					for (int i = 0, c = map.Count; i < c; i++)
						map[i].destination.Clear();
					break;
			}

			map = map.Where(x => AssetId.IsValid(x.source) || AssetId.IsValid(x.destination)).ToList();
		}

		public void Combine(AssetIdentifierTuple left, AssetIdentifierTuple right)
		{
			AssetIdentifierTuple res = new AssetIdentifierTuple();

			if (AssetId.IsValid(left.source) && AssetId.IsValid(right.destination))
			{
				res.source = new AssetId(left.source);
				res.destination = new AssetId(right.destination);
			}
			else if (AssetId.IsValid(left.destination) && AssetId.IsValid(right.source))
			{
				res.source = new AssetId(right.source);
				res.destination = new AssetId(left.destination);
			}
			else
			{
				return;
			}

			// duplicate, don't add
			if (res.AssetEquals(left) || res.AssetEquals(right))
				return;

			// if combine was successful, remove partial entries
			if (AssetId.IsValid(left.source) != AssetId.IsValid(left.destination))
				map.Remove(left);

			if (AssetId.IsValid(right.source) != AssetId.IsValid(right.destination))
				map.Remove(right);

			// add the new
			map.Add(res);
		}
	}

	[Serializable]
	class AssetId : IEquatable<AssetId>
	{
		const string k_MonoScriptTypeString = "UnityEditor.MonoScript";

		static readonly string[] k_MonoScriptTypeSplit = new string[1] {"::"};

		enum AssetType
		{
			Unknown = 0,
			Default = 1,
			MonoScript = 2,
			// add more as special cases require
		}

		/// <summary>
		/// A path relative to the root asset directory (ex, ProBuilder/About/Hello.cs).
		/// Stored per-asset because the path may change between upgrades. A single file name is stored at the tuple
		/// level.
		/// </summary>
		public string localPath
		{
			get { return m_LocalPath; }
		}

		public string name
		{
			get { return m_Name; }
		}

		/// <summary>
		/// Return the backing type of this asset. If the asset is a MonoScript, the associated mono class will be
		/// returned. To get the Unity asset type use assetType.
		/// </summary>
		public string type
		{
			get { return IsMonoScript() ? m_MonoScriptClass : m_Type; }
		}

		public string assetType
		{
			get { return IsMonoScript() ? k_MonoScriptTypeString : m_Type; }
		}

		/// <summary>
		/// File Ids associated with this asset.
		/// </summary>
		public string fileId
		{
			get { return m_FileId; }
		}

		/// <summary>
		/// Asset GUID.
		/// </summary>
		public string guid
		{
			get { return m_Guid; }
		}

		[SerializeField]
		string m_Guid;

		[SerializeField]
		string m_FileId;

		[SerializeField]
		string m_LocalPath;

		[SerializeField]
		string m_Name;

		[SerializeField]
		string m_Type;

		// the remaining properties are only relevant to monoscript files
		AssetType m_InternalType = AssetType.Unknown;
		string m_MonoScriptClass = null;
		bool m_IsEditorScript = false;

		public AssetId()
		{
			Clear();
		}

		public AssetId(AssetId other)
		{
			m_Guid = other.m_Guid;
			m_FileId = other.m_FileId;
			m_LocalPath = other.m_LocalPath;
			m_Name = other.m_Name;
			m_Type = other.m_Type;
		}

		public AssetId(UObject obj, string file, string guid, string localPath = null)
		{
			if (obj == null)
				throw new SystemException("Cannot initialize an AssetIdentifier with a null object");

			if (string.IsNullOrEmpty(guid))
				throw new SystemException("Cannot initialize an AssetIdentifier without a GUID");

			if (string.IsNullOrEmpty(file))
				throw new SystemException("Cannot initialize an AssetIdentifier without a FileId");

			m_FileId = file;
			m_Guid = guid;
			m_Name = obj.name;
			m_LocalPath = localPath;
			MonoScript ms = obj as MonoScript;
			if (ms != null)
				m_Type = string.Format("{0}{1}{2}", obj.GetType().ToString(), k_MonoScriptTypeSplit[0], ms.GetClass());
			else
				m_Type = obj.GetType().ToString();
		}

		public bool Equals(AssetId other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;

			return string.Equals(m_Guid, other.m_Guid) &&
			       string.Equals(m_FileId, other.m_FileId) &&
			       string.Equals(m_LocalPath, other.m_LocalPath);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((AssetId) obj);
		}

		public override int GetHashCode()
		{
			int hash = 0;

			unchecked
			{
				hash = (hash * 7) + (string.IsNullOrEmpty(m_Guid) ? 0 : m_Guid.GetHashCode());
				hash = (hash * 7) + (string.IsNullOrEmpty(m_FileId) ? 0 : m_FileId.GetHashCode());
				hash = (hash * 7) + (string.IsNullOrEmpty(m_LocalPath) ? 0 : m_LocalPath.GetHashCode());
			}

			return hash;
		}

		public void Clear()
		{
			m_Guid = "";
			m_FileId = "";
			m_LocalPath = "";
			m_Name = "";
			m_Type = "";
			m_InternalType = AssetType.Unknown;
			m_MonoScriptClass = null;
			m_IsEditorScript = false;
		}

		public void SetPathRelativeTo(string dir)
		{
			m_LocalPath = m_LocalPath.Replace(dir, "");
		}

		public static bool IsValid(AssetId id)
		{
			return !string.IsNullOrEmpty(id == null ? null : id.m_Guid);
		}

		public bool IsMonoScript()
		{
			if (m_InternalType == AssetType.Unknown)
			{
				if (m_Type.StartsWith(k_MonoScriptTypeString))
				{
					m_InternalType = AssetType.MonoScript;

					try
					{
						m_MonoScriptClass = m_Type.Split(k_MonoScriptTypeSplit, StringSplitOptions.RemoveEmptyEntries)[1];
						m_IsEditorScript = m_LocalPath.StartsWith("Editor/") || m_LocalPath.Contains("/Editor/");
					}
					catch
					{
						m_MonoScriptClass = "null";
//						pb_Log.Debug("Failed parsing type from monoscript \"" + m_Name + "\" (" + m_Type + ")");
					}
				}
				else
				{
					m_InternalType = AssetType.Default;
				}
			}

			return m_InternalType == AssetType.MonoScript;
		}

		bool GetNamespaceAndType(string classType, out string namespaceString, out string typeString)
		{
			namespaceString = null;
			typeString = null;

			if (string.IsNullOrEmpty(classType))
				return false;

			int last = classType.LastIndexOf('.');

			if (last < 0)
			{
				typeString = classType;
				return true;
			}

			namespaceString = classType.Substring(0, last);
			typeString = classType.Substring(last + 1, (classType.Length - last) - 1);

			return true;
		}

		public bool AssetEquals(AssetId other, NamespaceRemapObject namespaceRemap = null)
		{
			if (!assetType.Equals(other.assetType))
				return false;

			if (IsMonoScript())
			{
				// would be better to compare assemblies, but that's not possible when going from src to dll
				// however this at least catches the case where a type exists in both a runtime and Editor dll
				if (m_IsEditorScript == other.m_IsEditorScript)
				{
					// ideally we'd do a scan and find the closest match based on local path, but for now it's a
					// relatively controlled environment and we can deal with duplicate names on an as-needed basis

					// left namespace, left type, etc
					string ln, rn, lt, rt;

					if (GetNamespaceAndType(m_MonoScriptClass, out ln, out lt) &&
					    GetNamespaceAndType(other.m_MonoScriptClass, out rn, out rt))
					{
						if (!string.IsNullOrEmpty(ln))
						{
							// remapped left namespace
							string lrn;

							// if left namespace existed check for a remap, otherwise compare and return
							if (namespaceRemap != null && namespaceRemap.TryGetValue(ln, out lrn))
							{
								if (lrn.Equals(rn) && lt.Equals(rt))
									return true;
							}
							else
							{
								return ln.Equals(rn) && lt.Equals(rt);
							}
						}
						else
						{
							// left didn't have a namespace to begin with, so check against name only
							return lt.Equals(rt);
						}
					}
				}
			}
			else
			{
				return localPath.Equals(other.localPath);
			}

			return false;
		}

		internal bool AssetEquals2(AssetId other, NamespaceRemapObject namespaceRemap = null)
		{
			if (!assetType.Equals(other.assetType))
			{
				Debug.Log("AssetType != AssetType");
				return false;
			}

			if (IsMonoScript())
			{
				// would be better to compare assemblies, but that's not possible when going from src to dll
				// however this at least catches the case where a type exists in both a runtime and Editor dll
				if (m_IsEditorScript == other.m_IsEditorScript)
				{
					// ideally we'd do a scan and find the closest match based on local path, but for now it's a
					// relatively controlled environment and we can deal with duplicate names on an as-needed basis

					// left namespace, left type, etc
					string ln, rn, lt, rt;

					if (GetNamespaceAndType(m_MonoScriptClass, out ln, out lt) &&
					    GetNamespaceAndType(other.m_MonoScriptClass, out rn, out rt))
					{
						if (!string.IsNullOrEmpty(ln))
						{
							// remapped left namespace
							string lrn;

							// if left namespace existed check for a remap, otherwise compare and return
							if (namespaceRemap != null && namespaceRemap.TryGetValue(ln, out lrn))
							{
								Debug.Log("remapped -> " + lrn + "::" + lt + " == " + rn + "::" + rt);
								if (lrn.Equals(rn) && lt.Equals(rt))
									return true;
							}
							else
							{
								Debug.Log("non-remapped -> " + ln + "::" + lt + " == " + rn + "::" + rt);
								return ln.Equals(rn) && lt.Equals(rt);
							}
						}
						else
						{
							// left didn't have a namespace to begin with, so check against name only
							Debug.Log("type compare (" + lt + " == " + rt + ")");
							return lt.Equals(rt);
						}
					}
					else
					{
						Debug.Log("Couldn't get namespace");
					}
				}
				else
				{
					Debug.Log("IsEditorScript compare");
				}
			}
			else
			{
				Debug.Log("localPath compare");
				return localPath.Equals(other.localPath);
			}

			return false;
		}
	}
}