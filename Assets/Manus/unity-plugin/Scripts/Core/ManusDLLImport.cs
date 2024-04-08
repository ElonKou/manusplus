using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using AOT;

using UnityEngine;

namespace Manus
{
	public partial class CoreSDK
	{
		private const CallingConvention s_ImportCallingConvention = CallingConvention.Cdecl;
		private const CharSet s_ImportCharSet = CharSet.Ansi;

		private const string s_DLLName = "ManusSDK";
		private const int s_NumFingersOnHand = 5;
		private const int s_NumFlexSegmentsPerFinger = 2;
		private const int s_MaxNumImusOnGlove = s_NumFingersOnHand + 1;
		private const int s_MaxUsers = 8;
		private const int s_MaxNumCharsInUsername = 64;

		private const int s_MaxNumberOfCharsInMeasurement = 64;
		private const int s_MaxNumberOfCharsInHostName = 256;
		private const int s_MaxNumberOfCharsinIpAddress = 40;
		private const int s_MaxNumberOfCharsInTrackerId = 32;
		private const int s_MaxNumberOfCharsInTrackerManufacturer = 32;
		private const int s_MaxNumberOfCharsInTrackerProductName = 32;
		private const int s_MaxNumberOfCharsInTargetId = 32;
		private const int s_MaxNumberOfCharsInVersion = 16;

		private const uint s_UnitialisedId = 0;
		private const int s_MaxNumberOfHosts = 100;
		private const int s_MaxNumberOfDongles = 16;
		private const int s_MaxNumCharsInLicenseType = 64;
		private const int s_MaxNumberOfGloves = s_MaxNumberOfDongles * 2;
		private const int s_MaxNumberOfHapticDongles = s_MaxNumberOfDongles;
		private const int s_MaxNumberOfSkeletons = s_MaxNumberOfDongles;
		private const int s_MaxNumberPolygonUsers = s_MaxNumberOfSkeletons;
		private const int s_NumberOfTrackersPerPolygonSkeleton = 8;
		private const int s_MaxNumberOfTrackers = s_MaxNumberOfSkeletons * s_NumberOfTrackersPerPolygonSkeleton;

		private const int s_MaxBoneWeightsPerVertex = 4;

		private const int s_MaxNumCharsInNodeName = 256; // this is for a UTF8 string , NOT an ASCII CHAR array (same base type though)
		private const int s_MaxChainLength = 32;
		private const int s_MaxFingerIDS = 10;
		private const int s_MaxToeIDS = 10;

		private const int s_MaxNumCharsInSystemErrorMessage = 256;
		private const int s_MaxNumCharDebuggingID = 64;
		private const int s_MaxNumberOfErgonomicsData = s_MaxNumberOfGloves;

		private const int s_MaxChainsNodes = 999; //remove?

		private const int s_MaxNumberOfSessions = 8; // this is not the real limit for Core but just for the SDKClient to handle
		private const int s_MaxNumberOfSkeletonsPerSession = 16;
		private const int s_MaxNumCharsInSkeletonName = 256; // we already encountered 34 char names in unreal, but its utf8 so enbiggen even more!

		private const int s_MaxNumCharsInTimecodeInterfaceStrings = 64;
		private const int s_MaxNumberOfTimecodeInterfaces = 32;

		private const int s_MaxNumCharsInSessionName = 256;

		public static List<int> listOfFunctions;

		private static uint s_UninitalisedId = 0;

		public delegate void OnConnectedToCore( ManusHost p_Host );
		public delegate void OnConnectedToCorePtr( IntPtr p_HostPtr );
		public delegate void OnDisconnectFromCore( ManusHost p_Host );
		public delegate void OnDisconnectFromCorePtr( IntPtr p_HostPtr );

		public delegate void SkeletonStreamCallback( SkeletonStream p_SkeletonStream );
		public delegate void SkeletonStreamCallbackPtr( IntPtr p_SkeletonStreamPtr );
		protected delegate void InternalSkeletonStreamCallback( InternalSkeletonStreamInfo p_SkeletonStream );
		protected delegate void InternalSkeletonStreamCallbackPtr( IntPtr p_SkeletonStreamPtr );

		public delegate void LandscapeStreamCallback( Landscape p_LandscapeData );
		public delegate void LandscapeStreamCallbackPtr( IntPtr p_LandscapeDataPtr );

		public delegate void ErgonomicsStreamCallback( ErgonomicsStream p_ErgonomicsData );
		public delegate void ErgonomicsStreamCallbackPtr( IntPtr p_ErgonomicsDataPtr );

		public delegate void SystemStreamCallback( SystemMessage p_SystemData );
		public delegate void SystemStreamCallbackPtr( IntPtr p_SystemDataPtr );

		static OnConnectedToCore m_OnConnectedToCore = null;
		static OnDisconnectFromCore m_OnDisconnectFromCore = null;

		static SkeletonStreamCallback m_OnSkeletonData = null;
		static LandscapeStreamCallback m_OnLandscape = null;

		static ErgonomicsStreamCallback m_OnErgonomics = null;

		static SystemStreamCallback m_OnSystem = null;

		#region Wrapper startup and shutdown.

		public static SDKReturnCode Initialize( SessionType p_SessionType )
		{
			var t_Res = ManusDLLImport.CoreSdk_Initialize( p_SessionType );
			if( t_Res != SDKReturnCode.Success ) return t_Res;
			CoordinateSystemVUH t_VUH = new CoordinateSystemVUH()
			{
				handedness = Side.Left,
				up = AxisPolarity.PositiveY,
				view = AxisView.ZFromViewer,
				unitScale = 1.0f
			};
			return ManusDLLImport.CoreSdk_InitializeCoordinateSystemWithVUH( t_VUH, false );
		}

		public static SDKReturnCode ShutDown()
		{
			return ManusDLLImport.CoreSdk_ShutDown();
		}

		#endregion

		#region Utility functions

		public static SDKReturnCode WasDllBuiltInDebugConfiguration( out bool p_BuiltInDebug )
		{
			p_BuiltInDebug = false;
			return ManusDLLImport.CoreSdk_WasDllBuiltInDebugConfiguration( out p_BuiltInDebug );
		}

		public static SDKReturnCode GetTimestampInfo( Timestamp p_Timestamp, out TimestampInfo p_Info )
		{
			return ManusDLLImport.CoreSdk_GetTimestampInfo( p_Timestamp, out p_Info );
		}

		public static SDKReturnCode SetTimestampInfo( out Timestamp p_Timestamp, TimestampInfo p_Info )
		{
			return ManusDLLImport.CoreSdk_SetTimestampInfo( out p_Timestamp, p_Info );
		}

		#endregion

		#region Connection handling

		public static SDKReturnCode LookForHosts( uint p_WaitSeconds = 1, bool p_LoopbackOnly = false )
		{
			return ManusDLLImport.CoreSdk_LookForHosts( p_WaitSeconds, p_LoopbackOnly );
		}

		public static SDKReturnCode GetNumberOfAvailableHostsFound( out uint p_NumberOfAvailableHostsFound )
		{
			p_NumberOfAvailableHostsFound = 0;
			return ManusDLLImport.CoreSdk_GetNumberOfAvailableHostsFound( out p_NumberOfAvailableHostsFound );
		}

		public static SDKReturnCode GetAvailableHostsFound( out ManusHost[] p_Hosts, uint p_NumberOfHostsThatFitInArray )
		{
			p_Hosts = new ManusHost[p_NumberOfHostsThatFitInArray];
			return ManusDLLImport.CoreSdk_GetAvailableHostsFound( p_Hosts, p_NumberOfHostsThatFitInArray );
		}

		public static SDKReturnCode GetIsConnectedToCore( out bool p_ConnectedToCore )
		{
			p_ConnectedToCore = false;
			return ManusDLLImport.CoreSdk_GetIsConnectedToCore( out p_ConnectedToCore );
		}

		public static SDKReturnCode ConnectGRPC()
		{
			return ManusDLLImport.CoreSdk_ConnectGRPC();
		}

		public static SDKReturnCode ConnectToHost( ManusHost p_Host )
		{
			return ManusDLLImport.CoreSdk_ConnectToHost( p_Host );
		}

		public static SDKReturnCode GetVersionsAndCheckCompatibility( out ManusVersion p_SdkVersion, out ManusVersion p_CoreVersion,
			out bool p_AreVersionsCompatible )
		{
			p_SdkVersion.versionString = "";
			p_CoreVersion.versionString = "";
			p_AreVersionsCompatible = false;

			return ManusDLLImport.CoreSdk_GetVersionsAndCheckCompatibility( out p_SdkVersion, out p_CoreVersion, out p_AreVersionsCompatible );
		}

		public static SDKReturnCode RegisterCallbackForOnConnectedToCore( OnConnectedToCore p_OnConnectedToCoreCallback )
		{
			//if( m_OnConnectedToCore == null )
			{
				var t_Res = ManusDLLImport.CoreSdk_RegisterCallbackForOnConnect( ProcessInternalCallbackForOnConnectedToCore );
				if( t_Res != SDKReturnCode.Success ) return t_Res;
			}
			m_OnConnectedToCore = p_OnConnectedToCoreCallback;
			return SDKReturnCode.Success;
		}

		[MonoPInvokeCallback(typeof(OnConnectedToCorePtr))]
		private static void ProcessInternalCallbackForOnConnectedToCore( IntPtr p_HostPtr )
		{
			m_OnConnectedToCore( Marshal.PtrToStructure<ManusHost>( p_HostPtr ) );
		}

		public static SDKReturnCode RegisterCallbackForOnDisconnectedFromCore( OnDisconnectFromCore p_OnDisconnectedFromCoreCallback )
		{
			//if( m_OnDisconnectFromCore == null )
			{
				var t_Res = ManusDLLImport.CoreSdk_RegisterCallbackForOnDisconnect( ProcessInternalCallbackForOnDisconnectedFromCore );
				if( t_Res != SDKReturnCode.Success ) return t_Res;
			}
			m_OnDisconnectFromCore = p_OnDisconnectedFromCoreCallback;
			return SDKReturnCode.Success;
		}

		[MonoPInvokeCallback(typeof(OnDisconnectFromCorePtr))]
		private static void ProcessInternalCallbackForOnDisconnectedFromCore( IntPtr p_HostPtr )
		{
			m_OnDisconnectFromCore( Marshal.PtrToStructure<ManusHost>( p_HostPtr ) );
		}

		public static SDKReturnCode RegisterCallbackForLandscapeStream( LandscapeStreamCallback p_LandscapeCallback )
		{
			//if( m_OnLandscape == null )
			{
				var t_Res = ManusDLLImport.CoreSdk_RegisterCallbackForLandscapeStream( ProcessInternalCallbackForLandscapeStream );
				if( t_Res != SDKReturnCode.Success ) return t_Res;
			}
			m_OnLandscape = p_LandscapeCallback;
			return SDKReturnCode.Success;
		}

		[MonoPInvokeCallback(typeof(LandscapeStreamCallbackPtr))]
		private static void ProcessInternalCallbackForLandscapeStream( IntPtr p_LandscapeDataPtr )
		{
			m_OnLandscape( Marshal.PtrToStructure<Landscape>( p_LandscapeDataPtr ) );
		}

		public static SDKReturnCode RegisterCallbackForSystemStream( SystemStreamCallback p_OnSystem )
		{
			//if( m_OnSystem == null )
			{
				var t_Res = ManusDLLImport.CoreSdk_RegisterCallbackForSystemStream( ProcessInternalCallbackForSystemStream );
				if( t_Res != SDKReturnCode.Success ) return t_Res;
			}
			m_OnSystem = p_OnSystem;
			return SDKReturnCode.Success;
		}

		[MonoPInvokeCallback(typeof(SystemStreamCallbackPtr))]
		private static void ProcessInternalCallbackForSystemStream( IntPtr p_SystemDataPtr )
		{
			m_OnSystem( Marshal.PtrToStructure<SystemMessage>( p_SystemDataPtr ) );
		}

		public static SDKReturnCode RegisterCallbackForErgonomicsStream( ErgonomicsStreamCallback p_OnErgonomics )
		{
			//if( m_OnSystem == null )
			{
				var t_Res = ManusDLLImport.CoreSdk_RegisterCallbackForErgonomicsStream( ProcessInternalCallbackForErgonomicsStream );
				if( t_Res != SDKReturnCode.Success ) return t_Res;
			}
			m_OnErgonomics = p_OnErgonomics;
			return SDKReturnCode.Success;
		}

		[MonoPInvokeCallback(typeof(ErgonomicsStreamCallbackPtr))]
		private static void ProcessInternalCallbackForErgonomicsStream( IntPtr p_ErgonomicsDataPtr )
		{
			m_OnErgonomics( Marshal.PtrToStructure<ErgonomicsStream>( p_ErgonomicsDataPtr ) );
		}

		#endregion

		#region Basic glove interactions

		public static SDKReturnCode VibrateWristOfGlove( uint p_GloveId, float p_UnitStrength, ushort p_DurationInMilliseconds )
		{
			return ManusDLLImport.CoreSdk_VibrateWristOfGlove( p_GloveId, p_UnitStrength, p_DurationInMilliseconds );
		}

		public static SDKReturnCode VibrateFingers( uint p_DongleId, Side p_HandType, float[] p_Powers )
		{
			return ManusDLLImport.CoreSdk_VibrateFingers( p_DongleId, p_HandType, p_Powers );
		}

		public static SDKReturnCode VibrateFingersForSkeleton( uint p_SkeletonId, Side p_HandType, float[] p_Powers )
		{
			return ManusDLLImport.CoreSdk_VibrateFingersForSkeleton( p_SkeletonId, p_HandType, p_Powers );
		}

		public static SDKReturnCode GetGloveIdOfUser_UsingUserId( uint p_UserId, Side p_HandType, out uint p_GloveId )
		{
			p_GloveId = 0;
			return ManusDLLImport.CoreSdk_GetGloveIdOfUser_UsingUserId( p_UserId, p_HandType, out p_GloveId );
		}

		public static SDKReturnCode GetNumberOfAvailableGloves( out uint p_NumberOfAvailableGloves )
		{
			p_NumberOfAvailableGloves = 0;
			return ManusDLLImport.CoreSdk_GetNumberOfAvailableGloves( out p_NumberOfAvailableGloves );
		}

		public static SDKReturnCode GetIdsOfAvailableGloves( out uint[] p_IdsOfAvailableGloves, uint p_NumberOfIdsThatFitInArray )
		{
			p_IdsOfAvailableGloves = new uint[p_NumberOfIdsThatFitInArray];
			return ManusDLLImport.CoreSdk_GetIdsOfAvailableGloves( p_IdsOfAvailableGloves, p_NumberOfIdsThatFitInArray );
		}

		public static SDKReturnCode GetGlovesForDongle( uint p_DongleId, out uint p_LeftGloveId, out uint p_RightGloveId )
		{
			p_LeftGloveId = s_UninitalisedId;
			p_RightGloveId = s_UninitalisedId;
			return ManusDLLImport.CoreSdk_GetGlovesForDongle( p_DongleId, out p_LeftGloveId, out p_RightGloveId );
		}

		public static SDKReturnCode GetDataForGlove_UsingGloveId( uint p_GloveId, out GloveLandscapeData p_GloveData )
		{
			p_GloveData = new GloveLandscapeData();
			return ManusDLLImport.CoreSdk_GetDataForGlove_UsingGloveId( p_GloveId, out p_GloveData );
		}

		public static SDKReturnCode GetDataForDongle( uint p_DongleId, out DongleLandscapeData p_DongleData )
		{
			p_DongleData = new DongleLandscapeData();
			return ManusDLLImport.CoreSdk_GetDataForDongle( p_DongleId, out p_DongleData );
		}

		public static SDKReturnCode GetNumberOfDongles( out uint p_NumberOfDongles )
		{
			p_NumberOfDongles = 0;
			return ManusDLLImport.CoreSdk_GetNumberOfDongles( out p_NumberOfDongles );
		}

		public static SDKReturnCode GetDongleIds( out uint[] p_DongleIds, uint p_NumberOfIdsThatFitInArray )
		{
			p_DongleIds = new uint[s_MaxNumberOfDongles];
			return ManusDLLImport.CoreSdk_GetDongleIds( p_DongleIds, p_NumberOfIdsThatFitInArray );
		}

		#endregion

		#region Haptics module

		public static SDKReturnCode GetNumberOfHapticsDongles( out uint p_NumberOfHapticsDongles )
		{
			p_NumberOfHapticsDongles = 0;
			return ManusDLLImport.CoreSdk_GetNumberOfHapticsDongles( out p_NumberOfHapticsDongles );
		}

		public static SDKReturnCode GetHapticsDongleIds( out uint[] p_HapticDongleIds, uint p_NumberOfIdsThatFitInArray )
		{
			p_HapticDongleIds = new uint[p_NumberOfIdsThatFitInArray];
			return ManusDLLImport.CoreSdk_GetHapticsDongleIds( p_HapticDongleIds, p_NumberOfIdsThatFitInArray );
		}

		#endregion

		#region Polygon

		public static SDKReturnCode GetNumberOfAvailableUsers( out uint p_NumberOfAvailableUsers )
		{
			p_NumberOfAvailableUsers = 0;
			return ManusDLLImport.CoreSdk_GetNumberOfAvailableUsers( out p_NumberOfAvailableUsers );
		}

		public static SDKReturnCode GetIdsOfAvailableUsers( out uint[] p_IdsOfAvailablePolygonUsers, uint p_NumberOfIdsThatFitInArray )
		{
			p_IdsOfAvailablePolygonUsers = new uint[p_NumberOfIdsThatFitInArray];
			return ManusDLLImport.CoreSdk_GetIdsOfAvailableUsers( p_IdsOfAvailablePolygonUsers, p_NumberOfIdsThatFitInArray );
		}

		public static SDKReturnCode GetSkeletonInfo( uint p_SkeletonIndex, out InternalSkeletonInfo p_SkeletonInfo )
		{
			p_SkeletonInfo = new InternalSkeletonInfo();
			return ManusDLLImport.CoreSdk_GetSkeletonInfo( p_SkeletonIndex, out p_SkeletonInfo );
		}

		public static SDKReturnCode GetSkeletonData( uint p_SkeletonIndex, out SkeletonNode[] p_Nodes, uint p_NodeCount )
		{
			p_Nodes = new SkeletonNode[p_NodeCount];
			return ManusDLLImport.CoreSdk_GetSkeletonData( p_SkeletonIndex, p_Nodes, p_NodeCount );
		}

		#endregion

		#region Tracking

		public static SDKReturnCode GetNumberOfAvailableTrackers( out uint p_NumberOfAvailableTrackers )
		{
			p_NumberOfAvailableTrackers = 0;
			return ManusDLLImport.CoreSdk_GetNumberOfAvailableTrackers( out p_NumberOfAvailableTrackers );
		}

		public static SDKReturnCode GetIdsOfAvailableTrackers( out TrackerId[] p_IdsOfAvailableTrackers, uint p_NumberOfIdsThatFitInArray )
		{
			p_IdsOfAvailableTrackers = new TrackerId[p_NumberOfIdsThatFitInArray];
			return ManusDLLImport.CoreSdk_GetIdsOfAvailableTrackers( p_IdsOfAvailableTrackers, p_NumberOfIdsThatFitInArray );
		}

		public static SDKReturnCode GetNumberOfAvailableTrackersForUserId( out uint p_NumberOfAvailableTrackers, uint p_UserId )
		{
			p_NumberOfAvailableTrackers = 0;
			return ManusDLLImport.CoreSdk_GetNumberOfAvailableTrackersForUserId( out p_NumberOfAvailableTrackers, p_UserId );
		}

		public static SDKReturnCode GetIdsOfAvailableTrackersForUserId( out TrackerId[] p_IdsOfAvailableTrackers, uint p_UserId,
			uint p_NumberOfIdsThatFitInArray )
		{
			p_IdsOfAvailableTrackers = new TrackerId[p_NumberOfIdsThatFitInArray];
			return ManusDLLImport.CoreSdk_GetIdsOfAvailableTrackersForUserId( p_IdsOfAvailableTrackers, p_UserId, p_NumberOfIdsThatFitInArray );
		}
		
		public static SDKReturnCode GetNumberOfAvailableTrackersForUserIndex( out uint p_NumberOfAvailableTrackers, uint p_UserIndex )
		{
			p_NumberOfAvailableTrackers = 0;
			return ManusDLLImport.CoreSdk_GetNumberOfAvailableTrackersForUserIndex( out p_NumberOfAvailableTrackers, p_UserIndex );
		}

		public static SDKReturnCode GetIdsOfAvailableTrackersForUserIndex( out TrackerId[] p_IdsOfAvailableTrackers, uint p_UserIndex,
			uint p_NumberOfIdsThatFitInArray )
		{
			p_IdsOfAvailableTrackers = new TrackerId[p_NumberOfIdsThatFitInArray];
			return ManusDLLImport.CoreSdk_GetIdsOfAvailableTrackersForUserIndex( p_IdsOfAvailableTrackers, p_UserIndex, p_NumberOfIdsThatFitInArray );
		}

		public static SDKReturnCode GetDataForTracker_UsingTrackerId( TrackerId p_TrackerId, out TrackerData p_TrackerData )
		{
			p_TrackerData = new TrackerData();
			return ManusDLLImport.CoreSdk_GetDataForTracker_UsingTrackerId( p_TrackerId, out p_TrackerData );
		}

		public static SDKReturnCode GetDataForTracker_UsingIdAndType( uint p_UserID, TrackerType p_TrackerType, out TrackerData p_TrackerData )
		{
			p_TrackerData = new TrackerData();
			uint t_TrackerTypeUInt = (uint)p_TrackerType;
			return ManusDLLImport.CoreSdk_GetDataForTracker_UsingIdAndType( p_UserID, t_TrackerTypeUInt, ref p_TrackerData );
		}

		public static SDKReturnCode SendDataForTrackers( ref TrackerData[] p_TrackerData, uint p_NumberOfTrackers )
		{
			return ManusDLLImport.CoreSdk_SendDataForTrackers( p_TrackerData, p_NumberOfTrackers );
		}

		#endregion

		#region Skeleton

		public static SDKReturnCode RegisterCallbackForSkeletonStream( SkeletonStreamCallback p_Callback )
		{
			//if( m_OnSkeletonData == null )
			{
				var t_Res = ManusDLLImport.CoreSdk_RegisterCallbackForSkeletonStream( ProcessInternalSkeletonStream );
				if( t_Res != SDKReturnCode.Success ) return t_Res;
			}

			m_OnSkeletonData = p_Callback;
			return SDKReturnCode.Success;
		}

		[MonoPInvokeCallback(typeof(SkeletonStreamCallbackPtr))]
		private static void ProcessInternalSkeletonStream( IntPtr p_DataPtr )
		{
			var t_StreamInfo = Marshal.PtrToStructure<InternalSkeletonStreamInfo>( p_DataPtr );

			SkeletonStream t_Data = new SkeletonStream();
			t_Data.publishTime = t_StreamInfo.publishTime;
			t_Data.skeletons = new List<Skeleton>( (int) t_StreamInfo.skeletonsCount );
			for( int i = 0; i < t_StreamInfo.skeletonsCount; i++ )
			{
				InternalSkeletonInfo t_Info = new InternalSkeletonInfo();
				var t_Res = ManusDLLImport.CoreSdk_GetSkeletonInfo( (uint)i, out t_Info );
				if( t_Res != SDKReturnCode.Success ) return;
				var t_Skl = new Skeleton();
				t_Skl.id = t_Info.id;
				t_Skl.nodes = new SkeletonNode[t_Info.nodesCount];
				t_Res = ManusDLLImport.CoreSdk_GetSkeletonData( (uint)i, t_Skl.nodes, t_Info.nodesCount );
				if( t_Res != SDKReturnCode.Success ) return;
				t_Data.skeletons.Add( t_Skl );
			}

			m_OnSkeletonData( t_Data );
		}

		public static SDKReturnCode OverwriteSkeletonSetup( uint p_SkeletonSetupIndex, ref SkeletonSetupInfo p_Skeleton )
		{
			return ManusDLLImport.CoreSdk_OverwriteSkeletonSetup( p_SkeletonSetupIndex, p_Skeleton );
		}

		public static SDKReturnCode CreateSkeletonSetup( ref SkeletonSetupInfo p_Skeleton, out uint p_SkeletonSetupIndex )
		{
			p_SkeletonSetupIndex = 0;
			return ManusDLLImport.CoreSdk_CreateSkeletonSetup( p_Skeleton, out p_SkeletonSetupIndex );
		}

		public static SDKReturnCode AddNodeToSkeletonSetup( uint p_SkeletonSetupIndex, NodeSetup p_Node )
		{
			return ManusDLLImport.CoreSdk_AddNodeToSkeletonSetup( p_SkeletonSetupIndex, p_Node );
		}

		public static SDKReturnCode AddChainToSkeletonSetup( uint p_SkeletonSetupIndex, ChainSetup p_Chain, int p_Iteration = 0 )
		{
			p_Chain.settings.hand.fingerChainIdsUsed = p_Chain.settings.hand.fingerChainIds?.Length ?? 0;
			p_Chain.settings.foot.toeChainIdsUsed = p_Chain.settings.foot.toeChainIds?.Length ?? 0;
			p_Chain.settings.usedSettings = p_Chain.type;

			//This is dumb but keeps it from being an issue.....
			try
			{
				return ManusDLLImport.CoreSdk_AddChainToSkeletonSetup( p_SkeletonSetupIndex, p_Chain );
			}
			catch( Exception e )
			{
				if(p_Iteration > 250 )
				{
					return SDKReturnCode.Error;
				}

				Debug.LogError( $"Caught the memcpy error: {e.Message}" );
				return AddChainToSkeletonSetup( p_SkeletonSetupIndex, p_Chain, p_Iteration++ );
			}
		}

		public static SDKReturnCode AddColliderToSkeletonSetup( uint p_SkeletonSetupIndex, ColliderSetup p_Collider )
		{
			return ManusDLLImport.CoreSdk_AddColliderToSkeletonSetup( p_SkeletonSetupIndex, p_Collider );
		}

		public static SDKReturnCode AddMeshSetupToSkeletonSetup( uint p_SkeletonSetupIndex, uint p_NodeId, out uint p_MeshSetupIndex )
		{
			p_MeshSetupIndex = 0;
			return ManusDLLImport.CoreSdk_AddMeshSetupToSkeletonSetup( p_SkeletonSetupIndex, p_NodeId, out p_MeshSetupIndex );
		}

		public static SDKReturnCode AddVertexToMeshSetup( uint p_SkeletonSetupIndex, uint p_MeshSetupIndex, Vertex p_Vertex )
		{
			return ManusDLLImport.CoreSdk_AddVertexToMeshSetup( p_SkeletonSetupIndex, p_MeshSetupIndex, p_Vertex );
		}

		public static SDKReturnCode AddTriangleToMeshSetup( uint p_SkeletonSetupIndex, uint p_MeshSetupIndex, Triangle p_Triangle )
		{
			return ManusDLLImport.CoreSdk_AddTriangleToMeshSetup( p_SkeletonSetupIndex, p_MeshSetupIndex, p_Triangle );
		}

		public static SDKReturnCode OverwriteChainToSkeletonSetup( uint p_SkeletonSetupIndex, ChainSetup p_Chain )
		{
			p_Chain.settings.hand.fingerChainIdsUsed = p_Chain.settings.hand.fingerChainIds?.Length ?? 0;
			p_Chain.settings.foot.toeChainIdsUsed = p_Chain.settings.foot.toeChainIds.Length;
			p_Chain.settings.usedSettings = p_Chain.type;
			return ManusDLLImport.CoreSdk_OverwriteChainToSkeletonSetup( p_SkeletonSetupIndex, p_Chain );
		}

		public static SDKReturnCode GetSkeletonSetupArraySizes( uint p_SkeletonSetupIndex, out SkeletonSetupArraySizes p_SkeletonInfo )
		{
			p_SkeletonInfo = new SkeletonSetupArraySizes();
			return ManusDLLImport.CoreSdk_GetSkeletonSetupArraySizes( p_SkeletonSetupIndex, out p_SkeletonInfo );
		}

		public static SDKReturnCode AllocateChainsForSkeletonSetup( uint p_SkeletonSetupIndex )
		{
			return ManusDLLImport.CoreSdk_AllocateChainsForSkeletonSetup( p_SkeletonSetupIndex );
		}

		public static SDKReturnCode GetSkeletonSetupInfo( uint p_SkeletonSetupIndex, out SkeletonSetupInfo p_SkeletonSetupInfo )
		{
			return ManusDLLImport.CoreSdk_GetSkeletonSetupInfo( p_SkeletonSetupIndex, out p_SkeletonSetupInfo );
		}

		public static SDKReturnCode GetSkeletonSetupChains( uint p_SkeletonSetupIndex, out ChainSetup[] p_ChainSetup )
		{
			var t_Res = GetSkeletonSetupArraySizes( p_SkeletonSetupIndex, out SkeletonSetupArraySizes t_Info );
			if( t_Res != SDKReturnCode.Success || t_Info.chainsCount == 0 )
			{
				p_ChainSetup = Array.Empty<ChainSetup>();
				return t_Res;
			}
			p_ChainSetup = new ChainSetup[t_Info.chainsCount];
			return ManusDLLImport.CoreSdk_GetSkeletonSetupChains( p_SkeletonSetupIndex, p_ChainSetup );
		}

		public static SDKReturnCode GetSkeletonSetupNodes( uint p_SkeletonSetupIndex, out NodeSetup[] p_NodeSetup )
		{
			var t_Res = GetSkeletonSetupArraySizes( p_SkeletonSetupIndex, out SkeletonSetupArraySizes t_Info );
			if( t_Res != SDKReturnCode.Success || t_Info.nodesCount == 0 )
			{
				p_NodeSetup = Array.Empty<NodeSetup>();
				return t_Res;
			}
			p_NodeSetup = new NodeSetup[t_Info.nodesCount];
			return ManusDLLImport.CoreSdk_GetSkeletonSetupNodes( p_SkeletonSetupIndex, p_NodeSetup );
		}

		public static SDKReturnCode GetSkeletonSetupColliders( uint p_SkeletonSetupIndex, out ColliderSetup[] p_ColliderSetup )
		{
			var t_Res = GetSkeletonSetupArraySizes( p_SkeletonSetupIndex, out SkeletonSetupArraySizes t_Info );
			if( t_Res != SDKReturnCode.Success || t_Info.collidersCount == 0 )
			{
				p_ColliderSetup = Array.Empty<ColliderSetup>();
				return t_Res;
			}
			p_ColliderSetup = new ColliderSetup[t_Info.collidersCount];
			return ManusDLLImport.CoreSdk_GetSkeletonSetupColliders( p_SkeletonSetupIndex, p_ColliderSetup );
		}

		public static SDKReturnCode LoadSkeleton( uint p_SkeletonSetupIndex, out uint p_SkeletonId )
		{
			p_SkeletonId = 0;
			return ManusDLLImport.CoreSdk_LoadSkeleton( p_SkeletonSetupIndex, out p_SkeletonId );
		}

		public static SDKReturnCode UnloadSkeleton( uint p_SkeletonId )
		{
			return ManusDLLImport.CoreSdk_UnloadSkeleton( p_SkeletonId );
		}

		public static SDKReturnCode AllocateChains( uint p_SkeletonSetupIndex )
		{
			return ManusDLLImport.CoreSdk_AllocateChainsForSkeletonSetup( p_SkeletonSetupIndex );
		}

		public static SDKReturnCode GetTemporarySkeletonCountForAllSessions( ref TemporarySkeletonCountForSessions p_TemporarySkeletonCountForSessions )
		{
			return ManusDLLImport.CoreSdk_GetTemporarySkeletonCountForAllSessions( out p_TemporarySkeletonCountForSessions );
		}

		public static SDKReturnCode GetTemporarySkeletonsInfoForSession( uint p_SessionId, ref TemporarySkeletonsInfoForSession p_TemporarySkeletonsInfoForSession )
		{
			return ManusDLLImport.CoreSdk_GetTemporarySkeletonsInfoForSession( p_SessionId, out p_TemporarySkeletonsInfoForSession );
		}

		public static SDKReturnCode GetTemporarySkeleton( uint p_SkeletonSetupIndex, uint p_SessionId )
		{
			return ManusDLLImport.CoreSdk_GetTemporarySkeleton( p_SkeletonSetupIndex, p_SessionId );
		}

		public static SDKReturnCode SaveTemporarySkeleton( uint p_SkeletonSetupIndex, uint p_SessionId, bool p_IsSkeletonModified )
		{
			return ManusDLLImport.CoreSdk_SaveTemporarySkeleton( p_SkeletonSetupIndex, p_SessionId, p_IsSkeletonModified );
		}

		public static SDKReturnCode GetCompressedTemporarySkeletonData( uint p_SkeletonSetupIndex, uint p_SkeletonId, out byte[] p_TemporarySkeletonData )
		{
			uint t_TemporarySkeletonLengthInBytes;
			CoreSDK.SDKReturnCode t_Result = ManusDLLImport.CoreSdk_CompressTemporarySkeletonAndGetSize( p_SkeletonSetupIndex, p_SkeletonId, out t_TemporarySkeletonLengthInBytes );
			p_TemporarySkeletonData = new byte[t_TemporarySkeletonLengthInBytes];
			if( t_Result != CoreSDK.SDKReturnCode.Success )
			{
				return t_Result;
			}
			return ManusDLLImport.CoreSdk_GetCompressedTemporarySkeletonData( p_TemporarySkeletonData, t_TemporarySkeletonLengthInBytes );
		}

		public static SDKReturnCode GetTemporarySkeletonFromCompressedData( uint p_SkeletonSetupIndex, uint p_SessionId, byte[] p_TemporarySkeletonData )
		{
			return ManusDLLImport.CoreSdk_GetTemporarySkeletonFromCompressedData( p_SkeletonSetupIndex, p_SessionId, p_TemporarySkeletonData, (uint)p_TemporarySkeletonData.Length );
		}

		public static SDKReturnCode ClearAllTemporarySkeletons()
		{
			return ManusDLLImport.CoreSdk_ClearAllTemporarySkeletons();
		}

		public static SDKReturnCode GetSessionId( out uint p_SessionId )
		{
			p_SessionId = 0;
			return ManusDLLImport.CoreSdk_GetSessionId( out p_SessionId );
		}

		#endregion

		public enum SDKReturnCode
		{
			/// No issues occurred.
			Success,

			/// Something went wrong, but no specific reason can be given.
			Error,

			/// One of the arguments given had an invalid value.
			InvalidArgument,

			/// The size of an argument given (e.g. an array) does not match the size
			/// of the data that it is intended to hold.
			ArgumentSizeMismatch,

			/// A string of an unsupported size was encountered.
			UnsupportedStringSizeEncountered,

			/// The Core SDK is not available.
			SdkNotAvailable,

			/// The network host finder is not available.
			HostFinderNotAvailable,

			/// The data requested is not available.
			DataNotAvailable,

			/// Failed to allocate memory for something.
			MemoryError,

			/// Something went wrong in the SDK internally.
			InternalError,

			/// The function was not intended to be called at this time.
			FunctionCalledAtWrongTime,

			/// No connection to Core was made.
			NotConnected,

			/// The connection with Core timed out.
			ConnectionTimeout,

			/// using an uninitialized ID is bad.
			InvalidID,

			/// memory unallocated or just a null pointer passed where it wasn't supposed to be!
			NullPointer,

			/// null sequence type for polygon calibration
			InvalidSequence,

			/// don't forget to set the coordinate system type or there will be trouble
			NoCoordinateSystemSet,

			/// if everything is being terminated. don't restart
			SdkIsTerminating,

			/// the stub has been reset but someone is tryign to use it anyway. usually after a shutdown of the SDK.
			StubNullPointer,

			/// Skeleton could not be loaded. usually when using more then the max skeletons per session (16).
			SkeletonNotLoaded,

			/// Function not available for this version of the SDK
			FunctionNotAvailable,

			MAX_SIZE
		}

		public enum SessionType
		{
			Unknown,
			UnityPlugin,
			UnrealPlugin,
			CoreSDK,
			Xsens,
			Optitrack,
			MotionBuilder,
			VRED
		}

		public enum TrackerType : uint
		{
			Unknown,
			Head,
			Waist,
			LeftHand,
			RightHand,
			LeftFoot,
			RightFoot,
			LeftUpperArm,
			RightUpperArm,
			LeftUpperLeg,
			RightUpperLeg,
			Controller,
			Camera,

			MAX_SIZE
		}

		public enum TrackingQuality
		{
			Untrackable,
			BadTracking,
			Trackable,
		}

		public enum TrackerSystemType
		{
			Unknown,
			Antilatency,
			ART,
			OpenVR,
			Optitrack,
			Vicon,
			OpenXR,
		}

		public enum DevicePairedState
		{
			Unknown,
			Paired,
			Unpaired,
			Pairing,
		}

		public enum DeviceClassType
		{
			Unknown,
			Dongle,
			Glove,
			Glongle
		}

		public enum DeviceFamilyType
		{
			Unknown,
			Prime1,
			Prime2,
			PrimeX,
			Prime3,
			Quantum
		}

		public enum ProfileType // Keep this enum in sync with protodef version and the manusconvert functions
		{
			Hands,
			FullBody,

			MAX_SIZE
		}

		public enum MeasurementType // Keep this enum in sync with protodef version and the manusconvert functions
		{
			Unknown,
			PlayerHeight,
			SpineLength,
			NeckLength,
			UpperArmLength,
			LowerArmLength,
			ArmLength,
			ArmSpan,
			UpperLegLength,
			LowerLegLength,
			LegLength,
			HandLength,
			FootLength,
			HipWidth,
			ShoulderWidth,
			ShoulderHeight,
			HeadLength,
			Thickness,
			ArmRatio,
			LegRatio,

			MAX_SIZE // Warning, this value is used to define the UserProfile.Measurement[SIZE]
		}

		public enum TrackerOffsetType // Keep this enum in sync with protodef version and the manusconvert functions
		{
			Unknown,
			HeadTrackerToHead,
			HeadTrackerToTopOfHead,

			LeftHandTrackerToWrist,
			RightHandTrackerToWrist,
			LeftFootTrackerToAnkle,
			RightFootTrackerToAnkle,

			HipTrackerToHip,
			HipTrackerToLeftLeg,
			HipTrackerToRightLeg,

			LeftUpperArmTrackerToElbow,
			RightUpperArmTrackerToElbow,
			LeftUpperArmTrackerToShoulder,
			RightUpperArmTrackerToShoulder,

			MAX_SIZE // Warning, this value is used to define the UserProfile.TrackerOffset[SIZE]
		}

		public enum ExtraTrackerOffsetType
		{
			Unknown,
			HeadForward,
			HipForward,
			HipHeight,
			MAX_SIZE // Warning, this value is used to define the UserProfile.TrackerOffset[SIZE]
		}

		public enum MeasurementUnit
		{
			Meters,
			Percentage,
		}

		public enum MeasurementCategory
		{
			Misc,
			Generic,
			Arms,
			Legs,
			Body,
		}

		public enum UpdateStatusEnum
		{
			Unknown,
			NoUpdateAvailable,
			UpdateAvailable,
			MandatoryUpdateAvailable,
			Updating,
		}

		public enum SkeletonType
		{
			Invalid,
			Hand,
			Body,
			Both
		}

		public enum SkeletonTargetType
		{
			Invalid,
			UserData,
			UserIndexData,
			AnimationData,
			GloveData
		}

		public enum NodeType
		{
			Invalid,
			Joint,
			Mesh
		}

		[System.Flags]
		public enum NodeSettingsFlag
		{
			None = 0,
			IK = 1 << 0,
			Foot = 1 << 1,
			RotationOffset = 1 << 2,
			Leaf = 1 << 3
		}

		public enum ChainType
		{
			Invalid,
			Arm,
			Leg,
			Neck,
			Spine,
			FingerThumb,
			FingerIndex,
			FingerMiddle,
			FingerRing,
			FingerPinky,
			Pelvis,
			Head,
			Shoulder,
			Hand,
			Foot,
			Toe
		}

		public enum ColliderType
		{
			Invalid,
			Sphere,
			Capsule,
			Box
		}

		public enum CollisionType
		{
			None,
			Discrete,
			Continuous
		}

		public enum Side
		{
			Invalid,
			Left,
			Right,
			Center
		}

		public enum HandMotion
		{
			None,
			IMU,
			Tracker,
			Tracker_RotationOnly,
			Auto
		}

		#region login coordinate settings

		public enum AxisDirection
		{
			Invalid,
			Backward,
			Left,
			Down,
			Up,
			Right,
			Forward
		}

		public enum AxisView
		{
			Invalid,

			ZFromViewer,
			YFromViewer,
			XFromViewer,

			XToViewer,
			YToViewer,
			ZToViewer
		}

		public enum AxisPolarity
		{
			Invalid,

			NegativeZ,
			NegativeY,
			NegativeX,

			PositiveX,
			PositiveY,
			PositiveZ
		}

		#endregion

		public enum SystemMessageType
		{
			Unknown,
			LibDebugReplugDongle,
			LibDebugRxStall,
			LibDebugTxStall,

			TrackerError,
			TrackerOk,
			TrackerSystemOutOfDate,

			GloveSanityErrorPSOCInit,
			GloveSanityErrorQCBatV,
			GloveSanityErrorQCLRACalib,
			GloveSanityErrorQCFlexInit,
			GloveSanityErrorQCIMUInit,
			GloveSanityErrorQCIMUCalib,
			GloveSanityErrorQCID,
			GloveSanityErrorQCInterCPU,

			SessionConnectionVersionMismatch,

			TemporarySkeletonModified
		}

		public enum ErgonomicsDataType
		{
			LeftFingerThumbMCPSpread,
			LeftFingerThumbMCPStretch,
			LeftFingerThumbPIPStretch,
			LeftFingerThumbDIPStretch,

			LeftFingerIndexMCPSpread,
			LeftFingerIndexMCPStretch,
			LeftFingerIndexPIPStretch,
			LeftFingerIndexDIPStretch,

			LeftFingerMiddleMCPSpread,
			LeftFingerMiddleMCPStretch,
			LeftFingerMiddlePIPStretch,
			LeftFingerMiddleDIPStretch,

			LeftFingerRingMCPSpread,
			LeftFingerRingMCPStretch,
			LeftFingerRingPIPStretch,
			LeftFingerRingDIPStretch,

			LeftFingerPinkyMCPSpread,
			LeftFingerPinkyMCPStretch,
			LeftFingerPinkyPIPStretch,
			LeftFingerPinkyDIPStretch,

			RightFingerThumbMCPSpread,
			RightFingerThumbMCPStretch,
			RightFingerThumbPIPStretch,
			RightFingerThumbDIPStretch,

			RightFingerIndexMCPSpread,
			RightFingerIndexMCPStretch,
			RightFingerIndexPIPStretch,
			RightFingerIndexDIPStretch,

			RightFingerMiddleMCPSpread,
			RightFingerMiddleMCPStretch,
			RightFingerMiddlePIPStretch,
			RightFingerMiddleDIPStretch,

			RightFingerRingMCPSpread,
			RightFingerRingMCPStretch,
			RightFingerRingPIPStretch,
			RightFingerRingDIPStretch,

			RightFingerPinkyMCPSpread,
			RightFingerPinkyMCPStretch,
			RightFingerPinkyPIPStretch,
			RightFingerPinkyDIPStretch,

			MAX_SIZE
		}

		public enum LicenseType
		{
			Undefined,
			Polygon,
			CoreXO,
			CorePro,
			CoreXOPro,
		}

		public enum TimecodeFPS
		{
			TimecodeFPS_Undefined,
			TimecodeFPS_23_976,          // drop frame is active
			TimecodeFPS_24,
			TimecodeFPS_25,
			TimecodeFPS_29_97,           // drop frame is active
			TimecodeFPS_30,
			TimecodeFPS_50,
			TimecodeFPS_59_94,           // drop frame is active
			TimecodeFPS_60,
		}

		[System.Serializable]
		[StructLayout( LayoutKind.Sequential )]
		public struct ManusVec3
		{
			public float x;
			public float y;
			public float z;

			public ManusVec3( float p_X = 0.0f, float p_Y = 0.0f, float p_Z = 0.0f )
			{
				x = p_X;
				y = p_Y;
				z = p_Z;
			}
		}

		[System.Serializable]
		[StructLayout( LayoutKind.Sequential )]
		public struct ManusVec2
		{
			public float x;
			public float y;

			public ManusVec2( float p_X = 0.0f, float p_Y = 0.0f )
			{
				x = p_X;
				y = p_Y;
			}
		}

		[System.Serializable]
		[StructLayout( LayoutKind.Sequential )]
		public struct ManusQuaternion
		{
			public float w;
			public float x;
			public float y;
			public float z;

			public ManusQuaternion( float p_W = 1.0f, float p_X = 0.0f, float p_Y = 0.0f, float p_Z = 0.0f )
			{
				w = p_W;
				x = p_X;
				y = p_Y;
				z = p_Z;
			}
		}

		[StructLayout( LayoutKind.Sequential )]
		[System.Serializable]
		public struct ManusTransform
		{
			public ManusVec3 position;
			public ManusQuaternion rotation;
			public ManusVec3 scale;
		}

		/// @brief A Timestamp
		[StructLayout( LayoutKind.Sequential )]
		[System.Serializable]
		public struct TimestampInfo
		{
			public ushort fraction; //is either frame in timecode or miliseconds in non timecode
			public byte second;
			public byte minute;
			public byte hour;
			public byte day; //is 0 in timecode
			public byte month; //is 0 in timecode
			public uint year; //is 0 in timecode
			public bool timecode;

			public TimestampInfo( System.TimeSpan p_TimeSpan )
			{
				fraction = (ushort)p_TimeSpan.Milliseconds; //is either frame in timecode or miliseconds in non timecode
				second = (byte)p_TimeSpan.Seconds;
				minute = (byte)p_TimeSpan.Minutes;
				hour = (byte)p_TimeSpan.Hours;
				day = (byte)p_TimeSpan.Days; //is 0 in timecode
				month = 0; //is 0 in timecode
				year = 0; //is 0 in timecode
				timecode = false;
			}

			public Timestamp GetTimestamp()
			{
				Timestamp t_Res;
				SetTimestampInfo( out t_Res, this );
				return t_Res;
			}

			public DateTime ToDateTime()
			{
				int t_Year = (int)year;
				if( t_Year < 1 ) t_Year = 1;
				if( t_Year > 9999 ) t_Year = 9999;

				int t_Month = (int)month;
				if( t_Month < 1 ) t_Month = 1;
				if( t_Month > 9999 ) t_Month = 9999;

				int t_MaxDay = DateTime.DaysInMonth(t_Year,t_Month);
				int t_Day = (int)day;
				if( t_Day < 1 ) t_Day = 1;
				if( t_Day > t_MaxDay ) t_Day = t_MaxDay;

				int t_Hour = (int)hour;
				if( t_Hour > 23 ) t_Hour = 23;

				int t_Minute = (int)minute;
				if( t_Minute > 59 ) t_Minute = 59;

				int t_Second = (int)second;
				if( t_Second > 59 ) t_Second = 59;

				int t_Millisecond = (int)fraction;
				if( t_Millisecond > 999 ) t_Millisecond = 999;

				return new DateTime( t_Year, t_Month, t_Day, t_Hour, t_Minute, t_Second, t_Millisecond );
			}
		}

		/// @brief A compressed timestamp
		[StructLayout( LayoutKind.Sequential )]
		[System.Serializable]
		public struct Timestamp
		{
			public ulong time;

			public Timestamp( ulong p_Time )
			{
				time = p_Time;
			}

			public TimestampInfo GetInfo()
			{
				TimestampInfo t_Res;
				GetTimestampInfo( this, out t_Res );
				return t_Res;
			}
		}

		[StructLayout( LayoutKind.Sequential )]
		[System.Serializable]
		public struct Color
		{
			public float r;
			public float g;
			public float b;
			public float a;
		}

		[StructLayout( LayoutKind.Sequential )]
		[System.Serializable]
		public struct IMUCalibrationInfo
		{
			public uint mag;
			public uint acc;
			public uint gyr;
			public uint sys;
		}

		[StructLayout( LayoutKind.Sequential )]
		[System.Serializable]
		public struct Version
		{
			public uint major;
			public uint minor;
			public uint patch;

			[MarshalAs( UnmanagedType.ByValTStr, SizeConst = s_MaxNumberOfCharsInVersion )]
			public string label;

			[MarshalAs( UnmanagedType.ByValTStr, SizeConst = s_MaxNumberOfCharsInVersion )]
			public string sha;

			[MarshalAs( UnmanagedType.ByValTStr, SizeConst = s_MaxNumberOfCharsInVersion )]
			public string tag;
		}

		[StructLayout( LayoutKind.Sequential )]
		[System.Serializable]
		public struct FirmwareVersion
		{
			public int version;
			public Timestamp timestamp;
		}

		[StructLayout( LayoutKind.Sequential )]
		[System.Serializable]
		public struct ManusVersion
		{
			[MarshalAs( UnmanagedType.ByValTStr, SizeConst = s_MaxNumberOfCharsInVersion )]
			public string versionString;
		}

		#region Tracking

		[StructLayout( LayoutKind.Sequential )]
		[System.Serializable]
		public struct TrackerId
		{
			[MarshalAs( UnmanagedType.ByValTStr, SizeConst = s_MaxNumberOfCharsInTrackerId )]
			public string stringId;

			public TrackerId( string p_StringId )
			{
				stringId = p_StringId;
			}
		}

		[StructLayout( LayoutKind.Sequential )]
		[System.Serializable]
		public struct TrackerData
		{
			public Timestamp lastUpdateTime;
			public TrackerId trackerId;
			public uint userId;
			[MarshalAs( UnmanagedType.I1 )]
			public bool isHmd;
			public TrackerType type;
			public ManusQuaternion rotation;
			public ManusVec3 position;
			public TrackingQuality quality;

			public TrackerData( Timestamp p_LastUpdateTime, TrackerId p_TrackerId, uint p_UserId, bool p_IsHmd, TrackerType p_Type,
				ManusQuaternion p_Rotation, ManusVec3 p_Position, TrackingQuality p_Quality )
			{
				lastUpdateTime = p_LastUpdateTime;
				trackerId = p_TrackerId;
				userId = p_UserId;
				isHmd = p_IsHmd;
				type = p_Type;
				rotation = p_Rotation;
				position = p_Position;
				quality = p_Quality;
			}
		}

		#endregion

		[StructLayout( LayoutKind.Sequential )]
		[System.Serializable]
		public struct ManusHost
		{
			[MarshalAs( UnmanagedType.ByValTStr, SizeConst = s_MaxNumberOfCharsInHostName )]
			public string hostName;

			[MarshalAs( UnmanagedType.ByValTStr, SizeConst = s_MaxNumberOfCharsinIpAddress )]
			public string ipAddress;

			public Version manusCoreVersion;

			public ManusHost( string p_HostName, string p_IpAddress, Version p_ManusCoreVersion )
			{
				hostName = p_HostName;
				ipAddress = p_IpAddress;
				manusCoreVersion = p_ManusCoreVersion;
			}
		}

		#region Skeleton

		[StructLayout( LayoutKind.Sequential )]
		[System.Serializable]
		public struct SkeletonNode
		{
			public uint id;
			public ManusTransform transform;
		}

		[StructLayout( LayoutKind.Sequential )] //Had to be adjusted to public due to GetSkeletonInfo
		[System.Serializable]
		public struct InternalSkeletonInfo
		{
			public uint id;
			public uint nodesCount;
		}

		[StructLayout( LayoutKind.Sequential )]
		[System.Serializable]
		protected struct InternalSkeletonStreamInfo
		{
			public Timestamp publishTime;
			public uint skeletonsCount;
		}

		[StructLayout( LayoutKind.Sequential )]
		[System.Serializable]
		public struct Skeleton
		{
			public uint id;
			public SkeletonNode[] nodes;
		}

		[StructLayout( LayoutKind.Sequential )]
		[System.Serializable]
		public struct SkeletonStream
		{
			public Timestamp publishTime;
			public List<Skeleton> skeletons;
		}

		#endregion

		#region Ergonomics

		[StructLayout( LayoutKind.Sequential )]
		[System.Serializable]
		public struct ErgonomicsData
		{
			public uint id;
			[MarshalAs( UnmanagedType.I1 )]
			public bool isUserID;
			[MarshalAs( UnmanagedType.ByValArray, SizeConst = (int)ErgonomicsDataType.MAX_SIZE )]
			public float[] data;
		}

		[StructLayout( LayoutKind.Sequential )]
		[System.Serializable]
		public struct ErgonomicsStream
		{
			public Timestamp publishTime;

			[MarshalAs( UnmanagedType.ByValArray, SizeConst = s_MaxNumberOfErgonomicsData )]
			public ErgonomicsData[] data;

			public uint dataCount;
		}

		#endregion

		#region Landscape

		[StructLayout( LayoutKind.Sequential )]
		[System.Serializable]
		public struct DongleLandscapeData
		{
			public uint id;
			public DeviceClassType classType;
			public DeviceFamilyType familyType;
			[MarshalAs( UnmanagedType.I1 )]
			public bool isHaptics;

			public Version hardwareVersion;
			public Version firmwareVersion;
			public Timestamp firmwareTimestamp;

			public uint chargingState;

			public int channel;

			public UpdateStatusEnum updateStatus;

			[MarshalAs( UnmanagedType.ByValTStr, SizeConst = s_MaxNumCharsInLicenseType )]
			public string licenseType;

			public Timestamp lastSeen;

			public uint leftGloveID;
			public uint rightGloveID;

			public LicenseType licenseLevel;
		}

		[StructLayout( LayoutKind.Sequential )]
		[System.Serializable]
		public struct GloveLandscapeData
		{
			public uint id;
			public DeviceClassType classType;
			public DeviceFamilyType familyType;
			public Side side;
			[MarshalAs( UnmanagedType.I1 )]
			public bool isHaptics;

			public DevicePairedState pairedState;
			public uint dongleID;

			public Version hardwareVersion;
			public Version firmwareVersion;
			public Timestamp firmwareTimestamp;

			public UpdateStatusEnum updateStatus;

			public uint batteryPercentage;
			public int transmissionStrength;

			[MarshalAs( UnmanagedType.ByValArray, SizeConst = s_MaxNumImusOnGlove )]
			public IMUCalibrationInfo[] iMUCalibrationInfo;

			public Timestamp lastSeen;
			[MarshalAs( UnmanagedType.I1 )]
			public bool excluded;
		}

		[StructLayout( LayoutKind.Sequential )]
		public struct Measurement
		{
			public MeasurementType entryType;
			public float value;

			public MeasurementUnit unit;
			public MeasurementCategory category;
			[MarshalAs( UnmanagedType.ByValTStr, SizeConst = s_MaxNumberOfCharsInMeasurement )]
			public string displayName;
		}

		[StructLayout( LayoutKind.Sequential )]
		public struct TrackerOffset
		{
			public TrackerOffsetType entryType;
			public ManusVec3 translation;
			public ManusQuaternion rotation;
		}

		[StructLayout( LayoutKind.Sequential )]
		public struct ExtraTrackerOffset
		{
			public ExtraTrackerOffsetType entryType;
			public float value;
		}

		[StructLayout( LayoutKind.Sequential )]
		public struct TrackerLandscapeData
		{
			[MarshalAs( UnmanagedType.ByValTStr, SizeConst = s_MaxNumberOfCharsInTrackerId )]
			public string id;
			public TrackerType type;
			public TrackerSystemType systemType;
			public uint user;
			[MarshalAs( UnmanagedType.I1 )]
			public bool isHMD;
			[MarshalAs( UnmanagedType.ByValTStr, SizeConst = s_MaxNumberOfCharsInTrackerManufacturer )]
			public string manufacturer;
			[MarshalAs( UnmanagedType.ByValTStr, SizeConst = s_MaxNumberOfCharsInTrackerProductName )]
			public string productName;
		}

		[StructLayout( LayoutKind.Sequential )]
		public struct UserProfileLandscapeData
		{
			public ProfileType profileType;
			[MarshalAs( UnmanagedType.ByValArray, SizeConst = (int)MeasurementType.MAX_SIZE )]
			public Measurement[] bodyMeasurements;
			[MarshalAs( UnmanagedType.ByValArray, SizeConst = (int)TrackerOffsetType.MAX_SIZE )]
			public TrackerOffset[] trackerOffsets;
			[MarshalAs( UnmanagedType.ByValArray, SizeConst = (int)ExtraTrackerOffsetType.MAX_SIZE )]
			public ExtraTrackerOffset[] extraTrackerOffsets;

			public int ValidBodyMeasurementCount()
			{
				int t_Cnt = 0;
				for( int i = 0; i < bodyMeasurements.Length; i++ )
				{
					if( bodyMeasurements[i].entryType == CoreSDK.MeasurementType.Unknown )
						continue;
					t_Cnt++;
				}
				return t_Cnt;
			}
		}

		[StructLayout( LayoutKind.Sequential )]
		public struct UserLandscapeData
		{
			public uint id;
			[MarshalAs( UnmanagedType.ByValTStr, SizeConst = s_MaxNumCharsInUsername )]
			public string name;
			public Color color;
			public uint dongleID;
			public uint leftGloveID;
			public uint rightGloveID;
			public UserProfileLandscapeData profile;
			public uint userIndex;
		}

		[StructLayout( LayoutKind.Sequential )]
		public struct SkeletonLandscapeData
		{
			public uint id;
			[MarshalAs( UnmanagedType.ByValTStr, SizeConst = s_MaxNumberOfCharsInHostName )]
			public string session;
			public uint userId;
			public SkeletonType type;
			[MarshalAs( UnmanagedType.ByValTStr, SizeConst = s_MaxNumCharsInNodeName )]
			public string rootBoneName;
			[MarshalAs( UnmanagedType.I1 )]
			public bool scaled;
		}

		[StructLayout( LayoutKind.Sequential )]
		public struct DeviceLandscape
		{
			[MarshalAs( UnmanagedType.ByValArray, SizeConst = s_MaxNumberOfDongles )]
			public DongleLandscapeData[] dongles;
			public uint dongleCount;
			[MarshalAs( UnmanagedType.ByValArray, SizeConst = s_MaxNumberOfGloves )]
			public GloveLandscapeData[] gloves;
			public uint gloveCount;
		}

		[StructLayout( LayoutKind.Sequential )]
		public struct UserLandscape
		{
			[MarshalAs( UnmanagedType.ByValArray, SizeConst = s_MaxUsers )]
			public UserLandscapeData[] users;
			public uint userCount;
		}

		[StructLayout( LayoutKind.Sequential )]
		public struct SkeletonLandscape
		{
			[MarshalAs( UnmanagedType.ByValArray, SizeConst = s_MaxNumberOfSkeletons )]
			public SkeletonLandscapeData[] skeletons;
			public uint skeletonCount;
		}

		[StructLayout( LayoutKind.Sequential )]
		public struct TrackerLandscape
		{
			[MarshalAs( UnmanagedType.ByValArray, SizeConst = s_MaxNumberOfTrackers )]
			public TrackerLandscapeData[] trackers;
			public uint trackerCount;
		}

		[StructLayout( LayoutKind.Sequential )]
		public struct LicenseInfo
		{
			[MarshalAs( UnmanagedType.I1 )]
			public bool polygonData;
			[MarshalAs( UnmanagedType.I1 )]
			public bool recordingAndPlayback;
			[MarshalAs( UnmanagedType.I1 )]
			public bool timeCode;
			[MarshalAs( UnmanagedType.I1 )]
			public bool pluginsIcIdoVredSiemens;
			[MarshalAs( UnmanagedType.I1 )]
			public bool xsensSession;
			[MarshalAs( UnmanagedType.I1 )]
			public bool optitrackSession;
			[MarshalAs( UnmanagedType.I1 )]
			public bool unlimitedGloves;
			[MarshalAs( UnmanagedType.I1 )]
			public bool ergonomicData;
			[MarshalAs( UnmanagedType.I1 )]
			public bool pluginsMB;
		}

		[StructLayout( LayoutKind.Sequential )]
		public struct SettingsLandscape
		{
			public Version manusCoreVersion;
			public LicenseInfo license;
			[MarshalAs( UnmanagedType.I1 )]
			public bool playbackMode;
			[MarshalAs( UnmanagedType.I1 )]
			public bool ignoreSessionTimeOuts;

			public FirmwareVersion firmwareOne;
			public FirmwareVersion firmwareTwo;
		}

		[StructLayout( LayoutKind.Sequential )]
		public struct TimecodeInterface
		{
			[MarshalAs( UnmanagedType.ByValTStr, SizeConst = s_MaxNumCharsInTimecodeInterfaceStrings )]
			public string name;

			[MarshalAs( UnmanagedType.ByValTStr, SizeConst = s_MaxNumCharsInTimecodeInterfaceStrings )]
			public string api;
			public int index;
		}

		[StructLayout( LayoutKind.Sequential )]
		public struct TimeLandscape
		{
			[MarshalAs( UnmanagedType.ByValArray, SizeConst = s_MaxNumberOfTimecodeInterfaces )]
			public TimecodeInterface[] interfaces;
			public uint interfaceCount;

			public TimecodeInterface currentInterface;
			public TimecodeFPS fps;

			[MarshalAs( UnmanagedType.I1 )]
			public bool fakeTimecode;
			[MarshalAs( UnmanagedType.I1 )]
			public bool useSyncPulse;
			[MarshalAs( UnmanagedType.I1 )]
			public bool deviceKeepAlive;
			[MarshalAs( UnmanagedType.I1 )]
			public bool syncStatus;
			[MarshalAs( UnmanagedType.I1 )]
			public bool timecodeStatus;
		}

		[StructLayout( LayoutKind.Sequential )]
		public struct Landscape
		{
			public DeviceLandscape gloveDevices;
			public UserLandscape users;
			public SkeletonLandscape skeletons;
			public TrackerLandscape trackers;
			public SettingsLandscape settings;
			public TimeLandscape time;
		}

		#endregion

		[StructLayout( LayoutKind.Sequential )]
		[System.Serializable]
		public struct SkeletonSetupArraySizes
		{
			public uint nodesCount;
			public uint chainsCount;
			public uint collidersCount;
			public uint meshCount;
		}

		//Skeleton Data--------
		[System.Serializable]
		[StructLayout( LayoutKind.Sequential )]
		public struct NodeSettingsIK
		{
			public float ikAim;
		}

		[System.Serializable]
		[StructLayout( LayoutKind.Sequential )]
		public struct NodeSettingsFoot
		{
			public float heightFromGround;
		}

		[System.Serializable]
		[StructLayout( LayoutKind.Sequential )]
		public struct NodeSettingsRotationOffset
		{
			public ManusQuaternion value;
		}

		[StructLayout( LayoutKind.Sequential )]
		[System.Serializable]
		public struct NodeSettingsLeaf
		{
			public ManusVec3 direction;
			public float length;
		}

		[System.Serializable]
		[StructLayout( LayoutKind.Sequential )]
		public struct NodeSettings
		{
			public NodeSettingsFlag usedSettings;
			public NodeSettingsIK ik;
			public NodeSettingsFoot foot;
			public NodeSettingsRotationOffset rotationOffset;
			public NodeSettingsLeaf leaf;
		}

		[System.Serializable]
		[StructLayout( LayoutKind.Sequential )]
		public struct NodeSetup
		{
			public uint id;

			[MarshalAs( UnmanagedType.ByValTStr, SizeConst = s_MaxNumCharsInNodeName )]
			public string name;

			public NodeType type;
			public ManusTransform transform;
			public uint parentID;
			public NodeSettings settings;
		}

		[System.Serializable]
		[StructLayout( LayoutKind.Sequential )]
		public struct ChainSettingsPelvis
		{
			public float hipHeight;
			public float hipBendOffset;
			public float thicknessMultiplier;

			public static ChainSettingsPelvis Default()
			{
				return new ChainSettingsPelvis()
				{
					hipHeight = 1.0f, //TODO rename to hipHeightMultiplier
					hipBendOffset = 0.0f,
					thicknessMultiplier = 1.0f
				};
			}
		}

		[System.Serializable]
		[StructLayout( LayoutKind.Sequential )]
		public struct ChainSettingsLeg
		{
			[MarshalAs( UnmanagedType.I1 )]
			public bool reverseKneeDirection;
			public float kneeRotationOffset;
			public float footForwardOffset;
			public float footSideOffset;

			public static ChainSettingsLeg Default()
			{
				return new ChainSettingsLeg()
				{
					reverseKneeDirection = false,
					kneeRotationOffset = 0.0f,
					footForwardOffset = 0.0f,
					footSideOffset = 0.0f
				};
			}
		}

		[System.Serializable]
		[StructLayout( LayoutKind.Sequential )]
		public struct ChainSettingsSpine
		{
			public float spineBendOffset;

			public static ChainSettingsSpine Default()
			{
				return new ChainSettingsSpine() { spineBendOffset = 0.0f };
			}
		}

		[System.Serializable]
		[StructLayout( LayoutKind.Sequential )]
		public struct ChainSettingsNeck
		{
			public float neckBendOffset;

			public static ChainSettingsNeck Default()
			{
				return new ChainSettingsNeck() { neckBendOffset = 0.0f };
			}
		}

		[System.Serializable]
		[StructLayout( LayoutKind.Sequential )]
		public struct ChainSettingsHead
		{
			public float headPitchOffset;
			public float headYawOffset;
			public float headTiltOffset;
			[MarshalAs( UnmanagedType.I1 )]
			public bool useLeafAtEnd;

			public static ChainSettingsHead Default()
			{
				return new ChainSettingsHead()
				{
					headPitchOffset = 0.0f,
					headYawOffset = 0.0f,
					headTiltOffset = 0.0f,
					useLeafAtEnd = false
				};
			}
		}

		[System.Serializable]
		[StructLayout( LayoutKind.Sequential )]
		public struct ChainSettingsArm
		{
			public float armLengthMultiplier;
			public float elbowRotationOffset;

			public ManusVec3 armRotationOffset;

			public ManusVec3 positionMultiplier;
			public ManusVec3 positionOffset;

			public static ChainSettingsArm Default()
			{
				return new ChainSettingsArm()
				{
					armLengthMultiplier = 1.0f,
					elbowRotationOffset = 0.0f,
					armRotationOffset = new ManusVec3(),
					positionMultiplier = new ManusVec3( 1.0f, 1.0f, 1.0f ),
					positionOffset = new ManusVec3()
				};
			}
		}

		[System.Serializable]
		[StructLayout( LayoutKind.Sequential )]
		public struct ChainSettingsShoulder
		{
			public float forwardOffset;
			public float shrugOffset;

			public float forwardMultiplier;
			public float shrugMultiplier;

			public static ChainSettingsShoulder Default()
			{
				return new ChainSettingsShoulder()
				{
					forwardOffset = 0.0f,
					shrugOffset = 0.0f,
					forwardMultiplier = 1.0f,
					shrugMultiplier = 1.0f
				};
			}
		}

		[System.Serializable]
		[StructLayout( LayoutKind.Sequential )]
		public struct ChainSettingsFinger
		{
			[MarshalAs( UnmanagedType.I1 )]
			public bool useLeafAtEnd;
			public int metacarpalBoneId;
			public int handChainId;
			public int fingerWidth;

			public static ChainSettingsFinger Default()
			{
				return new ChainSettingsFinger()
				{
					useLeafAtEnd = false,
					metacarpalBoneId = -1,
					handChainId = -1,
					fingerWidth = 0
				};
			}
		}

		[System.Serializable]
		[StructLayout( LayoutKind.Sequential )]
		public struct ChainSettingsHand
		{
			[MarshalAs( UnmanagedType.ByValArray, SizeConst = s_MaxFingerIDS )]
			public int[] fingerChainIds;
			[NonSerialized]
			public int fingerChainIdsUsed;
			public HandMotion handMotion;

			public ChainSettingsHand( int p_FingerChainIdsUsed, int[] p_FingerChainIds, HandMotion p_HandMotion )
			{
				fingerChainIdsUsed = p_FingerChainIdsUsed;
				fingerChainIds = new int[p_FingerChainIdsUsed];
				for( int i = 0; i < fingerChainIds.Length; i++ )
				{
					fingerChainIds[i] = p_FingerChainIds[i];
				}
				handMotion = p_HandMotion;
			}

			public static ChainSettingsHand Default()
			{
				return new ChainSettingsHand()
				{
					fingerChainIds = new int[s_MaxFingerIDS],
					fingerChainIdsUsed = 0,
					handMotion = HandMotion.Auto
				};
			}
		}

		[System.Serializable]
		[StructLayout( LayoutKind.Sequential )]
		public struct ChainSettingsFoot
		{
			[MarshalAs( UnmanagedType.ByValArray, SizeConst = s_MaxToeIDS )]
			public int[] toeChainIds;

			[NonSerialized]
			public int toeChainIdsUsed;

			public ChainSettingsFoot( int p_FootToeChainIdsUsed, int[] p_FootToeChainIds )
			{
				toeChainIdsUsed = p_FootToeChainIdsUsed;
				toeChainIds = new int[p_FootToeChainIdsUsed];
				for( var i = 0; i < toeChainIds.Length; i++ )
				{
					toeChainIds[i] = p_FootToeChainIds[i];
				}
			}

			public static ChainSettingsFoot Default()
			{
				return new ChainSettingsFoot()
				{
					toeChainIds = new int[s_MaxToeIDS],
					toeChainIdsUsed = 0
				};
			}
		}

		[System.Serializable]
		[StructLayout( LayoutKind.Sequential )]
		public struct ChainSettingsToe
		{
			public int footChainId;
			[MarshalAs( UnmanagedType.I1 )]
			public bool useLeafAtEnd;

			public static ChainSettingsToe Default()
			{
				return new ChainSettingsToe()
				{
					footChainId = -1,
					useLeafAtEnd = false
				};
			}
		}

		[System.Serializable]
		[StructLayout( LayoutKind.Sequential )]
		public struct ChainSettings
		{
			[NonSerialized]
			public ChainType usedSettings;
			public ChainSettingsPelvis pelvis;
			public ChainSettingsLeg leg;
			public ChainSettingsSpine spine;
			public ChainSettingsNeck neck;
			public ChainSettingsHead head;
			public ChainSettingsArm arm;
			public ChainSettingsShoulder shoulder;
			public ChainSettingsFinger finger;
			public ChainSettingsHand hand;
			public ChainSettingsFoot foot;
			public ChainSettingsToe toe;

			public static ChainSettings Default()
			{
				return new ChainSettings()
				{
					pelvis = ChainSettingsPelvis.Default(),
					leg = ChainSettingsLeg.Default(),
					spine = ChainSettingsSpine.Default(),
					neck = ChainSettingsNeck.Default(),
					head = ChainSettingsHead.Default(),
					arm = ChainSettingsArm.Default(),
					shoulder = ChainSettingsShoulder.Default(),
					finger = ChainSettingsFinger.Default(),
					hand = ChainSettingsHand.Default(),
					foot = ChainSettingsFoot.Default(),
					toe = ChainSettingsToe.Default(),
				};
			}
		}

		[System.Serializable]
		[StructLayout( LayoutKind.Sequential )]
		public struct ChainSetup
		{
			public uint id;
			public ChainType type;
			public ChainType dataType;
			public uint dataIndex;
			public uint nodeIdCount;
			[MarshalAs( UnmanagedType.ByValArray, SizeConst = s_MaxChainLength )]
			public uint[] nodeIds;
			public ChainSettings settings;
			public Side side;
		}

		[System.Serializable]
		[StructLayout( LayoutKind.Sequential )]
		public struct SphereColliderSetup
		{
			public float radius;

			public static SphereColliderSetup Default()
			{
				return new SphereColliderSetup() { radius = 0 };
			}
		}

		[System.Serializable]
		[StructLayout( LayoutKind.Sequential )]
		public struct CapsuleColliderSetup
		{
			public float radius;
			public float length;

			public static CapsuleColliderSetup Default()
			{
				return new CapsuleColliderSetup()
				{
					radius = 0,
					length = 0
				};
			}
		}

		[System.Serializable]
		[StructLayout( LayoutKind.Sequential )]
		public struct BoxColliderSetup
		{
			public ManusVec3 size;

			public static BoxColliderSetup Default()
			{
				return new BoxColliderSetup()
				{
					size = new ManusVec3
					{
						x = 0,
						y = 0,
						z = 0
					},
				};
			}
		}

		[System.Serializable]
		[StructLayout( LayoutKind.Sequential )]
		public struct ColliderSetup
		{
			public uint nodeId;
			public ManusVec3 localPosition;
			public ManusVec3 localRotation;

			public ColliderType type;
			public SphereColliderSetup sphere;
			public CapsuleColliderSetup capsule;
			public BoxColliderSetup box;
		}

		[System.Serializable]
		[StructLayout( LayoutKind.Sequential )]
		public struct Weight
		{
			public uint nodeId;
			public float weightValue;

			public Weight(uint p_NodeId, float p_WeightValue)
			{
				nodeId = p_NodeId;
				weightValue = p_WeightValue;
			}
		}

		[System.Serializable]
		[StructLayout( LayoutKind.Sequential )]
		public struct Vertex
		{
			public ManusVec3 position;
			public uint weightCount;
			[MarshalAs( UnmanagedType.ByValArray, SizeConst = s_MaxBoneWeightsPerVertex )]
			public Weight[] weights;
		}

		[System.Serializable]
		[StructLayout( LayoutKind.Sequential )]
		public struct Triangle
		{
			public int vertexIndex1;
			public int vertexIndex2;
			public int vertexIndex3;
		}

		[System.Serializable]
		[StructLayout( LayoutKind.Sequential )]
		public struct SkeletonTargetUserData
		{
			public uint id;
		}

		[System.Serializable]
		[StructLayout( LayoutKind.Sequential )]
		public struct SkeletonTargetUserIndexData
		{
			public uint index;
		}

		[System.Serializable]
		[StructLayout( LayoutKind.Sequential )]
		public struct SkeletonTargetAnimationData
		{
			[MarshalAs( UnmanagedType.ByValTStr, SizeConst = s_MaxNumberOfCharsInTargetId )]
			public string id;
		}

		[System.Serializable]
		[StructLayout( LayoutKind.Sequential )]
		public struct SkeletonTargetGloveData
		{
			public uint id;
		}

		[System.Serializable]
		[StructLayout( LayoutKind.Sequential )]
		public struct SkeletonSettings
		{
			[MarshalAs( UnmanagedType.I1 )]
			public bool scaleToTarget;
			[MarshalAs( UnmanagedType.I1 )]
			public bool useEndPointApproximations;

			public CollisionType collisionType;
			public SkeletonTargetType targetType;
			public SkeletonTargetUserData skeletonTargetUserData;
			public SkeletonTargetUserIndexData skeletonTargetUserIndexData;
			public SkeletonTargetAnimationData skeletonTargetAnimationData;
			public SkeletonTargetGloveData skeletonTargetGloveData;
		}

		/// @brief All the skeleton setup that can be sent or received.
		[System.Serializable]
		[StructLayout( LayoutKind.Sequential )]
		public struct SkeletonSetupInfo
		{
			public uint id;
			public SkeletonType type;
			public SkeletonSettings settings;
			[MarshalAs( UnmanagedType.ByValTStr, SizeConst = s_MaxNumCharsInSkeletonName )]
			public string name;
		}

		[StructLayout( LayoutKind.Sequential )]
		public struct TemporarySkeletonInfo
		{
			[MarshalAs( UnmanagedType.ByValTStr, SizeConst = s_MaxNumCharsInSkeletonName )]
			public string name;
			public uint index;
		}

		[StructLayout( LayoutKind.Sequential )]
		public struct TemporarySkeletonsInfoForSession
		{
			public uint sessionId;
			[MarshalAs( UnmanagedType.ByValTStr, SizeConst = s_MaxNumCharsInSessionName )]
			public string sessionName;
			public uint skeletonCount;
			[MarshalAs( UnmanagedType.ByValArray, SizeConst = s_MaxNumberOfSkeletonsPerSession )]
			public TemporarySkeletonInfo[] skeletonInfo;
		}

		[StructLayout( LayoutKind.Sequential )]
		public struct TemporarySkeletonCountForSession
		{
			public uint sessionId;
			[MarshalAs( UnmanagedType.ByValTStr, SizeConst = s_MaxNumCharsInSkeletonName )]
			public string sessionName;
			public uint skeletonCount;
		}

		[StructLayout( LayoutKind.Sequential )]
		public struct TemporarySkeletonCountForSessions
		{
			public uint sessionCount;
			[MarshalAs( UnmanagedType.ByValArray, SizeConst = s_MaxNumberOfSessions )]
			public TemporarySkeletonCountForSession[] temporarySkeletonCountForSessions;
		}

		[StructLayout( LayoutKind.Sequential )]
		public struct SystemMessage
		{
			public SystemMessageType type;
			[MarshalAs( UnmanagedType.ByValTStr, SizeConst = s_MaxNumCharsInSystemErrorMessage )]
			public string infoString;
			public uint infoUInt;
		}
		//--End Skeleton Data

		// coordinate system settings
		[StructLayout( LayoutKind.Sequential )]
		[System.Serializable]
		public struct CoordinateSystemVUH
		{
			public AxisView view;
			public AxisPolarity up;
			public Side handedness;
			public float unitScale;
		}

		[StructLayout( LayoutKind.Sequential )]
		[System.Serializable]
		public struct CoordinateSystemDirection
		{
			public AxisDirection x;
			public AxisDirection y;
			public AxisDirection z;
			public float unitScale;
		}

		protected partial class ManusDLLImport
		{
			#region Wrapper startup and shutdown.

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_Initialize( SessionType p_Type );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_ShutDown();

			#endregion

			#region Utility functions

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_WasDllBuiltInDebugConfiguration( out bool p_WasBuiltInDebugConfiguration );

			/// @brief Gets the timestamp info (more readable form of timestamp).
			/// @param p_Timestamp Timestamp to get info from
			/// @param p_Info Info of the timestamp
			/// @return SDKReturnCode_Success if successful.
			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_GetTimestampInfo( Timestamp p_Timestamp, out TimestampInfo p_Info );

			/// @brief Sets the timestamp according to the info (more readable form of timestamp).
			/// @param p_Timestamp the Timestamp to set info of
			/// @param p_Info Info to get info from
			/// @return SDKReturnCode_Success if successful.
			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_SetTimestampInfo( out Timestamp p_Timestamp, TimestampInfo p_Info );
			#endregion

			#region Connection handling

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_LookForHosts( uint p_WaitSeconds = 1, bool p_LoopbackOnly = false );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_GetNumberOfAvailableHostsFound( out uint p_NumberOfAvailableHostsFound );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_GetAvailableHostsFound( [Out] ManusHost[] p_Host, uint p_NumberOfHostsThatFitInArray );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_GetIsConnectedToCore( out bool p_ConnectedToCore );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_ConnectGRPC();

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_ConnectToHost( ManusHost p_Host );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_InitializeCoordinateSystemWithVUH( CoordinateSystemVUH p_CoordinateSystem,
				bool p_UseWorldCoordinates = true );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_InitializeCoordinateSystemWithDirection( CoordinateSystemDirection p_CoordinateSystem,
				bool p_UseWorldCoordinates = true );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_GetVersionsAndCheckCompatibility( out ManusVersion p_SdkVersion,
				out ManusVersion p_CoreVersion, out bool p_AreVersionsCompatible );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_GetSessionId( out uint p_SessionId );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_RegisterCallbackForOnConnect( OnConnectedToCorePtr p_OnConnectToCore );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_RegisterCallbackForOnDisconnect( OnDisconnectFromCorePtr p_OnDisconnectFromCore );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_RegisterCallbackForSkeletonStream( InternalSkeletonStreamCallbackPtr p_OnSkeletonInfo );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_RegisterCallbackForLandscapeStream( LandscapeStreamCallbackPtr p_OnLandscape );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_RegisterCallbackForSystemStream( SystemStreamCallbackPtr p_OnSystem );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_RegisterCallbackForErgonomicsStream( ErgonomicsStreamCallbackPtr p_OnErgonomics );

			#endregion

			#region Basic glove interactions

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_VibrateWristOfGlove( uint p_GloveId, float p_UnitStrength, ushort p_DurationInMilliseconds );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_VibrateFingers( uint p_DongleId, Side p_HandType, [In] float[] p_Powers );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_VibrateFingersForSkeleton( uint p_SkeletonId, Side p_HandType, [In] float[] p_Powers );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_GetGloveIdOfUser_UsingUserId( uint p_UserId, Side p_HandType, out uint p_GloveId );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_GetNumberOfAvailableGloves( out uint p_NumberOfAvailableGloves );

			//Test again see if it works as intended
			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_GetIdsOfAvailableGloves( [Out] uint[] p_IdsOfAvailableGloves,
				uint p_NumberOfIdsThatFitInArray );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_GetGlovesForDongle( uint p_DongleId, out uint p_LeftGloveId, out uint p_RightGloveId );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_GetDataForGlove_UsingGloveId( uint p_GloveId, out GloveLandscapeData p_GloveData );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_GetDataForDongle( uint p_DongleId, out DongleLandscapeData p_DongleData );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_GetNumberOfDongles( out uint p_NumberOfDongles );

			//Look into this could be marshaling issue? Always returns 0 as ID
			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_GetDongleIds( [Out] uint[] p_DongleIds, uint p_NumberOfIdsThatFitInArray );

			#endregion

			#region Haptics module

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_GetNumberOfHapticsDongles( out uint p_NumberOfHapticsDongles );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_GetHapticsDongleIds( [Out] uint[] p_HapticDongleIds, uint p_NumberOfIdsThatFitInArray );

			#endregion

			#region Users

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_GetNumberOfAvailableUsers( out uint p_NumberOfAvailableUsers );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_GetIdsOfAvailableUsers( [Out] uint[] p_IdsOfAvailablePolygonUsers,
				uint p_NumberOfIdsThatFitInArray );

			#endregion

			#region Tracking

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_GetNumberOfAvailableTrackers( out uint p_NumberOfAvailableTrackers );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_GetIdsOfAvailableTrackers( [Out] TrackerId[] p_IdsOfAvailableTrackers,
				uint p_NumberOfIdsThatFitInArray );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_GetNumberOfAvailableTrackersForUserId( out uint p_NumberOfAvailableTrackers, uint p_UserId );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_GetIdsOfAvailableTrackersForUserId( [Out] TrackerId[] p_IdsOfAvailableTrackers, uint p_UserId,
				uint p_NumberOfIdsThatFitInArray );
			
			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_GetNumberOfAvailableTrackersForUserIndex( out uint p_NumberOfAvailableTrackers, uint p_UserIndex );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_GetIdsOfAvailableTrackersForUserIndex( [Out] TrackerId[] p_IdsOfAvailableTrackers, uint p_UserIndex,
				uint p_NumberOfIdsThatFitInArray );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_GetDataForTracker_UsingTrackerId( TrackerId p_TrackerId, out TrackerData p_TrackerData );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_GetDataForTracker_UsingIdAndType( uint p_UserId, uint p_TrackerType,
				ref TrackerData p_TrackerData );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_SendDataForTrackers( [In] TrackerData[] p_TrackerData, uint p_NumberOfTrackers );

			#endregion

			#region Skeletal System

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_GetSkeletonInfo( uint p_SkeletonIndex, out InternalSkeletonInfo p_SklInfo );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_GetSkeletonData( uint p_SkeletonIndex, [Out] SkeletonNode[] p_Nodes, uint p_NodeCount );

			#endregion

			#region Skeletal Setup

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_OverwriteSkeletonSetup( uint p_SkeletonSetupIndex, SkeletonSetupInfo p_Skeleton );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_CreateSkeletonSetup( SkeletonSetupInfo p_Skeleton, out uint p_SkeletonSetupIndex );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_AddNodeToSkeletonSetup( uint p_SkeletonSetupIndex, NodeSetup p_Node );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_AddChainToSkeletonSetup( uint p_SkeletonSetupIndex, ChainSetup p_Chain );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_AddColliderToSkeletonSetup( uint p_SkeletonSetupIndex, ColliderSetup p_Collider );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_AddMeshSetupToSkeletonSetup( uint p_SkeletonSetupIndex, uint p_NodeId, out uint p_MeshSetupIndex );
			
			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_AddVertexToMeshSetup( uint p_SkeletonSetupIndex, uint p_MeshSetupIndex, Vertex p_Vertex );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_AddTriangleToMeshSetup( uint p_SkeletonSetupIndex, uint p_MeshSetupIndex, Triangle p_Triangle );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_OverwriteChainToSkeletonSetup( uint p_SkeletonSetupIndex, ChainSetup p_Chain );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_GetSkeletonSetupArraySizes( uint p_SkeletonSetupIndex,
				out SkeletonSetupArraySizes p_ChainSetup );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_AllocateChainsForSkeletonSetup( uint p_SkeletonSetupIndex );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_GetSkeletonSetupInfo( uint p_SkeletonSetupIndex, out SkeletonSetupInfo p_SkeletonSetupInfo );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_GetSkeletonSetupChains( uint p_SkeletonSetupIndex, [Out] ChainSetup[] p_ChainSetup );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_GetSkeletonSetupNodes( uint p_SkeletonSetupIndex, [Out] NodeSetup[] p_NodeSetup );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_GetSkeletonSetupColliders( uint p_SkeletonSetupIndex, [Out] ColliderSetup[] p_ColliderSetup );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_LoadSkeleton( uint p_SkeletonSetupIndex, out uint p_SkeletonId );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_UnloadSkeleton( uint p_SkeletonId );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_SaveTemporarySkeleton( uint p_SkeletonSetupIndex, uint p_SkeletonId,
				bool p_IsSkeletonModified );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_CompressTemporarySkeletonAndGetSize( uint p_SkeletonSetupIndex, uint p_SkeletonId, out uint p_TemporarySkeletonLengthInBytes );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_GetCompressedTemporarySkeletonData( [In, Out] byte[] p_TemporarySkeletonData, uint p_TemporarySkeletonLengthInBytes );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_GetTemporarySkeleton( uint p_SkeletonSetupIndex, uint p_SessionId );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_GetTemporarySkeletonFromCompressedData( uint p_SkeletonSetupIndex, uint p_SessionId, [Out] byte[] p_TemporarySkeletonData, uint p_TemporarySkeletonLengthInBytes );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_ClearTemporarySkeleton( uint p_SkeletonSetupIndex, uint p_SessionId );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_ClearAllTemporarySkeletons();

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_GetTemporarySkeletonCountForAllSessions(
				out TemporarySkeletonCountForSessions p_TemporarySkeletonCountForSessions );

			[DllImport( s_DLLName, CallingConvention = s_ImportCallingConvention, CharSet = s_ImportCharSet )]
			public static extern SDKReturnCode CoreSdk_GetTemporarySkeletonsInfoForSession( 
				uint p_SessionId, out TemporarySkeletonsInfoForSession p_TemporarySkeletonsInfoForSession );

			#endregion
		}
	}
}
