using System.Collections.Generic;
using System.Linq;

using Manus.Skeletons;

using UnityEngine;

namespace Manus.Utility
{
	public static partial class Extensions
	{
		public static CoreSDK.ManusTransform ToManus( this TransformValues p_Transform )
		{
			return new CoreSDK.ManusTransform
			{
				position = p_Transform.position.ToManus(),
				rotation = p_Transform.rotation.ToManus(),
				scale = p_Transform.scale.ToManus()
			};
		}

		public static TransformValues FromManus( this CoreSDK.ManusTransform p_Transform )
		{
			return new TransformValues()
			{
				position = p_Transform.position.FromManus(),
				rotation = p_Transform.rotation.FromManus(),
				scale = p_Transform.scale.FromManus()
			};
		}

		public static CoreSDK.ManusVec3 ToManus( this Vector3 p_Vec3 )
		{
			return new CoreSDK.ManusVec3
			{
				x = p_Vec3.x,
				y = p_Vec3.y,
				z = p_Vec3.z
			};
		}

		public static CoreSDK.ManusQuaternion ToManus( this Quaternion p_Quaternion )
		{
			return new CoreSDK.ManusQuaternion
			{
				x = p_Quaternion.x,
				y = p_Quaternion.y,
				z = p_Quaternion.z,
				w = p_Quaternion.w
			};
		}

		public static Vector3 FromManus( this CoreSDK.ManusVec3 p_Vector3 )
		{
			return new Vector3
			{
				x = p_Vector3.x,
				y = p_Vector3.y,
				z = p_Vector3.z
			};
		}

		public static Quaternion FromManus( this CoreSDK.ManusQuaternion p_Quaternion )
		{
			return new Quaternion
			{
				x = p_Quaternion.x,
				y = p_Quaternion.y,
				z = p_Quaternion.z,
				w = p_Quaternion.w
			};
		}

		public static Chain FromChainSetup( this CoreSDK.ChainSetup p_ChainSetup )
		{
			return new Chain()
			{
				id = p_ChainSetup.id,
				type = p_ChainSetup.type,
				appliedDataType = p_ChainSetup.dataType,
				dataIndex = p_ChainSetup.dataIndex,
				dataSide = p_ChainSetup.side,
				nodeIds = p_ChainSetup.nodeIds.ToList().GetRange( 0, (int)p_ChainSetup.nodeIdCount ),
				settings = p_ChainSetup.settings.FromChainSettings()
			};
		}

		private static CoreSDK.ChainSettings FromChainSettings( this CoreSDK.ChainSettings p_ChainSettings )
		{
			switch( p_ChainSettings.usedSettings )
			{
				case CoreSDK.ChainType.Hand:
					p_ChainSettings.hand = new CoreSDK.ChainSettingsHand( p_ChainSettings.hand.fingerChainIdsUsed,
						p_ChainSettings.hand.fingerChainIds,
						p_ChainSettings.hand.handMotion );
					break;
				case CoreSDK.ChainType.Foot:
					p_ChainSettings.foot = new CoreSDK.ChainSettingsFoot( p_ChainSettings.foot.toeChainIdsUsed, p_ChainSettings.foot.toeChainIds );
					break;
			}

			return p_ChainSettings;
		}

		public static ColliderSetup FromColliderSetup( this CoreSDK.ColliderSetup p_ColliderSetup )
		{
			return new ColliderSetup()
			{
				nodeId = p_ColliderSetup.nodeId,
				localPosition = p_ColliderSetup.localPosition.FromManus(),
				localRotation = p_ColliderSetup.localRotation.FromManus(),
				type = p_ColliderSetup.type,
				sphere = p_ColliderSetup.sphere,
				capsule = p_ColliderSetup.capsule,
				box = p_ColliderSetup.box,
			};
		}

		public static Node FromNodeSetup( this CoreSDK.NodeSetup p_NodeSetup, List<Node> p_Nodes, SkeletonData p_TargetSkeleton )
		{
			Node t_Node = new Node();
			t_Node.name = null;
			t_Node.nodeName = p_NodeSetup.name;
			t_Node.id = p_NodeSetup.id;
			t_Node.type = p_NodeSetup.type;
			t_Node.transform = p_NodeSetup.transform.FromManus();
			t_Node.parentID = p_NodeSetup.parentID;
			t_Node.settings = p_NodeSetup.settings;
			t_Node.UpdateName();

			//Collect corresponding transforms
			Transform t_UnityNode = p_TargetSkeleton.nodes[0].unityTransform.GetComponentsInChildren<Transform>()
				.Where( p_Transform => p_Transform.name == t_Node.nodeName ).First();
			if( t_UnityNode == null ) return null;
			if( t_Node.id != t_Node.parentID )
			{
				Node t_ParentNode = null;
				foreach( var t_OtherNode in p_Nodes )
				{
					if( t_OtherNode.id == t_Node.parentID )
					{
						t_ParentNode = t_OtherNode;
						break;
					}
				}

				if( t_ParentNode != null && t_ParentNode.unityTransform != null )
				{
					t_UnityNode.SetParent( t_ParentNode.unityTransform );
				}
			}

			t_UnityNode.localPosition = p_NodeSetup.transform.position.FromManus();
			t_UnityNode.localRotation = p_NodeSetup.transform.rotation.FromManus();
			t_UnityNode.localScale = p_NodeSetup.transform.scale.FromManus();
			t_Node.unityTransform = t_UnityNode;

			return t_Node;
		}
	}
}
